import { useEffect, useState, useRef, useMemo } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import api from '../api/client';
import {
  DraftStatus, PlayerPosition,
  type DraftState, type AvailablePlayer, type PickBroadcast,
} from '../api/types';

const POS_LABEL: Record<number, string> = { 1: 'GK', 2: 'DEF', 3: 'MID', 4: 'FWD' };
const POS_COLOR: Record<number, string> = {
  1: 'text-yellow-400',
  2: 'text-blue-400',
  3: 'text-green-400',
  4: 'text-red-400',
};

export default function Draft() {
  const { leagueId: leagueIdParam } = useParams<{ leagueId: string }>();
  const navigate = useNavigate();
  const leagueId = parseInt(leagueIdParam ?? '0', 10);

  const [state, setState] = useState<DraftState | null>(null);
  const [available, setAvailable] = useState<AvailablePlayer[]>([]);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [now, setNow] = useState(Date.now());
  const [posFilter, setPosFilter] = useState<PlayerPosition | 0>(0);
  const [search, setSearch] = useState('');

  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // Load initial state + available players.
  const refresh = async () => {
    try {
      const [stateRes, avRes] = await Promise.all([
        api.get<DraftState>(`/draft/${leagueId}/state`),
        api.get<AvailablePlayer[]>(`/draft/${leagueId}/available`),
      ]);
      setState(stateRes.data);
      setAvailable(avRes.data);
      setError('');
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to load draft');
    }
  };

  useEffect(() => {
    if (!leagueId) return;
    refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [leagueId]);

  // SignalR live updates.
  useEffect(() => {
    if (!leagueId) return;
    const token = sessionStorage.getItem('fbl_token');
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/draft?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    conn.on('DraftStarted', () => refresh());
    conn.on('DraftReset', () => refresh());
    conn.on('PickMade', (b: PickBroadcast) => {
      // Optimistic UI: append the pick, advance the clock, drop player from available.
      setState(prev => prev ? {
        ...prev,
        picks: [...prev.picks, b.pick],
        currentPickNumber: b.nextPickNumber || prev.totalPicks + 1,
        currentPickerUserId: b.nextPickerUserId,
        currentPickerName: prev.members.find(m => m.userId === b.nextPickerUserId)?.displayName ?? null,
        currentPickDeadline: b.nextPickDeadline,
        myTurn: !!b.nextPickerUserId && b.nextPickerUserId === prev.members.find(m => m.userId === prev.currentPickerUserId && m.userId === prev.currentPickerUserId)?.userId
          ? false
          : false,
        status: b.status,
      } : prev);
      // Re-fetch the canonical state for accuracy (myTurn flag, pick count per member, etc.).
      refresh();
    });
    conn.on('DraftCompleted', () => refresh());

    conn.start()
      .then(() => conn.invoke('JoinDraft', leagueId))
      .catch(() => {});
    connectionRef.current = conn;

    return () => {
      conn.invoke('LeaveDraft', leagueId).catch(() => {});
      conn.stop();
    };
  }, [leagueId]);

  // 1Hz tick for the countdown timer.
  useEffect(() => {
    const t = setInterval(() => setNow(Date.now()), 500);
    return () => clearInterval(t);
  }, []);

  const startDraft = async () => {
    setBusy(true);
    try {
      const res = await api.post<DraftState>(`/draft/${leagueId}/start`);
      setState(res.data);
      refresh();
    } catch (e: any) {
      setError(e.response?.data?.message || 'Could not start draft');
    } finally { setBusy(false); }
  };

  const makePick = async (playerId: number) => {
    if (!state?.myTurn || busy) return;
    setBusy(true);
    try {
      await api.post(`/draft/${leagueId}/pick`, { playerId });
      // PickMade broadcast will refresh us.
    } catch (e: any) {
      setError(e.response?.data?.message || 'Pick failed');
    } finally { setBusy(false); }
  };

  const resetDraft = async () => {
    if (!state?.isCreator) return;
    if (!confirm('Reset the draft? This wipes all picks and the auto-created teams. Members stay.')) return;
    setBusy(true);
    try {
      await api.post(`/draft/${leagueId}/reset`);
      refresh();
    } catch (e: any) {
      setError(e.response?.data?.message || 'Reset failed');
    } finally { setBusy(false); }
  };

  // ---- Derived ----
  const filtered = useMemo(() => {
    let list = available;
    if (posFilter) list = list.filter(p => p.position === posFilter);
    if (search) {
      const q = search.toLowerCase();
      list = list.filter(p => p.name.toLowerCase().includes(q) || p.team.toLowerCase().includes(q));
    }
    return list.slice(0, 100);
  }, [available, posFilter, search]);

  const secondsLeft = useMemo(() => {
    if (!state?.currentPickDeadline) return null;
    return Math.max(0, Math.ceil((new Date(state.currentPickDeadline).getTime() - now) / 1000));
  }, [state?.currentPickDeadline, now]);

  if (!leagueId) return <div className="text-center py-16 text-red-400">Invalid league</div>;
  if (!state) {
    return (
      <div className="text-center py-16 text-slate-400">
        {error || 'Loading draft...'}
      </div>
    );
  }

  // ---- Render branches per status ----
  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <Link to="/leagues" className="text-slate-400 hover:text-white text-sm">&lsaquo; Back to leagues</Link>
          <h1 className="text-2xl font-bold text-white mt-1">{state.leagueName}</h1>
          <p className="text-slate-400 text-sm">Draft room &middot; {state.members.length}/{state.maxMembers} managers</p>
        </div>
        {state.isCreator && state.status !== DraftStatus.Pending && (
          <button
            onClick={resetDraft}
            disabled={busy}
            className="bg-red-500/20 border border-red-500/40 hover:bg-red-500/30 text-red-300 hover:text-red-200 text-xs font-semibold px-3 py-1.5 rounded-lg transition disabled:opacity-50"
            title="Wipes all picks and auto-created teams; restarts the draft"
          >
            Redraft
          </button>
        )}
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/30 text-red-400 px-4 py-2 rounded-lg mb-4 text-sm">
          {error}
          <button onClick={() => setError('')} className="float-right text-red-300 hover:text-white">&times;</button>
        </div>
      )}

      {state.status === DraftStatus.Pending && (
        <PendingPanel state={state} onStart={startDraft} busy={busy} />
      )}

      {state.status === DraftStatus.InProgress && (
        <InProgressPanel
          state={state}
          available={filtered}
          fullCount={available.length}
          secondsLeft={secondsLeft}
          posFilter={posFilter}
          setPosFilter={setPosFilter}
          search={search}
          setSearch={setSearch}
          makePick={makePick}
          busy={busy}
        />
      )}

      {state.status === DraftStatus.Completed && (
        <CompletedPanel state={state} onViewTeam={() => navigate(`/my-team?leagueId=${leagueId}`)} />
      )}
    </div>
  );
}

