using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

public class TransferService
{
    private readonly AppDbContext _db;

    public TransferService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TransferResultDto> MakeTransfer(int fantasyTeamId, MakeTransferDto dto, Gameweek currentGw)
    {
        if (currentGw.IsLocked)
            return new TransferResultDto(false, "Gameweek is locked. Transfers are closed.", 0, 0);

        var team = await _db.FantasyTeams
            .Include(t => t.Picks.Where(p => p.GameweekId == currentGw.Id))
            .ThenInclude(p => p.Player)
            .FirstOrDefaultAsync(t => t.Id == fantasyTeamId);

        if (team == null)
            return new TransferResultDto(false, "Team not found.", 0, 0);

        var playerOut = team.Picks.FirstOrDefault(p => p.PlayerId == dto.PlayerOutId);
        if (playerOut == null)
            return new TransferResultDto(false, "Player out is not in your squad.", 0, 0);

        var playerIn = await _db.BundesligaPlayers.FindAsync(dto.PlayerInId);
        if (playerIn == null)
            return new TransferResultDto(false, "Player in not found.", 0, 0);

        // Check if player is already in squad
        if (team.Picks.Any(p => p.PlayerId == dto.PlayerInId))
            return new TransferResultDto(false, "Player is already in your squad.", team.Budget, team.FreeTransfers);

        // Check max 3 from same team
        var sameTeamCount = team.Picks
            .Where(p => p.PlayerId != dto.PlayerOutId)
            .Count(p => p.Player.Team == playerIn.Team);
        if (sameTeamCount >= 3)
            return new TransferResultDto(false, "Maximum 3 players from the same team.", team.Budget, team.FreeTransfers);

        // Budget check
        var priceOut = playerOut.Player.Price;
        var priceIn = playerIn.Price;
        var newBudget = team.Budget + priceOut - priceIn;
        if (newBudget < 0)
            return new TransferResultDto(false, "Insufficient budget.", team.Budget, team.FreeTransfers);

        // Check if free transfer or -4 deduction
        bool isFree = team.FreeTransfers > 0;
        if (isFree)
            team.FreeTransfers--;
        else
            team.TotalPoints -= 4;

        // Execute transfer
        playerOut.PlayerId = dto.PlayerInId;
        team.Budget = newBudget;

        _db.Transfers.Add(new Transfer
        {
            FantasyTeamId = fantasyTeamId,
            GameweekId = currentGw.Id,
            PlayerInId = dto.PlayerInId,
            PlayerOutId = dto.PlayerOutId,
            PriceIn = priceIn,
            PriceOut = priceOut,
            IsFreeTransfer = isFree
        });

        await _db.SaveChangesAsync();

        return new TransferResultDto(true,
            isFree ? "Transfer complete (free transfer)." : "Transfer complete (-4 point deduction).",
            team.Budget, team.FreeTransfers);
    }
}
