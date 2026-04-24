namespace FBL.Api.Models;

public class FantasyPick
{
    public int Id { get; set; }

    public int FantasyTeamId { get; set; }
    public FantasyTeam FantasyTeam { get; set; } = null!;

    public int PlayerId { get; set; }
    public BundesligaPlayer Player { get; set; } = null!;

    public int GameweekId { get; set; }
    public Gameweek Gameweek { get; set; } = null!;

    // 1-11 = starting, 12-15 = bench
    public int SquadPosition { get; set; }

    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }
}
