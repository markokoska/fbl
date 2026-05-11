using System.Security.Claims;
using FBL.Api.Data;
using FBL.Api.DTOs;
using FBL.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WaiverController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly WaiverService _waivers;

    public WaiverController(AppDbContext db, WaiverService waivers)
    {
        _db = db;
        _waivers = waivers;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<bool> IsMember(int leagueId)
        => await _db.LeagueMembers.AnyAsync(m => m.LeagueId == leagueId && m.UserId == UserId);

    [HttpGet("{leagueId}/state")]
    public async Task<ActionResult<WaiverStateDto>> GetState(int leagueId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try { return await _waivers.GetState(leagueId, UserId); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{leagueId}/free-agents")]
    public async Task<ActionResult<List<FreeAgentDto>>> GetFreeAgents(int leagueId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        return await _waivers.GetFreeAgents(leagueId);
    }

    [HttpPost("{leagueId}/claims")]
    public async Task<ActionResult<WaiverClaimDto>> AddClaim(int leagueId, [FromBody] AddWaiverClaimDto dto)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try { return await _waivers.AddClaim(leagueId, UserId, dto.PlayerOutId, dto.PlayerInId); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{leagueId}/claims/{claimId}")]
    public async Task<ActionResult> RemoveClaim(int leagueId, int claimId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try { await _waivers.RemoveClaim(leagueId, UserId, claimId); return Ok(); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{leagueId}/claims/reorder")]
    public async Task<ActionResult> Reorder(int leagueId, [FromBody] ReorderClaimsDto dto)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try { await _waivers.ReorderClaims(leagueId, UserId, dto.ClaimIdsInOrder); return Ok(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{leagueId}/swap")]
    public async Task<ActionResult> DirectSwap(int leagueId, [FromBody] DirectSwapDto dto)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try { await _waivers.DirectSwap(leagueId, UserId, dto.PlayerOutId, dto.PlayerInId); return Ok(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Manual processing trigger — useful for admin testing without waiting for the 24h window.</summary>
    [HttpPost("{leagueId}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Process(int leagueId)
    {
        await _waivers.ProcessLeagueWaivers(leagueId);
        return Ok();
    }
}
