using System.ComponentModel.DataAnnotations;

namespace FBL.Api.Models;

public class FantasyTeam
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public decimal Budget { get; set; } = 100.0m;

    public int FreeTransfers { get; set; } = 1;

    public int TotalPoints { get; set; }

    /// <summary>
    /// Running points for the currently live gameweek. Reset to 0 when a new GW goes live.
    /// Consolidated into TotalPoints when the GW is finalised.
    /// </summary>
    public int GameweekPoints { get; set; }

    public ICollection<FantasyPick> Picks { get; set; } = new List<FantasyPick>();
    public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    public ICollection<ChipUsage> ChipUsages { get; set; } = new List<ChipUsage>();
}
