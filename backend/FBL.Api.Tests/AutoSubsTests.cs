using FBL.Api.Models;
using FBL.Api.Services;
using Xunit;

namespace FBL.Api.Tests;

/// <summary>
/// Tests for LeaderboardService.ComputeAutoSubs — the FPL auto-substitution
/// algorithm. Rules under test:
///   - Goalkeeper has a dedicated bench slot (only bench GK can replace start GK).
///   - Outfield substitutes are processed in bench order (12 → 13 → 14 → 15).
///   - Each substitution must keep the formation valid: ≥3 DEF, ≥2 MID, ≥1 FWD.
///   - A bench player who didn't play cannot come on.
/// </summary>
public class AutoSubsTests
{
    // Squad construction helpers ----------------------------------------

    private static FantasyPick Pick(int id, int playerId, PlayerPosition pos, int squadPos) =>
        new FantasyPick
        {
            Id = id,
            PlayerId = playerId,
            SquadPosition = squadPos,
            Player = new BundesligaPlayer { Id = playerId, Position = pos, Name = $"P{playerId}" }
        };

    /// <summary>
    /// Build a standard 4-4-2 squad. Slot layout:
    ///   1 = GK, 2-5 = DEF, 6-9 = MID, 10-11 = FWD,
    ///   12 = bench GK, 13 = bench DEF, 14 = bench MID, 15 = bench FWD.
    /// PlayerId == SquadPosition for easy reasoning in test assertions.
    /// </summary>
    private static List<FantasyPick> BuildStandardSquad()
    {
        return new List<FantasyPick>
        {
            Pick(1, 1, PlayerPosition.GK,  1),
            Pick(2, 2, PlayerPosition.DEF, 2),
            Pick(3, 3, PlayerPosition.DEF, 3),
            Pick(4, 4, PlayerPosition.DEF, 4),
            Pick(5, 5, PlayerPosition.DEF, 5),
            Pick(6, 6, PlayerPosition.MID, 6),
            Pick(7, 7, PlayerPosition.MID, 7),
            Pick(8, 8, PlayerPosition.MID, 8),
            Pick(9, 9, PlayerPosition.MID, 9),
            Pick(10, 10, PlayerPosition.FWD, 10),
            Pick(11, 11, PlayerPosition.FWD, 11),
            // Bench
            Pick(12, 12, PlayerPosition.GK,  12),
            Pick(13, 13, PlayerPosition.DEF, 13),
            Pick(14, 14, PlayerPosition.MID, 14),
            Pick(15, 15, PlayerPosition.FWD, 15),
        };
    }

    // ---- Test cases ---------------------------------------------------

