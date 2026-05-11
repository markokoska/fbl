using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

/// <summary>
/// Two-phase weekly turnover for Draft leagues:
///
///   Phase 1 (Queue) — open during the gameweek. Each manager queues
///   "swap player A out for player B in" claims, in priority order.
///
///   Phase 2 (FreeAgency) — opens 24h before the next GW deadline. The
///   queue resolves: managers ordered by reverse standings, each manager's
///   claims processed in priority order, success requires the target player
///   to still be unowned. After processing, anyone can do direct swaps with
///   any remaining unowned player, no queue.
///
///   Phase 3 (Locked) — once the GW deadline passes, no more changes.
/// </summary>
public class WaiverService
{
    private readonly AppDbContext _db;
    public WaiverService(AppDbContext db) { _db = db; }

    // ---- State ----------------------------------------------------------

    public async Task<WaiverStateDto> GetState(int leagueId, string userId)
    {
        var league = await _db.Leagues.FindAsync(leagueId)
            ?? throw new InvalidOperationException("League not found.");

        if (league.Type != LeagueType.Draft)
            throw new InvalidOperationException("Waivers only apply to Draft leagues.");

        if (league.DraftStatus != DraftStatus.Completed)
            return new WaiverStateDto(
                leagueId, league.Name, WaiverPhase.NoDraftYet,
                null, null, null, false,
                new List<WaiverClaimDto>(), new List<WaiverClaimDto>());

        var upcoming = await GetUpcomingGameweek();
        if (upcoming == null)
            return new WaiverStateDto(
                leagueId, league.Name, WaiverPhase.Locked,
                null, null, null, true,
                new List<WaiverClaimDto>(), new List<WaiverClaimDto>());

        var phase = ComputePhase(league, upcoming);
        var processAt = upcoming.Deadline - TimeSpan.FromHours(24);

        var myPending = await GetClaimDtos(leagueId, userId, upcoming.Id, WaiverClaimStatus.Pending);
        var lastResolved = await GetRecentResolved(leagueId, userId);

        return new WaiverStateDto(
            leagueId, league.Name, phase,
            upcoming.Number, upcoming.Deadline, processAt,
            upcoming.IsLocked,
            myPending, lastResolved
        );
    }

    public async Task<List<FreeAgentDto>> GetFreeAgents(int leagueId)
    {
        // "Free agents" = players not currently rostered by any team in this league
        // (using picks for the upcoming GW so newly-acquired players show as owned).
        var upcoming = await GetUpcomingGameweek();
        if (upcoming == null) return new List<FreeAgentDto>();

        var ownedIds = await _db.FantasyPicks
            .Where(p => p.GameweekId == upcoming.Id
                     && p.FantasyTeam.LeagueId == leagueId)
            .Select(p => p.PlayerId)
            .Distinct()
            .ToListAsync();

        var draftedIds = await _db.DraftPicks
            .Where(p => p.LeagueId == leagueId)
            .Select(p => p.PlayerId)
            .ToListAsync();
        var owned = ownedIds.Concat(draftedIds).ToHashSet();

        // Players dropped this GW are visible but flagged as locked.
        var dropped = await _db.DroppedPlayers
            .Where(d => d.LeagueId == leagueId && d.GameweekId == upcoming.Id)
            .Include(d => d.DroppedBy)
            .ToDictionaryAsync(d => d.PlayerId, d => d.DroppedBy.DisplayName);

        return await _db.BundesligaPlayers
            .Where(p => !owned.Contains(p.Id))
            .OrderByDescending(p => p.TotalPoints)
            .Select(p => new FreeAgentDto(
                p.Id, p.Name, p.Team, p.Position, p.Price, p.TotalPoints,
                false, null  // populated client-side after .ToList() — see below
            ))
            .ToListAsync()
            .ContinueWith(t => t.Result
                .Select(fa => dropped.TryGetValue(fa.Id, out var by)
                    ? fa with { IsLocked = true, DroppedByName = by }
                    : fa)
                .ToList());
    }

