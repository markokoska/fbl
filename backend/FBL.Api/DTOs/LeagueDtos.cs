using FBL.Api.Models;

namespace FBL.Api.DTOs;

public record CreateLeagueDto(
    string Name,
    LeagueType Type = LeagueType.Classic,
    int MaxMembers = 10
);

public record JoinLeagueDto(string JoinCode);

public record LeagueDto(
    int Id,
    string Name,
    string JoinCode,
    bool IsGlobal,
    LeagueType Type,
    int MemberCount,
    int MaxMembers,
    DraftStatus DraftStatus,
    bool HasMyTeam,
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
