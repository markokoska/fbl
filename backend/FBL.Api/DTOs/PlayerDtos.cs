using FBL.Api.Models;

namespace FBL.Api.DTOs;

public record PlayerDto(
    int Id,
    string Name,
    string Team,
    PlayerPosition Position,
    decimal Price,
    int TotalPoints,
    string? PhotoUrl,
    int Fitness,
    PlayerStatsDto Stats
);

public record PlayerDetailDto(
    int Id,
    string Name,
    string Team,
    PlayerPosition Position,
    decimal Price,
    int TotalPoints,
    string? PhotoUrl,
    int Fitness,
    PlayerStatsDto Stats,
    List<GameweekPointsDto> GameweekHistory,
    double OwnershipPercent
);

/// <summary>
/// Season aggregate stats for a player.
/// </summary>
public record PlayerStatsDto(
    int MatchesPlayed,
    int Goals,
    int Assists,
    int CleanSheets,
    int YellowCards,
    int RedCards,
    int PenaltiesMissed,
    int PenaltySaves,
    int OwnGoals,
    int BonusPoints,
    double ExpectedGoals,
    double ExpectedAssists
);

/// <summary>
/// Per-gameweek breakdown with individual stat counts.
/// </summary>
public record GameweekPointsDto(
    int GameweekNumber,
    int Points,
    GameweekStatsDto Stats,
    List<string> Events
);

/// <summary>
/// Stats for a single gameweek.
/// </summary>
public record GameweekStatsDto(
    int MinutesPlayed,
    int Goals,
    int Assists,
    int CleanSheets,
    int YellowCards,
    int RedCards,
    int PenaltiesMissed,
    int PenaltySaves,
    int OwnGoals,
    int BonusPoints
);
