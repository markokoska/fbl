# Fantasy Bundesliga — Code Walkthrough (start to end)

This document walks through every major flow in the app, from the moment the .NET process starts up to the moment a user sees scores update in real time. Read it sequentially — each section builds on the previous.

**Conventions used**:
- File paths are relative to the repo root (e.g. `backend/FBL.Api/Program.cs`)
- Code snippets are abbreviated; consult the actual file for the full version
- "User A", "User B" refer to two test accounts in different browser tabs

---

## 0. Project layout (10-second tour)

```
fbl/
├── backend/FBL.Api/
│   ├── Program.cs                  ← startup: services, middleware, migrations
│   ├── Controllers/                ← REST endpoints (one file per entity)
│   ├── Services/                   ← business logic (DraftService, WaiverService, …)
│   ├── Hubs/                       ← SignalR hubs (LiveScoreHub, DraftHub)
│   ├── Models/                     ← EF Core entities
│   ├── DTOs/                       ← request/response shapes
│   ├── Data/AppDbContext.cs        ← EF Core configuration
│   └── Migrations/                 ← auto-generated DB migrations
├── frontend/src/
│   ├── main.tsx + App.tsx          ← entry + routing
│   ├── pages/                      ← one file per route
│   ├── components/                 ← reusable UI (PitchView, TeamSelector, icons)
│   ├── api/client.ts + types.ts    ← axios instance + shared types
│   ├── context/AuthContext.tsx     ← user state + login/logout
│   └── hooks/useLeagueId.ts        ← URL param helper
├── Dockerfile                      ← multi-stage build (frontend + backend → 1 image)
└── docs/                           ← thesis + diagrams + this file
```

---

## 1. App startup — what `Program.cs` does

When the .NET process starts (locally via `dotnet run`, or in production when Render fires up the container), `Program.cs` runs top-to-bottom and configures everything before the first HTTP request is served.

**Step-by-step:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Read PORT env var (Render sets this)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://+:{port}");

// 2. Wire up DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configure ASP.NET Core Identity (users + roles)
builder.Services.AddIdentity<AppUser, IdentityRole>(...)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 4. Configure JWT bearer authentication
var jwtKey = builder.Configuration["Jwt:Key"];  // throws if not set
builder.Services.AddAuthentication(...).AddJwtBearer(options => { ... });

// 5. Register services + background workers
builder.Services.AddScoped<DraftService>();
builder.Services.AddScoped<WaiverService>();
builder.Services.AddHostedService<DraftAutoPickService>();
builder.Services.AddHostedService<WaiverProcessingService>();
// ... plus TransferService, LeaderboardService, ScoringService, TokenService

// 6. SignalR + controllers
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

// 7. Run pending migrations + seed roles
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // ← critical: every push to Render runs this
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new("Admin"));
    if (!await roleManager.RoleExistsAsync("Player")) await roleManager.CreateAsync(new("Player"));
}

// 8. Wire middleware pipeline (order matters!)
app.UseDefaultFiles();           // serves index.html for /
app.UseStaticFiles();            // serves React build from wwwroot
app.UseCors("AllowFrontend");
app.UseAuthentication();         // populates HttpContext.User from JWT
app.UseAuthorization();          // enforces [Authorize] attributes
app.MapControllers();
app.MapHub<LiveScoreHub>("/hubs/livescore");
app.MapHub<DraftHub>("/hubs/draft");
app.MapFallbackToFile("index.html");  // SPA fallback for client routes

