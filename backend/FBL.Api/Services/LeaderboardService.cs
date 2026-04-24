using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

public class LeaderboardService
{
    private readonly AppDbContext _db;

    public LeaderboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LeagueStandingDto>> GetGlobalLeaderboard(int page = 1, int pageSize = 50)
    {
        var standings = await _db.FantasyTeams
            .Include(t => t.User)
            .OrderByDescending(t => t.TotalPoints + t.GameweekPoints)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new LeagueStandingDto(
                0,
                t.UserId,
                t.User.DisplayName,
                t.Name,
                t.TotalPoints + t.GameweekPoints,
                t.GameweekPoints
            ))
            .ToListAsync();

        return standings.Select((s, i) => s with { Rank = (page - 1) * pageSize + i + 1 }).ToList();
    }

    public async Task<List<LeagueStandingDto>> GetLeagueStandings(int leagueId)
    {
        var members = await _db.LeagueMembers
            .Where(lm => lm.LeagueId == leagueId)
            .Include(lm => lm.User)
            .ThenInclude(u => u.FantasyTeam)
            .OrderByDescending(lm => (lm.User.FantasyTeam!.TotalPoints + lm.User.FantasyTeam.GameweekPoints))
            .ToListAsync();

        return members.Select((m, i) => new LeagueStandingDto(
            i + 1,
            m.UserId,
            m.User.DisplayName,
            m.User.FantasyTeam?.Name ?? "No Team",
            (m.User.FantasyTeam?.TotalPoints ?? 0) + (m.User.FantasyTeam?.GameweekPoints ?? 0),
            m.User.FantasyTeam?.GameweekPoints ?? 0
        )).ToList();
    }

    /// <summary>
    /// Compute auto-substitutions (FPL rules):
    ///   - Starter who didn't play (0 min) is subbed out
    ///   - Starting GK is replaced by bench GK only (dedicated slot)
    ///   - Outfield DNPs are replaced by bench outfielders in bench order (12→13→14→15),
    ///     provided the resulting formation stays valid (≥3 DEF, ≥2 MID, ≥1 FWD).
    ///   - A bench player who didn't play cannot come on.
    /// Returns the set of PlayerIds that make up the effective playing XI for scoring.
    /// </summary>
    public static (HashSet<int> effectivePlayingIds, List<(int outId, int inId)> subs) ComputeAutoSubs(
        List<FantasyPick> picks,
        HashSet<int> playedIds)
    {
        var starters = picks.Where(p => p.SquadPosition <= 11).ToList();
        var bench = picks.Where(p => p.SquadPosition >= 12).OrderBy(p => p.SquadPosition).ToList();

        var subs = new List<(int, int)>();
        var effective = new List<FantasyPick>(starters);

        // --- GK sub (dedicated slot) ---
        var startGk = starters.FirstOrDefault(p => p.Player.Position == PlayerPosition.GK);
        var benchGk = bench.FirstOrDefault(p => p.Player.Position == PlayerPosition.GK);
        if (startGk != null && !playedIds.Contains(startGk.PlayerId)
            && benchGk != null && playedIds.Contains(benchGk.PlayerId))
        {
            effective.Remove(startGk);
            effective.Add(benchGk);
            subs.Add((startGk.PlayerId, benchGk.PlayerId));
        }

        // --- Outfield subs, in bench order ---
        var usedBench = new HashSet<int>();
        var outfieldBench = bench
            .Where(p => p.Player.Position != PlayerPosition.GK && playedIds.Contains(p.PlayerId))
            .OrderBy(p => p.SquadPosition)
            .ToList();

        var dnpOutfield = starters
            .Where(p => p.Player.Position != PlayerPosition.GK && !playedIds.Contains(p.PlayerId))
            .OrderBy(p => p.SquadPosition)
            .ToList();

        foreach (var dnp in dnpOutfield)
        {
            foreach (var cand in outfieldBench)
            {
                if (usedBench.Contains(cand.Id)) continue;

                var trial = effective.Where(p => p.Id != dnp.Id).ToList();
                trial.Add(cand);

                int defs = trial.Count(p => p.Player.Position == PlayerPosition.DEF);
                int mids = trial.Count(p => p.Player.Position == PlayerPosition.MID);
                int fwds = trial.Count(p => p.Player.Position == PlayerPosition.FWD);

                if (defs >= 3 && mids >= 2 && fwds >= 1)
                {
                    effective = trial;
                    usedBench.Add(cand.Id);
                    subs.Add((dnp.PlayerId, cand.PlayerId));
                    break;
                }
            }
        }

        return (effective.Select(p => p.PlayerId).ToHashSet(), subs);
    }

    /// <summary>
    /// Compute per-team GameweekPoints for the given gameweek and write to the DB.
    /// Does NOT touch TotalPoints — that only changes on GW finalisation.
    /// Safe to call after every match event (idempotent).
    /// </summary>
    public async Task RecalculateLiveGameweekPoints(int gameweekId)
    {
        var teams = await _db.FantasyTeams
            .Include(t => t.Picks.Where(p => p.GameweekId == gameweekId))
                .ThenInclude(p => p.Player)
            .Include(t => t.ChipUsages.Where(c => c.GameweekId == gameweekId))
            .ToListAsync();

        var pointsByPlayer = await _db.MatchEvents
            .Where(e => e.GameweekId == gameweekId)
            .GroupBy(e => e.PlayerId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Points));

        var minutesByPlayer = await _db.MatchEvents
            .Where(e => e.GameweekId == gameweekId && e.EventType == EventType.MinutesPlayed)
            .GroupBy(e => e.PlayerId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Minute ?? 0));

        var playedIds = minutesByPlayer.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToHashSet();

        foreach (var team in teams)
        {
            if (team.Picks.Count == 0)
            {
                team.GameweekPoints = 0;
                continue;
            }

            var activeChip = team.ChipUsages.FirstOrDefault()?.ChipType;
            int gwPoints = 0;

            HashSet<int> scoringIds;
            if (activeChip == ChipType.BenchBoost)
            {
                scoringIds = team.Picks.Select(p => p.PlayerId).ToHashSet();
            }
            else
            {
                var (effective, _) = ComputeAutoSubs(team.Picks.ToList(), playedIds);
                scoringIds = effective;
            }

            var captainPick = team.Picks.FirstOrDefault(p => p.IsCaptain);
            var vicePick = team.Picks.FirstOrDefault(p => p.IsViceCaptain);
            int? captainId = captainPick?.PlayerId;

            if (captainPick != null && !playedIds.Contains(captainPick.PlayerId)
                && vicePick != null && playedIds.Contains(vicePick.PlayerId))
            {
                captainId = vicePick.PlayerId;
            }

            foreach (var pick in team.Picks)
            {
                if (!scoringIds.Contains(pick.PlayerId)) continue;
                if (!pointsByPlayer.TryGetValue(pick.PlayerId, out var pts)) pts = 0;

                int multiplier = 1;
                if (pick.PlayerId == captainId)
                    multiplier = activeChip == ChipType.TripleCaptain ? 3 : 2;

                gwPoints += pts * multiplier;
            }

            team.GameweekPoints = gwPoints;
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Called when a gameweek transitions to Finished.
    /// Recomputes one last time and consolidates GameweekPoints into TotalPoints.
    /// </summary>
    public async Task FinaliseGameweek(int gameweekId)
    {
        await RecalculateLiveGameweekPoints(gameweekId);

        var teams = await _db.FantasyTeams.ToListAsync();
        foreach (var t in teams)
        {
            t.TotalPoints += t.GameweekPoints;
            t.GameweekPoints = 0;
        }
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Called when a new gameweek goes Live. Resets the running GW counter for every team.
    /// </summary>
    public async Task ResetGameweekPoints()
    {
        var teams = await _db.FantasyTeams.ToListAsync();
        foreach (var t in teams) t.GameweekPoints = 0;
        await _db.SaveChangesAsync();
    }

    // Kept for backward compatibility with existing admin code paths
    public Task RecalculateAllPoints(int gameweekId) => FinaliseGameweek(gameweekId);
}