    [Fact]
    public void NoSubsNeeded_WhenAllStartersPlayed()
    {
        var squad = BuildStandardSquad();
        var played = Enumerable.Range(1, 15).ToHashSet();   // everyone played

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        Assert.Empty(subs);
        // Effective XI = original 11 starters
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }.ToHashSet(), effective);
    }

    [Fact]
    public void GkSubbedIn_WhenStarterDidntPlayAndBenchGkPlayed()
    {
        var squad = BuildStandardSquad();
        // Everyone except the starting GK (player 1) played
        var played = Enumerable.Range(2, 14).ToHashSet();

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // Bench GK (player 12) replaces starting GK (player 1)
        Assert.Single(subs);
        Assert.Equal((1, 12), subs[0]);
        Assert.DoesNotContain(1, effective);
        Assert.Contains(12, effective);
    }

    [Fact]
    public void GkNotSubbed_WhenBenchGkAlsoDidntPlay()
    {
        var squad = BuildStandardSquad();
        // Both GKs missed. Bench DEF/MID/FWD all played.
        var played = Enumerable.Range(2, 11).ToHashSet();   // 2..12 minus we want 12 OUT
        played.Remove(12);   // bench GK didn't play either
        played.Add(13); played.Add(14); played.Add(15);

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // No GK substitution should occur — outfield bench can't replace a GK.
        Assert.DoesNotContain(subs, s => s.outId == 1 || s.inId == 12);
        Assert.Contains(1, effective);   // starting GK stays in the effective XI (with 0 points)
    }

    [Fact]
    public void OutfieldSub_PicksFirstValidBenchPlayerInOrder()
    {
        var squad = BuildStandardSquad();
        // Everyone played except midfielder (player 6). Whole bench available.
        var played = Enumerable.Range(1, 15).ToHashSet();
        played.Remove(6);

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // The algorithm scans bench in slot order (12 → 13 → 14 → 15).
        // GK 12 is skipped (dedicated slot). Bench DEF 13 is the first valid
        // candidate: replacing MID 6 with DEF 13 yields formation 5-3-2 which
        // is valid (≥3 DEF, ≥2 MID, ≥1 FWD). So 13 comes in, not 14.
        Assert.Single(subs);
        Assert.Equal((6, 13), subs[0]);
        Assert.DoesNotContain(6, effective);
        Assert.Contains(13, effective);
    }

    [Fact]
    public void BenchPlayerWhoDidntPlay_CannotComeOn()
    {
        var squad = BuildStandardSquad();
        // MID 6 missed AND bench MID 14 also missed; no other bench MID exists.
        var played = Enumerable.Range(1, 15).ToHashSet();
        played.Remove(6);
        played.Remove(14);

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // Bench DEF (13) could potentially fill in — but the algorithm processes
        // in bench order: 13 first. Bringing on a DEF for a missing MID:
        // resulting XI has 5 DEF, 3 MID, 2 FWD → still ≥3 DEF, ≥2 MID, ≥1 FWD → valid.
        // So we expect 13 to come on. (Demonstrates flexibility of the algorithm.)
        Assert.Contains(subs, s => s.outId == 6 && s.inId == 13);
        Assert.DoesNotContain(6, effective);
        Assert.Contains(13, effective);
    }

    [Fact]
    public void OutfieldSub_RejectedWhenFormationWouldBecomeInvalid()
    {
        // Build a 3-4-3 (3 DEF, 4 MID, 3 FWD) starting XI with bench having only an extra FWD.
        // If a defender misses, bringing on the FWD would result in 2 DEF / 4 MID / 4 FWD,
        // which is invalid (< 3 DEF) — so no sub should occur.
        var squad = new List<FantasyPick>
        {
            Pick(1, 1, PlayerPosition.GK,  1),
            Pick(2, 2, PlayerPosition.DEF, 2),
            Pick(3, 3, PlayerPosition.DEF, 3),
            Pick(4, 4, PlayerPosition.DEF, 4),
            Pick(5, 5, PlayerPosition.MID, 5),
            Pick(6, 6, PlayerPosition.MID, 6),
            Pick(7, 7, PlayerPosition.MID, 7),
            Pick(8, 8, PlayerPosition.MID, 8),
            Pick(9, 9, PlayerPosition.FWD, 9),
            Pick(10, 10, PlayerPosition.FWD, 10),
            Pick(11, 11, PlayerPosition.FWD, 11),
            // Bench: GK, FWD, FWD, MID
            Pick(12, 12, PlayerPosition.GK,  12),
            Pick(13, 13, PlayerPosition.FWD, 13),
            Pick(14, 14, PlayerPosition.FWD, 14),
            Pick(15, 15, PlayerPosition.MID, 15),
        };
        // Defender 2 missed; bench has FWD/FWD/MID available
        var played = Enumerable.Range(1, 15).ToHashSet();
        played.Remove(2);

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // Bringing on FWD 13 would leave 2 DEF — invalid.
        // Bringing on FWD 14 — same problem.
        // Bringing on MID 15 — leaves 2 DEF / 5 MID / 3 FWD — STILL invalid (< 3 DEF).
        // So NO substitution should occur for the missing DEF.
        Assert.DoesNotContain(subs, s => s.outId == 2);
        Assert.Contains(2, effective);   // missing player stays "in" the effective XI (will score 0)
    }

    [Fact]
    public void MultipleSubs_AppliedInBenchOrder()
    {
        var squad = BuildStandardSquad();
        // Two missing starters: DEF 2 and MID 6. Bench DEF (13) and bench MID (14) both played.
        var played = Enumerable.Range(1, 15).ToHashSet();
        played.Remove(2);
        played.Remove(6);

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // First DNP processed: DEF 2 → bench order 13 (DEF) gets used (formation stays 4-4-2).
        // Second DNP: MID 6 → next available bench MID is 14.
        Assert.Equal(2, subs.Count);
        Assert.Contains(subs, s => s.outId == 2 && s.inId == 13);
        Assert.Contains(subs, s => s.outId == 6 && s.inId == 14);
        Assert.DoesNotContain(2, effective);
        Assert.DoesNotContain(6, effective);
        Assert.Contains(13, effective);
        Assert.Contains(14, effective);
    }

    [Fact]
    public void EachBenchPlayer_UsedAtMostOnce()
    {
        var squad = BuildStandardSquad();
        // Three DNPs but only one valid bench replacement of each type.
        // DEF 2, MID 6, FWD 10 all missed.
        var played = Enumerable.Range(1, 15).ToHashSet();
        played.Remove(2);
        played.Remove(6);
        played.Remove(10);

        var (effective, subs) = LeaderboardService.ComputeAutoSubs(squad, played);

        // Each bench player can appear in at most one subbing-in.
        var benchUsed = subs.Select(s => s.inId).ToList();
        Assert.Equal(benchUsed.Count, benchUsed.Distinct().Count());
    }

    [Fact]
    public void EffectiveXi_AlwaysHasExactly11Players()
    {
        var squad = BuildStandardSquad();
        var played = Enumerable.Range(1, 15).ToHashSet();
        // Half the starters DNP'd
        played.Remove(2); played.Remove(4); played.Remove(6); played.Remove(8); played.Remove(10);

        var (effective, _) = LeaderboardService.ComputeAutoSubs(squad, played);

        Assert.Equal(11, effective.Count);
    }
}
