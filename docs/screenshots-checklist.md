# Screenshots Checklist — Diplomska Figures

**20 figures total. 2 already done (diagrams). 18 screenshots to capture.**

For each: navigate to the URL, set up the state, capture the screenshot, save with the filename shown.
Crop tightly to the relevant content — don't include the browser address bar in final versions inserted into Word.

> **Tip:** before starting, make sure you have at least 3 test accounts ready (e.g. `markokoski`, `kuko`, `kofce`). Use 3 browser tabs (sessionStorage = each tab is a separate session) to set up multi-user states without logging in/out repeatedly.

> **Recommended viewport**: 1440×900 (laptop screen). Avoid huge 4K caps — they look weird at thesis figure size.

---

## ✅ Already done (diagrams)

| Figure | File | Status |
|---|---|---|
| **1** — Архитектура на системот | `docs/architecture-diagram.png` | ✅ Generated |
| **2** — ER дијаграм | `docs/er-diagram-core.png` | ✅ Generated |

---

## 📸 To capture (18 screenshots)

### Group A — Auth (no setup needed, fresh browser)

| # | Caption | URL | Setup | Capture | Save as |
|---|---|---|---|---|---|
| **3** | Страница за регистрација | `/register` | Log out first if needed | Whole form | `fig03-register.png` |
| **4** | Страница за најава | `/login` | Log out first | Whole form | `fig04-login.png` |

---

### Group B — Building & managing a team (logged in as User A)

| # | Caption | URL | Setup | Capture | Save as |
|---|---|---|---|---|---|
| **5** | Избор на 15 играчи со филтри | `/transfers` | Be a fresh user with no team yet, OR delete your existing team in DB | Show several players in the table + the position/cost counters at the top | `fig05-pick-squad.png` |
| **6** | My Team — pitch view | `/my-team` | Have a built team | Whole pitch + score cards above | `fig06-my-team.png` |
| **7** | Team Selector dropdown | `/my-team` | Have ≥2 teams (global + at least 1 league team). Click the team selector dropdown so it's open | Top-right area showing the dropdown expanded | `fig07-team-selector.png` |
| **8** | Transfer Market | `/transfers` | Have a team (existing user) | Squad bar on top + player table | `fig08-transfers.png` |

---

### Group C — Leagues (need a Classic league set up)

> **Setup once for this group:**
> 1. Tab A (creator): Leagues → Create League → choose **Classic**, name it `test-classic`
> 2. Tab B (different user): join via the join code shown
> 3. (Optional) Tab C: third user joins too

| # | Caption | URL | Setup | Capture | Save as |
|---|---|---|---|---|---|
| **9** | Create League form (Classic vs Draft) | `/leagues` | Click "Create League". Make sure both mode buttons are visible — Classic highlighted by default | The expanded create form | `fig09-create-league.png` |
| **10** | My Leagues list with badges | `/leagues` | Click "My Leagues" tab. Show at least 1 Classic + 1 Draft league | The leagues list | `fig10-my-leagues.png` |
| **11** | League detail + Create Team CTA | `/leagues` then click into a Classic league you don't yet have a team for | Show the green "Create Team" button + standings below | League detail panel | `fig11-league-detail-cta.png` |
| **12** | Classic league standings | `/leagues` then click into your Classic league | Show ≥2 members in the standings table | Standings table only | `fig12-classic-standings.png` |
| **13** | Modal — viewing another manager's team with GW arrows | `/leagues` then click into a league → click "View" on another manager | Wait for modal to open | Whole modal (with pitch view + ‹ GW X › arrows) | `fig13-view-team-modal.png` |

---

### Group D — Draft mode (need a Draft league with 2 members)

> **Setup once for this group:**
> 1. Tab A (creator): Leagues → Create League → **Draft**, max 2 members, name it `test-draft`
> 2. Tab B: join via join code
> 3. Both tabs go into the league → "Enter Lobby"

