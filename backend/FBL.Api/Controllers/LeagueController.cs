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
        var memberships = await _db.LeagueMembers
            .Where(lm => lm.UserId == UserId)
            .Include(lm => lm.League)
            .ToListAsync();

        var leagueIds = memberships.Select(m => m.League.Id).ToList();

        // Pre-load member counts and "do I have a team in this league?" lookups
        var memberCounts = await _db.LeagueMembers
            .Where(lm => leagueIds.Contains(lm.LeagueId))
            .GroupBy(lm => lm.LeagueId)
            .Select(g => new { LeagueId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LeagueId, x => x.Count);

        var myLeagueTeamIds = await _db.FantasyTeams
            .Where(t => t.UserId == UserId && t.LeagueId != null && leagueIds.Contains(t.LeagueId!.Value))
            .Select(t => t.LeagueId!.Value)
            .ToListAsync();
        var myLeagueTeamSet = myLeagueTeamIds.ToHashSet();

        return memberships.Select(m => new LeagueDto(
            m.League.Id,
            m.League.Name,
            m.League.JoinCode,
            m.League.IsGlobal,
            m.League.Type,
            memberCounts.GetValueOrDefault(m.League.Id, 0),
            m.League.MaxMembers,
            m.League.DraftStatus,
            myLeagueTeamSet.Contains(m.League.Id),
            new List<LeagueStandingDto>()
        )).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeagueDto>> GetLeague(int id)
    {
        var league = await _db.Leagues.FindAsync(id);
        if (league == null) return NotFound();

        var isMember = await _db.LeagueMembers
            .AnyAsync(lm => lm.LeagueId == id && lm.UserId == UserId);
        if (!isMember) return Forbid();

        var memberCount = await _db.LeagueMembers.CountAsync(lm => lm.LeagueId == id);
        var hasMyTeam = await _db.FantasyTeams.AnyAsync(t => t.UserId == UserId && t.LeagueId == id);
        var standings = await _leaderboardService.GetLeagueStandings(id);

        return new LeagueDto(
            league.Id, league.Name, league.JoinCode, league.IsGlobal,
            league.Type, memberCount, league.MaxMembers, league.DraftStatus,
            hasMyTeam, standings
        );
    }

    [HttpPost]
    public async Task<ActionResult<LeagueDto>> CreateLeague(CreateLeagueDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("League name is required.");

        if (dto.Type == LeagueType.Draft && (dto.MaxMembers < 2 || dto.MaxMembers > 20))
            return BadRequest("Draft leagues need between 2 and 20 members.");

        var joinCode = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var league = new League
        {
            Name = dto.Name,
            JoinCode = joinCode,
            CreatedByUserId = UserId,
            Type = dto.Type,
            MaxMembers = dto.Type == LeagueType.Draft ? dto.MaxMembers : 0,
            DraftStatus = dto.Type == LeagueType.Draft ? DraftStatus.Pending : DraftStatus.Completed,
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
            new LeagueDto(
                league.Id, league.Name, league.JoinCode, false,
                league.Type, 1, league.MaxMembers, league.DraftStatus,
                false, new List<LeagueStandingDto>()
            ));
    }

    [HttpPost("join")]
    public async Task<ActionResult> JoinLeague(JoinLeagueDto dto)
    {
        var league = await _db.Leagues.FirstOrDefaultAsync(l => l.JoinCode == dto.JoinCode);
        if (league == null) return NotFound("Invalid join code.");

        var alreadyMember = await _db.LeagueMembers
            .AnyAsync(lm => lm.LeagueId == league.Id && lm.UserId == UserId);
        if (alreadyMember) return BadRequest("Already a member.");

        // For Draft leagues, can only join while still pending and below cap
        if (league.Type == LeagueType.Draft)
        {
            if (league.DraftStatus != DraftStatus.Pending)
                return BadRequest("Draft has already started — you can't join now.");

            var memberCount = await _db.LeagueMembers.CountAsync(lm => lm.LeagueId == league.Id);
            if (memberCount >= league.MaxMembers)
                return BadRequest("League is full.");
        }

        _db.LeagueMembers.Add(new LeagueMember
        {
            LeagueId = league.Id,
            UserId = UserId
        });
        await _db.SaveChangesAsync();

        return Ok(new { league.Id, league.Name, league.Type });
    }
}
