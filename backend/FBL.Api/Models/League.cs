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

    public bool IsGlobal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LeagueMember> Members { get; set; } = new List<LeagueMember>();
}
