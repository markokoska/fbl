using System.ComponentModel.DataAnnotations;

namespace FBL.Api.Models;

public class League
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string JoinCode { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;
    public AppUser CreatedBy { get; set; } = null!;

    /// <summary>Kept for migration backwards-compat. New code should use Type.</summary>
    public bool IsGlobal { get; set; }

    public LeagueType Type { get; set; } = LeagueType.Classic;

    // ---- Draft-specific settings (only meaningful when Type == Draft) ----
    public int MaxMembers { get; set; } = 10;
    public DraftStatus DraftStatus { get; set; } = DraftStatus.Pending;
    public DateTime? DraftStartTime { get; set; }
    public int DraftPickSeconds { get; set; } = 60;
    public int CurrentPickNumber { get; set; }     // 1-based; 0 before draft starts
    public DateTime? CurrentPickDeadline { get; set; }

    /// <summary>The most recent GW for which waivers have been processed.</summary>
    public int? LastWaiverProcessedGameweekId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LeagueMember> Members { get; set; } = new List<LeagueMember>();
    public ICollection<FantasyTeam> Teams { get; set; } = new List<FantasyTeam>();
    public ICollection<DraftPick> DraftPicks { get; set; } = new List<DraftPick>();
}