app.Run();
```

**Key observations:**
- The `db.Database.Migrate()` call means *the database is always in sync with the code*. Push a new migration to `main`, Render redeploys, and the production DB schema updates automatically.
- The `MapFallbackToFile("index.html")` is critical for the SPA — when a user refreshes `/my-team?leagueId=5`, the request goes to .NET (not Vite), and without this fallback the user would get a 404.

---

## 2. Authentication flow (Registration → JWT)

### 2.1 Registration

User submits the form on `/register`. Frontend (`Register.tsx`) calls `auth.register(email, displayName, password)`, which hits `POST /api/auth/register`.

In `AuthController`:
1. Identity's `UserManager.CreateAsync(user, password)` hashes the password and inserts into `AspNetUsers`.
2. The user is added to the "Player" role.
3. A JWT is generated by `TokenService.CreateToken(user)` containing claims: `sub` (UserId), `name` (DisplayName), `role` (Player or Admin).
4. The JWT is signed with HMAC-SHA256 using `Jwt:Key` from config.
5. The response: `{ token, refreshToken, userId, displayName, email, roles }`.

### 2.2 Login

Same flow but uses `SignInManager.CheckPasswordSignInAsync` instead of creating the user. Returns the same shape of token.

### 2.3 Frontend: where the token lives

`AuthContext.tsx` calls `handleAuth(data)` which writes to `sessionStorage`:
```typescript
sessionStorage.setItem('fbl_token', data.token);
sessionStorage.setItem('fbl_user', JSON.stringify(u));
```

**Why sessionStorage and not localStorage?** sessionStorage is *per-tab*. This lets you log in as different users in different tabs of the same browser — invaluable for testing draft/league flows where you need 2+ users at once.

### 2.4 Authenticated request flow

`api/client.ts` creates an axios instance with a request interceptor:
```typescript
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('fbl_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

So every request automatically includes `Authorization: Bearer eyJ...`. On the backend:
1. The `JwtBearer` middleware validates the signature and expiration.
2. It populates `HttpContext.User` with claims.
3. Controllers read the user via `User.FindFirstValue(ClaimTypes.NameIdentifier)` to get the UserId.
4. `[Authorize]` attribute rejects unauthenticated requests with 401.

If the token is invalid/expired, axios's response interceptor catches the 401 and clears `sessionStorage`, redirecting to `/login`.

---

## 3. Three game modes — how one model supports all three

The architectural cornerstone of the project is the **nullable `LeagueId`** on `FantasyTeam`:

```csharp
public class FantasyTeam {
    public int Id { get; set; }
    public string UserId { get; set; }
    public int? LeagueId { get; set; }   // ← null = global team
    // ...
}
```

In `AppDbContext`:
```csharp
builder.Entity<FantasyTeam>()
    .HasIndex(t => new { t.UserId, t.LeagueId })
    .IsUnique();
```

This composite unique index means: a user can have **at most one team per league**, *and* at most one global team (where `LeagueId` is null). The same user can therefore have:
- 1 global team (LeagueId = null)
- 1 team for each Classic league they joined
- 1 team for each Draft league they joined (auto-created when draft completes)

Every team-related endpoint accepts an optional `?leagueId=` query parameter:
```csharp
[HttpGet]
public async Task<ActionResult<TeamDto>> GetMyTeam([FromQuery] int? leagueId = null)
{
    var team = await ResolveTeam(UserId, leagueId, includeChips: true);
    if (team == null) return NotFound();
    return await BuildCurrentTeamDto(team, currentGw);
}

private async Task<FantasyTeam?> ResolveTeam(string userId, int? leagueId, bool includeChips)
{
    if (leagueId == null)
        return await _db.FantasyTeams.FirstOrDefaultAsync(t =>
            t.UserId == userId && t.LeagueId == null);
    return await _db.FantasyTeams.FirstOrDefaultAsync(t =>
        t.UserId == userId && t.LeagueId == leagueId.Value);
}
```

The frontend mirrors this: `useLeagueId()` reads `?leagueId=` from the URL, and `leagueQuery(leagueId)` builds the query string for API calls. The `TeamSelector` component switches contexts by updating `?leagueId=` in the URL.

**Bottom line**: the same controller code handles global teams, classic-league teams, and draft-league teams without branching on game mode.

---

## 4. Building a squad (initial 15-player pick)

### 4.1 Frontend (`Transfers.tsx`)

When a user has no team yet (404 from `/api/team`), the page renders a "Pick Your Squad" UI:
- Filterable player table (search, position, club, sort by points/price)
- Persistent map of selected players (`selectedMap: Record<number, Player>`)
- Real-time validation: position counts, total cost, max-3-per-club

When the user clicks **Create Team**:
1. Frontend assembles 15 picks with squad positions 1-15 (1 GK + 4 DEF + 4 MID + 2 FWD as starters, rest on bench).
2. Captain = highest-totalPoints starter, Vice = second highest (auto-assigned).
3. POST to `/api/team?leagueId=X` (or no leagueId for global).

### 4.2 Backend (`TeamController.CreateTeam`)

```csharp
[HttpPost]
public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto dto, [FromQuery] int? leagueId = null)
{
    // 1. Validate league context
    if (leagueId != null) {
        var league = await _db.Leagues.FindAsync(leagueId.Value);
        if (league?.Type == LeagueType.Draft)
            return BadRequest("Draft teams are auto-created by the draft.");
        if (!await IsMember(leagueId.Value, UserId))
            return BadRequest("You are not a member of this league.");
    }

    // 2. Reject duplicates
    var existing = await ResolveTeam(UserId, leagueId);
    if (existing != null)
        return BadRequest(leagueId == null ? "You already have a global team."
                                           : "You already have a team for this league.");

    // 3. Validate squad rules
    if (dto.Picks.Count != 15) return BadRequest("Must pick exactly 15 players.");

    var players = await _db.BundesligaPlayers
        .Where(p => dto.Picks.Select(x => x.PlayerId).Contains(p.Id))
        .ToListAsync();

    if (players.Sum(p => p.Price) > 100m) return BadRequest("Over budget.");
    if (players.GroupBy(p => p.Team).Any(g => g.Count() > 3))
        return BadRequest("Max 3 players from the same team.");
    // … 2/5/5/3 position split, 1 captain, 1 vice
    
    // 4. Insert team + picks (transactionally)
    var team = new FantasyTeam {
        UserId = UserId, LeagueId = leagueId, Name = dto.TeamName,
        Budget = 100m - totalCost, FreeTransfers = 1
    };
    _db.FantasyTeams.Add(team);
    await _db.SaveChangesAsync();
    
    foreach (var pick in dto.Picks)
        _db.FantasyPicks.Add(new FantasyPick {
            FantasyTeamId = team.Id, PlayerId = pick.PlayerId,
            GameweekId = currentGw.Id, SquadPosition = pick.SquadPosition,
            IsCaptain = pick.IsCaptain, IsViceCaptain = pick.IsViceCaptain
        });
    await _db.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetMyTeam), null);
}
```

**The picks are tied to the *current* gameweek**. Every gameweek has its own snapshot of what each team looked like — that's how we can show historical "your team in GW12" views.

---

## 5. Viewing & managing your team

When the user navigates to `/my-team?leagueId=5`:

### 5.1 Frontend

`MyTeam.tsx` reads `leagueId` from the URL, then calls in parallel:
- `GET /api/team?leagueId=5` → current team
- `GET /api/gameweek/current` → countdown info
- `GET /api/team/history?leagueId=5` → past gameweeks for the history table

It renders a `<PitchView>` with the team's 15 picks. Clicking a player opens an action menu (Info / Swap / Captain / Vice). For draft teams, the Captain/Vice options are hidden (`isDraftTeam` prop).

When the user does a swap or sets a captain, `PitchView.savePicks()` calls:
```typescript
await api.put(`/team/picks${leagueQuery(leagueId)}`, { picks: newPicks });
```

### 5.2 Backend (`TeamController.UpdatePicks`)

```csharp
[HttpPut("picks")]
public async Task<ActionResult> UpdatePicks(UpdatePicksDto dto, [FromQuery] int? leagueId = null)
{
    var currentGw = await GetCurrentGameweek();
    if (currentGw.IsLocked) return BadRequest("Gameweek is locked.");
    
    var team = await ResolveTeam(UserId, leagueId);
    bool isDraftTeam = await IsDraftLeague(team.LeagueId);
    
    await EnsurePicksForGameweek(team.Id, currentGw.Id);
    
    var picks = await _db.FantasyPicks
        .Where(p => p.FantasyTeamId == team.Id && p.GameweekId == currentGw.Id)
        .ToListAsync();
    
    foreach (var pick in picks) {
        var update = dto.Picks.First(p => p.PlayerId == pick.PlayerId);
        pick.SquadPosition = update.SquadPosition;
        pick.IsCaptain = isDraftTeam ? false : update.IsCaptain;       // forced off for draft
        pick.IsViceCaptain = isDraftTeam ? false : update.IsViceCaptain;
    }
    await _db.SaveChangesAsync();
    return Ok();
}
```

Note **`EnsurePicksForGameweek`**: if the upcoming gameweek doesn't have picks yet for this team (because the team was created during a previous GW), copy the most recent picks forward. This is critical — without it, swaps/transfers would silently fail for newly-created teams.

---

## 6. Transfers (classic mode)

`TransferController.MakeTransfer` is straightforward but has 7 distinct validations:

```csharp
public async Task<TransferResultDto> MakeTransfer(int fantasyTeamId, MakeTransferDto dto, Gameweek currentGw)
{
    if (currentGw.IsLocked) return Fail("Gameweek is locked.");
    
    await EnsurePicksForGameweek(fantasyTeamId, currentGw.Id);
    
    var team = await _db.FantasyTeams
        .Include(t => t.Picks.Where(p => p.GameweekId == currentGw.Id))
        .ThenInclude(p => p.Player)
        .FirstOrDefaultAsync(t => t.Id == fantasyTeamId);
    
    var playerOut = team.Picks.FirstOrDefault(p => p.PlayerId == dto.PlayerOutId);
    if (playerOut == null) return Fail("Player out is not in your squad.");
    
    var playerIn = await _db.BundesligaPlayers.FindAsync(dto.PlayerInId);
    if (playerIn == null) return Fail("Player in not found.");
    if (team.Picks.Any(p => p.PlayerId == dto.PlayerInId)) return Fail("Already in squad.");
    
    // Max 3 from same team, EXCLUDING the outgoing one
    var sameTeamCount = team.Picks
        .Where(p => p.PlayerId != dto.PlayerOutId)
        .Count(p => p.Player.Team == playerIn.Team);
    if (sameTeamCount >= 3) return Fail("Max 3 players from the same team.");
    
    // Budget check using current cost basis
    var newBudget = team.Budget + playerOut.Player.Price - playerIn.Price;
    if (newBudget < 0) return Fail("Insufficient budget.");
    
    // Free transfer or -4 hit?
    bool isFree = team.FreeTransfers > 0;
    if (isFree) team.FreeTransfers--;
    else team.TotalPoints -= 4;
    
    // Apply: just swap the PlayerId on the existing FantasyPick row (preserves SquadPosition + Captain flags)
    playerOut.PlayerId = dto.PlayerInId;
    team.Budget = newBudget;
    
    _db.Transfers.Add(new Transfer { ... });    // history record
    await _db.SaveChangesAsync();
    
    return Success("...", team.Budget, team.FreeTransfers);
}
```

The transfer endpoint blocks draft teams: those use waivers, not transfers.

---

## 7. Leagues — create, join, list

Three endpoints in `LeagueController`:

### 7.1 Create

```csharp
[HttpPost]
public async Task<ActionResult<LeagueDto>> CreateLeague(CreateLeagueDto dto)
{
    var league = new League {
        Name = dto.Name,
        JoinCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),  // e.g. "A3F7B91C"
        CreatedByUserId = UserId,
        Type = dto.Type,
        MaxMembers = dto.Type == LeagueType.Draft ? dto.MaxMembers : 0,
        DraftStatus = dto.Type == LeagueType.Draft ? DraftStatus.Pending : DraftStatus.Completed,
    };
    _db.Leagues.Add(league);
    await _db.SaveChangesAsync();
    
    // Auto-join creator
    _db.LeagueMembers.Add(new LeagueMember { LeagueId = league.Id, UserId = UserId });
    await _db.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetLeague), new { id = league.Id }, ToDto(league));
}
```

### 7.2 Join via code

```csharp
[HttpPost("join")]
public async Task<ActionResult> JoinLeague(JoinLeagueDto dto)
{
    var league = await _db.Leagues.FirstOrDefaultAsync(l => l.JoinCode == dto.JoinCode);
    if (league == null) return NotFound("Invalid join code.");
    
    if (await _db.LeagueMembers.AnyAsync(lm => lm.LeagueId == league.Id && lm.UserId == UserId))
        return BadRequest("Already a member.");
    
    if (league.Type == LeagueType.Draft) {
        if (league.DraftStatus != DraftStatus.Pending)
            return BadRequest("Draft has already started.");
        var memberCount = await _db.LeagueMembers.CountAsync(lm => lm.LeagueId == league.Id);
        if (memberCount >= league.MaxMembers) return BadRequest("League is full.");
    }
    
    _db.LeagueMembers.Add(new LeagueMember { LeagueId = league.Id, UserId = UserId });
    await _db.SaveChangesAsync();
    return Ok();
}
```

### 7.3 League standings

`LeaderboardService.GetLeagueStandings(leagueId)` has two branches:

**Draft leagues**: only count teams explicitly created for this league.
```csharp
var draftTeams = await _db.FantasyTeams
    .Where(t => t.LeagueId == leagueId)
    .Include(t => t.User)
    .OrderByDescending(t => t.TotalPoints + t.GameweekPoints)
    .ToListAsync();
```

**Classic leagues**: prefer the per-league team if it exists, otherwise fall back to the member's global team. This handles legacy classic leagues from before per-league teams existed:
```csharp
var leagueSpecific = await _db.FantasyTeams
    .Where(t => userIds.Contains(t.UserId) && t.LeagueId == leagueId).ToDictionaryAsync(t => t.UserId);
var globalTeams = await _db.FantasyTeams
    .Where(t => userIds.Contains(t.UserId) && t.LeagueId == null).ToDictionaryAsync(t => t.UserId);

// pick whichever exists for each member
var team = leagueSpecific.GetValueOrDefault(uid) ?? globalTeams.GetValueOrDefault(uid);
```

---

## 8. Live snake draft — the big one

This is the most complex flow in the app. Three components work together:
1. **`DraftService`** — pure logic
2. **`DraftHub`** — SignalR broadcasts
3. **`DraftAutoPickService`** — background timer worker

### 8.1 Lobby

When a user enters `/draft/5`, `Draft.tsx` opens a SignalR connection:
```typescript
const conn = new signalR.HubConnectionBuilder()
    .withUrl(`/hubs/draft?access_token=${token}`)   // token in query because WebSocket can't set headers
    .withAutomaticReconnect()
    .build();

conn.on('DraftStarted', () => refresh());
conn.on('PickMade', (b) => { /* optimistic update + refresh */ });
conn.on('DraftCompleted', () => refresh());
conn.on('DraftReset', () => refresh());

await conn.start();
await conn.invoke('JoinDraft', leagueId);
```

`JoinDraft(leagueId)` on the hub adds the connection to the SignalR group `draft-{leagueId}`. All future broadcasts to that group go to every connected client.

In `Pending` status, the page renders a member list with empty slot placeholders. The creator sees a **Start Draft** button.

### 8.2 Starting the draft

```csharp
public async Task<DraftStateDto> StartDraft(int leagueId, string userId)
{
    var league = await LoadLeagueOrThrow(leagueId);
    if (league.CreatedByUserId != userId) throw new UnauthorizedAccessException(...);
    if (league.DraftStatus != DraftStatus.Pending) throw new InvalidOperationException(...);
    
    var memberCount = await _db.LeagueMembers.CountAsync(m => m.LeagueId == leagueId);
    if (memberCount < 2) throw new InvalidOperationException("Need ≥2 members.");
    
    league.DraftStatus = DraftStatus.InProgress;
    league.CurrentPickNumber = 1;
    league.CurrentPickDeadline = DateTime.UtcNow.AddSeconds(league.DraftPickSeconds);
    await _db.SaveChangesAsync();
    
    var state = await GetState(leagueId, userId);
    await _hub.Clients.Group($"draft-{leagueId}").SendAsync("DraftStarted", state);
    return state;
}
```

All connected clients receive `DraftStarted` and immediately re-render — they all flip to the "in progress" view simultaneously.

### 8.3 Snake order math

Given pick number P (1-based) and N members ordered by `JoinedAt`:

```csharp
public static int GetPickerIndex(int pickNumber, int memberCount)
{
    int round = ((pickNumber - 1) / memberCount) + 1;       // 1, 2, 3, …
    int posInRound = ((pickNumber - 1) % memberCount) + 1;  // 1..N
    return (round % 2 == 1) ? posInRound - 1                // odd round: forward
                            : memberCount - posInRound;     // even round: reverse
}
```

For 3 members:
- Pick 1 → idx 0 (round 1, pos 1)
- Pick 2 → idx 1 (round 1, pos 2)
- Pick 3 → idx 2 (round 1, pos 3)
- Pick 4 → idx 2 (round 2, pos 1, reversed)
- Pick 5 → idx 1 (round 2, pos 2, reversed)
- Pick 6 → idx 0 (round 2, pos 3, reversed)
- Pick 7 → idx 0 (round 3 forward again)
- ...

### 8.4 Making a pick

When user clicks "Pick", frontend calls `POST /api/draft/5/pick` with `{ playerId: 42 }`. The server:

```csharp
private async Task<DraftPickDto> ExecutePick(int leagueId, string userId, int playerId, bool isAutoPick)
{
    var league = await LoadLeagueOrThrow(leagueId);
    var members = await GetOrderedMembers(leagueId);
    
    // 1. Verify it's this user's turn
    int idx = GetPickerIndex(league.CurrentPickNumber, members.Count);
    if (members[idx].UserId != userId) throw new UnauthorizedAccessException("Not your turn.");
    
    // 2. Verify player isn't already drafted in this league
    bool alreadyDrafted = await _db.DraftPicks
        .AnyAsync(p => p.LeagueId == leagueId && p.PlayerId == playerId);
    if (alreadyDrafted) throw new InvalidOperationException("Already drafted.");
    
    // 3. Position cap check
    var myPicks = await _db.DraftPicks
        .Where(p => p.LeagueId == leagueId && p.UserId == userId)
        .Include(p => p.Player).ToListAsync();
    int existingAtPos = myPicks.Count(p => p.Player.Position == player.Position);
    if (existingAtPos >= SquadCaps[player.Position])
        throw new InvalidOperationException($"Already have {existingAtPos} {player.Position}s.");
    
    // 4. Persist
    _db.DraftPicks.Add(new DraftPick {
        LeagueId = leagueId, UserId = userId, PlayerId = playerId,
        Round = GetRound(league.CurrentPickNumber, members.Count),
        PickNumber = league.CurrentPickNumber,
        WasAutoPick = isAutoPick
    });
    
    // 5. Advance the clock
    bool isFinalPick = league.CurrentPickNumber == SquadSize * members.Count;  // 15 * N
    league.CurrentPickNumber++;
    league.CurrentPickDeadline = isFinalPick ? null
                                              : DateTime.UtcNow.AddSeconds(league.DraftPickSeconds);
    if (isFinalPick) league.DraftStatus = DraftStatus.Completed;
    await _db.SaveChangesAsync();
    
    // 6. Broadcast
    var pickDto = new DraftPickDto(...);
    await _hub.Clients.Group($"draft-{leagueId}").SendAsync("PickMade", new PickBroadcastDto(...));
    
    // 7. If final pick: create FantasyTeams
    if (isFinalPick) {
        await FinalizeDraft(leagueId);
        await _hub.Clients.Group($"draft-{leagueId}").SendAsync("DraftCompleted", new { leagueId });
    }
    return pickDto;
}
```

Notice the **two-layer ownership check**:
1. Application-layer (step 2): `await _db.DraftPicks.AnyAsync(...)`.
2. **Database-layer**: in `AppDbContext`, the unique index `(LeagueId, PlayerId)` on `DraftPicks` provides a hard guarantee. Even if two simultaneous requests both pass the app-layer check, only one INSERT will succeed; the other gets a unique-constraint violation.

### 8.5 Auto-pick on timeout

`DraftAutoPickService` is a `BackgroundService` that runs every 2 seconds while there's an active draft, every 30 seconds when idle:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        TimeSpan nextDelay = IdleInterval;  // 30s
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        bool anyActive = await db.Leagues.AnyAsync(l =>
            l.Type == LeagueType.Draft && l.DraftStatus == DraftStatus.InProgress);
        
        if (anyActive) {
            var draftService = scope.ServiceProvider.GetRequiredService<DraftService>();
            await draftService.ProcessExpiredAutoPicks();
            nextDelay = FastInterval;  // 2s
        }
        await Task.Delay(nextDelay, stoppingToken);
    }
}
```

Inside `ProcessExpiredAutoPicks`:

```csharp
var expired = await _db.Leagues
    .Where(l => l.Type == LeagueType.Draft
             && l.DraftStatus == DraftStatus.InProgress
             && l.CurrentPickDeadline < DateTime.UtcNow)
    .Select(l => l.Id).ToListAsync();

