using System.ComponentModel.DataAnnotations;

namespace FBL.Api.Models;

public class BundesligaPlayer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Team { get; set; } = string.Empty;

    public PlayerPosition Position { get; set; }

    public decimal Price { get; set; } = 5.0m;

    public int TotalPoints { get; set; }

    // Season aggregate stats
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int CleanSheets { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int PenaltiesMissed { get; set; }
    public int PenaltySaves { get; set; }
    public int OwnGoals { get; set; }
    public int BonusPoints { get; set; }
    public int MatchesPlayed { get; set; }

    // Advanced stats
    public double ExpectedGoals { get; set; }   // xG
    public double ExpectedAssists { get; set; }  // xA

    // Fitness: 0 = injured (red), 25 = doubtful (orange), 50 = questionable (yellow), 75 = minor knock (yellow-green), 100 = fit (green)
    public int Fitness { get; set; } = 100;

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public int ExternalId { get; set; }

    public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();
    public ICollection<FantasyPick> FantasyPicks { get; set; } = new List<FantasyPick>();
}