// ---- Sub-components ----

function PendingPanel({ state, onStart, busy }: {
  state: DraftState; onStart: () => void; busy: boolean;
}) {
  const canStart = state.isCreator && state.members.length >= 2;
  return (
    <div className="bg-slate-800 rounded-xl p-6 max-w-2xl">
      <h2 className="text-lg font-semibold text-white mb-2">Waiting for the draft to start</h2>
      <p className="text-slate-400 text-sm mb-4">
        {state.isCreator
          ? `You're the league creator — start when everyone has joined.`
          : `The league creator will start the draft when ready.`}
      </p>
      <div className="bg-slate-900 rounded-lg p-3 mb-4">
        <p className="text-xs uppercase tracking-wider text-slate-500 mb-2">Joined ({state.members.length}/{state.maxMembers})</p>
        <ul className="space-y-1">
          {state.members.map((m, i) => (
            <li key={m.userId} className="text-sm text-white">
              <span className="text-slate-500 mr-2">#{i + 1}</span>
              {m.displayName}
            </li>
          ))}
        </ul>
      </div>
      {state.isCreator && (
        <button
          onClick={onStart}
          disabled={!canStart || busy}
          className="bg-emerald-500 hover:bg-emerald-600 disabled:bg-slate-700 disabled:text-slate-500 text-white font-semibold px-5 py-2 rounded-lg text-sm transition"
        >
          {busy ? 'Starting...' : canStart ? 'Start Draft' : 'Need at least 2 managers'}
        </button>
      )}
    </div>
  );
}