foreach (var leagueId in expired) {
    var members = await GetOrderedMembers(leagueId);
    int idx = GetPickerIndex(league.CurrentPickNumber, members.Count);
    string pickerUserId = members[idx].UserId;
    int? autoPickId = await PickBestAvailableForUser(leagueId, pickerUserId);
    if (autoPickId.HasValue)
        await ExecutePick(leagueId, pickerUserId, autoPickId.Value, isAutoPick: true);
}
```

`PickBestAvailableForUser` picks the highest-totalPoints player at a position the user still needs:
```csharp
var stillNeeded = SquadCaps
    .Where(kv => myPicks.Count(p => p.Player.Position == kv.Key) < kv.Value)
    .Select(kv => kv.Key).ToHashSet();

var best = await _db.BundesligaPlayers
    .Where(p => !draftedIds.Contains(p.Id) && stillNeeded.Contains(p.Position))
    .OrderByDescending(p => p.TotalPoints)
    .Select(p => p.Id).FirstOrDefaultAsync();
```

### 8.6 Finalization — turning DraftPicks into FantasyTeams

When the final pick is made, `FinalizeDraft(leagueId)` runs. For each member:

```csharp
foreach (var m in members) {
    var picks = await _db.DraftPicks
        .Where(p => p.LeagueId == leagueId && p.UserId == m.UserId)
        .Include(p => p.Player).ToListAsync();
    if (picks.Count != 15) continue;
    
    var team = new FantasyTeam {
        UserId = m.UserId, LeagueId = leagueId,
        Name = $"{m.DisplayName}'s Squad", Budget = 0m, FreeTransfers = 0
    };
    _db.FantasyTeams.Add(team);
    await _db.SaveChangesAsync();
    
    // Layout: 1 GK, 4 DEF, 4 MID, 2 FWD as starters; rest on bench
    var ordered = new List<DraftPick>();
    ordered.AddRange(gks.Take(1)); ordered.AddRange(defs.Take(4));
    ordered.AddRange(mids.Take(4)); ordered.AddRange(fwds.Take(2));
    // Bench
    ordered.AddRange(gks.Skip(1)); ordered.AddRange(defs.Skip(4));
    ordered.AddRange(mids.Skip(4)); ordered.AddRange(fwds.Skip(2));
    
    int squadPos = 1;
    foreach (var dp in ordered) {
        _db.FantasyPicks.Add(new FantasyPick {
            FantasyTeamId = team.Id, PlayerId = dp.PlayerId, GameweekId = currentGw.Id,
            SquadPosition = squadPos, IsCaptain = false, IsViceCaptain = false  // no captain in draft mode
        });
        squadPos++;
    }
}
await _db.SaveChangesAsync();
```

After this runs, the draft league members all have `FantasyTeam` rows and can be managed exactly like classic teams (with the exception that transfers/chips are blocked).

---

## 9. Waivers — queue, processing, free agency

After the draft completes, ongoing roster management uses waivers, not transfers.

### 9.1 The four phases

Computed dynamically by `ComputePhase(league, upcomingGw)`:

| Condition | Phase | UI |
|---|---|---|
| `upcoming.IsLocked` (within 1.5h of kickoff) | `Locked` | Read-only; no actions |
| `LastWaiverProcessedGameweekId == upcoming.Id` | `FreeAgency` | Direct swap, first-come-first-served |
| `league.DraftStatus != Completed` | `NoDraftYet` | Disabled placeholder |
| Otherwise | `Queue` | Add/reorder/remove claims |

### 9.2 Queue-phase: adding a claim

```csharp
public async Task<WaiverClaimDto> AddClaim(int leagueId, string userId, int playerOutId, int playerInId)
{
    // Phase check
    if (ComputePhase(league, upcoming) != WaiverPhase.Queue)
        throw new InvalidOperationException("Queue is closed.");
    
    // Player out must be in my squad
    if (!await _db.FantasyPicks.AnyAsync(p => p.FantasyTeamId == team.Id 
        && p.GameweekId == upcoming.Id && p.PlayerId == playerOutId))
        throw new InvalidOperationException("Not in your squad.");
    
    // Same position required
    if (playerIn.Position != playerOut.Position)
        throw new InvalidOperationException("Must be same position.");
    
    // Reject if target was dropped this GW
    if (await IsLocked(leagueId, upcoming.Id, playerInId))
        throw new InvalidOperationException("Player is locked until next GW.");
    
    // Compute next priority
    int nextPriority = 1 + (await _db.WaiverClaims
        .Where(c => c.LeagueId == leagueId && c.UserId == userId
                 && c.GameweekId == upcoming.Id && c.Status == WaiverClaimStatus.Pending)
        .Select(c => (int?)c.Priority).MaxAsync() ?? 0);
    
    _db.WaiverClaims.Add(new WaiverClaim {
        LeagueId = leagueId, UserId = userId, GameweekId = upcoming.Id,
        PlayerOutId = playerOutId, PlayerInId = playerInId,
        Priority = nextPriority, Status = WaiverClaimStatus.Pending
    });
    await _db.SaveChangesAsync();
    return ToDto(...);
}
```

### 9.3 Processing the queue

`ProcessLeagueWaivers(leagueId)` runs either:
- Manually via the admin endpoint `POST /api/waiver/{id}/process`
- Automatically by `WaiverProcessingService` when the deadline is < 24h away

Algorithm:

```csharp
// Order managers by reverse standings (lowest points first)
var teams = await _db.FantasyTeams
    .Where(t => t.LeagueId == leagueId)
    .OrderBy(t => t.TotalPoints + t.GameweekPoints)
    .ToListAsync();

