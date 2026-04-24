import { useEffect, useState } from 'react';
import api from '../api/client';
import { GameweekStatus, type Player, type Gameweek as GwType } from '../api/types';

const eventTypes = [
  { value: 0, label: 'Minutes Played' },
  { value: 1, label: 'Goal' },
  { value: 2, label: 'Assist' },
  { value: 3, label: 'Clean Sheet' },
  { value: 4, label: 'Yellow Card' },
  { value: 5, label: 'Red Card' },
  { value: 6, label: 'Penalty Save' },
  { value: 7, label: 'Penalty Miss' },
  { value: 8, label: 'Own Goal' },
  { value: 9, label: 'Bonus' },
];

const TEAMS = [
  'Bayern Munich', 'Bayer Leverkusen', 'Borussia Dortmund', 'RB Leipzig',
  'VfB Stuttgart', 'Eintracht Frankfurt', 'SC Freiburg', 'VfL Wolfsburg',
  'TSG Hoffenheim', 'Werder Bremen', 'FC Augsburg', '1. FC Union Berlin',
  'Borussia Mönchengladbach', '1. FSV Mainz 05', 'Hamburger SV', 'FC St. Pauli',
  '1. FC Köln', '1. FC Heidenheim',
];

const POSITIONS = [
  { value: 1, label: 'GK' },
  { value: 2, label: 'DEF' },
  { value: 3, label: 'MID' },
  { value: 4, label: 'FWD' },
];

