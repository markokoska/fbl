using System.Text.Json;
using System.Text.Json.Serialization;
using FBL.Api.Data;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

public class OpenLigaImportService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly ScoringService _scoring;
    private readonly ILogger<OpenLigaImportService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OpenLigaImportService(AppDbContext db, IHttpClientFactory httpFactory,
        ScoringService scoring, ILogger<OpenLigaImportService> logger)
    {
        _db = db;
        _http = httpFactory.CreateClient();
        _scoring = scoring;
        _logger = logger;
    }

    /// <summary>
    /// Import all Bundesliga teams and players from completed matchdays.
    /// Players are discovered from goal scorer data across all matchdays.
    /// Also creates squad rosters from football-data.org if API key is available.
    /// </summary>
    public async Task<ImportResult> ImportAll(int season = 2025)
    {
        var result = new ImportResult();

        // Step 1: Get all teams
        var teamsJson = await _http.GetStringAsync(
            $"https://api.openligadb.de/getavailableteams/bl1/{season}");
        var teams = JsonSerializer.Deserialize<List<OlTeam>>(teamsJson, JsonOpts) ?? [];

        _logger.LogInformation("Found {Count} teams", teams.Count);

        // Step 2: Import all finished matchdays
        for (int matchday = 1; matchday <= 34; matchday++)
        {
            var matchesJson = await _http.GetStringAsync(
                $"https://api.openligadb.de/getmatchdata/bl1/{season}/{matchday}");
            var matches = JsonSerializer.Deserialize<List<OlMatch>>(matchesJson, JsonOpts) ?? [];

            var finishedMatches = matches.Where(m => m.MatchIsFinished).ToList();
            if (finishedMatches.Count == 0)
            {
                _logger.LogInformation("Matchday {Md}: no finished matches, stopping", matchday);
                result.LastCompletedMatchday = matchday - 1;
                break;
            }

            // Ensure gameweek exists
            var gw = await _db.Gameweeks.FirstOrDefaultAsync(g => g.Number == matchday);
            if (gw == null)
            {
                var firstKickoff = finishedMatches
                    .Select(m => m.MatchDateTimeUTC)
                    .Where(d => d != default)
                    .OrderBy(d => d)
                    .FirstOrDefault();

                gw = new Gameweek
                {
                    Number = matchday,
                    KickoffTime = firstKickoff != default ? firstKickoff : DateTime.UtcNow,
                    Status = GameweekStatus.Finished
                };
                _db.Gameweeks.Add(gw);
                await _db.SaveChangesAsync();
            }
            else if (gw.Status != GameweekStatus.Finished && finishedMatches.Count == matches.Count)
            {
                gw.Status = GameweekStatus.Finished;

                // Update kickoff time from actual data
                var firstKickoff = finishedMatches
                    .Select(m => m.MatchDateTimeUTC)
                    .Where(d => d != default)
                    .OrderBy(d => d)
                    .FirstOrDefault();
                if (firstKickoff != default)
                    gw.KickoffTime = firstKickoff;
            }

            foreach (var match in finishedMatches)
            {
                var team1Name = GetShortName(match.Team1, teams);
                var team2Name = GetShortName(match.Team2, teams);

                var finalResult = match.MatchResults?
                    .FirstOrDefault(r => r.ResultTypeID == 2); // 2 = final result
                var team1Score = finalResult?.PointsTeam1 ?? 0;
                var team2Score = finalResult?.PointsTeam2 ?? 0;

                bool team1CleanSheet = team2Score == 0;
                bool team2CleanSheet = team1Score == 0;

                // Process goals
                foreach (var goal in match.Goals ?? [])
                {
                    if (string.IsNullOrEmpty(goal.GoalGetterName)) continue;

                    // Determine which team the scorer plays for
                    var scorerTeam = DetermineGoalScorerTeam(goal, match, team1Name, team2Name);

                    var player = await EnsurePlayer(goal.GoalGetterName, scorerTeam,
                        goal.GoalGetterID);

                    // Check if event already exists for this player/gameweek/goal
                    var existingEvent = await _db.MatchEvents
                        .AnyAsync(e => e.GameweekId == gw.Id && e.PlayerId == player.Id
                            && e.Minute == goal.MatchMinute
                            && (e.EventType == EventType.Goal || e.EventType == EventType.OwnGoal));

                    if (existingEvent) continue;

                    if (goal.IsOwnGoal)
                    {
                        var pts = _scoring.CalculatePoints(EventType.OwnGoal, player.Position);
                        _db.MatchEvents.Add(new MatchEvent
                        {
                            GameweekId = gw.Id,
                            PlayerId = player.Id,
                            EventType = EventType.OwnGoal,
                            Minute = goal.MatchMinute,
                            Points = pts
                        });
                        player.TotalPoints += pts;
                        player.OwnGoals++;
                    }
                    else
                    {
                        var pts = _scoring.CalculatePoints(EventType.Goal, player.Position);
                        _db.MatchEvents.Add(new MatchEvent
                        {
                            GameweekId = gw.Id,
                            PlayerId = player.Id,
                            EventType = EventType.Goal,
                            Minute = goal.MatchMinute,
                            Points = pts
                        });
                        player.TotalPoints += pts;
                        player.Goals++;

                        result.GoalsImported++;
                    }
                }

                // Add minutes played events for all known players on these teams
                // (assumes all squad players got 90 min - admin can adjust)
                await AddMinutesPlayed(gw.Id, team1Name);
                await AddMinutesPlayed(gw.Id, team2Name);

                // Add clean sheet events
                if (team1CleanSheet)
                    await AddCleanSheets(gw.Id, team1Name);
                if (team2CleanSheet)
                    await AddCleanSheets(gw.Id, team2Name);
            }

            await _db.SaveChangesAsync();
            result.MatchdaysProcessed++;
            _logger.LogInformation("Matchday {Md}: processed {Count} matches",
                matchday, finishedMatches.Count);

            // Small delay to be polite to the API
            await Task.Delay(500);
        }

        // Create upcoming gameweeks for unplayed matchdays
        for (int matchday = result.LastCompletedMatchday + 1; matchday <= 34; matchday++)
        {
            var exists = await _db.Gameweeks.AnyAsync(g => g.Number == matchday);
            if (!exists)
            {
                // Try to get actual kickoff times
                try
                {
                    var matchesJson = await _http.GetStringAsync(
                        $"https://api.openligadb.de/getmatchdata/bl1/{season}/{matchday}");
                    var matches = JsonSerializer.Deserialize<List<OlMatch>>(matchesJson, JsonOpts) ?? [];

                    var firstKickoff = matches
                        .Select(m => m.MatchDateTimeUTC)
                        .Where(d => d != default)
                        .OrderBy(d => d)
                        .FirstOrDefault();

                    _db.Gameweeks.Add(new Gameweek
                    {
                        Number = matchday,
                        KickoffTime = firstKickoff != default ? firstKickoff : DateTime.UtcNow.AddDays((matchday - result.LastCompletedMatchday) * 7),
                        Status = GameweekStatus.Upcoming
                    });
                }
                catch
                {
                    _db.Gameweeks.Add(new Gameweek
                    {
                        Number = matchday,
                        KickoffTime = DateTime.UtcNow.AddDays((matchday - result.LastCompletedMatchday) * 7),
                        Status = GameweekStatus.Upcoming
                    });
                }

                await Task.Delay(300);
            }
        }

        await _db.SaveChangesAsync();

        // Recalculate all player stats from match events
        await RecalculatePlayerStats();

        // Set prices based on total points
        await SetPlayerPrices();

        result.PlayersInDb = await _db.BundesligaPlayers.CountAsync();
        return result;
    }

    private string DetermineGoalScorerTeam(OlGoal goal, OlMatch match, string team1, string team2)
    {
        // OpenLigaDB goals have running score - if team1 score increased, scorer is from team1
        // Compare with previous goal to determine which team scored
        var goals = (match.Goals ?? []).OrderBy(g => g.GoalID).ToList();
        var idx = goals.IndexOf(goal);

        if (idx == 0)
        {
            return goal.ScoreTeam1 > 0 ? team1 : team2;
        }

        var prev = goals[idx - 1];
        if (goal.IsOwnGoal)
        {
            // Own goal: if team1 score went up, the own goal was by a team2 player
            return goal.ScoreTeam1 > prev.ScoreTeam1 ? team2 : team1;
        }

        return goal.ScoreTeam1 > prev.ScoreTeam1 ? team1 : team2;
    }

    private async Task<BundesligaPlayer> EnsurePlayer(string name, string team, int externalId)
    {
        var player = await _db.BundesligaPlayers
            .FirstOrDefaultAsync(p => p.ExternalId == externalId && externalId != 0);

        if (player == null)
        {
            player = await _db.BundesligaPlayers
                .FirstOrDefaultAsync(p => p.Name == name && p.Team == team);
        }

        if (player == null)
        {
            player = new BundesligaPlayer
            {
                Name = name,
                Team = team,
                ExternalId = externalId,
                Position = PlayerPosition.FWD, // Default, will be refined
                Price = 6.0m
            };
            _db.BundesligaPlayers.Add(player);
            await _db.SaveChangesAsync();
        }
        else if (player.Team != team)
        {
            player.Team = team; // Player transferred
        }

        return player;
    }

    private async Task AddMinutesPlayed(int gameweekId, string team)
    {
        var players = await _db.BundesligaPlayers
            .Where(p => p.Team == team)
            .ToListAsync();

        foreach (var player in players)
        {
            var alreadyHas = await _db.MatchEvents
                .AnyAsync(e => e.GameweekId == gameweekId && e.PlayerId == player.Id
                    && e.EventType == EventType.MinutesPlayed);

            if (!alreadyHas)
            {
                int pts = _scoring.CalculateMinutesPoints(90);
                _db.MatchEvents.Add(new MatchEvent
                {
                    GameweekId = gameweekId,
                    PlayerId = player.Id,
                    EventType = EventType.MinutesPlayed,
                    Minute = 90,
                    Points = pts
                });
                player.TotalPoints += pts;
                player.MatchesPlayed++;
            }
        }
    }

    private async Task AddCleanSheets(int gameweekId, string team)
    {
        var players = await _db.BundesligaPlayers
            .Where(p => p.Team == team && (p.Position == PlayerPosition.GK || p.Position == PlayerPosition.DEF))
            .ToListAsync();

        foreach (var player in players)
        {
            var alreadyHas = await _db.MatchEvents
                .AnyAsync(e => e.GameweekId == gameweekId && e.PlayerId == player.Id
                    && e.EventType == EventType.CleanSheet);

            if (!alreadyHas)
            {
                int pts = _scoring.CalculatePoints(EventType.CleanSheet, player.Position);
                _db.MatchEvents.Add(new MatchEvent
                {
                    GameweekId = gameweekId,
                    PlayerId = player.Id,
                    EventType = EventType.CleanSheet,
                    Minute = null,
                    Points = pts
                });
                player.TotalPoints += pts;
                player.CleanSheets++;
            }
        }
    }

    /// <summary>
    /// Rebuild all aggregate stats on BundesligaPlayer from MatchEvents.
    /// This ensures stats are always consistent even after manual edits.
    /// </summary>
    private async Task RecalculatePlayerStats()
    {
        var players = await _db.BundesligaPlayers
            .Include(p => p.MatchEvents)
            .ToListAsync();

        foreach (var player in players)
        {
            player.Goals = player.MatchEvents.Count(e => e.EventType == EventType.Goal);
            player.Assists = player.MatchEvents.Count(e => e.EventType == EventType.Assist);
            player.CleanSheets = player.MatchEvents.Count(e => e.EventType == EventType.CleanSheet);
            player.YellowCards = player.MatchEvents.Count(e => e.EventType == EventType.YellowCard);
            player.RedCards = player.MatchEvents.Count(e => e.EventType == EventType.RedCard);
            player.PenaltiesMissed = player.MatchEvents.Count(e => e.EventType == EventType.PenaltyMiss);
            player.PenaltySaves = player.MatchEvents.Count(e => e.EventType == EventType.PenaltySave);
            player.OwnGoals = player.MatchEvents.Count(e => e.EventType == EventType.OwnGoal);
            player.BonusPoints = player.MatchEvents
                .Where(e => e.EventType == EventType.Bonus)
                .Sum(e => e.Points);
            player.MatchesPlayed = player.MatchEvents.Count(e => e.EventType == EventType.MinutesPlayed);
            player.TotalPoints = player.MatchEvents.Sum(e => e.Points);
        }

        await _db.SaveChangesAsync();
    }

    private async Task SetPlayerPrices()
    {
        var players = await _db.BundesligaPlayers.ToListAsync();
        if (players.Count == 0) return;

        var maxPoints = players.Max(p => p.TotalPoints);
        if (maxPoints == 0) maxPoints = 1;

        foreach (var player in players)
        {
            // Price range: 4.0 - 13.0 based on points relative to max
            var ratio = (double)player.TotalPoints / maxPoints;
            var basePrice = player.Position switch
            {
                PlayerPosition.GK => 4.5m,
                PlayerPosition.DEF => 4.0m,
                PlayerPosition.MID => 5.0m,
                PlayerPosition.FWD => 5.5m,
                _ => 5.0m
            };
            player.Price = Math.Round(basePrice + (decimal)ratio * 8.0m, 1);
        }

        await _db.SaveChangesAsync();
    }

    private static string GetShortName(OlTeamRef? teamRef, List<OlTeam> allTeams)
    {
        if (teamRef == null) return "Unknown";
        var match = allTeams.FirstOrDefault(t => t.TeamId == teamRef.TeamId);
        return match?.ShortName ?? teamRef.TeamName ?? "Unknown";
    }

    // OpenLigaDB response models
    public class OlTeam
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = "";
        public string? ShortName { get; set; }
        public string? TeamIconUrl { get; set; }
    }

    public class OlTeamRef
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? ShortName { get; set; }
    }

    public class OlMatch
    {
        public int MatchID { get; set; }
        public DateTime MatchDateTimeUTC { get; set; }
        public bool MatchIsFinished { get; set; }
        public OlTeamRef? Team1 { get; set; }
        public OlTeamRef? Team2 { get; set; }
        public List<OlMatchResult>? MatchResults { get; set; }
        public List<OlGoal>? Goals { get; set; }
        public OlGroup? Group { get; set; }
    }

    public class OlGroup
    {
        public string? GroupName { get; set; }
        public int GroupOrderID { get; set; }
    }

    public class OlMatchResult
    {
        public int ResultTypeID { get; set; } // 1=halftime, 2=final
        public int PointsTeam1 { get; set; }
        public int PointsTeam2 { get; set; }
    }

    public class OlGoal
    {
        public int GoalID { get; set; }
        public int ScoreTeam1 { get; set; }
        public int ScoreTeam2 { get; set; }
        public int? MatchMinute { get; set; }
        public int GoalGetterID { get; set; }
        public string GoalGetterName { get; set; } = "";
        public bool IsPenalty { get; set; }
        public bool IsOwnGoal { get; set; }
        public bool IsOvertime { get; set; }
    }

    public class ImportResult
    {
        public int MatchdaysProcessed { get; set; }
        public int GoalsImported { get; set; }
        public int PlayersInDb { get; set; }
        public int LastCompletedMatchday { get; set; }
    }
}