foreach (var team in teams) {
    // Process this manager's claims in priority order
    var myClaims = allClaims.Where(c => c.UserId == team.UserId).OrderBy(c => c.Priority);
    
    foreach (var claim in myClaims) {
        // Re-validate at processing time
        var ownedPick = await _db.FantasyPicks.FirstOrDefaultAsync(p =>
            p.FantasyTeamId == team.Id && p.GameweekId == upcoming.Id
            && p.PlayerId == claim.PlayerOutId);
        if (ownedPick == null) {
            claim.Status = WaiverClaimStatus.Failed;
            claim.FailureReason = "Player out is no longer in your squad.";
            continue;
        }
        
        bool taken = await _db.FantasyPicks.AnyAsync(p =>
            p.GameweekId == upcoming.Id && p.FantasyTeam.LeagueId == leagueId
            && p.PlayerId == claim.PlayerInId);
        if (taken) {
            claim.Status = WaiverClaimStatus.Failed;
            claim.FailureReason = "Already claimed by higher waiver priority.";
            continue;
        }
        
        if (await IsLocked(leagueId, upcoming.Id, claim.PlayerInId)) {
            claim.Status = WaiverClaimStatus.Failed;
            claim.FailureReason = "Player is locked.";
            continue;
        }
        
        // SUCCEED: swap and record drop
        ownedPick.PlayerId = claim.PlayerInId;
        claim.Status = WaiverClaimStatus.Succeeded;
        RecordDrop(leagueId, upcoming.Id, claim.PlayerOutId, team.UserId);
    }
    await _db.SaveChangesAsync();   // save per-user so subsequent users see fresh state
}

