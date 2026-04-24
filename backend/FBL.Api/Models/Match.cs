using System.ComponentModel.DataAnnotations;

namespace FBL.Api.Models;

public class Match
{
    public int Id { get; set; }

    public int GameweekId { get; set; }
    public Gameweek Gameweek { get; set; } = null!;

    [Required, MaxLength(100)]
    public string HomeTeam { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string AwayTeam { get; set; } = string.Empty;

    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }

    public bool IsFinished { get; set; }
}
