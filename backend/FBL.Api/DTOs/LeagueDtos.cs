namespace FBL.Api.DTOs;

public record CreateLeagueDto(string Name);

public record JoinLeagueDto(string JoinCode);

public record LeagueDto(
    int Id,
    string Name,
    string JoinCode,
    bool IsGlobal,
    List<LeagueStandingDto> Standings
);

public record LeagueStandingDto(
    int Rank,
    string UserId,
    string DisplayName,
    string TeamName,
    int TotalPoints,
    int GameweekPoints
);