league.LastWaiverProcessedGameweekId = upcoming.Id;
await _db.SaveChangesAsync();
```

Critical detail: `await _db.SaveChangesAsync()` runs **per user**, not at the end. This ensures that when User B's claim is processed, the system sees that User A's earlier swap actually happened (so the player is now owned by User A).

### 9.4 Free Agency — direct swap

After `LastWaiverProcessedGameweekId == upcoming.Id`, the queue is closed and direct swaps are allowed:

```csharp
public async Task DirectSwap(int leagueId, string userId, int playerOutId, int playerInId)
{
    // Phase must be FreeAgency
    if (ComputePhase(league, upcoming) != WaiverPhase.FreeAgency)
        throw new InvalidOperationException("FA is not open.");
    
    // Same validations as AddClaim, but executed immediately
    var pick = await _db.FantasyPicks.FirstOrDefaultAsync(p =>
        p.FantasyTeamId == team.Id && p.GameweekId == upcoming.Id && p.PlayerId == playerOutId);
    if (pick == null) throw new InvalidOperationException("Not in your squad.");
    
    if (playerIn.Position != playerOut.Position) throw new InvalidOperationException("Must be same position.");
    if (await Anyone(leagueId, upcoming.Id, playerInId)) throw new InvalidOperationException("Already owned.");
    if (await IsLocked(leagueId, upcoming.Id, playerInId)) throw new InvalidOperationException("Locked.");
    
    pick.PlayerId = playerInId;
    RecordDrop(leagueId, upcoming.Id, playerOutId, userId);
    await _db.SaveChangesAsync();
}
```

### 9.5 Drop locks

When a player is dropped (via successful waiver claim OR free agency swap), the `DroppedPlayer` record locks them for the rest of this gameweek:

```csharp
private void RecordDrop(int leagueId, int gameweekId, int playerId, string userId)
{
    _db.DroppedPlayers.Add(new DroppedPlayer {
        LeagueId = leagueId, GameweekId = gameweekId,
        PlayerId = playerId, DroppedByUserId = userId,
        DroppedAt = DateTime.UtcNow
    });
}