    /// <summary>True if a player was dropped in this league this GW and can't be re-acquired yet.</summary>
    private async Task<bool> IsLocked(int leagueId, int gameweekId, int playerId)
    {
        return await _db.DroppedPlayers.AnyAsync(d =>
            d.LeagueId == leagueId && d.GameweekId == gameweekId && d.PlayerId == playerId);
    }

    private void RecordDrop(int leagueId, int gameweekId, int playerId, string userId)
    {
        _db.DroppedPlayers.Add(new DroppedPlayer
        {
            LeagueId = leagueId,
            GameweekId = gameweekId,
            PlayerId = playerId,
            DroppedByUserId = userId,
            DroppedAt = DateTime.UtcNow,
        });
    }

    // ---- Claim CRUD -----------------------------------------------------

    public async Task<WaiverClaimDto> AddClaim(int leagueId, string userId, int playerOutId, int playerInId)
    {
        var league = await GetDraftLeagueOrThrow(leagueId);
        var team = await GetMyTeamOrThrow(leagueId, userId);
        var upcoming = await GetUpcomingGameweek()
            ?? throw new InvalidOperationException("No upcoming gameweek.");

        var phase = ComputePhase(league, upcoming);
        if (phase != WaiverPhase.Queue)
            throw new InvalidOperationException("Waiver queue is closed. Use direct swap (free agency) instead.");

        await EnsurePicksForGameweek(team.Id, upcoming.Id);

        // playerOut must be in my squad for the upcoming GW
        bool ownsPlayerOut = await _db.FantasyPicks.AnyAsync(p =>
            p.FantasyTeamId == team.Id && p.GameweekId == upcoming.Id && p.PlayerId == playerOutId);
        if (!ownsPlayerOut)
            throw new InvalidOperationException("PlayerOut is not in your squad.");

        // playerIn must exist; ownership is checked at processing time (could change before then)
        var playerIn = await _db.BundesligaPlayers.FindAsync(playerInId)
            ?? throw new InvalidOperationException("PlayerIn not found.");
        var playerOut = await _db.BundesligaPlayers.FindAsync(playerOutId)
            ?? throw new InvalidOperationException("PlayerOut not found.");
        if (playerIn.Position != playerOut.Position)
            throw new InvalidOperationException("Swap must be same position.");

        // Disallow duplicate claim (same out+in)
        bool dup = await _db.WaiverClaims.AnyAsync(c =>
            c.LeagueId == leagueId && c.UserId == userId && c.GameweekId == upcoming.Id
            && c.Status == WaiverClaimStatus.Pending
            && c.PlayerOutId == playerOutId && c.PlayerInId == playerInId);
        if (dup)
            throw new InvalidOperationException("You already have a claim for this swap.");

        // Reject if target player was dropped this GW.
        if (await IsLocked(leagueId, upcoming.Id, playerInId))
            throw new InvalidOperationException("That player was dropped this gameweek and is locked until next GW.");

        // Next priority = current max + 1
        int nextPriority = 1 + (await _db.WaiverClaims
            .Where(c => c.LeagueId == leagueId && c.UserId == userId && c.GameweekId == upcoming.Id
                     && c.Status == WaiverClaimStatus.Pending)
            .Select(c => (int?)c.Priority)
            .MaxAsync() ?? 0);

        var claim = new WaiverClaim
        {
            LeagueId = leagueId,
            UserId = userId,
            GameweekId = upcoming.Id,
            PlayerOutId = playerOutId,
            PlayerInId = playerInId,
            Priority = nextPriority,
            Status = WaiverClaimStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        _db.WaiverClaims.Add(claim);
        await _db.SaveChangesAsync();

        return ToDto(claim, playerOut, playerIn);
    }

    public async Task RemoveClaim(int leagueId, string userId, int claimId)
    {
        var claim = await _db.WaiverClaims.FindAsync(claimId)
            ?? throw new InvalidOperationException("Claim not found.");
        if (claim.LeagueId != leagueId || claim.UserId != userId)
            throw new UnauthorizedAccessException("Not your claim.");
        if (claim.Status != WaiverClaimStatus.Pending)
            throw new InvalidOperationException("Claim already processed.");

        _db.WaiverClaims.Remove(claim);
        await _db.SaveChangesAsync();
    }

    public async Task ReorderClaims(int leagueId, string userId, List<int> claimIdsInOrder)
    {
        var upcoming = await GetUpcomingGameweek()
            ?? throw new InvalidOperationException("No upcoming gameweek.");

        var mine = await _db.WaiverClaims
            .Where(c => c.LeagueId == leagueId && c.UserId == userId
                     && c.GameweekId == upcoming.Id && c.Status == WaiverClaimStatus.Pending)
            .ToListAsync();

        var idSet = mine.Select(c => c.Id).ToHashSet();
        if (claimIdsInOrder.Count != mine.Count || !claimIdsInOrder.All(idSet.Contains))
            throw new InvalidOperationException("Reorder list must contain exactly all your pending claims.");

        for (int i = 0; i < claimIdsInOrder.Count; i++)
        {
            var c = mine.First(x => x.Id == claimIdsInOrder[i]);
            c.Priority = i + 1;
        }
        await _db.SaveChangesAsync();
    }

    /// <summary>Free-agency direct swap (post-queue-processing, pre-deadline).</summary>
    public async Task DirectSwap(int leagueId, string userId, int playerOutId, int playerInId)
    {
        var league = await GetDraftLeagueOrThrow(leagueId);
        var team = await GetMyTeamOrThrow(leagueId, userId);
        var upcoming = await GetUpcomingGameweek()
            ?? throw new InvalidOperationException("No upcoming gameweek.");

        var phase = ComputePhase(league, upcoming);
        if (phase != WaiverPhase.FreeAgency)
            throw new InvalidOperationException("Free agency is not open right now.");

        await EnsurePicksForGameweek(team.Id, upcoming.Id);

        var pick = await _db.FantasyPicks.FirstOrDefaultAsync(p =>
            p.FantasyTeamId == team.Id && p.GameweekId == upcoming.Id && p.PlayerId == playerOutId)
            ?? throw new InvalidOperationException("PlayerOut is not in your squad.");

        var playerIn = await _db.BundesligaPlayers.FindAsync(playerInId)
            ?? throw new InvalidOperationException("PlayerIn not found.");
        var playerOut = await _db.BundesligaPlayers.FindAsync(playerOutId)
            ?? throw new InvalidOperationException("PlayerOut not found.");
        if (playerIn.Position != playerOut.Position)
            throw new InvalidOperationException("Swap must be same position.");

        // playerIn must not be owned by anyone in this league for the upcoming GW
        bool taken = await _db.FantasyPicks.AnyAsync(p =>
            p.GameweekId == upcoming.Id
            && p.FantasyTeam.LeagueId == leagueId
            && p.PlayerId == playerInId);
        if (taken)
            throw new InvalidOperationException("Player is already owned in this league.");

        // Reject if target player was dropped this GW.
        if (await IsLocked(leagueId, upcoming.Id, playerInId))
            throw new InvalidOperationException("That player was dropped this gameweek and is locked until next GW.");

        pick.PlayerId = playerInId;
        // Record the drop so the outgoing player can't be picked back up this GW.
        RecordDrop(leagueId, upcoming.Id, playerOutId, userId);
        await _db.SaveChangesAsync();
    }

    // ---- Queue processing -----------------------------------------------

    /// <summary>
    /// Resolves the queue for a single league's upcoming GW. Idempotent —
    /// running it a second time does nothing if already processed.
    /// </summary>
    public async Task ProcessLeagueWaivers(int leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId);
        if (league == null) return;
        if (league.Type != LeagueType.Draft) return;
        if (league.DraftStatus != DraftStatus.Completed) return;

        var upcoming = await GetUpcomingGameweek();
        if (upcoming == null) return;
        if (league.LastWaiverProcessedGameweekId == upcoming.Id) return; // already done

        // Order managers by reverse standings (lowest TotalPoints + GameweekPoints first).
        var teams = await _db.FantasyTeams
            .Where(t => t.LeagueId == leagueId)
            .OrderBy(t => t.TotalPoints + t.GameweekPoints)
            .ToListAsync();

        // Ensure picks exist for the upcoming GW for every team in this league.
        foreach (var t in teams) await EnsurePicksForGameweek(t.Id, upcoming.Id);

        // Per-user ordered lists of pending claims.
        var allClaims = await _db.WaiverClaims
            .Where(c => c.LeagueId == leagueId
                     && c.GameweekId == upcoming.Id
                     && c.Status == WaiverClaimStatus.Pending)
            .OrderBy(c => c.Priority)
            .ToListAsync();

        foreach (var team in teams)
        {
            var myClaims = allClaims.Where(c => c.UserId == team.UserId).OrderBy(c => c.Priority).ToList();

            foreach (var claim in myClaims)
            {
                claim.ProcessedAt = DateTime.UtcNow;

                // PlayerOut must still be in this team's squad for the upcoming GW.
                var ownedPick = await _db.FantasyPicks.FirstOrDefaultAsync(p =>
                    p.FantasyTeamId == team.Id && p.GameweekId == upcoming.Id
                    && p.PlayerId == claim.PlayerOutId);
                if (ownedPick == null)
                {
                    claim.Status = WaiverClaimStatus.Failed;
                    claim.FailureReason = "Player out is no longer in your squad.";
                    continue;
                }

                // PlayerIn must still be unowned in this league.
                bool taken = await _db.FantasyPicks.AnyAsync(p =>
                    p.GameweekId == upcoming.Id
                    && p.FantasyTeam.LeagueId == leagueId
                    && p.PlayerId == claim.PlayerInId);
                if (taken)
                {
                    claim.Status = WaiverClaimStatus.Failed;
                    claim.FailureReason = "Target player was claimed by someone with higher waiver priority.";
                    continue;
                }

                // Reject if PlayerIn is locked from a previous drop this GW.
                if (await IsLocked(leagueId, upcoming.Id, claim.PlayerInId))
                {
                    claim.Status = WaiverClaimStatus.Failed;
                    claim.FailureReason = "Target player was dropped this gameweek and is locked.";
                    continue;
                }

                // Apply the swap.
                ownedPick.PlayerId = claim.PlayerInId;
                claim.Status = WaiverClaimStatus.Succeeded;
                // Record the drop so PlayerOut is locked for the rest of this GW.
                RecordDrop(leagueId, upcoming.Id, claim.PlayerOutId, team.UserId);
            }
            await _db.SaveChangesAsync(); // Save per-user so subsequent users see fresh state.
        }

        league.LastWaiverProcessedGameweekId = upcoming.Id;
        await _db.SaveChangesAsync();
    }

