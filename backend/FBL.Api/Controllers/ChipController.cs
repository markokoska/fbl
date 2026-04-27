using System.Security.Claims;
using FBL.Api.Data;
using FBL.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChipController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChipController(AppDbContext db)
    {
        _db = db;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<FantasyTeam?> ResolveTeam(int? leagueId, bool includeChips = false)
    {
        IQueryable<FantasyTeam> q = _db.FantasyTeams;
        if (includeChips) q = q.Include(t => t.ChipUsages);
        if (leagueId == null)
            return await q.FirstOrDefaultAsync(t => t.UserId == UserId && t.LeagueId == null);
        return await q.FirstOrDefaultAsync(t => t.UserId == UserId && t.LeagueId == leagueId.Value);
    }

    [HttpGet("available")]
    public async Task<ActionResult<object>> GetAvailableChips([FromQuery] int? leagueId = null)
    {
        var team = await ResolveTeam(leagueId, includeChips: true);
        if (team == null) return NotFound();

        var usedChips = team.ChipUsages.Select(c => c.ChipType).ToList();

        var wildcardCount = usedChips.Count(c => c == ChipType.Wildcard);
        var available = new
        {
            Wildcard = wildcardCount < 2,
            BenchBoost = !usedChips.Contains(ChipType.BenchBoost),
            TripleCaptain = !usedChips.Contains(ChipType.TripleCaptain),
            FreeHit = !usedChips.Contains(ChipType.FreeHit)
        };

        return Ok(available);
    }

    [HttpPost("activate")]
    public async Task<ActionResult> ActivateChip([FromBody] ChipType chipType, [FromQuery] int? leagueId = null)
    {
        var currentGw = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();

        if (currentGw == null) return BadRequest("No upcoming gameweek.");
        if (currentGw.IsLocked) return BadRequest("Gameweek is locked. Cannot activate chip.");

        var team = await ResolveTeam(leagueId, includeChips: true);
        if (team == null) return NotFound();

        if (team.ChipUsages.Any(c => c.GameweekId == currentGw.Id))
            return BadRequest("You've already activated a chip this gameweek.");

        var usedChips = team.ChipUsages.Select(c => c.ChipType).ToList();
        if (chipType == ChipType.Wildcard && usedChips.Count(c => c == ChipType.Wildcard) >= 2)
            return BadRequest("Both wildcards used.");
        if (chipType != ChipType.Wildcard && usedChips.Contains(chipType))
            return BadRequest($"{chipType} already used this season.");

        _db.ChipUsages.Add(new ChipUsage
        {
            FantasyTeamId = team.Id,
            GameweekId = currentGw.Id,
            ChipType = chipType
        });

        await _db.SaveChangesAsync();
        return Ok(new { Message = $"{chipType} activated for Gameweek {currentGw.Number}." });
    }

    [HttpDelete("deactivate")]
    public async Task<ActionResult> DeactivateChip([FromQuery] int? leagueId = null)
    {
        var currentGw = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();

        if (currentGw == null || currentGw.IsLocked)
            return BadRequest("Cannot deactivate chip.");

        var team = await ResolveTeam(leagueId);
        if (team == null) return NotFound();

        var chip = await _db.ChipUsages
            .FirstOrDefaultAsync(c => c.FantasyTeamId == team.Id && c.GameweekId == currentGw.Id);

        if (chip == null) return BadRequest("No active chip.");

        _db.ChipUsages.Remove(chip);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Chip deactivated." });
    }
}
