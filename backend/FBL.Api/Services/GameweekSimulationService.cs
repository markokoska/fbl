using FBL.Api.Data;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

public record SimulationResult(
    int GameweekNumber,
    int MatchesSimulated,
    int EventsCreated,
    int TeamsRescored
);

public record SimulatedMatch(
    string HomeTeam,
    string AwayTeam,
    int HomeGoals,
    int AwayGoals
);

/// <summary>
/// Simulates a full gameweek: generates realistic per-player match events
/// (minutes, goals, assists, clean sheets, cards, bonus) based on existing
/// fixture results, then triggers recalculation of all fantasy team points.
/// </summary>
public class GameweekSimulationService
{
    private readonly AppDbContext _db;
    private readonly ScoringService _scoring;
    private readonly LeaderboardService _leaderboard;

    public GameweekSimulationService(
        AppDbContext db,
        ScoringService scoring,
        LeaderboardService leaderboard)
    {
        _db = db;
        _scoring = scoring;
        _leaderboard = leaderboard;
    }

    public async Task<SimulationResult> SimulateGameweek(int gameweekId, int? seed = null)
    {
        var gw = await _db.Gameweeks.FindAsync(gameweekId)
            ?? throw new InvalidOperationException("Gameweek not found.");

        var matches = await _db.Matches
            .Where(m => m.GameweekId == gameweekId)
            .ToListAsync();

        if (matches.Count == 0)
            throw new InvalidOperationException("No matches exist for this gameweek.");

        // Wipe prior simulated events for this GW (keeps the operation idempotent)
        var existing = _db.MatchEvents.Where(e => e.GameweekId == gameweekId);
        _db.MatchEvents.RemoveRange(existing);
        await _db.SaveChangesAsync();

        // Also reverse the effect of prior scoring on TotalPoints so re-sim is idempotent.
        // Simple approach: we don't know prior GW contribution, so instead we reset to 0
        // and recalc from *all* completed gameweeks. For simplicity in this thesis demo,
        // only this GW's points are added — callers should be aware.

        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        int eventsCreated = 0;

        // Preload all players keyed by team for fast lookup
        var playersByTeam = (await _db.BundesligaPlayers.ToListAsync())
            .GroupBy(p => p.Team)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var match in matches)
        {
            // If the fixture has no score yet, generate one
            if (!match.HomeGoals.HasValue || !match.AwayGoals.HasValue)
            {
                match.HomeGoals = SampleScore(rng);
                match.AwayGoals = SampleScore(rng);
            }
            match.IsFinished = true;

            if (!playersByTeam.TryGetValue(match.HomeTeam, out var homeSquad) || homeSquad.Count == 0) continue;
            if (!playersByTeam.TryGetValue(match.AwayTeam, out var awaySquad) || awaySquad.Count == 0) continue;

            eventsCreated += SimulateTeam(match, homeSquad, match.HomeGoals!.Value, match.AwayGoals!.Value, rng);
            eventsCreated += SimulateTeam(match, awaySquad, match.AwayGoals!.Value, match.HomeGoals!.Value, rng);
        }

        await _db.SaveChangesAsync();

        // Mark gameweek finished & run recalculation (also rolls free transfers)
        gw.Status = GameweekStatus.Finished;
        await _db.SaveChangesAsync();

        await _leaderboard.RecalculateAllPoints(gameweekId);

        // Roll free transfers (+1 each, capped at 5)
        var teams = await _db.FantasyTeams.ToListAsync();
        foreach (var t in teams)
            t.FreeTransfers = Math.Min(t.FreeTransfers + 1, 5);

        // Copy picks forward to the next gameweek
        var nextGw = await _db.Gameweeks
            .Where(g => g.Number > gw.Number)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();

        if (nextGw != null)
        {
            var existingNext = await _db.FantasyPicks.AnyAsync(p => p.GameweekId == nextGw.Id);
            if (!existingNext)
            {
                var allPicks = await _db.FantasyPicks
                    .Where(p => p.GameweekId == gameweekId)
                    .ToListAsync();

                foreach (var pick in allPicks)
                {
                    _db.FantasyPicks.Add(new FantasyPick
                    {
                        FantasyTeamId = pick.FantasyTeamId,
                        PlayerId = pick.PlayerId,
                        GameweekId = nextGw.Id,
                        SquadPosition = pick.SquadPosition,
                        IsCaptain = pick.IsCaptain,
                        IsViceCaptain = pick.IsViceCaptain
                    });
                }
            }
        }

