namespace FBL.Api.Models;

/// <summary>
/// One row per pick made during the live initial snake draft of a Draft league.
/// The unique index on (LeagueId, PlayerId) enforces single ownership.
/// </summary>
public class DraftPick
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int PlayerId { get; set; }
    public BundesligaPlayer Player { get; set; } = null!;

    /// <summary>1-based round number (1..15 for a standard squad).</summary>
    public int Round { get; set; }

    /// <summary>1-based overall pick number across the whole draft (1 .. 15 * MaxMembers).</summary>
    public int PickNumber { get; set; }

    public DateTime PickedAt { get; set; } = DateTime.UtcNow;

    /// <summary>True if the pick was made by the auto-pick fallback after timer expired.</summary>
    public bool WasAutoPick { get; set; }
}
