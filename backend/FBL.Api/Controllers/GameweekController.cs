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
public class GameweekController : ControllerBase
{
    private readonly AppDbContext _db;

    public GameweekController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<GameweekDto>>> GetAll()
    {
        var gameweeks = await _db.Gameweeks
            .OrderBy(g => g.Number)
            .Select(g => new GameweekDto(
                g.Id, g.Number, g.KickoffTime,
                g.KickoffTime.AddMinutes(-90),
                g.Status, DateTime.UtcNow >= g.KickoffTime.AddMinutes(-90)
            ))
            .ToListAsync();

        return gameweeks;
    }

    [HttpGet("current")]
    public async Task<ActionResult<GameweekDto>> GetCurrent()
    {
        var gw = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming || g.Status == GameweekStatus.Live)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();

        if (gw == null) return NotFound("No active gameweek.");

        return new GameweekDto(gw.Id, gw.Number, gw.KickoffTime,
            gw.Deadline, gw.Status, gw.IsLocked);
    }

    [HttpGet("{id}/scores")]
    public async Task<ActionResult<object>> GetGameweekScores(int id)
    {
        var gw = await _db.Gameweeks.FindAsync(id);
        if (gw == null) return NotFound();

        var events = await _db.MatchEvents
            .Where(e => e.GameweekId == id)
            .Include(e => e.Player)
            .GroupBy(e => e.Player)
            .Select(g => new
            {
                PlayerId = g.Key.Id,
                PlayerName = g.Key.Name,
                Team = g.Key.Team,
                TotalPoints = g.Sum(e => e.Points),
                Events = g.Select(e => new { e.EventType, e.Minute, e.Points }).ToList()
            })
            .OrderByDescending(p => p.TotalPoints)
            .ToListAsync();

        return Ok(new { Gameweek = gw.Number, Scores = events });
    }
}
