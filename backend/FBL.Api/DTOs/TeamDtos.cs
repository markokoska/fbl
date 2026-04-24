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
    ChipType? ActiveChip
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