private async Task<bool> IsLocked(int leagueId, int gameweekId, int playerId)
{
    return await _db.DroppedPlayers.AnyAsync(d =>
        d.LeagueId == leagueId && d.GameweekId == gameweekId && d.PlayerId == playerId);
}
```

When the upcoming GW changes (next one becomes "upcoming"), the lookup naturally moves to the new `gameweekId` — old locks become irrelevant. No cleanup needed.

In `GetFreeAgents`, locked players are returned with `IsLocked: true` and the `DroppedByName` (so the UI can show the tooltip "Dropped by X this GW").

---

## 10. Scoring system

### 10.1 Match events ingestion

The admin enters match events via the admin panel:
- `POST /api/admin/event` — single event (goal, assist, card, etc.)
- `POST /api/admin/events/batch` — bulk insert

Each event is a row in `MatchEvent`:
```csharp
public class MatchEvent {
    public int Id { get; set; }
    public int GameweekId { get; set; }
    public int PlayerId { get; set; }
    public EventType EventType { get; set; }   // MinutesPlayed, Goal, Assist, …
    public int? Minute { get; set; }
    public int Points { get; set; }            // pre-computed FPL points value
}
```

After inserting events, the admin triggers `LeaderboardService.RecalculateLiveGameweekPoints(gameweekId)` which:
1. Loads every `FantasyTeam` with their picks for this GW.
2. Loads all events for the GW.
3. Computes `playedIds` (players with > 0 minutes).
4. For each team:
   - Determine scoring IDs (effective XI after auto-subs, OR all 15 if Bench Boost chip active)
   - Determine captain (with vice fallback if captain DNP'd)
   - Sum: `points * multiplier` for each scoring player
5. Save `team.GameweekPoints`.
6. Broadcast `StandingsUpdate` via SignalR to all connected clients.

### 10.2 Auto-substitution algorithm

The trickiest part. From `LeaderboardService.ComputeAutoSubs`:

```csharp
public static (HashSet<int> effectiveIds, List<(int outId, int inId)> subs) ComputeAutoSubs(
    List<FantasyPick> picks, HashSet<int> playedIds)
{
    var starters = picks.Where(p => p.SquadPosition <= 11).ToList();
    var bench = picks.Where(p => p.SquadPosition >= 12).OrderBy(p => p.SquadPosition).ToList();
    var subs = new List<(int, int)>();
    var effective = new List<FantasyPick>(starters);
    
    // Step 1: GK substitution (dedicated slot)
    var startGk = starters.FirstOrDefault(p => p.Player.Position == PlayerPosition.GK);
    var benchGk = bench.FirstOrDefault(p => p.Player.Position == PlayerPosition.GK);
    if (startGk != null && !playedIds.Contains(startGk.PlayerId)
        && benchGk != null && playedIds.Contains(benchGk.PlayerId)) {
        effective.Remove(startGk);
        effective.Add(benchGk);
        subs.Add((startGk.PlayerId, benchGk.PlayerId));
    }
    
    // Step 2: Outfield subs in bench order, only if formation stays valid
    var dnpOutfield = starters
        .Where(p => p.Player.Position != PlayerPosition.GK && !playedIds.Contains(p.PlayerId))
        .OrderBy(p => p.SquadPosition).ToList();
    
    var outfieldBench = bench
        .Where(p => p.Player.Position != PlayerPosition.GK && playedIds.Contains(p.PlayerId))
        .OrderBy(p => p.SquadPosition).ToList();
    
    var usedBench = new HashSet<int>();
    
    foreach (var dnp in dnpOutfield) {
        foreach (var cand in outfieldBench) {
            if (usedBench.Contains(cand.Id)) continue;
            
            // Try this swap
            var trial = effective.Where(p => p.Id != dnp.Id).ToList();
            trial.Add(cand);
            
            // Check formation validity: ≥3 DEF, ≥2 MID, ≥1 FWD
            int defs = trial.Count(p => p.Player.Position == PlayerPosition.DEF);
            int mids = trial.Count(p => p.Player.Position == PlayerPosition.MID);
            int fwds = trial.Count(p => p.Player.Position == PlayerPosition.FWD);
            
            if (defs >= 3 && mids >= 2 && fwds >= 1) {
                effective = trial;
                usedBench.Add(cand.Id);
                subs.Add((dnp.PlayerId, cand.PlayerId));
                break;
            }
        }
    }
    
    return (effective.Select(p => p.PlayerId).ToHashSet(), subs);
}
```

Why this matters: a manager who plays a 3-4-3 formation can only have a striker subbed in for another striker (since dropping below 1 forward would be invalid). The trial-and-validate loop ensures we never produce an invalid formation.

### 10.3 Captain DNP fallback

```csharp
var captainPick = picks.FirstOrDefault(p => p.IsCaptain);
var vicePick = picks.FirstOrDefault(p => p.IsViceCaptain);
int? captainId = captainPick?.PlayerId;

