import { useEffect, useState, useMemo } from 'react';
import api from '../api/client';
import { PlayerPosition, type Player, type Team } from '../api/types';
import { getTeamShort, getTeamDisplayName } from '../utils/teamAssets';

const posLabel = (p: PlayerPosition) => ['', 'GK', 'DEF', 'MID', 'FWD'][p];
const posColor = (p: PlayerPosition) =>
  ['', 'text-yellow-400', 'text-blue-400', 'text-green-400', 'text-red-400'][p];

const POS_LIMITS: Record<number, number> = {
  [PlayerPosition.GK]: 2,
  [PlayerPosition.DEF]: 5,
  [PlayerPosition.MID]: 5,
  [PlayerPosition.FWD]: 3,
};

export default function Transfers() {
  const [players, setPlayers] = useState<Player[]>([]);
  const [team, setTeam] = useState<Team | null>(null);
  const [search, setSearch] = useState('');
  const [posFilter, setPosFilter] = useState<string>('');
  const [teamFilter, setTeamFilter] = useState('');
  const [teams, setTeams] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState('totalPoints');
  const [message, setMessage] = useState('');
  const [selectedOut, setSelectedOut] = useState<number | null>(null);

  // Squad creation state — map persists across filter/page changes
  const [selectedMap, setSelectedMap] = useState<Record<number, Player>>({});
  const [creating, setCreating] = useState(false);
  const [teamName, setTeamName] = useState('');
  const [showSelected, setShowSelected] = useState(false);

  useEffect(() => {
    loadData();
  }, [search, posFilter, teamFilter, sortBy]);

  const loadData = async () => {
    const params = new URLSearchParams();
    if (search) params.set('search', search);
    if (posFilter) params.set('position', posFilter);
    if (teamFilter) params.set('team', teamFilter);
    params.set('sortBy', sortBy);
    params.set('pageSize', '100');

    const [playersRes, teamsRes] = await Promise.all([
      api.get<Player[]>(`/players?${params}`),
      api.get<string[]>('/players/teams'),
    ]);
    setPlayers(playersRes.data);
    setTeams(teamsRes.data);

    try {
      const teamRes = await api.get<Team>('/team');
      setTeam(teamRes.data);
    } catch {
      setTeam(null);
    }
  };

  // Derived counts from the persistent map
  const selected = useMemo(() => Object.values(selectedMap), [selectedMap]);
  const posCounts = useMemo(() => {
    const counts: Record<number, number> = { 1: 0, 2: 0, 3: 0, 4: 0 };
    for (const p of selected) counts[p.position] = (counts[p.position] || 0) + 1;
    return counts;
  }, [selected]);
  const teamCounts = useMemo(() => {
    const counts: Record<string, number> = {};
    for (const p of selected) counts[p.team] = (counts[p.team] || 0) + 1;
    return counts;
  }, [selected]);
  const totalCost = useMemo(() => selected.reduce((s, p) => s + p.price, 0), [selected]);

  const canAddPlayer = (player: Player) => {
    if (selected.length >= 15) return false;
    if ((posCounts[player.position] || 0) >= POS_LIMITS[player.position]) return false;
    if ((teamCounts[player.team] || 0) >= 3) return false;
    return true;
  };

  const togglePlayer = (player: Player) => {
    setSelectedMap((prev) => {
      if (prev[player.id]) {
        const next = { ...prev };
        delete next[player.id];
        return next;
      }
      if (!canAddPlayer(player)) return prev;
      return { ...prev, [player.id]: player };
    });
  };

  const removePlayer = (id: number) => {
    setSelectedMap((prev) => {
      const next = { ...prev };
      delete next[id];
      return next;
    });
  };

  const handleTransfer = async (playerInId: number) => {
    if (!selectedOut) {
      setMessage('Select a player to transfer out first');
      return;
    }
    try {
      const res = await api.post('/transfer', { playerOutId: selectedOut, playerInId });
      setMessage(res.data.message);
      setSelectedOut(null);
      loadData();
    } catch (err: any) {
      setMessage(err.response?.data?.message || 'Transfer failed');
    }
  };

  const handleCreateTeam = async () => {
    if (selected.length !== 15) {
      setMessage(`Select exactly 15 players. You have ${selected.length}.`);
      return;
    }
    if (!teamName.trim()) {
      setMessage('Enter a team name.');
      return;
    }
    if (totalCost > 100) {
      setMessage(`Total cost ${totalCost.toFixed(1)}M exceeds 100M budget.`);
      return;
    }

    setCreating(true);

    const gks = selected.filter((p) => p.position === PlayerPosition.GK);
    const defs = selected.filter((p) => p.position === PlayerPosition.DEF);
    const mids = selected.filter((p) => p.position === PlayerPosition.MID);
    const fwds = selected.filter((p) => p.position === PlayerPosition.FWD);

    // Starting: 1 GK, 4 DEF, 4 MID, 2 FWD — Bench: rest
    let pos = 1;
    const picks = [
      ...gks.slice(0, 1).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      ...defs.slice(0, 4).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      ...mids.slice(0, 4).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      ...fwds.slice(0, 2).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      // Bench
      ...gks.slice(1).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      ...defs.slice(4).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      ...mids.slice(4).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
      ...fwds.slice(2).map((p) => ({ playerId: p.id, squadPosition: pos++, isCaptain: false, isViceCaptain: false })),
    ];

    // Captain = highest-scoring starter, VC = second highest
    const startingPicks = picks.filter((p) => p.squadPosition <= 11);
    if (startingPicks.length > 0) {
      const best = startingPicks.reduce((a, b) => {
        const pa = selected.find((s) => s.id === a.playerId)!;
        const pb = selected.find((s) => s.id === b.playerId)!;
        return pa.totalPoints >= pb.totalPoints ? a : b;
      });
      best.isCaptain = true;

      const second = startingPicks.filter((p) => p !== best).reduce((a, b) => {
        const pa = selected.find((s) => s.id === a.playerId)!;
        const pb = selected.find((s) => s.id === b.playerId)!;
        return pa.totalPoints >= pb.totalPoints ? a : b;
      });
      if (second) second.isViceCaptain = true;
    }

    try {
      await api.post('/team', { teamName, picks });
      setMessage('Team created!');
      setSelectedMap({});
      loadData();
    } catch (err: any) {
      setMessage(err.response?.data || 'Failed to create team');
    } finally {
      setCreating(false);
    }
  };

  const isCreatingTeam = !team;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-white mb-2">
        {isCreatingTeam ? 'Pick Your Squad' : 'Transfer Market'}
      </h1>

      {/* Squad creation panel */}
      {isCreatingTeam && (
        <div className="bg-slate-800 rounded-xl p-4 mb-6 space-y-3">
          <div className="flex items-center gap-4 flex-wrap">
            <input
              type="text"
              placeholder="Team name..."
              value={teamName}
              onChange={(e) => setTeamName(e.target.value)}
              className="bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-emerald-400"
            />
            <button
              onClick={handleCreateTeam}
              disabled={creating || selected.length !== 15}
              className="ml-auto bg-emerald-500 hover:bg-emerald-600 disabled:bg-slate-600 text-white font-semibold px-6 py-2 rounded-lg text-sm transition"
            >
              {creating ? 'Creating...' : 'Create Team'}
            </button>
          </div>

          {/* Position counters */}
          <div className="flex items-center gap-3 flex-wrap">
            {([
              [PlayerPosition.GK, 'GK', 'text-yellow-400'],
              [PlayerPosition.DEF, 'DEF', 'text-blue-400'],
              [PlayerPosition.MID, 'MID', 'text-green-400'],
              [PlayerPosition.FWD, 'FWD', 'text-red-400'],
            ] as [PlayerPosition, string, string][]).map(([pos, label, color]) => {
              const count = posCounts[pos] || 0;
              const limit = POS_LIMITS[pos];
              const full = count >= limit;
              return (
                <span key={pos} className={`text-sm font-medium ${full ? color : 'text-slate-400'}`}>
                  {label}: <span className={full ? 'font-bold' : ''}>{count}/{limit}</span>
                </span>
              );
            })}
            <span className="text-slate-600">|</span>
            <span className="text-slate-400 text-sm">
              Players: <span className="text-white font-semibold">{selected.length}/15</span>
            </span>
            <span className="text-slate-600">|</span>
            <span className="text-slate-400 text-sm">
              Cost: <span className={`font-semibold ${totalCost > 100 ? 'text-red-400' : 'text-emerald-400'}`}>
                {totalCost.toFixed(1)}M / 100.0M
              </span>
            </span>
            <span className="text-slate-400 text-sm">
              Remaining: <span className="text-emerald-400 font-semibold">{(100 - totalCost).toFixed(1)}M</span>
            </span>
          </div>

          {/* Selected players summary */}
          {selected.length > 0 && (
            <div>
              <button
                onClick={() => setShowSelected(!showSelected)}
                className="text-xs text-slate-400 hover:text-white transition"
              >
                {showSelected ? 'Hide' : 'Show'} selected players ({selected.length})
              </button>
              {showSelected && (
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {selected
                    .sort((a, b) => a.position - b.position || a.name.localeCompare(b.name))
                    .map((p) => (
                      <span
                        key={p.id}
                        className="inline-flex items-center gap-1 bg-slate-700 text-xs text-white px-2 py-1 rounded"
                      >
                        <span className={posColor(p.position)}>{posLabel(p.position)}</span>
                        {p.name.split(' ').slice(-1)[0]}
                        <span className="text-slate-500">{getTeamShort(p.team)}</span>
                        <span className="text-slate-500">{p.price.toFixed(1)}</span>
                        <button
                          onClick={() => removePlayer(p.id)}
                          className="text-red-400 hover:text-red-300 ml-0.5"
                        >
                          &times;
                        </button>
                      </span>
                    ))}
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Transfer mode header */}
      {!isCreatingTeam && (
        <p className="text-slate-400 text-sm mb-6">
          Budget: <span className="text-emerald-400 font-semibold">{team.budget.toFixed(1)}M</span> |
          Free Transfers: <span className="text-emerald-400 font-semibold">{team.freeTransfers}</span>
          {selectedOut && (
            <span className="ml-4 text-amber-400">
              Transferring out: {team.picks.find((p) => p.playerId === selectedOut)?.playerName}
              <button onClick={() => setSelectedOut(null)} className="ml-2 text-red-400 hover:text-red-300">Cancel</button>
            </span>
          )}
        </p>
      )}

      {message && (
        <div className="bg-slate-800 border border-slate-600 text-slate-200 px-4 py-2 rounded-lg mb-4 text-sm">
          {message}
        </div>
      )}

      {/* Squad for transfer out */}
      {team && !selectedOut && (
        <div className="bg-slate-800 rounded-xl p-4 mb-6">
          <h3 className="text-sm font-semibold text-slate-400 mb-3">Select a player to transfer out:</h3>
          <div className="flex flex-wrap gap-2">
            {team.picks.map((p) => (
              <button
                key={p.playerId}
                onClick={() => setSelectedOut(p.playerId)}
                className="bg-slate-700 hover:bg-slate-600 text-white text-xs px-3 py-1.5 rounded-lg transition"
              >
                {p.playerName} ({posLabel(p.position)})
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="flex gap-3 mb-4 flex-wrap">
        <input
          type="text"
          placeholder="Search players..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-emerald-400 flex-1 min-w-[200px]"
        />
        <select
          value={posFilter}
          onChange={(e) => setPosFilter(e.target.value)}
          className="bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm"
        >
          <option value="">All Positions</option>
          <option value="1">GK</option>
          <option value="2">DEF</option>
          <option value="3">MID</option>
          <option value="4">FWD</option>
        </select>
        <select
          value={teamFilter}
          onChange={(e) => setTeamFilter(e.target.value)}
          className="bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm"
        >
          <option value="">All Teams</option>
          {teams.map((t) => (
            <option key={t} value={t}>{getTeamDisplayName(t)}</option>
          ))}
        </select>
        <select
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
          className="bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm"
        >
          <option value="totalPoints">Points</option>
          <option value="price">Price</option>
          <option value="name">Name</option>
        </select>
      </div>

      {/* Players table */}
      <div className="bg-slate-800 rounded-xl overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="text-slate-400 text-xs uppercase border-b border-slate-700">
              <th className="text-left px-4 py-3">Player</th>
              <th className="text-left px-4 py-3">Pos</th>
              <th className="text-left px-4 py-3">Team</th>
              <th className="text-right px-4 py-3">Price</th>
              <th className="text-right px-4 py-3">Points</th>
              <th className="text-right px-4 py-3">Action</th>
            </tr>
          </thead>
          <tbody>
            {players.map((player) => {
              const isSelected = !!selectedMap[player.id];
              const isInSquad = team?.picks.some((p) => p.playerId === player.id);
              const addable = !isSelected && canAddPlayer(player);

              return (
                <tr
                  key={player.id}
                  className={`border-b border-slate-700/50 hover:bg-slate-700/30 ${isSelected ? 'bg-emerald-500/10' : ''}`}
                >
                  <td className="px-4 py-3 text-white font-medium">
                    <div className="flex items-center gap-2">
                      <img
                        src={`/players/${player.id}.png`}
                        alt=""
                        className="w-8 h-8 rounded-full object-cover bg-slate-700 flex-shrink-0"
                        onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                      />
                      {player.name}
                    </div>
                  </td>
                  <td className={`px-4 py-3 text-sm font-medium ${posColor(player.position)}`}>
                    {posLabel(player.position)}
                  </td>
                  <td className="px-4 py-3 text-slate-400 text-sm">{getTeamDisplayName(player.team)}</td>
                  <td className="px-4 py-3 text-right text-slate-400">{player.price.toFixed(1)}</td>
                  <td className="px-4 py-3 text-right font-semibold text-white">{player.totalPoints}</td>
                  <td className="px-4 py-3 text-right">
                    {isCreatingTeam ? (
                      isSelected ? (
                        <button
                          onClick={() => removePlayer(player.id)}
                          className="text-xs px-3 py-1 rounded font-semibold bg-red-500/20 text-red-400 hover:bg-red-500/30 transition"
                        >
                          Remove
                        </button>
                      ) : (
                        <button
                          onClick={() => togglePlayer(player)}
                          disabled={!addable}
                          className={`text-xs px-3 py-1 rounded font-semibold transition ${
                            addable
                              ? 'bg-emerald-500/20 text-emerald-400 hover:bg-emerald-500/30'
                              : 'bg-slate-700/50 text-slate-600 cursor-not-allowed'
                          }`}
                        >
                          {!addable && (posCounts[player.position] || 0) >= POS_LIMITS[player.position]
                            ? `${posLabel(player.position)} Full`
                            : !addable && (teamCounts[player.team] || 0) >= 3
                            ? '3/team'
                            : 'Add'}
                        </button>
                      )
                    ) : isInSquad ? (
                      <span className="text-xs text-slate-500">In Squad</span>
                    ) : selectedOut ? (
                      <button
                        onClick={() => handleTransfer(player.id)}
                        className="text-xs px-3 py-1 rounded font-semibold bg-emerald-500/20 text-emerald-400 hover:bg-emerald-500/30 transition"
                      >
                        Transfer In
                      </button>
                    ) : null}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
