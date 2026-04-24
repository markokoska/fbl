using FBL.Api.Models;

namespace FBL.Api.DTOs;

public record AddMatchEventDto(
    int GameweekId,
    int PlayerId,
    EventType EventType,
    int? Minute
);

public record BatchMatchEventDto(
    int GameweekId,
    List<MatchEventItemDto> Events
);

public record MatchEventItemDto(
    int PlayerId,
    EventType EventType,
    int? Minute
);

public record GameweekDto(
    int Id,
    int Number,
    DateTime KickoffTime,
    DateTime Deadline,
    GameweekStatus Status,
    bool IsLocked
);

public record UpdateGameweekDto(
    DateTime? KickoffTime,
    GameweekStatus? Status
);

public record BulkPlayerDto(
    string Name,
    string Team,
    PlayerPosition Position,
    int MatchesPlayed,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards
);

public record UpdatePlayerPriceDto(decimal Price);
public record UpdatePlayerFitnessDto(int Fitness);
public record UpdateMatchDto(int? HomeGoals, int? AwayGoals, bool IsFinished);
