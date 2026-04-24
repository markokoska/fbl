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

    [HttpGet]
    public async Task<ActionResult<TeamDto>> GetMyTeam()
    {
        var currentGw = await GetCurrentGameweek();

        var team = await _db.FantasyTeams
            .Include(t => t.ChipUsages)
            .FirstOrDefaultAsync(t => t.UserId == UserId);

        if (team == null) return NotFound("You haven't created a team yet.");

        // Ensure picks exist for the current GW (auto-copy-forward if missing)
        if (currentGw != null)
            await EnsurePicksForGameweek(team.Id, currentGw.Id);

        // Now load picks for this GW
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

        return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
            team.TotalPoints, gwPoints, pickDtos, activeChip);
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto dto)
    {
        var existingTeam = await _db.FantasyTeams.AnyAsync(t => t.UserId == UserId);
        if (existingTeam)
            return BadRequest("You already have a team.");

        if (dto.Picks.Count != 15)
            return BadRequest("You must pick exactly 15 players.");

        var playerIds = dto.Picks.Select(p => p.PlayerId).ToList();
        var players = await _db.BundesligaPlayers
            .Where(p => playerIds.Contains(p.Id))
            .ToListAsync();

        if (players.Count != 15)
            return BadRequest("Some players were not found.");

        // Validate budget
        var totalCost = players.Sum(p => p.Price);
        if (totalCost > 100m)
            return BadRequest($"Total cost {totalCost}M exceeds budget of 100M.");

        // Validate max 3 per team
        var teamGroups = players.GroupBy(p => p.Team);
        if (teamGroups.Any(g => g.Count() > 3))
            return BadRequest("Maximum 3 players from the same team.");

        // Validate formation: 2 GK, 5 DEF, 5 MID, 3 FWD
        var positionCounts = players.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.Count());
        if (positionCounts.GetValueOrDefault(PlayerPosition.GK) != 2 ||
            positionCounts.GetValueOrDefault(PlayerPosition.DEF) != 5 ||
            positionCounts.GetValueOrDefault(PlayerPosition.MID) != 5 ||
            positionCounts.GetValueOrDefault(PlayerPosition.FWD) != 3)
            return BadRequest("Squad must have 2 GK, 5 DEF, 5 MID, 3 FWD.");

        // Validate exactly 1 captain and 1 vice captain
        if (dto.Picks.Count(p => p.IsCaptain) != 1 || dto.Picks.Count(p => p.IsViceCaptain) != 1)
            return BadRequest("You must have exactly one captain and one vice captain.");

        var currentGw = await GetCurrentGameweek();
        if (currentGw == null)
            return BadRequest("No active gameweek found.");

        var team = new FantasyTeam
        {
            UserId = UserId,
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
    public async Task<ActionResult<TeamDto>> GetUserTeam(string userId)
    {
        var currentGw = await GetCurrentGameweek();

        var team = await _db.FantasyTeams
            .Include(t => t.ChipUsages)
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (team == null) return NotFound("Team not found.");

        // Ensure picks exist for the current GW
        if (currentGw != null)
            await EnsurePicksForGameweek(team.Id, currentGw.Id);

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

        return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
            team.TotalPoints, gwPoints, pickDtos, activeChip);
    }

    [HttpPut("picks")]
    public async Task<ActionResult> UpdatePicks(UpdatePicksDto dto)
    {
        var currentGw = await GetCurrentGameweek();
        if (currentGw == null) return BadRequest("No active gameweek.");
        if (currentGw.IsLocked) return BadRequest("Gameweek is locked. Changes must be made at least 1h30m before kickoff.");

        var team = await _db.FantasyTeams
            .FirstOrDefaultAsync(t => t.UserId == UserId);

        if (team == null) return NotFound("Team not found.");

        // Ensure picks exist for this GW first
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
    public async Task<ActionResult<TeamDto>> GetMyTeamAtGameweek(int gwNumber)
    {
        var team = await _db.FantasyTeams
            .Include(t => t.ChipUsages)
            .FirstOrDefaultAsync(t => t.UserId == UserId);
        if (team == null) return NotFound();
        return await GetTeamAtGameweek(team, gwNumber);
    }

    [HttpGet("user/{userId}/gameweek/{gwNumber}")]
    public async Task<ActionResult<TeamDto>> GetUserTeamAtGameweek(string userId, int gwNumber)
    {
        var team = await _db.FantasyTeams
            .Include(t => t.ChipUsages)
            .FirstOrDefaultAsync(t => t.UserId == userId);
        if (team == null) return NotFound();
        return await GetTeamAtGameweek(team, gwNumber);
    }

    private async Task<TeamDto> GetTeamAtGameweek(FantasyTeam team, int gwNumber)
    {
        var gw = await _db.Gameweeks.FirstOrDefaultAsync(g => g.Number == gwNumber);
        if (gw == null)
            return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
                team.TotalPoints, 0, new List<PickDetailDto>(), null);

        var picks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == team.Id && p.GameweekId == gw.Id)
            .Include(p => p.Player)
            .OrderBy(p => p.SquadPosition)
            .ToListAsync();

        if (picks.Count == 0)
            return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
                team.TotalPoints, 0, new List<PickDetailDto>(), null);

        var activeChip = team.ChipUsages.FirstOrDefault(c => c.GameweekId == gw.Id)?.ChipType;

        // Points scored by each player this GW
        var gwEventMap = await _db.MatchEvents
            .Where(e => e.GameweekId == gw.Id)
            .GroupBy(e => e.PlayerId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Points));

        // Who actually played (had >0 minutes)
        var playedIds = (await _db.MatchEvents
            .Where(e => e.GameweekId == gw.Id && e.EventType == EventType.MinutesPlayed)
            .GroupBy(e => e.PlayerId)
            .Select(g => new { PlayerId = g.Key, Minutes = g.Sum(e => e.Minute ?? 0) })
            .ToListAsync())
            .Where(m => m.Minutes > 0)
            .Select(m => m.PlayerId)
            .ToHashSet();

        // Compute auto-subs (same logic as live scoring)
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

        // Captain/vice-captain DNP: if captain didn't play, vice-captain gets the double
        var captainPick = picks.FirstOrDefault(p => p.IsCaptain);
        var vicePick = picks.FirstOrDefault(p => p.IsViceCaptain);
        int? captainId = captainPick?.PlayerId;
        if (captainPick != null && !playedIds.Contains(captainPick.PlayerId)
            && vicePick != null && playedIds.Contains(vicePick.PlayerId))
            captainId = vicePick.PlayerId;

        // Swap squad positions so PitchView shows the effective XI (sub replaces starter's slot)
        var positionMap = picks.ToDictionary(p => p.PlayerId, p => p.SquadPosition);
        foreach (var (outId, inId) in subs)
        {
            int tmp = positionMap[outId];
            positionMap[outId] = positionMap[inId];
            positionMap[inId] = tmp;
        }

        // Build DTOs with adjusted positions
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

        // Score only the effective XI (with correct captain multiplier)
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

        return new TeamDto(team.Id, team.Name, team.Budget, team.FreeTransfers,
            team.TotalPoints, gwPoints, pickDtos, activeChip);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<GameweekHistoryDto>>> GetMyHistory()
    {
        var team = await _db.FantasyTeams.FirstOrDefaultAsync(t => t.UserId == UserId);
        if (team == null) return NotFound();
        return await ComputeHistory(team.Id);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<List<GameweekHistoryDto>>> GetUserHistory(string userId)
    {
        var team = await _db.FantasyTeams.FirstOrDefaultAsync(t => t.UserId == userId);
        if (team == null) return NotFound();
        return await ComputeHistory(team.Id);
    }

    private async Task<List<GameweekHistoryDto>> ComputeHistory(int teamId)
    {
        // Get all finished gameweeks
        var finishedGws = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Finished)
            .OrderBy(g => g.Number)
            .ToListAsync();

        if (finishedGws.Count == 0) return new List<GameweekHistoryDto>();

        // Get all picks for this team across all finished GWs
        var allPicks = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == teamId && finishedGws.Select(g => g.Id).Contains(p.GameweekId))
            .Include(p => p.Player)
            .ToListAsync();

        // Get chip usages
        var chipUsages = await _db.ChipUsages
            .Where(c => c.FantasyTeamId == teamId)
            .ToListAsync();

        // Get all events for finished GWs, grouped by GW then player
        var allEvents = await _db.MatchEvents
            .Where(e => finishedGws.Select(g => g.Id).Contains(e.GameweekId))
            .GroupBy(e => new { e.GameweekId, e.PlayerId })
            .Select(g => new { g.Key.GameweekId, g.Key.PlayerId, Points = g.Sum(e => e.Points) })
            .ToListAsync();

        var eventLookup = allEvents
            .GroupBy(e => e.GameweekId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(e => e.PlayerId, e => e.Points));

        // Minutes for auto-sub logic
        var minutesData = await _db.MatchEvents
            .Where(e => finishedGws.Select(g => g.Id).Contains(e.GameweekId) && e.EventType == EventType.MinutesPlayed)
            .GroupBy(e => new { e.GameweekId, e.PlayerId })
            .Select(g => new { g.Key.GameweekId, g.Key.PlayerId, Minutes = g.Sum(e => e.Minute ?? 0) })
            .ToListAsync();

        var minutesLookup = minutesData
            .GroupBy(m => m.GameweekId)
            .ToDictionary(g => g.Key, g => g.Where(m => m.Minutes > 0).Select(m => m.PlayerId).ToHashSet());

        // All teams' total points for ranking (we'll accumulate)
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

        // Compute rough overall rank (based on cumulative at end = current total)
        // For simplicity, rank = position among all teams by TotalPoints
        int myTotal = allTeamTotals.FirstOrDefault(t => t.Id == teamId)?.TotalPoints ?? 0;
        int rank = allTeamTotals.Count(t => t.TotalPoints > myTotal) + 1;
        if (result.Count > 0)
            result[result.Count - 1] = result[result.Count - 1] with { OverallRank = rank };

        return result;
    }

    /// <summary>
    /// If the team has no picks for the target gameweek, find the most recent GW
    /// that DOES have picks and copy them forward. This handles the gap when
    /// gameweeks are advanced without an explicit copy.
    /// </summary>
    private async Task EnsurePicksForGameweek(int teamId, int targetGwId)
    {
        bool hasPicks = await _db.FantasyPicks
            .AnyAsync(p => p.FantasyTeamId == teamId && p.GameweekId == targetGwId);

        if (hasPicks) return;

        // Find the most recent GW that has picks for this team
        var latestPick = await _db.FantasyPicks
            .Where(p => p.FantasyTeamId == teamId)
            .OrderByDescending(p => p.GameweekId)
            .FirstOrDefaultAsync();

        if (latestPick == null) return; // No picks exist at all

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
