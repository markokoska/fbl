namespace FBL.Api.Models;

public class Transfer
{
    public int Id { get; set; }

    public int FantasyTeamId { get; set; }
    public FantasyTeam FantasyTeam { get; set; } = null!;

    public int GameweekId { get; set; }

    public int PlayerInId { get; set; }
    public BundesligaPlayer PlayerIn { get; set; } = null!;

    public int PlayerOutId { get; set; }
    public BundesligaPlayer PlayerOut { get; set; } = null!;

    public decimal PriceIn { get; set; }
    public decimal PriceOut { get; set; }

    public bool IsFreeTransfer { get; set; }

    public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
}