function InProgressPanel({
  state, available, fullCount, secondsLeft, posFilter, setPosFilter,
  search, setSearch, makePick, busy,
}: {
  state: DraftState;
  available: AvailablePlayer[];
  fullCount: number;
  secondsLeft: number | null;
  posFilter: PlayerPosition | 0;
  setPosFilter: (p: PlayerPosition | 0) => void;
  search: string;
  setSearch: (s: string) => void;
  makePick: (id: number) => void;
  busy: boolean;
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
      {/* LEFT: clock + members + my squad so far */}
      <div className="lg:col-span-1 space-y-4">
        <div className="bg-slate-800 rounded-xl p-4">
          <p className="text-xs uppercase tracking-wider text-slate-500 mb-1">Pick {state.currentPickNumber} of {state.totalPicks}</p>
          <p className="text-lg font-semibold text-white">
            {state.myTurn
              ? <span className="text-emerald-400">You're on the clock</span>
              : <>On the clock: <span className="text-amber-400">{state.currentPickerName}</span></>}
          </p>
          <p className="text-3xl font-bold tabular-nums mt-2 text-white">
            {secondsLeft != null ? `${secondsLeft}s` : '--'}
          </p>
          <p className="text-xs text-slate-500 mt-1">Round {state.currentRound}</p>
        </div>

        <DraftBoard state={state} />
      </div>

      {/* RIGHT: available players */}
      <div className="lg:col-span-2 bg-slate-800 rounded-xl p-4">
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-sm uppercase tracking-wider text-slate-400 font-semibold">Available Players ({fullCount})</h2>
        </div>
        <div className="flex gap-2 mb-3 flex-wrap">
          <input
            type="text" placeholder="Search..." value={search}
            onChange={e => setSearch(e.target.value)}
            className="bg-slate-900 border border-slate-700 rounded-lg px-3 py-1.5 text-white text-sm flex-1 min-w-[200px] focus:outline-none focus:border-emerald-400"
          />
          <select
            value={posFilter}
            onChange={e => setPosFilter(parseInt(e.target.value) as PlayerPosition | 0)}
            className="bg-slate-900 border border-slate-700 rounded-lg px-3 py-1.5 text-white text-sm focus:outline-none focus:border-emerald-400"
          >
            <option value={0}>All</option>
            <option value={1}>GK</option>
            <option value={2}>DEF</option>
            <option value={3}>MID</option>
            <option value={4}>FWD</option>
          </select>
        </div>
        <div className="overflow-y-auto max-h-[60vh]">
          <table className="w-full text-sm">
            <thead className="sticky top-0 bg-slate-800">
              <tr className="text-slate-500 text-xs uppercase border-b border-slate-700">
                <th className="text-left px-3 py-2">Player</th>
                <th className="text-center px-2 py-2">Pos</th>
                <th className="text-left px-2 py-2">Team</th>
                <th className="text-right px-2 py-2">Pts</th>
                <th className="text-right px-2 py-2 w-24"></th>
              </tr>
            </thead>
            <tbody>
              {available.length === 0 && (
                <tr><td colSpan={5} className="text-center text-slate-500 py-8">No players match.</td></tr>
              )}
              {available.map(p => (
                <tr key={p.id} className="border-b border-slate-700/40 hover:bg-slate-700/20">
                  <td className="px-3 py-2 text-white">{p.name}</td>
                  <td className={`px-2 py-2 text-center text-xs font-bold ${POS_COLOR[p.position]}`}>{POS_LABEL[p.position]}</td>
                  <td className="px-2 py-2 text-slate-400">{p.team}</td>
                  <td className="px-2 py-2 text-right tabular-nums text-white font-semibold">{p.totalPoints}</td>
                  <td className="px-2 py-2 text-right">
                    <button
                      onClick={() => makePick(p.id)}
                      disabled={!state.myTurn || busy}
                      className="text-xs px-3 py-1 rounded font-semibold bg-emerald-500 hover:bg-emerald-600 disabled:bg-slate-700 disabled:text-slate-500 text-white transition"
                    >
                      Pick
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

/** Draft board: shows each manager and the players they've drafted, in pick order. */
function DraftBoard({ state }: { state: DraftState }) {
  const picksByUser = useMemo(() => {
    const map: Record<string, typeof state.picks> = {};
    for (const m of state.members) map[m.userId] = [];
    for (const p of state.picks) (map[p.userId] = map[p.userId] || []).push(p);
    return map;
  }, [state.members, state.picks]);

  return (
    <div className="bg-slate-800 rounded-xl p-4">
      <h3 className="text-sm uppercase tracking-wider text-slate-400 font-semibold mb-3">Draft Board</h3>
      <div className="space-y-3">
        {state.members.map((m) => {
          const isCurrent = m.userId === state.currentPickerUserId;
          const myPicks = picksByUser[m.userId] || [];
          return (
            <div
              key={m.userId}
              className={`bg-slate-900 rounded-lg p-2 border-l-4 ${isCurrent ? 'border-emerald-400' : 'border-transparent'}`}
            >
              <p className="text-xs font-semibold text-white mb-1">
                {m.displayName} <span className="text-slate-500 font-normal">— {myPicks.length}/15</span>
              </p>
              <div className="flex flex-wrap gap-1">
                {myPicks.map(p => (
                  <span key={p.pickNumber} className={`text-[10px] px-1.5 py-0.5 rounded bg-slate-800 ${POS_COLOR[p.position]}`}>
                    {p.playerName.split(' ').slice(-1)[0]}
                    {p.wasAutoPick && <span className="text-slate-500 ml-1">(auto)</span>}
                  </span>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function CompletedPanel({ state, onViewTeam }: { state: DraftState; onViewTeam: () => void }) {
  return (
    <div className="bg-slate-800 rounded-xl p-6 text-center">
      <h2 className="text-2xl font-bold text-white mb-2">Draft complete! 🎉</h2>
      <p className="text-slate-400 mb-4">All {state.totalPicks} picks made. Teams have been created.</p>
      <button
        onClick={onViewTeam}
        className="bg-emerald-500 hover:bg-emerald-600 text-white font-semibold px-5 py-2 rounded-lg text-sm transition"
      >
        View Your Team
      </button>
    </div>
  );
}
