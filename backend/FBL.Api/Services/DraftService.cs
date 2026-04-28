using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Hubs;
using FBL.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

/// <summary>
/// All draft logic in one place: snake order, pick validation, auto-pick on
/// timeout, conversion of completed drafts into FantasyTeam/FantasyPick rows.
/// </summary>
public class DraftService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<DraftHub> _hub;

    // Standard FPL squad caps. Pick is rejected if it would exceed any of these.
    private static readonly Dictionary<PlayerPosition, int> SquadCaps = new()
    {
        { PlayerPosition.GK, 2 },
        { PlayerPosition.DEF, 5 },
        { PlayerPosition.MID, 5 },
        { PlayerPosition.FWD, 3 },
    };
    private const int SquadSize = 15;

    public DraftService(AppDbContext db, IHubContext<DraftHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    // ---- Snake order math ------------------------------------------------

    /// <summary>
    /// Given pick number P (1-based) and N members ordered by join time,
    /// returns the 0-based index of the member who should pick.
    /// Round 1: 0,1,..,N-1. Round 2 (snake): N-1,N-2,..,0. Round 3: 0,1,..,N-1. Etc.
    /// </summary>
    public static int GetPickerIndex(int pickNumber, int memberCount)
    {
        if (memberCount <= 0) throw new ArgumentException("memberCount must be positive");
        int round = ((pickNumber - 1) / memberCount) + 1;       // 1-based
        int posInRound = ((pickNumber - 1) % memberCount) + 1;  // 1-based
        return (round % 2 == 1) ? posInRound - 1 : memberCount - posInRound;
    }

    public static int GetRound(int pickNumber, int memberCount)
        => ((pickNumber - 1) / memberCount) + 1;

    // ---- Public API ------------------------------------------------------

    /// <summary>Creator-only. Flips the draft from Pending to InProgress and starts the clock.</summary>
    public async Task<DraftStateDto> StartDraft(int leagueId, string userId)
    {
        var league = await LoadLeagueOrThrow(leagueId);

        if (league.Type != LeagueType.Draft)
            throw new InvalidOperationException("League is not a draft league.");
        if (league.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("Only the league creator can start the draft.");
        if (league.DraftStatus != DraftStatus.Pending)
            throw new InvalidOperationException("Draft has already started or completed.");

        var memberCount = await _db.LeagueMembers.CountAsync(m => m.LeagueId == leagueId);
        if (memberCount < 2)
            throw new InvalidOperationException("Need at least 2 members to start a draft.");

        league.DraftStatus = DraftStatus.InProgress;
        league.CurrentPickNumber = 1;
        league.CurrentPickDeadline = DateTime.UtcNow.AddSeconds(league.DraftPickSeconds);
        await _db.SaveChangesAsync();

        var state = await GetState(leagueId, userId);
        await _hub.Clients.Group($"draft-{leagueId}").SendAsync("DraftStarted", state);
        return state;
    }

    /// <summary>Build a complete view of the draft for the given viewer.</summary>
    public async Task<DraftStateDto> GetState(int leagueId, string viewerUserId)
    {
        var league = await LoadLeagueOrThrow(leagueId);
        var members = await GetOrderedMembers(leagueId);
        var picks = await GetPickDtos(leagueId);

        var memberCount = members.Count;
        int totalPicks = SquadSize * Math.Max(memberCount, 1);

        string? currentPickerUserId = null;
        string? currentPickerName = null;
        if (league.DraftStatus == DraftStatus.InProgress && memberCount > 0
            && league.CurrentPickNumber >= 1 && league.CurrentPickNumber <= totalPicks)
        {
            int idx = GetPickerIndex(league.CurrentPickNumber, memberCount);
            currentPickerUserId = members[idx].UserId;
            currentPickerName = members[idx].DisplayName;
        }

        var memberDtos = members.Select((m, i) => new DraftMemberDto(
            m.UserId,
            m.DisplayName,
            i,
            picks.Count(p => p.UserId == m.UserId)
        )).ToList();

        return new DraftStateDto(
            league.Id,
            league.Name,
            league.DraftStatus,
            league.MaxMembers,
            totalPicks,
            league.CurrentPickNumber,
            memberCount > 0 ? GetRound(Math.Max(league.CurrentPickNumber, 1), memberCount) : 0,
            currentPickerUserId,
            currentPickerName,
            league.CurrentPickDeadline,
            league.DraftPickSeconds,
            currentPickerUserId == viewerUserId,
            league.CreatedByUserId == viewerUserId,
            memberDtos,
            picks
        );
    }

    /// <summary>List all players in the league not yet drafted.</summary>
    public async Task<List<AvailablePlayerDto>> GetAvailablePlayers(int leagueId)
    {
        var draftedIds = await _db.DraftPicks
            .Where(p => p.LeagueId == leagueId)
            .Select(p => p.PlayerId)
            .ToListAsync();

        return await _db.BundesligaPlayers
            .Where(p => !draftedIds.Contains(p.Id))
            .OrderByDescending(p => p.TotalPoints)
            .Select(p => new AvailablePlayerDto(
                p.Id, p.Name, p.Team, p.Position, p.Price, p.TotalPoints
            ))
            .ToListAsync();
    }

    /// <summary>User-initiated pick. Validates turn ownership, ownership uniqueness, and squad caps.</summary>
    public async Task<DraftPickDto> MakePick(int leagueId, string userId, int playerId)
    {
        return await ExecutePick(leagueId, userId, playerId, isAutoPick: false);
    }

    /// <summary>
    /// Creator-only. Wipes all draft picks + the auto-created teams (and their
    /// nested picks/transfers/chip usages cascade from the FK), and resets the
    /// draft to Pending so it can be run again. Members are kept.
    /// </summary>
    public async Task<DraftStateDto> ResetDraft(int leagueId, string userId)
    {
        var league = await LoadLeagueOrThrow(leagueId);
        if (league.Type != LeagueType.Draft)
            throw new InvalidOperationException("Not a draft league.");
        if (league.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("Only the league creator can reset the draft.");

        // Drop all teams created for this league. FantasyPicks, Transfers, ChipUsages
        // cascade from FantasyTeam FK.
        var teams = await _db.FantasyTeams.Where(t => t.LeagueId == leagueId).ToListAsync();
        if (teams.Count > 0) _db.FantasyTeams.RemoveRange(teams);

        // Drop all draft picks.
        var picks = await _db.DraftPicks.Where(p => p.LeagueId == leagueId).ToListAsync();
        if (picks.Count > 0) _db.DraftPicks.RemoveRange(picks);

        // Drop any waiver claims tied to this league (no point keeping pre-reset claims).
        var claims = await _db.WaiverClaims.Where(c => c.LeagueId == leagueId).ToListAsync();
        if (claims.Count > 0) _db.WaiverClaims.RemoveRange(claims);

        league.DraftStatus = DraftStatus.Pending;
        league.CurrentPickNumber = 0;
        league.CurrentPickDeadline = null;
        league.LastWaiverProcessedGameweekId = null;

        await _db.SaveChangesAsync();

        var state = await GetState(leagueId, userId);
        await _hub.Clients.Group($"draft-{leagueId}").SendAsync("DraftReset", state);
        return state;
    }

    /// <summary>
    /// Called periodically by the auto-pick background service. For each draft whose
    /// pick deadline has elapsed, picks the highest-rated available player at a
    /// position the current user still needs.
    /// </summary>
    public async Task ProcessExpiredAutoPicks()
    {
        var now = DateTime.UtcNow;
        var expired = await _db.Leagues
            .Where(l => l.Type == LeagueType.Draft
                     && l.DraftStatus == DraftStatus.InProgress
                     && l.CurrentPickDeadline != null
                     && l.CurrentPickDeadline < now)
            .Select(l => l.Id)
            .ToListAsync();

        foreach (var leagueId in expired)
        {
            try
            {
                var league = await _db.Leagues.FindAsync(leagueId);
                if (league == null || league.DraftStatus != DraftStatus.InProgress) continue;

                var members = await GetOrderedMembers(leagueId);
                if (members.Count == 0) continue;

                int idx = GetPickerIndex(league.CurrentPickNumber, members.Count);
                string pickerUserId = members[idx].UserId;

                int? autoPickPlayerId = await PickBestAvailableForUser(leagueId, pickerUserId);
                if (autoPickPlayerId == null) continue;

                await ExecutePick(leagueId, pickerUserId, autoPickPlayerId.Value, isAutoPick: true);
            }
            catch
            {
                // Don't let one bad league kill the loop.
            }
        }
    }

    // ---- Internals -------------------------------------------------------

    private async Task<DraftPickDto> ExecutePick(int leagueId, string userId, int playerId, bool isAutoPick)
    {
        var league = await LoadLeagueOrThrow(leagueId);

        if (league.DraftStatus != DraftStatus.InProgress)
            throw new InvalidOperationException("Draft is not in progress.");

        var members = await GetOrderedMembers(leagueId);
        if (members.Count == 0)
            throw new InvalidOperationException("No members.");

        int totalPicks = SquadSize * members.Count;
        if (league.CurrentPickNumber < 1 || league.CurrentPickNumber > totalPicks)
            throw new InvalidOperationException("Draft pick number is out of bounds.");

        int idx = GetPickerIndex(league.CurrentPickNumber, members.Count);
        var picker = members[idx];
        if (picker.UserId != userId)
            throw new UnauthorizedAccessException("It's not your turn to pick.");

        // Player must exist and not already be drafted in this league.
        var player = await _db.BundesligaPlayers.FindAsync(playerId);
        if (player == null) throw new InvalidOperationException("Player not found.");

        bool alreadyDrafted = await _db.DraftPicks
            .AnyAsync(p => p.LeagueId == leagueId && p.PlayerId == playerId);
        if (alreadyDrafted)
            throw new InvalidOperationException("Player is already owned in this draft.");

        // Position-cap check.
        var myPicks = await _db.DraftPicks
            .Where(p => p.LeagueId == leagueId && p.UserId == userId)
            .Include(p => p.Player)
            .ToListAsync();
        int existingAtPos = myPicks.Count(p => p.Player.Position == player.Position);
        if (existingAtPos >= SquadCaps[player.Position])
            throw new InvalidOperationException(
                $"You already have {SquadCaps[player.Position]} {player.Position}s — pick a different position.");

        // Persist the pick.
        var pickRow = new DraftPick
        {
            LeagueId = leagueId,
            UserId = userId,
            PlayerId = playerId,
            Round = GetRound(league.CurrentPickNumber, members.Count),
            PickNumber = league.CurrentPickNumber,
            PickedAt = DateTime.UtcNow,
            WasAutoPick = isAutoPick,
        };
        _db.DraftPicks.Add(pickRow);

        // Advance.
        bool isFinalPick = league.CurrentPickNumber == totalPicks;
        league.CurrentPickNumber++;
        league.CurrentPickDeadline = isFinalPick
            ? null
            : DateTime.UtcNow.AddSeconds(league.DraftPickSeconds);
        if (isFinalPick) league.DraftStatus = DraftStatus.Completed;

        await _db.SaveChangesAsync();

        // Build broadcast payload.
        var pickDto = new DraftPickDto(
            pickRow.PickNumber,
            pickRow.Round,
            userId,
            picker.DisplayName,
            playerId,
            player.Name,
            player.Team,
            player.Position,
            isAutoPick,
            pickRow.PickedAt
        );

        string? nextPickerUserId = null;
        if (!isFinalPick)
        {
            int nextIdx = GetPickerIndex(league.CurrentPickNumber, members.Count);
            nextPickerUserId = members[nextIdx].UserId;
        }

        var broadcast = new PickBroadcastDto(
            leagueId,
            pickDto,
            isFinalPick ? 0 : league.CurrentPickNumber,
            nextPickerUserId,
            league.CurrentPickDeadline,
            league.DraftStatus
        );
        await _hub.Clients.Group($"draft-{leagueId}").SendAsync("PickMade", broadcast);

        // If we just placed the final pick, materialise FantasyTeams.
        if (isFinalPick)
        {
            await FinalizeDraft(leagueId);
            await _hub.Clients.Group($"draft-{leagueId}").SendAsync("DraftCompleted", new { leagueId });
        }

        return pickDto;
    }

    /// <summary>
    /// Convert each member's set of DraftPick rows into a real FantasyTeam with
    /// FantasyPick rows for the current GW. Uses the standard 1-4-4-2 starting XI
    /// with the rest on the bench, picks captain/vice from highest-totalPoints starters.
    /// </summary>
    private async Task FinalizeDraft(int leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        if (league == null) return;

        var currentGw = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming || g.Status == GameweekStatus.Live)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();
        if (currentGw == null) return;

        var members = await GetOrderedMembers(leagueId);
        foreach (var m in members)
        {
            // Skip if the team already exists (idempotency in case finalize runs twice).
            bool exists = await _db.FantasyTeams.AnyAsync(t => t.UserId == m.UserId && t.LeagueId == leagueId);
            if (exists) continue;

            var picks = await _db.DraftPicks
                .Where(p => p.LeagueId == leagueId && p.UserId == m.UserId)
                .Include(p => p.Player)
                .ToListAsync();
            if (picks.Count != SquadSize) continue;

            var team = new FantasyTeam
            {
                UserId = m.UserId,
                LeagueId = leagueId,
                Name = $"{m.DisplayName}'s Squad",
                Budget = 0m,            // Draft mode ignores budget
                FreeTransfers = 0,      // Draft uses waivers, not transfers
            };
            _db.FantasyTeams.Add(team);
            await _db.SaveChangesAsync();

            // Layout: 1 GK, 4 DEF, 4 MID, 2 FWD as starters; rest on bench.
            var byPos = picks.GroupBy(p => p.Player.Position).ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Player.TotalPoints).ToList());
            var gks = byPos.GetValueOrDefault(PlayerPosition.GK, new());
            var defs = byPos.GetValueOrDefault(PlayerPosition.DEF, new());
            var mids = byPos.GetValueOrDefault(PlayerPosition.MID, new());
            var fwds = byPos.GetValueOrDefault(PlayerPosition.FWD, new());

            var ordered = new List<DraftPick>();
            ordered.AddRange(gks.Take(1));
            ordered.AddRange(defs.Take(4));
            ordered.AddRange(mids.Take(4));
            ordered.AddRange(fwds.Take(2));
            // Bench
            ordered.AddRange(gks.Skip(1));
            ordered.AddRange(defs.Skip(4));
            ordered.AddRange(mids.Skip(4));
            ordered.AddRange(fwds.Skip(2));

            // Draft mode: no captain or vice captain. All multipliers stay at 1x.
            int squadPos = 1;
            foreach (var dp in ordered)
            {
                _db.FantasyPicks.Add(new FantasyPick
                {
                    FantasyTeamId = team.Id,
                    PlayerId = dp.PlayerId,
                    GameweekId = currentGw.Id,
                    SquadPosition = squadPos,
                    IsCaptain = false,
                    IsViceCaptain = false,
                });
                squadPos++;
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task<int?> PickBestAvailableForUser(int leagueId, string userId)
    {
        // What positions does this user still need?
        var myPicks = await _db.DraftPicks
            .Where(p => p.LeagueId == leagueId && p.UserId == userId)
            .Include(p => p.Player)
            .ToListAsync();

        var stillNeeded = SquadCaps
            .Where(kv => myPicks.Count(p => p.Player.Position == kv.Key) < kv.Value)
            .Select(kv => kv.Key)
            .ToHashSet();

        if (stillNeeded.Count == 0) return null;

        var draftedIds = await _db.DraftPicks
            .Where(p => p.LeagueId == leagueId)
            .Select(p => p.PlayerId)
            .ToListAsync();

        var best = await _db.BundesligaPlayers
            .Where(p => !draftedIds.Contains(p.Id) && stillNeeded.Contains(p.Position))
            .OrderByDescending(p => p.TotalPoints)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        return best == 0 ? null : best;
    }

    private async Task<League> LoadLeagueOrThrow(int leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        if (league == null) throw new InvalidOperationException("League not found.");
        return league;
    }

    /// <summary>Members ordered by JoinedAt ascending = snake order baseline.</summary>
    private async Task<List<MemberInfo>> GetOrderedMembers(int leagueId)
    {
        return await _db.LeagueMembers
            .Where(m => m.LeagueId == leagueId)
            .Include(m => m.User)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new MemberInfo(m.UserId, m.User.DisplayName))
            .ToListAsync();
    }

    private async Task<List<DraftPickDto>> GetPickDtos(int leagueId)
    {
        return await _db.DraftPicks
            .Where(p => p.LeagueId == leagueId)
            .Include(p => p.Player)
            .Include(p => p.User)
            .OrderBy(p => p.PickNumber)
            .Select(p => new DraftPickDto(
                p.PickNumber,
                p.Round,
                p.UserId,
                p.User.DisplayName,
                p.PlayerId,
                p.Player.Name,
                p.Player.Team,
                p.Player.Position,
                p.WasAutoPick,
                p.PickedAt
            ))
            .ToListAsync();
    }

    private record MemberInfo(string UserId, string DisplayName);
}
