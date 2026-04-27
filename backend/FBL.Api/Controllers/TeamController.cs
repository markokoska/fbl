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
public class TeamController : ControllerBase
{
    private readonly AppDbContext _db;

    public TeamController(AppDbContext db)
    {
        _db = db;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // --- Helpers --------------------------------------------------------

    /// <summary>
    /// Find a user's team in a given league context. leagueId == null means the global team.
    /// EF can't translate `t.LeagueId == leagueId` cleanly when leagueId is nullable, so we
    /// branch explicitly.
    /// </summary>
    private async Task<FantasyTeam?> ResolveTeam(string userId, int? leagueId, bool includeChips = false)
    {
        IQueryable<FantasyTeam> query = _db.FantasyTeams;
        if (includeChips) query = query.Include(t => t.ChipUsages);

        if (leagueId == null)
            return await query.FirstOrDefaultAsync(t => t.UserId == userId && t.LeagueId == null);
        else
            return await query.FirstOrDefaultAsync(t => t.UserId == userId && t.LeagueId == leagueId.Value);
    }

    // --- My teams -------------------------------------------------------

    /// <summary>List all teams owned by the current user (for the team-switcher UI).</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<MyTeamSummaryDto>>> GetMyTeams()
    {
        var teams = await _db.FantasyTeams
            .Where(t => t.UserId == UserId)
            .Include(t => t.League)
            .ToListAsync();

        return teams.Select(t => new MyTeamSummaryDto(
            t.Id,
            t.Name,
            t.LeagueId,
            t.League?.Name ?? "Global",
            t.League?.Type ?? Models.LeagueType.Global,
            t.TotalPoints,
            t.GameweekPoints
        )).ToList();
    }

    [HttpGet]
    public async Task<ActionResult<TeamDto>> GetMyTeam([FromQuery] int? leagueId = null)
    {
        var currentGw = await GetCurrentGameweek();

        var team = await ResolveTeam(UserId, leagueId, includeChips: true);
        if (team == null) return NotFound("You haven't created a team yet.");

        if (currentGw != null)
            await EnsurePicksForGameweek(team.Id, currentGw.Id);

        return await BuildCurrentTeamDto(team, currentGw);
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto dto, [FromQuery] int? leagueId = null)
    {
        // ---- Mode-specific validation ----
        if (leagueId != null)
        {
            var league = await _db.Leagues.FindAsync(leagueId.Value);
            if (league == null) return NotFound("League not found.");

            // Draft league teams are created automatically when the draft completes,
            // not by the user.
            if (league.Type == Models.LeagueType.Draft)
                return BadRequest("Draft league teams are auto-created by the draft. Join the draft instead.");

            var isMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == leagueId.Value && lm.UserId == UserId);
            if (!isMember) return BadRequest("You are not a member of this league.");
        }

        var existing = await ResolveTeam(UserId, leagueId);
        if (existing != null)
            return BadRequest(leagueId == null
                ? "You already have a global team."
                : "You already have a team for this league.");

        if (dto.Picks.Count != 15)
            return BadRequest("You must pick exactly 15 players.");

        var playerIds = dto.Picks.Select(p => p.PlayerId).ToList();
        var players = await _db.BundesligaPlayers
            .Where(p => playerIds.Contains(p.Id))
            .ToListAsync();

        if (players.Count != 15)
            return BadRequest("Some players were not found.");

        var totalCost = players.Sum(p => p.Price);
        if (totalCost > 100m)
            return BadRequest($"Total cost {totalCost}M exceeds budget of 100M.");

        var teamGroups = players.GroupBy(p => p.Team);
        if (teamGroups.Any(g => g.Count() > 3))
            return BadRequest("Maximum 3 players from the same team.");

        var positionCounts = players.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.Count());
        if (positionCounts.GetValueOrDefault(PlayerPosition.GK) != 2 ||
            positionCounts.GetValueOrDefault(PlayerPosition.DEF) != 5 ||
            positionCounts.GetValueOrDefault(PlayerPosition.MID) != 5 ||
            positionCounts.GetValueOrDefault(PlayerPosition.FWD) != 3)
            return BadRequest("Squad must have 2 GK, 5 DEF, 5 MID, 3 FWD.");

        if (dto.Picks.Count(p => p.IsCaptain) != 1 || dto.Picks.Count(p => p.IsViceCaptain) != 1)
            return BadRequest("You must have exactly one captain and one vice captain.");

        var currentGw = await GetCurrentGameweek();
        if (currentGw == null)
            return BadRequest("No active gameweek found.");

        var team = new FantasyTeam
        {
            UserId = UserId,
            LeagueId = leagueId,
            Name = dto.TeamName,
            Budget = 100m - totalCost,
            FreeTransfers = 1
        };

        _db.FantasyTeams.Add(team);
        await _db.SaveChangesAsync();

