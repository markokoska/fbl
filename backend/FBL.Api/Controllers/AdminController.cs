using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Hubs;
using FBL.Api.Models;
using FBL.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScoringService _scoring;
    private readonly LeaderboardService _leaderboard;
    private readonly DataImportService _dataImport;
    private readonly OpenLigaImportService _openLigaImport;
    private readonly GameweekSimulationService _simulation;
    private readonly IHubContext<LiveScoreHub> _hub;

    public AdminController(
        AppDbContext db,
        ScoringService scoring,
        LeaderboardService leaderboard,
        DataImportService dataImport,
        OpenLigaImportService openLigaImport,
        GameweekSimulationService simulation,
        IHubContext<LiveScoreHub> hub)
    {
        _db = db;
        _scoring = scoring;
        _leaderboard = leaderboard;
        _dataImport = dataImport;
        _openLigaImport = openLigaImport;
        _simulation = simulation;
        _hub = hub;
    }

    [HttpPost("import-players")]
    public async Task<ActionResult> ImportPlayers()
    {
        var count = await _dataImport.ImportTeamsAndPlayers();
        return Ok(new { Message = $"Imported {count} new players." });
    }

    [HttpPost("import-openliga")]
    public async Task<ActionResult> ImportFromOpenLiga()
    {
        var result = await _openLigaImport.ImportAll();
        return Ok(new
        {
            Message = $"Imported {result.MatchdaysProcessed} matchdays, {result.GoalsImported} goals. {result.PlayersInDb} players in database. Last completed: GW{result.LastCompletedMatchday}."
        });
    }

    [HttpPost("event")]
    public async Task<ActionResult> AddMatchEvent(AddMatchEventDto dto)
    {
        var player = await _db.BundesligaPlayers.FindAsync(dto.PlayerId);
        if (player == null) return NotFound("Player not found.");

        var gw = await _db.Gameweeks.FindAsync(dto.GameweekId);
        if (gw == null) return NotFound("Gameweek not found.");

        int points;
        if (dto.EventType == EventType.MinutesPlayed && dto.Minute.HasValue)
            points = _scoring.CalculateMinutesPoints(dto.Minute.Value);
        else if (dto.EventType == EventType.Bonus && dto.Minute.HasValue)
            points = dto.Minute.Value; // Minute field used for bonus value (1-3)
        else
            points = _scoring.CalculatePoints(dto.EventType, player.Position);

        var matchEvent = new MatchEvent
        {
            GameweekId = dto.GameweekId,
            PlayerId = dto.PlayerId,
            EventType = dto.EventType,
            Minute = dto.Minute,
            Points = points
        };

        _db.MatchEvents.Add(matchEvent);

        // Update player total points and stats
        player.TotalPoints += points;
        UpdatePlayerStats(player, dto.EventType, points);

        await _db.SaveChangesAsync();

        // Recalculate live GW points for all fantasy teams
        await _leaderboard.RecalculateLiveGameweekPoints(dto.GameweekId);

        // Notify live subscribers
        await _hub.Clients.Group($"gw-{dto.GameweekId}").SendAsync("MatchEvent", new
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            Team = player.Team,
            EventType = dto.EventType.ToString(),
            dto.Minute,
            Points = points
        });

        // Broadcast standings update so leagues/standings pages refresh
        await _hub.Clients.All.SendAsync("StandingsUpdate", new { GameweekId = dto.GameweekId });

        return Ok(new { Points = points });
    }

    [HttpPost("events/batch")]
    public async Task<ActionResult> AddBatchEvents(BatchMatchEventDto dto)
    {
        var gw = await _db.Gameweeks.FindAsync(dto.GameweekId);
        if (gw == null) return NotFound("Gameweek not found.");

        var playerIds = dto.Events.Select(e => e.PlayerId).Distinct().ToList();
        var players = await _db.BundesligaPlayers
            .Where(p => playerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var evt in dto.Events)
        {
            if (!players.TryGetValue(evt.PlayerId, out var player)) continue;

            int points;
            if (evt.EventType == EventType.MinutesPlayed && evt.Minute.HasValue)
                points = _scoring.CalculateMinutesPoints(evt.Minute.Value);
            else if (evt.EventType == EventType.Bonus && evt.Minute.HasValue)
                points = evt.Minute.Value;
            else
                points = _scoring.CalculatePoints(evt.EventType, player.Position);

            _db.MatchEvents.Add(new MatchEvent
            {
                GameweekId = dto.GameweekId,
                PlayerId = evt.PlayerId,
                EventType = evt.EventType,
                Minute = evt.Minute,
                Points = points
            });

            player.TotalPoints += points;
            UpdatePlayerStats(player, evt.EventType, points);
        }

        await _db.SaveChangesAsync();

        // Recalculate live GW points for all fantasy teams
        await _leaderboard.RecalculateLiveGameweekPoints(dto.GameweekId);

        await _hub.Clients.Group($"gw-{dto.GameweekId}").SendAsync("BatchUpdate", new
        {
            GameweekId = dto.GameweekId,
            EventCount = dto.Events.Count
        });

        // Broadcast standings update
        await _hub.Clients.All.SendAsync("StandingsUpdate", new { GameweekId = dto.GameweekId });

        return Ok(new { Message = $"Added {dto.Events.Count} events." });
    }

    [HttpPut("gameweek/{id}")]
    public async Task<ActionResult> UpdateGameweek(int id, UpdateGameweekDto dto)
    {
        var gw = await _db.Gameweeks.FindAsync(id);
        if (gw == null) return NotFound();

        if (dto.KickoffTime.HasValue)
            gw.KickoffTime = dto.KickoffTime.Value;
        if (dto.Status.HasValue)
        {
            gw.Status = dto.Status.Value;

            if (dto.Status.Value == GameweekStatus.Live)
            {
                // Reset running GW counter for every team
                await _leaderboard.ResetGameweekPoints();
            }
            else if (dto.Status.Value == GameweekStatus.Finished)
            {
                // Finalise: consolidate GameweekPoints → TotalPoints, reset to 0
                await _leaderboard.FinaliseGameweek(id);

                // Roll over free transfers for next GW
                var teams = await _db.FantasyTeams.ToListAsync();
                foreach (var team in teams)
                {
                    team.FreeTransfers = Math.Min(team.FreeTransfers + 1, 5);
                }

                // Copy picks to the next gameweek so teams carry forward
                var nextGw = await _db.Gameweeks
                    .Where(g => g.Number > gw.Number)
                    .OrderBy(g => g.Number)
                    .FirstOrDefaultAsync();

                if (nextGw != null)
                {
                    var allPicks = await _db.FantasyPicks
                        .Where(p => p.GameweekId == id)
                        .ToListAsync();

                    // Only copy if no picks already exist for the next GW
                    var existingNext = await _db.FantasyPicks
                        .AnyAsync(p => p.GameweekId == nextGw.Id);

                    if (!existingNext)
                    {
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
            }

            // Notify all clients about the GW status change
            await _hub.Clients.All.SendAsync("StandingsUpdate", new { GameweekId = id });
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("gameweeks/seed")]
    public async Task<ActionResult> SeedGameweeks()
    {
        if (await _db.Gameweeks.AnyAsync())
            return BadRequest("Gameweeks already exist.");

        // Seed 34 matchdays - admin will set actual kickoff times
        for (int i = 1; i <= 34; i++)
        {
            _db.Gameweeks.Add(new Gameweek
            {
                Number = i,
                KickoffTime = DateTime.UtcNow.AddDays(i * 7), // placeholder
                Status = i == 1 ? GameweekStatus.Upcoming : GameweekStatus.Upcoming
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { Message = "Seeded 34 gameweeks." });
    }

    [HttpPost("seed-admin")]
    [AllowAnonymous]
    public async Task<ActionResult> SeedAdminEndpoint([FromServices] Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager,
        [FromServices] Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager)
    {
        // Only works if no admin exists
        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if (admins.Any()) return BadRequest("Admin already exists.");

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("Player"))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Player"));

        var admin = new AppUser
        {
            Email = "admin@fbl.com",
            UserName = "admin@fbl.com",
            DisplayName = "Admin"
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (!result.Succeeded) return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(admin, "Admin");

        return Ok(new { Message = "Admin user created. Email: admin@fbl.com, Password: Admin123!" });
    }

    [HttpPost("players/bulk")]
    public async Task<ActionResult> BulkImportPlayers(List<BulkPlayerDto> players)
    {
        if (players == null || players.Count == 0)
            return BadRequest("No players provided.");

        var rng = new Random(42); // deterministic seed for reproducibility
        var created = 0;

        foreach (var dto in players)
        {
            // Skip if player already exists (same name + team)
            var exists = await _db.BundesligaPlayers
                .AnyAsync(p => p.Name == dto.Name && p.Team == dto.Team);
            if (exists) continue;

            var player = new BundesligaPlayer
            {
                Name = dto.Name,
                Team = dto.Team,
                Position = dto.Position,
                MatchesPlayed = dto.MatchesPlayed,
                Goals = dto.Goals,
                Assists = dto.Assists,
                YellowCards = dto.YellowCards,
                RedCards = dto.RedCards
            };

            // --- Auto-fill realistic stats ---
            AutoFillStats(player, rng);

            // --- Calculate total points using FPL scoring ---
            player.TotalPoints = CalculateTotalPoints(player);

            // --- Set price based on total points and position ---
            player.Price = CalculatePrice(player);

            _db.BundesligaPlayers.Add(player);
            created++;
        }

        await _db.SaveChangesAsync();

        return Ok(new { Message = $"Created {created} players (skipped {players.Count - created} duplicates)." });
    }

    private static void AutoFillStats(BundesligaPlayer p, Random rng)
    {
        // Clean sheets: GK/DEF get ~30-45% of matches, MID ~20%, FWD 0
        p.CleanSheets = p.Position switch
        {
            PlayerPosition.GK => (int)(p.MatchesPlayed * (0.30 + rng.NextDouble() * 0.15)),
            PlayerPosition.DEF => (int)(p.MatchesPlayed * (0.25 + rng.NextDouble() * 0.15)),
            PlayerPosition.MID => (int)(p.MatchesPlayed * (0.10 + rng.NextDouble() * 0.10)),
            _ => 0
        };

        // Own goals: rare, mostly DEF/MID
        p.OwnGoals = p.Position switch
        {
            PlayerPosition.DEF => rng.NextDouble() < 0.15 ? 1 : 0,
            PlayerPosition.MID => rng.NextDouble() < 0.08 ? 1 : 0,
            _ => 0
        };

        // Penalty saves: GK only, 0-3 per season
        p.PenaltySaves = p.Position == PlayerPosition.GK
            ? (rng.NextDouble() < 0.3 ? rng.Next(1, 4) : 0)
            : 0;

        // Penalty misses: rare, FWD/MID who score goals
        p.PenaltiesMissed = (p.Position is PlayerPosition.FWD or PlayerPosition.MID) && p.Goals >= 3
            ? (rng.NextDouble() < 0.2 ? 1 : 0)
            : 0;

        // Bonus points: top performers get more (1-3 per strong match)
        double performanceScore = p.Goals * 3.0 + p.Assists * 2.0 + p.CleanSheets * 1.5;
        int bonusMatches = (int)(p.MatchesPlayed * Math.Min(0.5, performanceScore / 30.0));
        p.BonusPoints = 0;
        for (int i = 0; i < bonusMatches; i++)
            p.BonusPoints += rng.Next(1, 4); // 1, 2, or 3 bonus per awarded match

        // xG: slightly above goals for underperformers, slightly below for overperformers
        double xgVariance = (rng.NextDouble() - 0.4) * 0.3; // slight positive bias
        p.ExpectedGoals = Math.Max(0, Math.Round(p.Goals + p.Goals * xgVariance, 1));

        // xA: similar to assists
        double xaVariance = (rng.NextDouble() - 0.4) * 0.3;
        p.ExpectedAssists = Math.Max(0, Math.Round(p.Assists + p.Assists * xaVariance, 1));
    }

    private int CalculateTotalPoints(BundesligaPlayer p)
    {
        int pts = 0;

        // Minutes played: 2 pts per match (assume 60+ min)
        pts += p.MatchesPlayed * 2;

        // Goals
        pts += p.Goals * _scoring.CalculatePoints(EventType.Goal, p.Position);

        // Assists
        pts += p.Assists * 3;

        // Clean sheets
        pts += p.CleanSheets * _scoring.CalculatePoints(EventType.CleanSheet, p.Position);

        // Yellow cards
        pts += p.YellowCards * -1;

        // Red cards
        pts += p.RedCards * -3;

        // Penalty saves
        pts += p.PenaltySaves * 5;

        // Penalty misses
        pts += p.PenaltiesMissed * -2;

        // Own goals
        pts += p.OwnGoals * -2;

        // Bonus
        pts += p.BonusPoints;

        return pts;
    }

    private static decimal CalculatePrice(BundesligaPlayer p)
    {
        // Base price by position
        decimal basePrice = p.Position switch
        {
            PlayerPosition.GK => 4.5m,
            PlayerPosition.DEF => 4.5m,
            PlayerPosition.MID => 5.0m,
            PlayerPosition.FWD => 5.5m,
            _ => 5.0m
        };

        // Add value based on total points (roughly 0.1M per 5 points)
        decimal pointsBonus = Math.Round(p.TotalPoints / 50.0m, 1) * 0.5m;

        // Goal bonus for attackers
        decimal goalBonus = p.Position switch
        {
            PlayerPosition.FWD => p.Goals * 0.3m,
            PlayerPosition.MID => p.Goals * 0.4m,
            PlayerPosition.DEF => p.Goals * 0.5m,
            PlayerPosition.GK => p.Goals * 0.5m,
            _ => 0m
        };

        decimal price = basePrice + pointsBonus + goalBonus;

        // Clamp between 4.0 and 13.0
        price = Math.Max(4.0m, Math.Min(13.0m, price));

        // Round to nearest 0.5
        return Math.Round(price * 2, MidpointRounding.AwayFromZero) / 2;

    }

    [HttpPut("player/{id}/price")]
    public async Task<ActionResult> UpdatePlayerPrice(int id, [FromBody] UpdatePlayerPriceDto dto)
    {
        var player = await _db.BundesligaPlayers.FindAsync(id);
        if (player == null) return NotFound("Player not found.");
        player.Price = dto.Price;
        await _db.SaveChangesAsync();
        return Ok(new { Message = $"{player.Name} price updated to {player.Price}M" });
    }

    [HttpPut("player/{id}/fitness")]
    public async Task<ActionResult> UpdatePlayerFitness(int id, [FromBody] UpdatePlayerFitnessDto dto)
    {
        var player = await _db.BundesligaPlayers.FindAsync(id);
        if (player == null) return NotFound("Player not found.");
        player.Fitness = dto.Fitness;
        await _db.SaveChangesAsync();
        return Ok(new { Message = $"{player.Name} fitness updated to {player.Fitness}" });
    }

    [HttpPost("simulate-gameweek/{id}")]
    public async Task<ActionResult> SimulateGameweek(int id, [FromQuery] int? seed = null)
    {
        try
        {
            var result = await _simulation.SimulateGameweek(id, seed);
            await _hub.Clients.Group($"gw-{id}").SendAsync("BatchUpdate", new
            {
                GameweekId = id,
                EventCount = result.EventsCreated
            });
            return Ok(new
            {
                Message = $"Simulated GW{result.GameweekNumber}: {result.MatchesSimulated} matches, {result.EventsCreated} events, {result.TeamsRescored} teams rescored."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("match/{id}")]
    public async Task<ActionResult> UpdateMatchResult(int id, [FromBody] UpdateMatchDto dto)
    {
        var match = await _db.Matches.FindAsync(id);
        if (match == null) return NotFound("Match not found.");
        match.HomeGoals = dto.HomeGoals;
        match.AwayGoals = dto.AwayGoals;
        match.IsFinished = dto.IsFinished;
        await _db.SaveChangesAsync();
        return Ok(new { Message = $"{match.HomeTeam} {dto.HomeGoals}-{dto.AwayGoals} {match.AwayTeam}" });
    }

    private static void UpdatePlayerStats(BundesligaPlayer player, EventType eventType, int points)
    {
        switch (eventType)
        {
            case EventType.Goal: player.Goals++; break;
            case EventType.Assist: player.Assists++; break;
            case EventType.CleanSheet: player.CleanSheets++; break;
            case EventType.YellowCard: player.YellowCards++; break;
            case EventType.RedCard: player.RedCards++; break;
            case EventType.PenaltyMiss: player.PenaltiesMissed++; break;
            case EventType.PenaltySave: player.PenaltySaves++; break;
            case EventType.OwnGoal: player.OwnGoals++; break;
            case EventType.Bonus: player.BonusPoints += points; break;
            case EventType.MinutesPlayed: player.MatchesPlayed++; break;
        }
    }
}