    /// <summary>Called by the background service. Processes any league whose 24h window has opened.</summary>
    public async Task ProcessDueLeagues()
    {
        var upcoming = await GetUpcomingGameweek();
        if (upcoming == null) return;
        if (DateTime.UtcNow < upcoming.Deadline - TimeSpan.FromHours(24)) return;
        if (DateTime.UtcNow >= upcoming.Deadline) return; // too late, locked

        var dueLeagueIds = await _db.Leagues
            .Where(l => l.Type == LeagueType.Draft
                     && l.DraftStatus == DraftStatus.Completed
                     && (l.LastWaiverProcessedGameweekId == null || l.LastWaiverProcessedGameweekId != upcoming.Id))
            .Select(l => l.Id)
            .ToListAsync();

        foreach (var id in dueLeagueIds)
        {
            try { await ProcessLeagueWaivers(id); }
            catch { /* keep looping */ }
        }
    }

    // ---- Helpers --------------------------------------------------------

    private WaiverPhase ComputePhase(League league, Gameweek upcoming)
    {
        if (upcoming.IsLocked) return WaiverPhase.Locked;
        if (league.LastWaiverProcessedGameweekId == upcoming.Id) return WaiverPhase.FreeAgency;
        return WaiverPhase.Queue;
    }

    private async Task<Gameweek?> GetUpcomingGameweek()
    {
        return await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();
    }

