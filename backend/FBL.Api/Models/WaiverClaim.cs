namespace FBL.Api.Models;

public enum WaiverClaimStatus
{
    Pending,    // queued, not yet processed
    Succeeded,  // processed, transfer applied
    Failed      // processed, target player already taken or other reason
}

/// <summary>
/// A queued waiver claim in a Draft league.
///
/// Lifecycle:
///   1. User queues claims throughout the GW. Each claim says "swap PlayerOut for PlayerIn".
///   2. 24h before the next GW deadline, the system processes the queue:
///        - Order users by reverse standings (worst first).
///        - For each user, process their claims in Priority order (1, 2, 3...).
///        - A claim succeeds if PlayerIn is still unowned in the league at that moment.
///        - On success, the swap is applied to the user's team for the upcoming GW.
///        - On failure (player already taken), the claim is marked Failed and we move on.
///   3. After processing, "free agency" opens: any user can swap freely with remaining unowned players.
/// </summary>
public class WaiverClaim
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    /// <summary>The gameweek this claim is FOR (the upcoming GW it will take effect in).</summary>
    public int GameweekId { get; set; }
    public Gameweek Gameweek { get; set; } = null!;

    public int PlayerOutId { get; set; }
    public BundesligaPlayer PlayerOut { get; set; } = null!;

    public int PlayerInId { get; set; }
    public BundesligaPlayer PlayerIn { get; set; } = null!;

    /// <summary>User's own ordering of their claims (1 = process first).</summary>
    public int Priority { get; set; }

    public WaiverClaimStatus Status { get; set; } = WaiverClaimStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
}