| # | Caption | URL | Setup | Capture | Save as |
|---|---|---|---|---|---|
| **14** | Draft Lobby (waiting for start) | `/draft/{leagueId}` | Both members joined, draft NOT started yet. View as creator | Lobby panel with member list + Start button | `fig14-draft-lobby.png` |
| **15** | Draft Room in progress | `/draft/{leagueId}` | Click Start Draft. Make a few picks so the draft board has content. Capture **as the manager whose turn it ISN'T** so you see "On the clock: <other>" + circular timer ticking | Whole draft room (left column with timer + board, right with available players) | `fig15-draft-in-progress.png` |
| **16** | Draft completed | `/draft/{leagueId}` | Run through ALL 30 picks (15 × 2 managers). When draft completes, you land on the trophy panel | Whole completed panel | `fig16-draft-completed.png` |

> **Faster way to set up Group D**: set `MaxMembers = 2` and use the **auto-pick timeout** to advance the draft quickly — just don't pick and the server will pick for you after 60s. Or set `DraftPickSeconds = 5` directly in DB to speed the draft up.

---

### Group E — Waivers (need a completed Draft league)

> **Setup once for this group:**
> Use the same Draft league from Group D (now `DraftStatus = Completed`). Navigate from MyTeam → Waivers button (purple, top-right).

| # | Caption | URL | Setup | Capture | Save as |
|---|---|---|---|---|---|
| **17** | Waivers — Queue phase | `/waivers/{leagueId}` | Default state after draft completes (queue is open). Add 2-3 claims by clicking a squad player + a free agent | Whole page with My Squad / My Queue / Available Players visible | `fig17-waivers-queue.png` |
| **18** | Waivers — Free Agency phase | `/waivers/{leagueId}` | Trigger waiver processing manually via the admin endpoint (see below) → page now shows Free Agency phase with "Swap In" buttons | Same layout but in FA mode | `fig18-waivers-free-agency.png` |
| **19** | Locked player (drop lock) | `/waivers/{leagueId}` | After a Free Agency swap, scroll the available players list to find the dropped player — it shows with 🔒 Locked icon | Crop to one row showing the locked entry, with a row above/below for context | `fig19-locked-player.png` |

> **To trigger waiver processing manually** (you have Admin role on `kofce`):
> Open browser DevTools console while on the app:
> ```js
> fetch('/api/waiver/<leagueId>/process', {
>   method: 'POST',
>   headers: { 'Authorization': 'Bearer ' + sessionStorage.getItem('fbl_token') }
> }).then(r => console.log(r.status));
> ```
> Then refresh the Waivers page.

---

### Group F — Admin

| # | Caption | URL | Setup | Capture | Save as |
|---|---|---|---|---|---|
| **20** | Admin Dashboard | `/admin` | Logged in as Admin (e.g. `kofce`) | The gameweeks/matches editor section | `fig20-admin-dashboard.png` |

---

## 📋 Workflow recommendation

Don't capture all 18 in one go — do them in groups. Realistic sessions:

| Session | Time | What |
|---|---|---|
| **1** | ~15 min | Group A + B (auth + my team) — easiest, no multi-user needed |
| **2** | ~30 min | Group C (Leagues) — set up classic league + capture 5 figures |
| **3** | ~45 min | Group D (Draft) — most involved; run through entire snake draft |
| **4** | ~20 min | Group E (Waivers) — needs Group D done first |
| **5** | ~5 min | Group F (Admin) |

**Total: ~2 hours of clicking + capturing.**

---

## After capturing

1. Save all `figXX-*.png` files in `docs/screenshots/` folder
2. In the Word document, find each `[ОВДЕ СЛИКА: Слика N. ...]` placeholder and replace with the corresponding image
3. Apply consistent caption styling — most theses use centered italic text **below** the image with the format `Слика N. <caption>`
4. Make sure all images render at consistent width (e.g. 14-15 cm) so the document looks professional
