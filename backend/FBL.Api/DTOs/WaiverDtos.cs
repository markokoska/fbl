using FBL.Api.Models;

namespace FBL.Api.DTOs;

public enum WaiverPhase
{
    Queue,        // accepting queued claims; processing hasn't run yet for the upcoming GW
    FreeAgency,   // queue processed; direct swaps allowed
    Locked,       // GW deadline passed (or no upcoming GW)
    NoDraftYet,   // draft hasn't completed; waivers don't apply yet
}

public record WaiverStateDto(
    int LeagueId,
    string LeagueName,
    WaiverPhase Phase,
    int? UpcomingGameweekNumber,
    DateTime? UpcomingDeadline,
    DateTime? ProcessAt,           // when the queue is scheduled to resolve (24h before deadline)
    bool IsLocked,                  // true if upcoming GW is locked
    List<WaiverClaimDto> MyClaims,  // pending claims, ordered by priority
    List<WaiverClaimDto> RecentResolved  // last GW's processed claims for this user (history)
);

public record WaiverClaimDto(
    int Id,
    int PlayerOutId,
    string PlayerOutName,
    string PlayerOutTeam,
    PlayerPosition PlayerOutPosition,
    int PlayerInId,
    string PlayerInName,
    string PlayerInTeam,
    PlayerPosition PlayerInPosition,
    int Priority,
    WaiverClaimStatus Status,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record FreeAgentDto(
    int Id,
    string Name,
    string Team,
    PlayerPosition Position,
    decimal Price,
    int TotalPoints,
    bool IsLocked,           // dropped this GW; can't be picked back up until next GW
    string? DroppedByName    // who dropped them (null when not locked)
);

public record AddWaiverClaimDto(int PlayerOutId, int PlayerInId);

public record DirectSwapDto(int PlayerOutId, int PlayerInId);

public record ReorderClaimsDto(List<int> ClaimIdsInOrder);
