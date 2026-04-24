using System.Security.Claims;
using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Models;
using FBL.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransferController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TransferService _transferService;

    public TransferController(AppDbContext db, TransferService transferService)
    {
        _db = db;
        _transferService = transferService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<ActionResult<TransferResultDto>> MakeTransfer(MakeTransferDto dto)
    {
        var team = await _db.FantasyTeams.FirstOrDefaultAsync(t => t.UserId == UserId);
        if (team == null) return NotFound("Team not found.");

        var currentGw = await _db.Gameweeks
            .Where(g => g.Status == GameweekStatus.Upcoming)
            .OrderBy(g => g.Number)
            .FirstOrDefaultAsync();

        if (currentGw == null) return BadRequest("No upcoming gameweek.");
        if (currentGw.IsLocked) return BadRequest("Transfers are locked. Deadline has passed.");

        var result = await _transferService.MakeTransfer(team.Id, dto, currentGw);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<object>>> GetTransferHistory()
    {
        var team = await _db.FantasyTeams.FirstOrDefaultAsync(t => t.UserId == UserId);
        if (team == null) return NotFound();

        var transfers = await _db.Transfers
            .Where(t => t.FantasyTeamId == team.Id)
            .Include(t => t.PlayerIn)
            .Include(t => t.PlayerOut)
            .OrderByDescending(t => t.TransferredAt)
            .Select(t => new
            {
                t.Id,
                t.GameweekId,
                PlayerIn = new { t.PlayerIn.Name, t.PlayerIn.Team, t.PriceIn },
                PlayerOut = new { t.PlayerOut.Name, t.PlayerOut.Team, t.PriceOut },
                t.IsFreeTransfer,
                t.TransferredAt
            })
            .ToListAsync();

        return Ok(transfers);
    }
}