        foreach (var pick in dto.Picks)
        {
            _db.FantasyPicks.Add(new FantasyPick
            {
                FantasyTeamId = team.Id,
                PlayerId = pick.PlayerId,
                GameweekId = currentGw.Id,
                SquadPosition = pick.SquadPosition,
                IsCaptain = pick.IsCaptain,
                IsViceCaptain = pick.IsViceCaptain
            });
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyTeam), null);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<TeamDto>> GetUserTeam(string userId, [FromQuery] int? leagueId = null)
    {
        var currentGw = await GetCurrentGameweek();

        var team = await ResolveTeam(userId, leagueId, includeChips: true);
        if (team == null) return NotFound("Team not found.");

        if (currentGw != null)
            await EnsurePicksForGameweek(team.Id, currentGw.Id);

        return await BuildCurrentTeamDto(team, currentGw);
    }

    [HttpPut("picks")]
    public async Task<ActionResult> UpdatePicks(UpdatePicksDto dto, [FromQuery] int? leagueId = null)
    {
        var currentGw = await GetCurrentGameweek();
        if (currentGw == null) return BadRequest("No active gameweek.");
        if (currentGw.IsLocked) return BadRequest("Gameweek is locked. Changes must be made at least 1h30m before kickoff.");

        var team = await ResolveTeam(UserId, leagueId);
        if (team == null) return NotFound("Team not found.");

        await EnsurePicksForGameweek(team.Id, currentGw.Id);

        var picks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == team.Id && p.GameweekId == currentGw.Id)
            .ToListAsync();

        if (dto.Picks.Count != picks.Count)
            return BadRequest("Pick count mismatch.");

        foreach (var pick in picks)
        {
            var update = dto.Picks.FirstOrDefault(p => p.PlayerId == pick.PlayerId);
            if (update == null) return BadRequest($"Player {pick.PlayerId} not found in update.");

            pick.SquadPosition = update.SquadPosition;
            pick.IsCaptain = update.IsCaptain;
            pick.IsViceCaptain = update.IsViceCaptain;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("gameweek/{gwNumber}")]
    public async Task<ActionResult<TeamDto>> GetMyTeamAtGameweek(int gwNumber, [FromQuery] int? leagueId = null)
    {
        var team = await ResolveTeam(UserId, leagueId, includeChips: true);
        if (team == null) return NotFound();
        return await GetTeamAtGameweek(team, gwNumber);
    }

    [HttpGet("user/{userId}/gameweek/{gwNumber}")]
    public async Task<ActionResult<TeamDto>> GetUserTeamAtGameweek(string userId, int gwNumber, [FromQuery] int? leagueId = null)
    {
        var team = await ResolveTeam(userId, leagueId, includeChips: true);
        if (team == null) return NotFound();
        return await GetTeamAtGameweek(team, gwNumber);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<GameweekHistoryDto>>> GetMyHistory([FromQuery] int? leagueId = null)
    {
        var team = await ResolveTeam(UserId, leagueId);
        if (team == null) return NotFound();
        return await ComputeHistory(team.Id);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<List<GameweekHistoryDto>>> GetUserHistory(string userId, [FromQuery] int? leagueId = null)
    {
        var team = await ResolveTeam(userId, leagueId);
        if (team == null) return NotFound();
        return await ComputeHistory(team.Id);
    }

    // --- Private helpers ------------------------------------------------

    private async Task<TeamDto> BuildCurrentTeamDto(FantasyTeam team, Gameweek? currentGw)
    {
        var picks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == team.Id && p.GameweekId == (currentGw != null ? currentGw.Id : 0))
            .Include(p => p.Player)
            .OrderBy(p => p.SquadPosition)
            .ToListAsync();

        var activeChip = currentGw != null
            ? team.ChipUsages.FirstOrDefault(c => c.GameweekId == currentGw.Id)?.ChipType
            : null;

        var gwEvents = currentGw != null
            ? await _db.MatchEvents
                .Where(e => e.GameweekId == currentGw.Id)
                .GroupBy(e => e.PlayerId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Points))
            : new Dictionary<int, int>();

        var pickDtos = picks.Select(p => new PickDetailDto(
            p.PlayerId,
            p.Player.Name,
            p.Player.Team,
            p.Player.Position,
            p.Player.Price,
            p.SquadPosition,
            p.IsCaptain,
            p.IsViceCaptain,
            gwEvents.GetValueOrDefault(p.PlayerId, 0)
        )).ToList();

        int gwPoints = pickDtos
            .Where(p => p.SquadPosition <= 11 || activeChip == ChipType.BenchBoost)
            .Sum(p =>
            {
                int mult = 1;
                if (p.IsCaptain) mult = activeChip == ChipType.TripleCaptain ? 3 : 2;
                return p.GameweekPoints * mult;
            });

        // Resolve league context
        Models.LeagueType? leagueType = null;
        string? leagueName = null;
        if (team.LeagueId != null)
        {
            var league = await _db.Leagues
                .Where(l => l.Id == team.LeagueId.Value)
                .Select(l => new { l.Name, l.Type })
                .FirstOrDefaultAsync();
            if (league != null)
            {
                leagueName = league.Name;
                leagueType = league.Type;
            }
        }

        return new TeamDto(
            team.Id, team.Name, team.Budget, team.FreeTransfers,
            team.TotalPoints, gwPoints, pickDtos, activeChip,
            team.LeagueId, leagueName, leagueType
        );
    }

    private async Task<TeamDto> GetTeamAtGameweek(FantasyTeam team, int gwNumber)
    {
        var gw = await _db.Gameweeks.FirstOrDefaultAsync(g => g.Number == gwNumber);
        if (gw == null)
            return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
                team.TotalPoints, 0, new List<PickDetailDto>(), null, team.LeagueId, null, null);

        var picks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == team.Id && p.GameweekId == gw.Id)
            .Include(p => p.Player)
            .OrderBy(p => p.SquadPosition)
            .ToListAsync();

        if (picks.Count == 0)
            return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
                team.TotalPoints, 0, new List<PickDetailDto>(), null, team.LeagueId, null, null);

        var activeChip = team.ChipUsages.FirstOrDefault(c => c.GameweekId == gw.Id)?.ChipType;

        var gwEventMap = await _db.MatchEvents
            .Where(e => e.GameweekId == gw.Id)
            .GroupBy(e => e.PlayerId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Points));

        var playedIds = (await _db.MatchEvents
            .Where(e => e.GameweekId == gw.Id && e.EventType == EventType.MinutesPlayed)
            .GroupBy(e => e.PlayerId)
            .Select(g => new { PlayerId = g.Key, Minutes = g.Sum(e => e.Minute ?? 0) })
            .ToListAsync())
            .Where(m => m.Minutes > 0)
            .Select(m => m.PlayerId)
            .ToHashSet();

        HashSet<int> scoringIds;
        List<(int outId, int inId)> subs;
        if (activeChip == ChipType.BenchBoost)
        {
            scoringIds = picks.Select(p => p.PlayerId).ToHashSet();
            subs = new List<(int, int)>();
        }
        else
        {
            (scoringIds, subs) = LeaderboardService.ComputeAutoSubs(picks, playedIds);
        }

        var captainPick = picks.FirstOrDefault(p => p.IsCaptain);
        var vicePick = picks.FirstOrDefault(p => p.IsViceCaptain);
        int? captainId = captainPick?.PlayerId;
        if (captainPick != null && !playedIds.Contains(captainPick.PlayerId)
            && vicePick != null && playedIds.Contains(vicePick.PlayerId))
            captainId = vicePick.PlayerId;

        var positionMap = picks.ToDictionary(p => p.PlayerId, p => p.SquadPosition);
        foreach (var (outId, inId) in subs)
        {
            int tmp = positionMap[outId];
            positionMap[outId] = positionMap[inId];
            positionMap[inId] = tmp;
        }

        var pickDtos = picks
            .Select(p => new PickDetailDto(
                p.PlayerId,
                p.Player.Name,
                p.Player.Team,
                p.Player.Position,
                p.Player.Price,
                positionMap[p.PlayerId],
                p.IsCaptain,
                p.IsViceCaptain,
                gwEventMap.GetValueOrDefault(p.PlayerId, 0)
            ))
            .OrderBy(p => p.SquadPosition)
            .ToList();

        int gwPoints = 0;
        foreach (var pick in picks)
        {
            if (!scoringIds.Contains(pick.PlayerId)) continue;
            int pts = gwEventMap.GetValueOrDefault(pick.PlayerId, 0);
            int mult = pick.PlayerId == captainId
                ? (activeChip == ChipType.TripleCaptain ? 3 : 2)
                : 1;
            gwPoints += pts * mult;
        }

        return new TeamDto(
            team.Id, team.Name, team.Budget, team.FreeTransfers,
            team.TotalPoints, gwPoints, pickDtos, activeChip,
            team.LeagueId, null, null
        );
    }

    private async Task<List<GameweekHistoryDto>> ComputeHistory(int teamId)
    {
        var finishedGws = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Finished)
            .OrderBy(g => g.Number)
            .ToListAsync();

        if (finishedGws.Count == 0) return new List<GameweekHistoryDto>();

        var allPicks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == teamId && finishedGws.Select(g => g.Id).Contains(p.GameweekId))
            .Include(p => p.Player)
            .ToListAsync();

        var chipUsages = await _db.ChipUsages
            .Where(c => c.FantasyTeamId == teamId)
            .ToListAsync();

        var allEvents = await _db.MatchEvents
            .Where(e => finishedGws.Select(g => g.Id).Contains(e.GameweekId))
            .GroupBy(e => new { e.GameweekId, e.PlayerId })
            .Select(g => new { g.Key.GameweekId, g.Key.PlayerId, Points = g.Sum(e => e.Points) })
            .ToListAsync();

        var eventLookup = allEvents
            .GroupBy(e => e.GameweekId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(e => e.PlayerId, e => e.Points));

        var minutesData = await _db.MatchEvents
            .Where(e => finishedGws.Select(g => g.Id).Contains(e.GameweekId) && e.EventType == EventType.MinutesPlayed)
            .GroupBy(e => new { e.GameweekId, e.PlayerId })
            .Select(g => new { g.Key.GameweekId, g.Key.PlayerId, Minutes = g.Sum(e => e.Minute ?? 0) })
            .ToListAsync();

        var minutesLookup = minutesData
            .GroupBy(m => m.GameweekId)
            .ToDictionary(g => g.Key, g => g.Where(m => m.Minutes > 0).Select(m => m.PlayerId).ToHashSet());

        var allTeamTotals = await _db.FantasyTeams
            .Select(t => new { t.Id, t.TotalPoints, t.GameweekPoints })
            .ToListAsync();

        int cumulative = 0;
        var result = new List<GameweekHistoryDto>();

        foreach (var gw in finishedGws)
        {
            var gwPicks = allPicks.Where(p => p.GameweekId == gw.Id).ToList();
            if (gwPicks.Count == 0)
            {
                result.Add(new GameweekHistoryDto(gw.Number, 0, cumulative, 0));
                continue;
            }

            var activeChip = chipUsages.FirstOrDefault(c => c.GameweekId == gw.Id)?.ChipType;
            var gwEventMap = eventLookup.GetValueOrDefault(gw.Id, new Dictionary<int, int>());
            var playedIds = minutesLookup.GetValueOrDefault(gw.Id, new HashSet<int>());

            HashSet<int> scoringIds;
            if (activeChip == ChipType.BenchBoost)
            {
                scoringIds = gwPicks.Select(p => p.PlayerId).ToHashSet();
            }
            else
            {
                var (effective, _) = LeaderboardService.ComputeAutoSubs(gwPicks, playedIds);
                scoringIds = effective;
            }

            var captainPick = gwPicks.FirstOrDefault(p => p.IsCaptain);
            var vicePick = gwPicks.FirstOrDefault(p => p.IsViceCaptain);
            int? captainId = captainPick?.PlayerId;
            if (captainPick != null && !playedIds.Contains(captainPick.PlayerId)
                && vicePick != null && playedIds.Contains(vicePick.PlayerId))
                captainId = vicePick.PlayerId;

            int gwPoints = 0;
            foreach (var pick in gwPicks)
            {
                if (!scoringIds.Contains(pick.PlayerId)) continue;
                int pts = gwEventMap.GetValueOrDefault(pick.PlayerId, 0);
                int mult = 1;
                if (pick.PlayerId == captainId)
                    mult = activeChip == ChipType.TripleCaptain ? 3 : 2;
                gwPoints += pts * mult;
            }

            cumulative += gwPoints;
            result.Add(new GameweekHistoryDto(gw.Number, gwPoints, cumulative, 0));
        }

        int myTotal = allTeamTotals.FirstOrDefault(t => t.Id == teamId)?.TotalPoints ?? 0;
        int rank = allTeamTotals.Count(t => t.TotalPoints > myTotal) + 1;
        if (result.Count > 0)
            result[result.Count - 1] = result[result.Count - 1] with { OverallRank = rank };

        return result;
    }

    private async Task EnsurePicksForGameweek(int teamId, int targetGwId)
    {
        bool hasPicks = await _db.FantasyPicks
            .AnyAsync(p => p.FantasyTeamId == teamId && p.GameweekId == targetGwId);

        if (hasPicks) return;

        var latestPick = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == teamId)
            .OrderByDescending(p => p.GameweekId)
            .FirstOrDefaultAsync();

        if (latestPick == null) return;

        var sourcePicks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == teamId && p.GameweekId == latestPick.GameweekId)
            .ToListAsync();

        foreach (var pick in sourcePicks)
        {
            _db.FantasyPicks.Add(new FantasyPick
            {
                FantasyTeamId = teamId,
                PlayerId = pick.PlayerId,
                GameweekId = targetGwId,
                SquadPosition = pick.SquadPosition,
                IsCaptain = pick.IsCaptain,
                IsViceCaptain = pick.IsViceCaptain
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task<Gameweek?> GetCurrentGameweek()
    {
        return await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming || g.Status == GameweekStatus.Live)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();
    }
}
