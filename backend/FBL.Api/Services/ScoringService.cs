using FBL.Api.Models;

namespace FBL.Api.Services;

public class ScoringService
{
    public int CalculatePoints(EventType eventType, PlayerPosition position)
    {
        return eventType switch
        {
            EventType.MinutesPlayed => 2, // We'll use 2 for >=60min, caller handles <60
            EventType.Goal => position switch
            {
                PlayerPosition.GK => 6,
                PlayerPosition.DEF => 6,
                PlayerPosition.MID => 5,
                PlayerPosition.FWD => 4,
                _ => 0
            },
            EventType.Assist => 3,
            EventType.CleanSheet => position switch
            {
                PlayerPosition.GK => 4,
                PlayerPosition.DEF => 4,
                PlayerPosition.MID => 1,
                PlayerPosition.FWD => 0,
                _ => 0
            },
            EventType.YellowCard => -1,
            EventType.RedCard => -3,
            EventType.PenaltySave => 5,
            EventType.PenaltyMiss => -2,
            EventType.OwnGoal => -2,
            EventType.Bonus => 0, // Bonus points are set directly (1, 2, or 3)
            EventType.SavesMade => 0, // Every 3 saves = 1 point, handled separately
            _ => 0
        };
    }

    public int CalculateMinutesPoints(int minutesPlayed)
    {
        if (minutesPlayed >= 60) return 2;
        if (minutesPlayed > 0) return 1;
        return 0;
    }
}
