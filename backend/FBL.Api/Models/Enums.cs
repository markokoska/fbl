namespace FBL.Api.Models;

public enum PlayerPosition
{
    GK = 1,
    DEF = 2,
    MID = 3,
    FWD = 4
}

public enum EventType
{
    MinutesPlayed,
    Goal,
    Assist,
    CleanSheet,
    YellowCard,
    RedCard,
    PenaltySave,
    PenaltyMiss,
    OwnGoal,
    Bonus,
    SavesMade
}

public enum GameweekStatus
{
    Upcoming,
    Live,
    Finished
}

public enum ChipType
{
    Wildcard,
    BenchBoost,
    TripleCaptain,
    FreeHit
}

public enum LeagueType
{
    Global,
    Classic,
    Draft
}

public enum DraftStatus
{
    Pending,    // accepting members, draft hasn't started
    InProgress, // live draft happening now
    Completed   // draft done, season running
}