if (captainPick != null && !playedIds.Contains(captainPick.PlayerId)
    && vicePick != null && playedIds.Contains(vicePick.PlayerId)) {
    captainId = vicePick.PlayerId;   // vice gets the ×2 if captain DNP'd
}
```

### 10.4 Final scoring pass

```csharp
int gwPoints = 0;
foreach (var pick in picks) {
    if (!scoringIds.Contains(pick.PlayerId)) continue;  // not in effective XI
    int pts = gwEventMap.GetValueOrDefault(pick.PlayerId, 0);
    int mult = pick.PlayerId == captainId
        ? (activeChip == ChipType.TripleCaptain ? 3 : 2)
        : 1;
    gwPoints += pts * mult;
}
team.GameweekPoints = gwPoints;
```

### 10.5 Live broadcast

After `RecalculateLiveGameweekPoints`:
```csharp
await _hub.Clients.Group($"gw-{gameweekId}").SendAsync("StandingsUpdate");
```

Connected clients receive this and re-fetch the standings. This is what makes the global leaderboard tick up in real time as goals are entered.

---

## 11. Putting it all together — a request lifecycle

Concrete example: User clicks "Pick" in a draft.

1. **Browser**: `Draft.tsx` → `api.post('/draft/5/pick', { playerId: 42 })`
2. **axios interceptor** adds `Authorization: Bearer eyJ...`
3. **Network**: HTTPS request to `https://fbl-6qkh.onrender.com/api/draft/5/pick`
4. **Render reverse proxy**: forwards to the .NET container on port 8080
5. **ASP.NET Core middleware pipeline**:
   - `UseDefaultFiles` → no, this isn't a static file
   - `UseStaticFiles` → no
   - `UseCors` → adds CORS headers (or not, since same-origin)
   - `UseAuthentication` → validates JWT, populates `HttpContext.User`
   - `UseAuthorization` → checks `[Authorize]` on `DraftController`
   - `MapControllers` → routes to `DraftController.Pick(5, dto)`
