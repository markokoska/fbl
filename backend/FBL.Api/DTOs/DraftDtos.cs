using FBL.Api.Models;

namespace FBL.Api.DTOs;

/// <summary>Snapshot of a draft used by both REST polling and SignalR clients.</summary>
public record DraftStateDto(
    int LeagueId,
    string LeagueName,
    DraftStatus Status,
    int MaxMembers,
    int TotalPicks,           // 15 * MaxMembers
    int CurrentPickNumber,    // 0 before draft starts; 1..TotalPicks during; > TotalPicks when done
    int CurrentRound,         // derived from CurrentPickNumber and member count
    string? CurrentPickerUserId,
    string? CurrentPickerName,
    DateTime? CurrentPickDeadline,
    int PickSeconds,
    bool MyTurn,
    bool IsCreator,
    List<DraftMemberDto> Members,
    List<DraftPickDto> Picks
);

public record DraftMemberDto(
    string UserId,
    string DisplayName,
    int OrderIndex,           // 0-based; matches snake order
    int PicksMade
);

public record DraftPickDto(
    int PickNumber,
    int Round,
    string UserId,
    string DisplayName,
    int PlayerId,
    string PlayerName,
    string PlayerTeam,
    PlayerPosition Position,
    bool WasAutoPick,
    DateTime PickedAt
);

public record AvailablePlayerDto(
    int Id,
    string Name,
    string Team,
    PlayerPosition Position,
    decimal Price,
    int TotalPoints
);

public record MakePickDto(int PlayerId);

/// <summary>Broadcast payload sent over SignalR after each pick.</summary>
public record PickBroadcastDto(
    int LeagueId,
    DraftPickDto Pick,
    int NextPickNumber,
    string? NextPickerUserId,
    DateTime? NextPickDeadline,
    DraftStatus Status        // Completed if this was the final pick
);
