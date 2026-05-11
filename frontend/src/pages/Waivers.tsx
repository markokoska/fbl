import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import api from '../api/client';
import {
  PlayerPosition, WaiverClaimStatus, WaiverPhase,
  type WaiverState, type FreeAgent, type Team,
} from '../api/types';
import { LockIcon, ChevronUpIcon, ChevronDownIcon, XCircleIcon, CheckCircleIcon } from '../components/icons';

const POS_LABEL: Record<number, string> = { 1: 'GK', 2: 'DEF', 3: 'MID', 4: 'FWD' };
const POS_COLOR: Record<number, string> = {
  1: 'text-yellow-400', 2: 'text-blue-400', 3: 'text-green-400', 4: 'text-red-400',
};

export default function Waivers() {
  const { leagueId: p } = useParams<{ leagueId: string }>();
  const leagueId = parseInt(p ?? '0', 10);

  const [state, setState] = useState<WaiverState | null>(null);
  const [team, setTeam] = useState<Team | null>(null);
  const [freeAgents, setFreeAgents] = useState<FreeAgent[]>([]);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [posFilter, setPosFilter] = useState<PlayerPosition | 0>(0);
  const [search, setSearch] = useState('');
  const [selectedOut, setSelectedOut] = useState<number | null>(null);

  const refresh = async () => {
    try {
      const [stateRes, teamRes, faRes] = await Promise.all([
        api.get<WaiverState>(`/waiver/${leagueId}/state`),
        api.get<Team>(`/team?leagueId=${leagueId}`),
        api.get<FreeAgent[]>(`/waiver/${leagueId}/free-agents`),
      ]);
      setState(stateRes.data);
      setTeam(teamRes.data);
      setFreeAgents(faRes.data);
      setError('');
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to load waivers');
    }
  };

  useEffect(() => {
    if (leagueId) refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [leagueId]);

  // Filter free agents by position of selected playerOut, plus user filters.
  const selectedOutPick = team?.picks.find(p => p.playerId === selectedOut);
  const filteredFreeAgents = useMemo(() => {
    let list = freeAgents;
    if (selectedOutPick) {
      list = list.filter(p => p.position === selectedOutPick.position);
    } else if (posFilter) {
      list = list.filter(p => p.position === posFilter);
    }
    if (search) {
      const q = search.toLowerCase();
      list = list.filter(p => p.name.toLowerCase().includes(q) || p.team.toLowerCase().includes(q));
    }
    return list.slice(0, 100);
  }, [freeAgents, posFilter, search, selectedOutPick]);

  const addClaim = async (playerInId: number) => {
    if (!selectedOut) {
      setError('Select a player to swap out first.');
      return;
    }
    setBusy(true);
    try {
      await api.post(`/waiver/${leagueId}/claims`, { playerOutId: selectedOut, playerInId });
      setSelectedOut(null);
      refresh();
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to add claim');
    } finally { setBusy(false); }
  };

  const directSwap = async (playerInId: number) => {
    if (!selectedOut) { setError('Select a player to swap out first.'); return; }
    setBusy(true);
    try {
      await api.post(`/waiver/${leagueId}/swap`, { playerOutId: selectedOut, playerInId });
      setSelectedOut(null);
      refresh();
    } catch (e: any) {
      setError(e.response?.data?.message || 'Swap failed');
    } finally { setBusy(false); }
  };

  const removeClaim = async (id: number) => {
    setBusy(true);
    try {
      await api.delete(`/waiver/${leagueId}/claims/${id}`);
      refresh();
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to remove claim');
    } finally { setBusy(false); }
  };

  const reorder = async (claimId: number, direction: -1 | 1) => {
    if (!state) return;
    const ids = state.myClaims.map(c => c.id);
    const idx = ids.indexOf(claimId);
    const swapWith = idx + direction;
    if (idx < 0 || swapWith < 0 || swapWith >= ids.length) return;
    [ids[idx], ids[swapWith]] = [ids[swapWith], ids[idx]];
    setBusy(true);
    try {
      await api.put(`/waiver/${leagueId}/claims/reorder`, { claimIdsInOrder: ids });
      refresh();
    } catch (e: any) {
      setError(e.response?.data?.message || 'Reorder failed');
    } finally { setBusy(false); }
  };

  if (!state || !team) {
    return <div className="text-center py-16 text-slate-400">{error || 'Loading...'}</div>;
  }

  const phaseLabel = {
    [WaiverPhase.Queue]: 'Waiver Queue Open',
    [WaiverPhase.FreeAgency]: 'Free Agency',
    [WaiverPhase.Locked]: 'Locked',
    [WaiverPhase.NoDraftYet]: 'Draft not complete',
  }[state.phase];

  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-4 flex-wrap gap-3">
        <div>
          <Link to="/leagues" className="text-slate-400 hover:text-white text-sm">&lsaquo; Back to leagues</Link>
          <h1 className="text-2xl font-bold text-white mt-1">{state.leagueName} — Waivers</h1>
          <p className="text-slate-400 text-sm">
            <span className={`inline-block px-2 py-0.5 rounded text-xs font-bold uppercase ${
              state.phase === WaiverPhase.Queue ? 'bg-amber-500/20 text-amber-300' :
              state.phase === WaiverPhase.FreeAgency ? 'bg-emerald-500/20 text-emerald-300' :
              'bg-slate-500/20 text-slate-300'
            }`}>{phaseLabel}</span>
            {state.upcomingGameweekNumber && (
              <span className="ml-2">For GW{state.upcomingGameweekNumber}</span>
            )}
          </p>
        </div>
        <Link to={`/my-team?leagueId=${leagueId}`} className="text-slate-400 hover:text-white text-sm">View squad &rsaquo;</Link>
      </div>

      {/* Phase explainer */}
      <div className="bg-slate-800 rounded-xl p-3 mb-4 text-sm text-slate-300">
        {state.phase === WaiverPhase.Queue && (
          <>
            Add claims now. They'll resolve <strong>{state.processAt ? `at ${new Date(state.processAt).toLocaleString()}` : '24h before kickoff'}</strong> in
            reverse standings order. After that, free agency opens for any remaining unowned players.
          </>
        )}
        {state.phase === WaiverPhase.FreeAgency && <>Queue resolved. Any player not currently rostered is yours for the taking — first-come, first-served.</>}
        {state.phase === WaiverPhase.Locked && <>Locked until the next gameweek.</>}
        {state.phase === WaiverPhase.NoDraftYet && <>Waivers will open once your league's draft completes.</>}
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/30 text-red-400 px-3 py-2 rounded-lg mb-3 text-sm flex justify-between">
          <span>{error}</span>
          <button onClick={() => setError('')} className="text-red-300 hover:text-white">&times;</button>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* LEFT: my squad (click to mark as out) + my queue */}
        <div className="lg:col-span-1 space-y-4">
          <div className="bg-slate-800 rounded-xl p-3">
            <h3 className="text-xs uppercase tracking-wider text-slate-500 mb-2 font-semibold">My Squad — click to swap out</h3>
            <ul className="space-y-1">
              {team.picks.sort((a, b) => a.squadPosition - b.squadPosition).map(p => {
                const isSelected = selectedOut === p.playerId;
                return (
                  <li key={p.playerId}>
                    <button
                      onClick={() => setSelectedOut(isSelected ? null : p.playerId)}
                      className={`w-full text-left px-2 py-1.5 rounded text-xs flex items-center gap-2 transition ${
                        isSelected ? 'bg-amber-500/20 text-amber-300 ring-1 ring-amber-400'
                        : 'bg-slate-900 text-white hover:bg-slate-700'
                      }`}
                    >
                      <span className={`font-bold ${POS_COLOR[p.position]} w-8`}>{POS_LABEL[p.position]}</span>
                      <span className="flex-1 truncate">{p.playerName}</span>
                      <span className="text-slate-500 text-[10px]">{p.team}</span>
                    </button>
                  </li>
                );
              })}
            </ul>
          </div>

          {/* My queued claims */}
          {state.phase === WaiverPhase.Queue && (
            <div className="bg-slate-800 rounded-xl p-3">
              <h3 className="text-xs uppercase tracking-wider text-slate-500 mb-2 font-semibold">My Queue ({state.myClaims.length})</h3>
              {state.myClaims.length === 0 ? (
                <p className="text-slate-500 text-xs">No claims yet. Click a player on your squad, then a free agent on the right.</p>
              ) : (
                <ol className="space-y-1.5">
                  {state.myClaims.map((c, i) => (
                    <li key={c.id} className="bg-slate-900 rounded p-2 text-xs">
                      <div className="flex items-center justify-between gap-2 mb-1">
                        <span className="text-slate-400 font-bold">#{i + 1}</span>
                        <div className="flex items-center gap-1">
                          <button onClick={() => reorder(c.id, -1)} disabled={busy || i === 0}
                            className="text-slate-500 hover:text-white disabled:opacity-30 p-0.5 cursor-pointer disabled:cursor-not-allowed transition-colors"
                            aria-label="Move claim up">
                            <ChevronUpIcon className="w-3.5 h-3.5" />
                          </button>
                          <button onClick={() => reorder(c.id, 1)} disabled={busy || i === state.myClaims.length - 1}
                            className="text-slate-500 hover:text-white disabled:opacity-30 p-0.5 cursor-pointer disabled:cursor-not-allowed transition-colors"
                            aria-label="Move claim down">
                            <ChevronDownIcon className="w-3.5 h-3.5" />
                          </button>
                          <button onClick={() => removeClaim(c.id)} disabled={busy}
                            className="text-red-400 hover:text-red-300 p-0.5 cursor-pointer transition-colors"
                            aria-label="Remove claim">
                            <XCircleIcon className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </div>
                      <div className="text-white">
                        <span className="text-red-400">OUT</span> {c.playerOutName}
                      </div>
                      <div className="text-white">
                        <span className="text-emerald-400">IN</span> {c.playerInName}
                      </div>
                    </li>
                  ))}
                </ol>
              )}
            </div>
          )}

          {/* Last resolved claims (history) */}
          {state.recentResolved.length > 0 && (
            <div className="bg-slate-800 rounded-xl p-3">
              <h3 className="text-xs uppercase tracking-wider text-slate-500 mb-2 font-semibold">Last Resolved</h3>
              <ul className="space-y-1.5">
                {state.recentResolved.map(c => (
                  <li key={c.id} className="bg-slate-900 rounded p-2 text-xs">
                    <div className="flex items-center gap-2 mb-1">
                      <span className={`inline-flex items-center gap-1 text-[10px] uppercase font-bold px-1.5 py-0.5 rounded ${
                        c.status === WaiverClaimStatus.Succeeded ? 'bg-emerald-500/20 text-emerald-400' : 'bg-red-500/20 text-red-400'
                      }`}>
                        {c.status === WaiverClaimStatus.Succeeded
                          ? <><CheckCircleIcon className="w-3 h-3" /> Won</>
                          : <><XCircleIcon className="w-3 h-3" /> Lost</>}
                      </span>
                      <span className="text-slate-500">{c.processedAt ? new Date(c.processedAt).toLocaleDateString() : ''}</span>
                    </div>
                    <div className="text-slate-400">
                      {c.playerOutName} → {c.playerInName}
                    </div>
                    {c.failureReason && <p className="text-red-400/80 text-[10px] mt-0.5">{c.failureReason}</p>}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>

        {/* RIGHT: free agents (clickable based on phase) */}
        <div className="lg:col-span-2 bg-slate-800 rounded-xl p-3">
          <div className="flex items-center justify-between mb-3 flex-wrap gap-2">
            <h2 className="text-xs uppercase tracking-wider text-slate-400 font-semibold">
              Available Players ({freeAgents.length})
            </h2>
            {selectedOutPick && (
              <p className="text-xs text-slate-400">
                Filtering to <span className={`font-bold ${POS_COLOR[selectedOutPick.position]}`}>{POS_LABEL[selectedOutPick.position]}s</span> only (must match position).
              </p>
            )}
          </div>
          <div className="flex gap-2 mb-3 flex-wrap">
            <input type="text" placeholder="Search..." value={search} onChange={e => setSearch(e.target.value)}
              className="bg-slate-900 border border-slate-700 rounded-lg px-3 py-1.5 text-white text-sm flex-1 min-w-[200px] focus:outline-none focus:border-emerald-400" />
            {!selectedOutPick && (
              <select value={posFilter} onChange={e => setPosFilter(parseInt(e.target.value) as PlayerPosition | 0)}
                className="bg-slate-900 border border-slate-700 rounded-lg px-3 py-1.5 text-white text-sm focus:outline-none focus:border-emerald-400">
                <option value={0}>All</option>
                <option value={1}>GK</option>
                <option value={2}>DEF</option>
                <option value={3}>MID</option>
                <option value={4}>FWD</option>
              </select>
            )}
          </div>
          <div className="overflow-y-auto max-h-[60vh]">
            <table className="w-full text-sm">
              <thead className="sticky top-0 bg-slate-800">
                <tr className="text-slate-500 text-xs uppercase border-b border-slate-700">
                  <th className="text-left px-3 py-2">Player</th>
                  <th className="text-center px-2 py-2">Pos</th>
                  <th className="text-left px-2 py-2">Team</th>
                  <th className="text-right px-2 py-2">Pts</th>
                  <th className="text-right px-2 py-2 w-28"></th>
                </tr>
              </thead>
              <tbody>
                {filteredFreeAgents.length === 0 && (
                  <tr><td colSpan={5} className="text-center text-slate-500 py-8">No matches.</td></tr>
                )}
                {filteredFreeAgents.map(p => {
                  const enabled = !!selectedOut && !busy && !p.isLocked &&
                    (state.phase === WaiverPhase.Queue || state.phase === WaiverPhase.FreeAgency);
                  const isFA = state.phase === WaiverPhase.FreeAgency;
                  return (
                    <tr key={p.id} className={`border-b border-slate-700/40 hover:bg-slate-700/20 ${p.isLocked ? 'opacity-60' : ''}`}>
                      <td className="px-3 py-2 text-white">{p.name}</td>
                      <td className={`px-2 py-2 text-center text-xs font-bold ${POS_COLOR[p.position]}`}>{POS_LABEL[p.position]}</td>
                      <td className="px-2 py-2 text-slate-400">{p.team}</td>
                      <td className="px-2 py-2 text-right tabular-nums text-white font-semibold">{p.totalPoints}</td>
                      <td className="px-2 py-2 text-right">
                        {p.isLocked ? (
                          <span
                            className="inline-flex items-center gap-1 text-xs px-2.5 py-1 rounded font-semibold bg-slate-700/50 text-slate-400 cursor-not-allowed"
                            title={p.droppedByName ? `Dropped by ${p.droppedByName} this GW — locked until next GW` : 'Locked until next GW'}
                          >
                            <LockIcon className="w-3 h-3" /> Locked
                          </span>
                        ) : (
                          <button
                            onClick={() => isFA ? directSwap(p.id) : addClaim(p.id)}
                            disabled={!enabled}
                            className="text-xs px-3 py-1 rounded font-semibold bg-emerald-500 hover:bg-emerald-600 disabled:bg-slate-700 disabled:text-slate-500 text-white transition"
                          >
                            {isFA ? 'Swap In' : 'Queue'}
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
