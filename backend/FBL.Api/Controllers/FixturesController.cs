using FBL.Api.Data;
using FBL.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FixturesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FixturesController(AppDbContext db) => _db = db;

    /// <summary>Get all fixtures for a specific gameweek</summary>
    [HttpGet("gameweek/{gwNumber}")]
    public async Task<ActionResult> GetByGameweek(int gwNumber)
    {
        var matches = await _db.Matches
            .Include(m => m.Gameweek)
            .Where(m => m.Gameweek.Number == gwNumber)
            .OrderBy(m => m.Id)
            .Select(m => new
            {
                m.Id,
                GameweekNumber = m.Gameweek.Number,
                m.HomeTeam,
                m.AwayTeam,
                m.HomeGoals,
                m.AwayGoals,
                m.IsFinished
            })
            .ToListAsync();

        return Ok(matches);
    }

    /// <summary>Get all fixtures for a specific team</summary>
    [HttpGet("team")]
    public async Task<ActionResult> GetByTeam([FromQuery] string team)
    {
        if (string.IsNullOrEmpty(team)) return BadRequest("Team is required");

        var matches = await _db.Matches
            .Include(m => m.Gameweek)
            .Where(m => m.HomeTeam == team || m.AwayTeam == team)
            .OrderBy(m => m.Gameweek.Number)
            .Select(m => new
            {
                m.Id,
                GameweekNumber = m.Gameweek.Number,
                m.HomeTeam,
                m.AwayTeam,
                m.HomeGoals,
                m.AwayGoals,
                m.IsFinished
            })
            .ToListAsync();

        return Ok(matches);
    }

    /// <summary>Get Bundesliga standings calculated from match results</summary>
    [HttpGet("standings")]
    public async Task<ActionResult> GetStandings()
    {
        var finishedMatches = await _db.Matches
            .Include(m => m.Gameweek)
            .Where(m => m.IsFinished && m.HomeGoals.HasValue && m.AwayGoals.HasValue)
            .OrderBy(m => m.Gameweek.Number)
            .ToListAsync();

        var teamStats = new Dictionary<string, TeamStanding>();

        foreach (var m in finishedMatches)
        {
            if (!teamStats.ContainsKey(m.HomeTeam))
                teamStats[m.HomeTeam] = new TeamStanding { Team = m.HomeTeam };
            if (!teamStats.ContainsKey(m.AwayTeam))
                teamStats[m.AwayTeam] = new TeamStanding { Team = m.AwayTeam };

            var home = teamStats[m.HomeTeam];
            var away = teamStats[m.AwayTeam];

            home.Played++;
            away.Played++;
            home.GoalsFor += m.HomeGoals!.Value;
            home.GoalsAgainst += m.AwayGoals!.Value;
            away.GoalsFor += m.AwayGoals!.Value;
            away.GoalsAgainst += m.HomeGoals!.Value;

            string homeResult, awayResult;
            if (m.HomeGoals > m.AwayGoals)
            {
                home.Won++; home.Points += 3; away.Lost++;
                homeResult = "W"; awayResult = "L";
            }
            else if (m.HomeGoals < m.AwayGoals)
            {
                away.Won++; away.Points += 3; home.Lost++;
                homeResult = "L"; awayResult = "W";
            }
            else
            {
                home.Drawn++; away.Drawn++; home.Points++; away.Points++;
                homeResult = "D"; awayResult = "D";
            }

            home.Form.Add(homeResult);
            away.Form.Add(awayResult);
        }

        var standings = teamStats.Values
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ThenBy(t => t.Team)
            .Select((t, i) => new
            {
                Position = i + 1,
                t.Team,
                t.Played,
                t.Won,
                t.Drawn,
                t.Lost,
                t.GoalsFor,
                t.GoalsAgainst,
                t.GoalDifference,
                t.Points,
                Form = t.Form.TakeLast(5).ToList()
            })
            .ToList();

        return Ok(standings);
    }

    private class TeamStanding
    {
        public string Team { get; set; } = "";
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points { get; set; }
        public List<string> Form { get; set; } = new();
    }
}
