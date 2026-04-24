using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlayersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> GetAll(
        [FromQuery] PlayerPosition? position,
        [FromQuery] string? team,
        [FromQuery] string? search,
        [FromQuery] string sortBy = "totalPoints",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.BundesligaPlayers.AsQueryable();

        if (position.HasValue)
            query = query.Where(p => p.Position == position.Value);
        if (!string.IsNullOrEmpty(team))
            query = query.Where(p => p.Team == team);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search));

        query = sortBy.ToLower() switch
        {
            "price" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "goals" => query.OrderByDescending(p => p.Goals),
            "assists" => query.OrderByDescending(p => p.Assists),
            "cleansheets" => query.OrderByDescending(p => p.CleanSheets),
            "bonuspoints" => query.OrderByDescending(p => p.BonusPoints),
            "xg" => query.OrderByDescending(p => p.ExpectedGoals),
            "xa" => query.OrderByDescending(p => p.ExpectedAssists),
            _ => query.OrderByDescending(p => p.TotalPoints)
        };

        var players = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlayerDto(
                p.Id, p.Name, p.Team, p.Position, p.Price, p.TotalPoints, p.PhotoUrl, p.Fitness,
                new PlayerStatsDto(
                    p.MatchesPlayed, p.Goals, p.Assists, p.CleanSheets,
                    p.YellowCards, p.RedCards, p.PenaltiesMissed,
                    p.PenaltySaves, p.OwnGoals, p.BonusPoints,
                    p.ExpectedGoals, p.ExpectedAssists
                )
            ))
            .ToListAsync();

        return players;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDetailDto>> GetById(int id)
    {
        var player = await _db.BundesligaPlayers
            .Include(p => p.MatchEvents)
            .ThenInclude(e => e.Gameweek)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null) return NotFound();

        var totalTeams = await _db.FantasyTeams.CountAsync();
        var ownedBy = await _db.FantasyPicks
            .Where(fp => fp.PlayerId == id)
            .Select(fp => fp.FantasyTeamId)
            .Distinct()
            .CountAsync();

        var ownershipPct = totalTeams > 0 ? (double)ownedBy / totalTeams * 100 : 0;

        var gwHistory = player.MatchEvents
            .GroupBy(e => e.Gameweek)
            .OrderBy(g => g.Key.Number)
            .Select(g => new GameweekPointsDto(
                g.Key.Number,
                g.Sum(e => e.Points),
                new GameweekStatsDto(
                    g.Where(e => e.EventType == EventType.MinutesPlayed).Select(e => e.Minute ?? 0).FirstOrDefault(),
                    g.Count(e => e.EventType == EventType.Goal),
                    g.Count(e => e.EventType == EventType.Assist),
                    g.Count(e => e.EventType == EventType.CleanSheet),
                    g.Count(e => e.EventType == EventType.YellowCard),
                    g.Count(e => e.EventType == EventType.RedCard),
                    g.Count(e => e.EventType == EventType.PenaltyMiss),
                    g.Count(e => e.EventType == EventType.PenaltySave),
                    g.Count(e => e.EventType == EventType.OwnGoal),
                    g.Where(e => e.EventType == EventType.Bonus).Sum(e => e.Minute ?? 0) // bonus value stored in Minute
                ),
                g.Select(e => e.EventType.ToString()).ToList()
            ))
            .ToList();

        var stats = new PlayerStatsDto(
            player.MatchesPlayed, player.Goals, player.Assists, player.CleanSheets,
            player.YellowCards, player.RedCards, player.PenaltiesMissed,
            player.PenaltySaves, player.OwnGoals, player.BonusPoints,
            player.ExpectedGoals, player.ExpectedAssists
        );

        return new PlayerDetailDto(
            player.Id, player.Name, player.Team, player.Position,
            player.Price, player.TotalPoints, player.PhotoUrl, player.Fitness,
            stats, gwHistory, Math.Round(ownershipPct, 1)
        );
    }

    [HttpGet("teams")]
    public async Task<ActionResult<List<string>>> GetTeams()
    {
        var teams = await _db.BundesligaPlayers
            .Select(p => p.Team)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        return teams;
    }
}
