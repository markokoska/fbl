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
public class DraftController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DraftService _draft;

    public DraftController(AppDbContext db, DraftService draft)
    {
        _db = db;
        _draft = draft;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<bool> IsMember(int leagueId)
        => await _db.LeagueMembers.AnyAsync(m => m.LeagueId == leagueId && m.UserId == UserId);

    [HttpGet("{leagueId}/state")]
    public async Task<ActionResult<DraftStateDto>> GetState(int leagueId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try
        {
            return await _draft.GetState(leagueId, UserId);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{leagueId}/available")]
    public async Task<ActionResult<List<AvailablePlayerDto>>> GetAvailable(int leagueId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        return await _draft.GetAvailablePlayers(leagueId);
    }

    [HttpPost("{leagueId}/start")]
    public async Task<ActionResult<DraftStateDto>> Start(int leagueId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try
        {
            return await _draft.StartDraft(leagueId, UserId);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{leagueId}/pick")]
    public async Task<ActionResult<DraftPickDto>> Pick(int leagueId, [FromBody] MakePickDto dto)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try
        {
            return await _draft.MakePick(leagueId, UserId, dto.PlayerId);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{leagueId}/reset")]
    public async Task<ActionResult<DraftStateDto>> Reset(int leagueId)
    {
        if (!await IsMember(leagueId)) return Forbid();
        try
        {
            return await _draft.ResetDraft(leagueId, UserId);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