export default function AdminDashboard() {
  const [gameweeks, setGameweeks] = useState<GwType[]>([]);
  const [players, setPlayers] = useState<Player[]>([]);
  const [selectedGw, setSelectedGw] = useState<number>(0);
  const [selectedPlayer, setSelectedPlayer] = useState<number>(0);
  const [eventType, setEventType] = useState<number>(1);
  const [minute, setMinute] = useState<string>('');
  const [message, setMessage] = useState('');
  const [playerSearch, setPlayerSearch] = useState('');
  const [importing, setImporting] = useState(false);
  const [bulkText, setBulkText] = useState('');
  const [bulkTeam, setBulkTeam] = useState(TEAMS[0]);
  const [bulkPosition, setBulkPosition] = useState(1);
  const [bulkImporting, setBulkImporting] = useState(false);
  const [priceSearch, setPriceSearch] = useState('');
  const [priceEdits, setPriceEdits] = useState<Record<number, string>>({});
  const [fitnessEdits, setFitnessEdits] = useState<Record<number, string>>({});
  const [matchGw, setMatchGw] = useState<number>(0);
  const [matches, setMatches] = useState<any[]>([]);
  const [matchEdits, setMatchEdits] = useState<Record<number, { hg: string; ag: string; fin: boolean }>>({});

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    const [gwRes, playersRes] = await Promise.all([
      api.get<GwType[]>('/gameweek'),
      api.get<Player[]>('/players?pageSize=500'),
    ]);
    setGameweeks(gwRes.data);
    setPlayers(playersRes.data);

    const liveGw = gwRes.data.find((g) => g.status === GameweekStatus.Live);
    if (liveGw) setSelectedGw(liveGw.id);
    else if (gwRes.data.length > 0) setSelectedGw(gwRes.data[0].id);
  };

  const addEvent = async () => {
    if (!selectedGw || !selectedPlayer) {
      setMessage('Select gameweek and player');
      return;
    }
    try {
      const res = await api.post('/admin/event', {
        gameweekId: selectedGw,
        playerId: selectedPlayer,
        eventType,
        minute: minute ? parseInt(minute) : null,
      });
      setMessage(`Event added! Points: ${res.data.points}`);
    } catch (err: any) {
      setMessage(err.response?.data || 'Failed to add event');
    }
  };

  const updateGwStatus = async (id: number, status: GameweekStatus) => {
    try {
      await api.put(`/admin/gameweek/${id}`, { status });
      setMessage(`Gameweek updated to ${['Upcoming', 'Live', 'Finished'][status]}`);
      loadData();
    } catch (err: any) {
      setMessage(err.response?.data || 'Failed');
    }
  };

  const seedGameweeks = async () => {
    try {
      const res = await api.post('/admin/gameweeks/seed');
      setMessage(res.data.message);
      loadData();
    } catch (err: any) {
      setMessage(err.response?.data || 'Failed');
    }
  };

  const [simulating, setSimulating] = useState<number | null>(null);
  const simulateGameweek = async (id: number, gwNumber: number) => {
    if (!confirm(`Simulate GW${gwNumber}? This generates random match events, finalises scores and rescoring all fantasy teams. Existing events for this GW will be wiped.`)) return;
    setSimulating(id);
    setMessage(`Simulating GW${gwNumber}...`);
    try {
      const res = await api.post(`/admin/simulate-gameweek/${id}`);
      setMessage(res.data.message);
      loadData();
    } catch (err: any) {
      setMessage(err.response?.data?.message || err.response?.data || 'Simulation failed');
    } finally {
      setSimulating(null);
    }
  };

  const importPlayers = async () => {
    setImporting(true);
    setMessage('Importing players... This may take a few minutes.');
    try {
      const res = await api.post('/admin/import-players');
      setMessage(res.data.message);
      loadData();
    } catch (err: any) {
      setMessage(err.response?.data || 'Import failed');
    } finally {
      setImporting(false);
    }
  };

  const loadMatches = async (gwNum: number) => {
    try {
      const res = await api.get(`/fixtures/gameweek/${gwNum}`);
      setMatches(res.data);
      const edits: Record<number, { hg: string; ag: string; fin: boolean }> = {};
      for (const m of res.data) {
        edits[m.id] = { hg: m.homeGoals?.toString() ?? '', ag: m.awayGoals?.toString() ?? '', fin: m.isFinished };
      }
      setMatchEdits(edits);
    } catch { setMatches([]); }
  };

  const saveMatch = async (matchId: number) => {
    const e = matchEdits[matchId];
    if (!e) return;
    try {
      const res = await api.put(`/admin/match/${matchId}`, {
        homeGoals: e.hg !== '' ? parseInt(e.hg) : null,
        awayGoals: e.ag !== '' ? parseInt(e.ag) : null,
        isFinished: e.fin,
      });
      setMessage(res.data.message);
    } catch (err: any) { setMessage(err.response?.data || 'Failed'); }
  };

  const savePrice = async (playerId: number) => {
    const val = priceEdits[playerId];
    if (!val) return;
    try {
      const res = await api.put(`/admin/player/${playerId}/price`, { price: parseFloat(val) });
      setMessage(res.data.message);
      loadData();
    } catch (err: any) { setMessage(err.response?.data || 'Failed'); }
  };

  const saveFitness = async (playerId: number) => {
    const val = fitnessEdits[playerId];
    if (val === undefined) return;
    try {
      const res = await api.put(`/admin/player/${playerId}/fitness`, { fitness: parseInt(val) });
      setMessage(res.data.message);
      loadData();
    } catch (err: any) { setMessage(err.response?.data || 'Failed'); }
  };

  const pricePlayers = priceSearch
    ? players.filter((p) => p.name.toLowerCase().includes(priceSearch.toLowerCase()) || p.team.toLowerCase().includes(priceSearch.toLowerCase()))
    : [];

  const filteredPlayers = playerSearch
    ? players.filter((p) => p.name.toLowerCase().includes(playerSearch.toLowerCase()))
    : players.slice(0, 20);

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-amber-400 mb-6">Admin Dashboard</h1>

      {message && (
        <div className="bg-slate-800 border border-slate-600 text-slate-200 px-4 py-2 rounded-lg mb-4 text-sm">
          {message}
        </div>
      )}

      {/* Quick actions */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
        <button
          onClick={async () => {
            setImporting(true);
            setMessage('Importing from OpenLigaDB... This pulls all 25 completed matchdays with goals, clean sheets, and player data. May take 1-2 minutes.');
            try {
              const res = await api.post('/admin/import-openliga');
              setMessage(res.data.message);
              loadData();
            } catch (err: any) {
              setMessage(err.response?.data || 'Import failed');
            } finally {
              setImporting(false);
            }
          }}
          disabled={importing}
          className="bg-emerald-900/50 hover:bg-emerald-800/50 border border-emerald-700 text-white p-4 rounded-xl transition text-left disabled:opacity-50"
        >
          <p className="font-semibold text-emerald-400">Import from OpenLigaDB</p>
          <p className="text-slate-400 text-sm">All teams, goals, clean sheets + real kickoff times (free, no API key)</p>
        </button>
        <button
          onClick={importPlayers}
          disabled={importing}
          className="bg-slate-800 hover:bg-slate-700 border border-slate-700 text-white p-4 rounded-xl transition text-left disabled:opacity-50"
        >
          <p className="font-semibold">Import from football-data.org</p>
          <p className="text-slate-400 text-sm">Full squad rosters (requires API key)</p>
        </button>
        <button
          onClick={seedGameweeks}
          className="bg-slate-800 hover:bg-slate-700 border border-slate-700 text-white p-4 rounded-xl transition text-left"
        >
          <p className="font-semibold">Seed Gameweeks</p>
          <p className="text-slate-400 text-sm">Create 34 matchdays (manual)</p>
        </button>
        <button
          onClick={() => api.post('/admin/seed-admin').then((r) => setMessage(r.data.message)).catch((e) => setMessage(e.response?.data || 'Failed'))}
          className="bg-slate-800 hover:bg-slate-700 border border-slate-700 text-white p-4 rounded-xl transition text-left"
        >
          <p className="font-semibold">Seed Admin</p>
          <p className="text-slate-400 text-sm">Create admin@fbl.com</p>
        </button>
      </div>

      {/* Bulk player import */}
      <div className="bg-slate-800 rounded-xl p-6 mb-8">
        <h2 className="text-lg font-semibold text-white mb-2">Bulk Player Import</h2>
        <p className="text-slate-400 text-sm mb-3">
          Select team & position, then paste player data. Each player is 9 lines:<br/>
          <span className="text-slate-500 font-mono">ShirtNo, Name, Age, Matches, Minutes, Goals, Assists, Yellows, Reds</span>
        </p>
        <div className="grid grid-cols-2 gap-4 mb-3">
          <div>
            <label className="block text-sm text-slate-400 mb-1">Team</label>
            <select
              value={bulkTeam}
              onChange={(e) => setBulkTeam(e.target.value)}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm"
            >
              {TEAMS.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm text-slate-400 mb-1">Position</label>
            <select
              value={bulkPosition}
              onChange={(e) => setBulkPosition(Number(e.target.value))}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm"
            >
              {POSITIONS.map((p) => <option key={p.value} value={p.value}>{p.label}</option>)}
            </select>
          </div>
        </div>
        <textarea
          value={bulkText}
          onChange={(e) => setBulkText(e.target.value)}
          rows={16}
          className="w-full bg-slate-900 border border-slate-600 rounded-lg px-3 py-2 text-slate-200 text-sm font-mono focus:outline-none focus:border-amber-400 mb-3"
          placeholder={`5\nDoekhi Danilho\n27\n26\n2340\n4\n0\n3\n0`}
        />
        <button
          onClick={async () => {
            const lines = bulkText.split('\n').map(l => l.trim()).filter(l => l.length > 0);
            if (lines.length < 9 || lines.length % 9 !== 0) {
              setMessage(`Need 9 lines per player (got ${lines.length} lines). Format: ShirtNo, Name, Age, Matches, Minutes, Goals, Assists, Yellows, Reds`);
              return;
            }
            const playerList = [];
            for (let i = 0; i < lines.length; i += 9) {
              playerList.push({
                name: lines[i + 1],        // Name
                team: bulkTeam,
                position: bulkPosition,
                matchesPlayed: parseInt(lines[i + 3]) || 0,  // Matches
                goals: parseInt(lines[i + 5]) || 0,          // Goals
                assists: parseInt(lines[i + 6]) || 0,        // Assists
                yellowCards: parseInt(lines[i + 7]) || 0,    // Yellows
                redCards: parseInt(lines[i + 8]) || 0,        // Reds
              });
            }
            try {
              setBulkImporting(true);
              setMessage(`Importing ${playerList.length} players...`);
              const res = await api.post('/admin/players/bulk', playerList);
              setMessage(res.data.message);
              setBulkText('');
              loadData();
            } catch (err: any) {
              setMessage(err.response?.data || 'Import failed');
            } finally {
              setBulkImporting(false);
            }
          }}
          disabled={bulkImporting}
          className="bg-amber-500 hover:bg-amber-600 text-black font-semibold px-6 py-2 rounded-lg transition disabled:opacity-50"
        >
          {bulkImporting ? 'Importing...' : `Import Players (${bulkTeam} - ${POSITIONS.find(p => p.value === bulkPosition)?.label})`}
        </button>
      </div>

      {/* Gameweek management */}
      <div className="bg-slate-800 rounded-xl p-6 mb-8">
        <h2 className="text-lg font-semibold text-white mb-4">Gameweek Management</h2>
        <div className="max-h-64 overflow-y-auto space-y-2">
          {gameweeks.map((gw) => (
            <div
              key={gw.id}
              className="flex items-center justify-between bg-slate-700/50 rounded-lg p-3"
            >
              <div>
                <span className="text-white font-medium">GW{gw.number}</span>
                <span className="text-slate-400 text-sm ml-3">
                  {new Date(gw.kickoffTime).toLocaleDateString()}
                </span>
                <span className={`ml-3 text-xs font-medium ${
                  ['text-slate-400', 'text-green-400', 'text-slate-500'][gw.status]
                }`}>
                  {['Upcoming', 'Live', 'Finished'][gw.status]}
                </span>
              </div>
              <div className="flex gap-2">
                {gw.status === 0 && (
                  <button
                    onClick={() => updateGwStatus(gw.id, GameweekStatus.Live)}
                    className="text-xs bg-green-500/20 text-green-400 px-3 py-1 rounded hover:bg-green-500/30"
                  >
                    Start
                  </button>
                )}
                {gw.status === 1 && (
                  <button
                    onClick={() => updateGwStatus(gw.id, GameweekStatus.Finished)}
                    className="text-xs bg-red-500/20 text-red-400 px-3 py-1 rounded hover:bg-red-500/30"
                  >
                    Finish
                  </button>
                )}
                <button
                  onClick={() => simulateGameweek(gw.id, gw.number)}
                  disabled={simulating === gw.id}
                  className="text-xs bg-purple-500/20 text-purple-300 px-3 py-1 rounded hover:bg-purple-500/30 disabled:opacity-50"
                  title="Generate random match events + rescore all fantasy teams"
                >
                  {simulating === gw.id ? 'Simulating...' : '⚡ Simulate'}
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Edit Match Results */}
      <div className="bg-slate-800 rounded-xl p-6 mb-8">
        <h2 className="text-lg font-semibold text-white mb-4">Edit Match Results</h2>
        <div className="flex gap-4 mb-4 items-end">
          <div className="flex-1">
            <label className="block text-sm text-slate-400 mb-1">Gameweek</label>
            <select
              value={matchGw}
              onChange={(e) => { const v = Number(e.target.value); setMatchGw(v); if (v) loadMatches(v); }}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm"
            >
              <option value={0}>Select GW</option>
              {gameweeks.map((gw) => (
                <option key={gw.id} value={gw.number}>GW{gw.number}</option>
              ))}
            </select>
          </div>
        </div>
        {matches.length > 0 && (
          <div className="space-y-2">
            {matches.map((m) => (
              <div key={m.id} className="flex items-center gap-2 bg-slate-700/50 rounded-lg p-3">
                <span className="text-white text-sm w-40 truncate text-right">{m.homeTeam}</span>
                <input
                  type="number"
                  value={matchEdits[m.id]?.hg ?? ''}
                  onChange={(e) => setMatchEdits({ ...matchEdits, [m.id]: { ...matchEdits[m.id], hg: e.target.value } })}
                  className="w-12 bg-slate-600 border border-slate-500 rounded px-2 py-1 text-white text-center text-sm"
                />
                <span className="text-slate-400">-</span>
                <input
                  type="number"
                  value={matchEdits[m.id]?.ag ?? ''}
                  onChange={(e) => setMatchEdits({ ...matchEdits, [m.id]: { ...matchEdits[m.id], ag: e.target.value } })}
                  className="w-12 bg-slate-600 border border-slate-500 rounded px-2 py-1 text-white text-center text-sm"
                />
                <span className="text-white text-sm w-40 truncate">{m.awayTeam}</span>
                <label className="flex items-center gap-1 text-xs text-slate-400 ml-2">
                  <input
                    type="checkbox"
                    checked={matchEdits[m.id]?.fin ?? false}
                    onChange={(e) => setMatchEdits({ ...matchEdits, [m.id]: { ...matchEdits[m.id], fin: e.target.checked } })}
                    className="rounded"
                  />
                  FT
                </label>
                <button
                  onClick={() => saveMatch(m.id)}
                  className="text-xs bg-emerald-500/20 text-emerald-400 px-3 py-1 rounded hover:bg-emerald-500/30 ml-auto"
                >
                  Save
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Edit Player Prices & Fitness */}
      <div className="bg-slate-800 rounded-xl p-6 mb-8">
        <h2 className="text-lg font-semibold text-white mb-4">Edit Player Prices & Fitness</h2>
        <input
          type="text"
          placeholder="Search player or team..."
          value={priceSearch}
          onChange={(e) => setPriceSearch(e.target.value)}
          className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-amber-400 mb-4"
        />
        {priceSearch && pricePlayers.length > 0 && (
          <div className="space-y-2 max-h-80 overflow-y-auto">
            {pricePlayers.slice(0, 30).map((p) => (
              <div key={p.id} className="flex items-center gap-3 bg-slate-700/50 rounded-lg p-3">
                <div className="flex-1 min-w-0">
                  <span className="text-white text-sm font-medium">{p.name}</span>
                  <span className="text-slate-400 text-xs ml-2">({p.team})</span>
                </div>
                {/* Price */}
                <div className="flex items-center gap-1">
                  <label className="text-xs text-slate-400">Price:</label>
                  <input
                    type="number"
                    step="0.5"
                    defaultValue={p.price}
                    onChange={(e) => setPriceEdits({ ...priceEdits, [p.id]: e.target.value })}
                    className="w-16 bg-slate-600 border border-slate-500 rounded px-2 py-1 text-white text-center text-sm"
                  />
                  <button
                    onClick={() => savePrice(p.id)}
                    className="text-xs bg-emerald-500/20 text-emerald-400 px-2 py-1 rounded hover:bg-emerald-500/30"
                  >
                    Save
                  </button>
                </div>
                {/* Fitness */}
                <div className="flex items-center gap-1">
                  <label className="text-xs text-slate-400">Fitness:</label>
                  <input
                    type="number"
                    min="0"
                    max="100"
                    defaultValue={p.fitness}
                    onChange={(e) => setFitnessEdits({ ...fitnessEdits, [p.id]: e.target.value })}
                    className="w-14 bg-slate-600 border border-slate-500 rounded px-2 py-1 text-white text-center text-sm"
                  />
                  <button
                    onClick={() => saveFitness(p.id)}
                    className="text-xs bg-amber-500/20 text-amber-400 px-2 py-1 rounded hover:bg-amber-500/30"
                  >
                    Save
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
        {priceSearch && pricePlayers.length === 0 && (
          <p className="text-slate-500 text-sm">No players found</p>
        )}
      </div>

      {/* Add match event */}
      <div className="bg-slate-800 rounded-xl p-6">
        <h2 className="text-lg font-semibold text-white mb-4">Add Match Event</h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block text-sm text-slate-400 mb-1">Gameweek</label>
            <select
              value={selectedGw}
              onChange={(e) => setSelectedGw(Number(e.target.value))}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm"
            >
              <option value={0}>Select GW</option>
              {gameweeks.filter((g) => g.status === GameweekStatus.Live).map((gw) => (
                <option key={gw.id} value={gw.id}>GW{gw.number} (Live)</option>
              ))}
              {gameweeks.filter((g) => g.status !== GameweekStatus.Live).map((gw) => (
                <option key={gw.id} value={gw.id}>GW{gw.number}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm text-slate-400 mb-1">Search Player</label>
            <input
              type="text"
              placeholder="Type player name..."
              value={playerSearch}
              onChange={(e) => setPlayerSearch(e.target.value)}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-amber-400"
            />
          </div>
        </div>

        {/* Player select */}
        <div className="mb-4 max-h-40 overflow-y-auto bg-slate-700/50 rounded-lg p-2 space-y-1">
          {filteredPlayers.map((p) => (
            <button
              key={p.id}
              onClick={() => setSelectedPlayer(p.id)}
              className={`w-full text-left px-3 py-1.5 rounded text-sm transition ${
                selectedPlayer === p.id
                  ? 'bg-amber-500/20 text-amber-400'
                  : 'text-slate-300 hover:bg-slate-600'
              }`}
            >
              {p.name} <span className="text-slate-500">({p.team})</span>
            </button>
          ))}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
          <div>
            <label className="block text-sm text-slate-400 mb-1">Event Type</label>
            <select
              value={eventType}
              onChange={(e) => setEventType(Number(e.target.value))}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm"
            >
              {eventTypes.map((et) => (
                <option key={et.value} value={et.value}>{et.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm text-slate-400 mb-1">
              {eventType === 0 ? 'Minutes Played' : eventType === 9 ? 'Bonus Points (1-3)' : 'Minute'}
            </label>
            <input
              type="number"
              value={minute}
              onChange={(e) => setMinute(e.target.value)}
              placeholder={eventType === 0 ? '90' : eventType === 9 ? '1-3' : 'Optional'}
              className="w-full bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-amber-400"
            />
          </div>

          <div className="flex items-end">
            <button
              onClick={addEvent}
              className="w-full bg-amber-500 hover:bg-amber-600 text-black font-semibold py-2 rounded-lg transition"
            >
              Add Event
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
