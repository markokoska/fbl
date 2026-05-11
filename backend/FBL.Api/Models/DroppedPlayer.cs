namespace FBL.Api.Models;

/// <summary>
/// Records a player being dropped from a draft league's roster during a waiver
/// or free-agency swap. The player stays "locked" — unavailable to be picked
/// back up by anyone in this league — until the gameweek they were dropped
/// for has passed (i.e. when the next GW becomes the upcoming one).
/// </summary>
public class DroppedPlayer
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public int PlayerId { get; set; }
    public BundesligaPlayer Player { get; set; } = null!;

    /// <summary>The upcoming GW at the time of the drop. Lock applies for this GW only.</summary>
    public int GameweekId { get; set; }
    public Gameweek Gameweek { get; set; } = null!;

    public string DroppedByUserId { get; set; } = string.Empty;
    public AppUser DroppedBy { get; set; } = null!;

    public DateTime DroppedAt { get; set; } = DateTime.UtcNow;
}
