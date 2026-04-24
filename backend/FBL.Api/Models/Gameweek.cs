using System.ComponentModel.DataAnnotations;

namespace FBL.Api.Models;

public class Gameweek
{
    public int Id { get; set; }

    public int Number { get; set; }

    public DateTime KickoffTime { get; set; }

    public DateTime Deadline => KickoffTime.AddMinutes(-90);

    public GameweekStatus Status { get; set; } = GameweekStatus.Upcoming;

    public bool IsLocked => DateTime.UtcNow >= Deadline;

    public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();
    public ICollection<FantasyPick> FantasyPicks { get; set; } = new List<FantasyPick>();
    public ICollection<ChipUsage> ChipUsages { get; set; } = new List<ChipUsage>();
}
