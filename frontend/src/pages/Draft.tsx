import { useEffect, useState, useRef, useMemo } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import api from '../api/client';
import {
  DraftStatus, PlayerPosition,
  type DraftState, type AvailablePlayer, type PickBroadcast,
} from '../api/types';
import { ClockIcon, FlameIcon, TrophyIcon } from '../components/icons';

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
      <div className="flex items-center justify-between mb-6 flex-wrap gap-3">
        <div>
          <Link to="/leagues" className="text-slate-400 hover:text-white text-sm transition-colors">&lsaquo; Back to leagues</Link>
          <h1 className="heading-display text-3xl text-white mt-2">{state.leagueName}</h1>
          <p className="text-slate-400 text-sm mt-1">
            Draft Room &middot; <span className="text-emerald-400 font-semibold">{state.members.length}/{state.maxMembers}</span> managers
          </p>
        </div>
        {state.isCreator && state.status !== DraftStatus.Pending && (
          <button
            onClick={resetDraft}
            disabled={busy}
            className="bg-red-500/10 border border-red-500/40 hover:bg-red-500/20 text-red-300 hover:text-red-200 text-xs font-semibold uppercase tracking-wider px-4 py-2 rounded-lg transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
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
  const slotsLeft = Math.max(0, state.maxMembers - state.members.length);
  return (
    <div className="bg-slate-800 rounded-2xl card-glow p-6 max-w-2xl">
      <h2 className="heading-display text-2xl text-white mb-2">Lobby</h2>
      <p className="text-slate-400 text-sm mb-5">
        {state.isCreator
          ? `You're the league creator — start the draft when everyone has joined.`
          : `Waiting for the league creator to start the draft.`}
      </p>

      <div className="bg-slate-900/60 rounded-xl p-4 mb-5">
        <div className="flex items-center justify-between mb-3">
          <p className="text-xs uppercase tracking-[0.2em] text-slate-500 font-bold">Joined</p>
          <p className="text-xs tabular-nums text-slate-400">
            <span className="text-emerald-400 font-bold">{state.members.length}</span>
            <span className="text-slate-600">/{state.maxMembers}</span>
          </p>
        </div>
        <ul className="space-y-1.5">
          {state.members.map((m, i) => (
            <li key={m.userId} className="flex items-center gap-3 text-sm">
              <span className="heading-display text-xs text-slate-600 w-5 text-center">{i + 1}</span>
              <span className="text-white font-medium">{m.displayName}</span>
            </li>
          ))}
          {Array.from({ length: slotsLeft }).map((_, i) => (
            <li key={`empty-${i}`} className="flex items-center gap-3 text-sm">
              <span className="heading-display text-xs text-slate-700 w-5 text-center">{state.members.length + i + 1}</span>
              <span className="text-slate-600 italic">Empty slot</span>
            </li>
          ))}
        </ul>
      </div>

      {state.isCreator && (
        <button
          onClick={onStart}
          disabled={!canStart || busy}
          className="w-full sm:w-auto bg-emerald-500 hover:bg-emerald-400 disabled:bg-slate-700 disabled:text-slate-500 text-white heading-display text-sm px-6 py-3 rounded-lg transition-all duration-200 cursor-pointer disabled:cursor-not-allowed shadow-[0_0_20px_rgba(34,197,94,0.4)] disabled:shadow-none"
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
  // Circular progress ring around the timer.
  const pct = secondsLeft != null && state.pickSeconds > 0
    ? Math.max(0, Math.min(1, secondsLeft / state.pickSeconds))
    : 1;
  const ringSize = 96;
  const strokeWidth = 6;
  const radius = (ringSize - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const dashOffset = circumference * (1 - pct);
  const urgent = secondsLeft != null && secondsLeft <= 10;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
      {/* LEFT: clock + draft board */}
      <div className="lg:col-span-1 space-y-4">
        {/* Hero clock card */}
        <div className={`card-glow rounded-2xl p-5 text-center ${state.myTurn ? 'bg-gradient-to-br from-emerald-500/10 to-slate-800 border-emerald-400/40' : 'bg-slate-800'}`}>
          <p className="text-[10px] uppercase tracking-[0.2em] text-slate-500 mb-1">
            Pick {state.currentPickNumber} of {state.totalPicks} &middot; Round {state.currentRound}
          </p>
          <p className="heading-display text-base mb-3">
            {state.myTurn
              ? <span className="text-emerald-400">YOU'RE ON THE CLOCK</span>
              : <span className="text-slate-300">{state.currentPickerName}</span>}
          </p>

          {/* Circular timer */}
          <div className="relative inline-flex items-center justify-center" style={{ width: ringSize, height: ringSize }}>
            <svg width={ringSize} height={ringSize} className="-rotate-90 absolute">
              <circle
                cx={ringSize / 2} cy={ringSize / 2} r={radius}
                stroke="rgb(51 65 85)" strokeWidth={strokeWidth} fill="none"
              />
              <circle
                cx={ringSize / 2} cy={ringSize / 2} r={radius}
                stroke={urgent ? 'rgb(239 68 68)' : state.myTurn ? 'rgb(34 197 94)' : 'rgb(245 158 11)'}
                strokeWidth={strokeWidth}
                strokeDasharray={circumference}
                strokeDashoffset={dashOffset}
                strokeLinecap="round"
                fill="none"
                className="transition-all duration-500 ease-linear"
              />
            </svg>
            <span className={`heading-display text-3xl tabular-nums ${urgent ? 'text-red-400 text-glow-red' : state.myTurn ? 'text-emerald-300 text-glow-emerald' : 'text-amber-300 text-glow-amber'}`}>
              {secondsLeft != null ? secondsLeft : '--'}
            </span>
          </div>
          <p className="text-[10px] uppercase tracking-wider text-slate-500 mt-2 inline-flex items-center gap-1">
            <ClockIcon className="w-3 h-3" /> seconds
          </p>
        </div>

        <DraftBoard state={state} />
      </div>

      {/* RIGHT: available players */}
      <div className="lg:col-span-2 bg-slate-800 rounded-2xl card-glow p-4">
        <div className="flex items-center justify-between mb-3">
          <h2 className="heading-display text-sm text-slate-300 inline-flex items-center gap-2">
            <FlameIcon className="w-4 h-4 text-orange-400" /> Available Players
            <span className="text-slate-500 normal-case tracking-normal">({fullCount})</span>
          </h2>
        </div>
        <div className="flex gap-2 mb-3 flex-wrap">
          <input
            type="text" placeholder="Search players or teams..." value={search}
            onChange={e => setSearch(e.target.value)}
            className="bg-slate-900 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm flex-1 min-w-[200px] focus:outline-none focus:border-emerald-400 transition-colors"
          />
          <select
            value={posFilter}
            onChange={e => setPosFilter(parseInt(e.target.value) as PlayerPosition | 0)}
            className="bg-slate-900 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-emerald-400 transition-colors cursor-pointer"
          >
            <option value={0}>All Positions</option>
            <option value={1}>GK</option>
            <option value={2}>DEF</option>
            <option value={3}>MID</option>
            <option value={4}>FWD</option>
          </select>
        </div>
        <div className="overflow-y-auto max-h-[60vh] rounded-lg">
          <table className="w-full text-sm">
            <thead className="sticky top-0 bg-slate-800 z-10">
              <tr className="text-slate-500 text-[10px] uppercase tracking-wider border-b border-slate-700">
                <th className="text-left px-3 py-2 font-semibold">Player</th>
                <th className="text-center px-2 py-2 font-semibold">Pos</th>
                <th className="text-left px-2 py-2 font-semibold">Team</th>
                <th className="text-right px-2 py-2 font-semibold">Pts</th>
                <th className="text-right px-2 py-2 w-24"></th>
              </tr>
            </thead>
            <tbody>
              {available.length === 0 && (
                <tr><td colSpan={5} className="text-center text-slate-500 py-12">No players match your filters.</td></tr>
              )}
              {available.map(p => (
                <tr key={p.id} className="border-b border-slate-700/40 hover:bg-slate-700/30 transition-colors">
                  <td className="px-3 py-2.5 text-white font-medium">{p.name}</td>
                  <td className={`px-2 py-2.5 text-center text-xs font-bold ${POS_COLOR[p.position]}`}>{POS_LABEL[p.position]}</td>
                  <td className="px-2 py-2.5 text-slate-400">{p.team}</td>
                  <td className="px-2 py-2.5 text-right tabular-nums text-white font-bold">{p.totalPoints}</td>
                  <td className="px-2 py-2.5 text-right">
                    <button
                      onClick={() => makePick(p.id)}
                      disabled={!state.myTurn || busy}
                      className="text-xs uppercase tracking-wider px-3 py-1.5 rounded-md font-bold bg-emerald-500 hover:bg-emerald-400 disabled:bg-slate-700 disabled:text-slate-500 text-white transition-all duration-200 cursor-pointer disabled:cursor-not-allowed shadow-[0_0_12px_rgba(34,197,94,0.3)] disabled:shadow-none"
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
    <div className="bg-slate-800 rounded-2xl card-glow p-4">
      <h3 className="heading-display text-sm text-slate-300 mb-3">Draft Board</h3>
      <div className="space-y-2">
        {state.members.map((m) => {
          const isCurrent = m.userId === state.currentPickerUserId;
          const myPicks = picksByUser[m.userId] || [];
          return (
            <div
              key={m.userId}
              className={`relative rounded-lg p-2.5 transition-all ${
                isCurrent
                  ? 'bg-emerald-500/10 ring-1 ring-emerald-400/60 shadow-[0_0_16px_rgba(34,197,94,0.2)]'
                  : 'bg-slate-900/60'
              }`}
            >
              <div className="flex items-center justify-between mb-1.5">
                <p className="text-xs font-bold text-white inline-flex items-center gap-1.5">
                  {isCurrent && <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 pulse-ring" />}
                  {m.displayName}
                </p>
                <span className="text-[10px] tabular-nums font-semibold text-slate-400">
                  {myPicks.length}<span className="text-slate-600">/15</span>
                </span>
              </div>
              <div className="flex flex-wrap gap-1">
                {myPicks.length === 0 && <span className="text-[10px] text-slate-600 italic">No picks yet</span>}
                {myPicks.map(p => (
                  <span
                    key={p.pickNumber}
                    className={`text-[10px] px-1.5 py-0.5 rounded font-semibold bg-slate-800/80 ${POS_COLOR[p.position]}`}
                    title={`Pick #${p.pickNumber} — ${p.playerName} (${p.playerTeam})${p.wasAutoPick ? ' [auto]' : ''}`}
                  >
                    {p.playerName.split(' ').slice(-1)[0]}
                    {p.wasAutoPick && <span className="text-slate-500 ml-0.5">·a</span>}
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
    <div className="bg-gradient-to-br from-emerald-500/10 via-slate-800 to-slate-800 rounded-2xl card-glow p-8 text-center">
      <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-emerald-500/20 mb-4">
        <TrophyIcon className="w-8 h-8 text-emerald-400" />
      </div>
      <h2 className="heading-display text-3xl text-white mb-2 text-glow-emerald">Draft Complete</h2>
      <p className="text-slate-400 mb-6">
        All <span className="text-emerald-400 font-bold tabular-nums">{state.totalPicks}</span> picks made.
        Teams have been created.
      </p>
      <button
        onClick={onViewTeam}
        className="bg-emerald-500 hover:bg-emerald-400 text-white heading-display text-sm px-6 py-3 rounded-lg transition-all duration-200 cursor-pointer shadow-[0_0_20px_rgba(34,197,94,0.4)]"
      >
        View Your Squad
      </button>
    </div>
  );
}
