using System.Security.Claims;
using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Models;
using FBL.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeagueController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LeaderboardService _leaderboardService;

    public LeagueController(AppDbContext db, LeaderboardService leaderboardService)
    {
        _db = db;
        _leaderboardService = leaderboardService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("global")]
    public async Task<ActionResult<List<LeagueStandingDto>>> GetGlobalLeaderboard([FromQuery] int page = 1)
    {
        return await _leaderboardService.GetGlobalLeaderboard(page);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<LeagueDto>>> GetMyLeagues()
    {
        var leagues = await _db.LeagueMembers
            .Where(lm => lm.UserId == UserId)
            .Include(lm => lm.League)
            .Select(lm => new LeagueDto(
                lm.League.Id,
                lm.League.Name,
                lm.League.JoinCode,
                lm.League.IsGlobal,
                new List<LeagueStandingDto>()
            ))
            .ToListAsync();

        return leagues;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeagueDto>> GetLeague(int id)
    {
        var league = await _db.Leagues.FindAsync(id);
        if (league == null) return NotFound();

        var isMember = await _db.LeagueMembers
            .AnyAsync(lm => lm.LeagueId == id && lm.UserId == UserId);
        if (!isMember) return Forbid();

        var standings = await _leaderboardService.GetLeagueStandings(id);

        return new LeagueDto(league.Id, league.Name, league.JoinCode, league.IsGlobal, standings);
    }

    [HttpPost]
    public async Task<ActionResult<LeagueDto>> CreateLeague(CreateLeagueDto dto)
    {
        var joinCode = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var league = new League
        {
            Name = dto.Name,
            JoinCode = joinCode,
            CreatedByUserId = UserId
        };

        _db.Leagues.Add(league);
        await _db.SaveChangesAsync();

        // Auto-join creator
        _db.LeagueMembers.Add(new LeagueMember
        {
            LeagueId = league.Id,
            UserId = UserId
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLeague), new { id = league.Id },
            new LeagueDto(league.Id, league.Name, league.JoinCode, false, new List<LeagueStandingDto>()));
    }

    [HttpPost("join")]
    public async Task<ActionResult> JoinLeague(JoinLeagueDto dto)
    {
        var league = await _db.Leagues.FirstOrDefaultAsync(l => l.JoinCode == dto.JoinCode);
        if (league == null) return NotFound("Invalid join code.");

        var alreadyMember = await _db.LeagueMembers
            .AnyAsync(lm => lm.LeagueId == league.Id && lm.UserId == UserId);
        if (alreadyMember) return BadRequest("Already a member.");

        _db.LeagueMembers.Add(new LeagueMember
        {
            LeagueId = league.Id,
            UserId = UserId
        });
        await _db.SaveChangesAsync();

        return Ok(new { league.Id, league.Name });
    }
}