        await _db.SaveChangesAsync();

        return new SimulationResult(gw.Number, matches.Count, eventsCreated, teams.Count);
    }

    // ========== Helpers ==========

    private static int SampleScore(Random rng)
    {
        // Weighted goal distribution roughly matching Bundesliga averages
        double r = rng.NextDouble();
        if (r < 0.25) return 0;
        if (r < 0.55) return 1;
        if (r < 0.80) return 2;
        if (r < 0.92) return 3;
        if (r < 0.98) return 4;
        return 5;
    }

    /// <summary>
    /// Generate per-player events for one side of a match.
    /// goalsFor = goals the team scored; goalsAgainst = goals conceded.
    /// </summary>
    private int SimulateTeam(Match match, List<BundesligaPlayer> squad, int goalsFor, int goalsAgainst, Random rng)
    {
        int events = 0;
        // Track events created just for this side so bonus tally doesn't pull in the other team's events
        var sideEvents = new List<MatchEvent>();

        // Choose a starting XI: prefer fit players & a sensible 1-4-4-2-ish shape.
        var starters = PickStartingXI(squad, rng);
        var bench = squad.Except(starters).Take(7).ToList();

        // Minutes: most starters play 90, a few get subbed (45-75min), some bench players come on (15-45min)
        var minutesByPlayer = new Dictionary<int, int>();
        var playedIds = new HashSet<int>();

        foreach (var p in starters)
        {
            int min = 90;
            double r = rng.NextDouble();
            if (r < 0.15) min = rng.Next(45, 80); // subbed off
            else if (r < 0.20) min = rng.Next(1, 45); // early sub / injured
            // If fitness is low, more likely to not start full 90
            if (p.Fitness < 60 && rng.NextDouble() < 0.3) min = rng.Next(0, 60);
            if (min > 0)
            {
                sideEvents.Add(AddEvent(match.GameweekId, p.Id, EventType.MinutesPlayed, min, _scoring.CalculateMinutesPoints(min)));
                events++;
            }
            minutesByPlayer[p.Id] = min;
            if (min > 0) playedIds.Add(p.Id);
        }

        // Bring on 2-3 subs from bench
        int subsOn = rng.Next(1, 4);
        foreach (var p in bench.Take(subsOn))
        {
            int min = rng.Next(10, 46);
            sideEvents.Add(AddEvent(match.GameweekId, p.Id, EventType.MinutesPlayed, min, _scoring.CalculateMinutesPoints(min)));
            events++;
            minutesByPlayer[p.Id] = min;
            playedIds.Add(p.Id);
        }

        // --- Goals & assists ---
        // Prefer FWD > MID > DEF > GK for goals. Exclude players who didn't play.
        var scorers = starters.Concat(bench.Where(p => playedIds.Contains(p.Id))).ToList();

        for (int g = 0; g < goalsFor; g++)
        {
            var scorer = WeightedPick(scorers, p => p.Position switch
            {
                PlayerPosition.FWD => 4.0,
                PlayerPosition.MID => 2.0,
                PlayerPosition.DEF => 0.5,
                _ => 0.05
            }, rng);
            if (scorer == null) break;
            sideEvents.Add(AddEvent(match.GameweekId, scorer.Id, EventType.Goal, null,
                _scoring.CalculatePoints(EventType.Goal, scorer.Position)));
            events++;

            // 65% chance of an assist from a different player
            if (rng.NextDouble() < 0.65)
            {
                var assisters = scorers.Where(p => p.Id != scorer.Id).ToList();
                var assister = WeightedPick(assisters, p => p.Position switch
                {
                    PlayerPosition.MID => 3.0,
                    PlayerPosition.FWD => 2.0,
                    PlayerPosition.DEF => 1.0,
                    _ => 0.1
                }, rng);
                if (assister != null)
                {
                    sideEvents.Add(AddEvent(match.GameweekId, assister.Id, EventType.Assist, null, 3));
                    events++;
                }
            }
        }

        // --- Clean sheets ---
        if (goalsAgainst == 0)
        {
            foreach (var p in starters)
            {
                if (!minutesByPlayer.TryGetValue(p.Id, out var min) || min < 60) continue;
                int pts = _scoring.CalculatePoints(EventType.CleanSheet, p.Position);
                if (pts > 0)
                {
                    sideEvents.Add(AddEvent(match.GameweekId, p.Id, EventType.CleanSheet, null, pts));
                    events++;
                }
            }
        }

        // --- Cards ---
        // Expected ~2 yellows per team per match, ~0.1 reds
        int yellows = rng.Next(1, 4);
        for (int i = 0; i < yellows; i++)
        {
            var target = scorers[rng.Next(scorers.Count)];
            sideEvents.Add(AddEvent(match.GameweekId, target.Id, EventType.YellowCard, null, -1));
            events++;
        }
        if (rng.NextDouble() < 0.12)
        {
            var target = scorers[rng.Next(scorers.Count)];
            sideEvents.Add(AddEvent(match.GameweekId, target.Id, EventType.RedCard, null, -3));
            events++;
        }

        // --- Bonus (BPS-lite): top 3 performers by our own point tally get 3/2/1 ---
        var tally = new Dictionary<int, int>();
        foreach (var p in scorers) tally[p.Id] = 0;
        // Re-read freshly-added events for this side
        // We can just compute BPS heuristically: goals=3, assists=2, CS=2 (GK/DEF), -1 yellow
        foreach (var p in scorers)
        {
            int bps = 0;
            if (minutesByPlayer.TryGetValue(p.Id, out var m) && m >= 60) bps += 3;
            // We'd ideally count this match's events per player — simple proxy using pending queue
            tally[p.Id] = bps;
        }
        // Add BPS from this side's events only
        foreach (var ev in sideEvents)
        {
            if (!tally.ContainsKey(ev.PlayerId)) continue;
            tally[ev.PlayerId] += ev.EventType switch
            {
                EventType.Goal => 6,
                EventType.Assist => 3,
                EventType.CleanSheet => 2,
                EventType.YellowCard => -1,
                EventType.RedCard => -3,
                _ => 0
            };
        }

        var topThree = tally.OrderByDescending(kv => kv.Value).Take(3).ToList();
        int[] bonusValues = { 3, 2, 1 };
        for (int i = 0; i < topThree.Count && i < 3; i++)
        {
            if (topThree[i].Value <= 0) break;
            AddEvent(match.GameweekId, topThree[i].Key, EventType.Bonus, bonusValues[i], bonusValues[i]);
            events++;
        }

        return events;
    }

    private List<BundesligaPlayer> PickStartingXI(List<BundesligaPlayer> squad, Random rng)
    {
        // 1 GK + 4 DEF + 4 MID + 2 FWD. Fall back gracefully if squad is thin.
        List<BundesligaPlayer> Select(PlayerPosition pos, int n) =>
            squad.Where(p => p.Position == pos)
                 .OrderByDescending(p => p.Fitness + rng.NextDouble() * 30) // fitness + jitter
                 .Take(n)
                 .ToList();

        var xi = new List<BundesligaPlayer>();
        xi.AddRange(Select(PlayerPosition.GK, 1));
        xi.AddRange(Select(PlayerPosition.DEF, 4));
        xi.AddRange(Select(PlayerPosition.MID, 4));
        xi.AddRange(Select(PlayerPosition.FWD, 2));
        return xi;
    }

    private static T? WeightedPick<T>(List<T> items, Func<T, double> weight, Random rng) where T : class
    {
        if (items.Count == 0) return null;
        double total = items.Sum(weight);
        if (total <= 0) return items[rng.Next(items.Count)];
        double r = rng.NextDouble() * total;
        double acc = 0;
        foreach (var item in items)
        {
            acc += weight(item);
            if (r <= acc) return item;
        }
        return items[items.Count - 1];
    }

    private MatchEvent AddEvent(int gwId, int playerId, EventType type, int? minute, int points)
    {
        var ev = new MatchEvent
        {
            GameweekId = gwId,
            PlayerId = playerId,
            EventType = type,
            Minute = minute,
            Points = points
        };
        _db.MatchEvents.Add(ev);
        return ev;
    }
}