6. **DraftController.Pick**:
   - Checks `IsMember(5)` → SQL: `SELECT 1 FROM "LeagueMembers" WHERE ...`
   - Calls `_draft.MakePick(5, userId, 42)`
7. **DraftService.MakePick → ExecutePick**:
   - Loads league, members, validates turn
   - Loads my draft picks, validates position cap
   - INSERT `DraftPick` row
   - UPDATE `League.CurrentPickNumber`, `CurrentPickDeadline`
   - `SaveChanges()` → SQL: `BEGIN; INSERT...; UPDATE...; COMMIT;`
8. **SignalR broadcast**: `_hub.Clients.Group("draft-5").SendAsync("PickMade", payload)`
9. **All connected clients in draft room 5** receive the event → their `Draft.tsx` handler fires → optimistic UI update + `refresh()` → re-fetches state → re-renders

Every line of this chain is in the code we've walked through.

---

## Summary

The whole app is built around 3 architectural pillars:

1. **Nullable `LeagueId` on FantasyTeam** — enables one user, many teams, one schema, three game modes
2. **EF Core unique indexes** — `(UserId, LeagueId)` ensures one team per league; `(LeagueId, PlayerId)` on `DraftPicks` ensures single ownership in draft. The DB itself enforces correctness.
3. **SignalR groups + IHostedService** — turn the static request/response API into a real-time multiplayer system. Group names like `draft-5` and `gw-12` scope broadcasts to interested parties only.

Everything else is bookkeeping around these three ideas.
