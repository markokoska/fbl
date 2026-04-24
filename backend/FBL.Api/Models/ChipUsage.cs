namespace FBL.Api.Models;

public class ChipUsage
{
    public int Id { get; set; }

    public int FantasyTeamId { get; set; }
    public FantasyTeam FantasyTeam { get; set; } = null!;

    public int GameweekId { get; set; }
    public Gameweek Gameweek { get; set; } = null!;

    public ChipType ChipType { get; set; }
}
