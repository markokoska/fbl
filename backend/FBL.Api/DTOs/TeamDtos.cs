using FBL.Api.Models;

namespace FBL.Api.DTOs;

public record CreateTeamDto(
    string TeamName,
    List<PickDto> Picks
);

public record PickDto(
    int PlayerId,
    int SquadPosition,
    bool IsCaptain,
    bool IsViceCaptain
);

public record TeamDto(
    int Id,
    string Name,
    decimal Budget,
    int FreeTransfers,
    int TotalPoints,
    int GameweekPoints,
    List<PickDetailDto> Picks,
    ChipType? ActiveChip,
    int? LeagueId,
    string? LeagueName,
    LeagueType? LeagueType
);

public record PickDetailDto(
    int PlayerId,
    string PlayerName,
    string Team,
    PlayerPosition Position,
    decimal Price,
    int SquadPosition,
    bool IsCaptain,
    bool IsViceCaptain,
    int GameweekPoints
);

public record UpdatePicksDto(
    List<PickDto> Picks
);

public record GameweekHistoryDto(
    int GameweekNumber,
    int Points,
    int CumulativePoints,
    int OverallRank
);

/// <summary>
/// Summary entry returned by GET /api/team/mine for the team-switcher UI.
/// </summary>
public record MyTeamSummaryDto(
    int TeamId,
    string TeamName,
    int? LeagueId,           // null = global team
    string? LeagueName,      // "Global" or league name
    LeagueType LeagueContext, // Global / Classic / Draft
    int TotalPoints,
    int GameweekPoints
);
