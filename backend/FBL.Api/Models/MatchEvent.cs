namespace FBL.Api.Models;

public class MatchEvent
{
    public int Id { get; set; }

    public int GameweekId { get; set; }
    public Gameweek Gameweek { get; set; } = null!;

    public int PlayerId { get; set; }
    public BundesligaPlayer Player { get; set; } = null!;

    public EventType EventType { get; set; }

    public int? Minute { get; set; }

    public int Points { get; set; }
}