    private async Task<League> GetDraftLeagueOrThrow(int leagueId)
    {
        var league = await _db.Leagues.FindAsync(leagueId)
            ?? throw new InvalidOperationException("League not found.");
        if (league.Type != LeagueType.Draft)
            throw new InvalidOperationException("Not a draft league.");
        if (league.DraftStatus != DraftStatus.Completed)
            throw new InvalidOperationException("Draft is not completed yet.");
        return league;
    }

    private async Task<FantasyTeam> GetMyTeamOrThrow(int leagueId, string userId)
    {
        return await _db.FantasyTeams.FirstOrDefaultAsync(t =>
            t.LeagueId == leagueId && t.UserId == userId)
            ?? throw new InvalidOperationException("You don't have a team in this league.");
    }

    private async Task EnsurePicksForGameweek(int teamId, int targetGwId)
    {
        bool hasPicks = await _db.FantasyPicks.AnyAsync(p =>
            p.FantasyTeamId == teamId && p.GameweekId == targetGwId);
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

    private async Task<List<WaiverClaimDto>> GetClaimDtos(int leagueId, string userId, int gwId, WaiverClaimStatus status)
    {
        return await _db.WaiverClaims
            .Where(c => c.LeagueId == leagueId && c.UserId == userId
                     && c.GameweekId == gwId && c.Status == status)
            .Include(c => c.PlayerOut)
            .Include(c => c.PlayerIn)
            .OrderBy(c => c.Priority)
            .Select(c => new WaiverClaimDto(
                c.Id,
                c.PlayerOutId, c.PlayerOut.Name, c.PlayerOut.Team, c.PlayerOut.Position,
                c.PlayerInId, c.PlayerIn.Name, c.PlayerIn.Team, c.PlayerIn.Position,
                c.Priority, c.Status, c.FailureReason, c.CreatedAt, c.ProcessedAt))
            .ToListAsync();
    }

    private async Task<List<WaiverClaimDto>> GetRecentResolved(int leagueId, string userId)
    {
        // Last 10 resolved claims for context/history.
        return await _db.WaiverClaims
            .Where(c => c.LeagueId == leagueId && c.UserId == userId
                     && c.Status != WaiverClaimStatus.Pending)
            .Include(c => c.PlayerOut)
            .Include(c => c.PlayerIn)
            .OrderByDescending(c => c.ProcessedAt)
            .Take(10)
            .Select(c => new WaiverClaimDto(
                c.Id,
                c.PlayerOutId, c.PlayerOut.Name, c.PlayerOut.Team, c.PlayerOut.Position,
                c.PlayerInId, c.PlayerIn.Name, c.PlayerIn.Team, c.PlayerIn.Position,
                c.Priority, c.Status, c.FailureReason, c.CreatedAt, c.ProcessedAt))
            .ToListAsync();
    }

    private static WaiverClaimDto ToDto(WaiverClaim c, BundesligaPlayer playerOut, BundesligaPlayer playerIn)
        => new(c.Id,
            c.PlayerOutId, playerOut.Name, playerOut.Team, playerOut.Position,
            c.PlayerInId, playerIn.Name, playerIn.Team, playerIn.Position,
            c.Priority, c.Status, c.FailureReason, c.CreatedAt, c.ProcessedAt);
}
