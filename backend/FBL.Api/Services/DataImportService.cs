using System.Text.Json;
using System.Text.Json.Serialization;
using FBL.Api.Data;
using FBL.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FBL.Api.Services;

public class DataImportService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(AppDbContext db, IHttpClientFactory httpFactory, IConfiguration config, ILogger<DataImportService> logger)
    {
        _db = db;
        _http = httpFactory.CreateClient("FootballData");
        _config = config;
        _logger = logger;
    }

    public async Task<int> ImportTeamsAndPlayers()
    {
        var apiKey = _config["FootballData:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("FootballData API key not configured. Set FootballData:ApiKey in appsettings.");

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);

        // Bundesliga competition code = BL1
        var teamsResponse = await _http.GetAsync("https://api.football-data.org/v4/competitions/BL1/teams");
        teamsResponse.EnsureSuccessStatusCode();

        var teamsJson = await teamsResponse.Content.ReadAsStringAsync();
        var teamsData = JsonSerializer.Deserialize<TeamsResponse>(teamsJson, JsonOpts);

        if (teamsData?.Teams == null) return 0;

        int count = 0;

        foreach (var team in teamsData.Teams)
        {
            var squadResponse = await _http.GetAsync($"https://api.football-data.org/v4/teams/{team.Id}");
            if (!squadResponse.IsSuccessStatusCode) continue;

            var squadJson = await squadResponse.Content.ReadAsStringAsync();
            var squadData = JsonSerializer.Deserialize<TeamDetailResponse>(squadJson, JsonOpts);

            if (squadData?.Squad == null) continue;

            foreach (var player in squadData.Squad)
            {
                var position = MapPosition(player.Position);
                if (position == null) continue;

                var existing = await _db.BundesligaPlayers
                    .FirstOrDefaultAsync(p => p.ExternalId == player.Id);

                if (existing != null)
                {
                    existing.Name = player.Name;
                    existing.Team = team.ShortName ?? team.Name;
                    existing.Position = position.Value;
                }
                else
                {
                    _db.BundesligaPlayers.Add(new BundesligaPlayer
                    {
                        ExternalId = player.Id,
                        Name = player.Name,
                        Team = team.ShortName ?? team.Name,
                        Position = position.Value,
                        Price = EstimatePrice(position.Value),
                    });
                    count++;
                }
            }

            // Respect rate limit (10 req/min on free tier)
            await Task.Delay(6500);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Imported {Count} new players", count);
        return count;
    }

    private static PlayerPosition? MapPosition(string? pos) => pos switch
    {
        "Goalkeeper" => PlayerPosition.GK,
        "Defence" or "Left-Back" or "Right-Back" or "Centre-Back" => PlayerPosition.DEF,
        "Midfield" or "Central Midfield" or "Attacking Midfield" or "Defensive Midfield"
            or "Left Midfield" or "Right Midfield" or "Left Winger" or "Right Winger" => PlayerPosition.MID,
        "Offence" or "Centre-Forward" => PlayerPosition.FWD,
        _ => null
    };

    private static decimal EstimatePrice(PlayerPosition pos) => pos switch
    {
        PlayerPosition.GK => 5.0m,
        PlayerPosition.DEF => 5.0m,
        PlayerPosition.MID => 6.5m,
        PlayerPosition.FWD => 7.0m,
        _ => 5.0m
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // API response models
    private class TeamsResponse
    {
        public List<TeamInfo>? Teams { get; set; }
    }

    private class TeamInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ShortName { get; set; }
    }

    private class TeamDetailResponse
    {
        public List<SquadMember>? Squad { get; set; }
    }

    private class SquadMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Position { get; set; }
    }
}
