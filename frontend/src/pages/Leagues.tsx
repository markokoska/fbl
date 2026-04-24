import { useEffect, useState, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import api from '../api/client';
import { GameweekStatus, type League, type LeagueStanding, type Team, type Gameweek as GwType, type GameweekHistory } from '../api/types';
import PitchView, { type Formation } from '../components/team/PitchView';

export default function Leagues() {
  const [myLeagues, setMyLeagues] = useState<League[]>([]);
  const [globalStandings, setGlobalStandings] = useState<LeagueStanding[]>([]);
  const [selectedLeague, setSelectedLeague] = useState<League | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [newName, setNewName] = useState('');
  const [joinCode, setJoinCode] = useState('');
  const [message, setMessage] = useState('');
  const [tab, setTab] = useState<'global' | 'my'>('global');
  const [viewingTeam, setViewingTeam] = useState<{ userId: string; managerName: string; teamName: string; history: GameweekHistory[] } | null>(null);
  const [viewingGwTeam, setViewingGwTeam] = useState<{ gwNumber: number; team: Team } | null>(null);
  const [viewGwFormation, setViewGwFormation] = useState<Formation>('4-4-2');
  const [isLive, setIsLive] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const selectedLeagueRef = useRef<League | null>(null);

  useEffect(() => { selectedLeagueRef.current = selectedLeague; }, [selectedLeague]);

  const viewUserTeam = async (userId: string, managerName: string, teamName: string) => {
    try {
      const histRes = await api.get<GameweekHistory[]>(`/team/user/${userId}/history`).catch(() => ({ data: [] as GameweekHistory[] }));
      setViewingTeam({ userId, managerName, teamName, history: histRes.data });
      setViewingGwTeam(null);
    } catch {
      setMessage('Could not load team');
    }
  };

  const openUserGwTeam = async (userId: string, gwNumber: number) => {
    try {
      const res = await api.get<Team>(`/team/user/${userId}/gameweek/${gwNumber}`);
      setViewingGwTeam({ gwNumber, team: res.data });
    } catch { /* ignore */ }
  };

  const loadData = useCallback(async () => {
    const [globalRes, leaguesRes] = await Promise.all([
      api.get<LeagueStanding[]>('/league/global'),
      api.get<League[]>('/league/my'),
    ]);
    setGlobalStandings(globalRes.data);
    setMyLeagues(leaguesRes.data);

    if (selectedLeagueRef.current) {
      try {
        const res = await api.get<League>(`/league/${selectedLeagueRef.current.id}`);
        setSelectedLeague(res.data);
      } catch { /* ignore */ }
    }
  }, []);

  useEffect(() => {
    loadData();
    api.get<GwType[]>('/gameweek').then((r) => {
      setIsLive(r.data.some((g) => g.status === GameweekStatus.Live));
    });
  }, [loadData]);

  useEffect(() => {
    const token = localStorage.getItem('fbl_token');
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/livescore?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    connection.on('StandingsUpdate', () => { loadData(); setIsLive(true); });
    connection.start().catch(() => {});
    connectionRef.current = connection;
    return () => { connection.stop(); };
  }, [loadData]);

  const createLeague = async () => {
    if (!newName.trim()) return;
    try {
      await api.post('/league', { name: newName });
      setNewName(''); setShowCreate(false); setMessage('League created!'); loadData();
    } catch (err: any) { setMessage(err.response?.data || 'Failed to create league'); }
  };

  const joinLeague = async () => {
    if (!joinCode.trim()) return;
    try {
      await api.post('/league/join', { joinCode });
      setJoinCode(''); setMessage('Joined league!'); loadData();
    } catch (err: any) { setMessage(err.response?.data || 'Failed to join'); }
  };

  const viewLeague = async (id: number) => {
    const res = await api.get<League>(`/league/${id}`);
    setSelectedLeague(res.data);
  };

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <h1 className="text-2xl font-bold text-white">Leagues</h1>
        {isLive && <LiveBadge />}
      </div>

      {message && (
        <div className="bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 px-4 py-2 rounded-lg mb-4 text-sm">
          {message}
        </div>
      )}

      <div className="flex gap-3 mb-6 flex-wrap">
        <button onClick={() => setShowCreate(!showCreate)} className="bg-emerald-500 hover:bg-emerald-600 text-white text-sm font-semibold px-4 py-2 rounded-lg transition">
          Create League
        </button>
        <div className="flex gap-2">
          <input
            type="text" placeholder="Enter join code..." value={joinCode}
            onChange={(e) => setJoinCode(e.target.value)}
            className="bg-slate-800 border border-slate-700 rounded-lg px-3 py-2 text-white text-sm focus:outline-none focus:border-emerald-400"
          />
          <button onClick={joinLeague} className="bg-slate-700 hover:bg-slate-600 text-white text-sm px-4 py-2 rounded-lg transition">Join</button>
        </div>
      </div>

      {showCreate && (
        <div className="bg-slate-800 rounded-xl p-4 mb-6 flex gap-3">
          <input
            type="text" placeholder="League name..." value={newName}
            onChange={(e) => setNewName(e.target.value)}
            className="bg-slate-700 border border-slate-600 rounded-lg px-3 py-2 text-white text-sm flex-1 focus:outline-none focus:border-emerald-400"
          />
          <button onClick={createLeague} className="bg-emerald-500 hover:bg-emerald-600 text-white text-sm font-semibold px-4 py-2 rounded-lg transition">Create</button>
        </div>
      )}

      <div className="flex gap-1 bg-slate-800 rounded-lg p-1 mb-6 w-fit">
        <button
          onClick={() => { setTab('global'); setSelectedLeague(null); }}
          className={`px-4 py-2 rounded text-sm font-medium transition ${tab === 'global' ? 'bg-emerald-500 text-white' : 'text-slate-400 hover:text-white'}`}
        >
          Global Leaderboard
        </button>
        <button
          onClick={() => setTab('my')}
          className={`px-4 py-2 rounded text-sm font-medium transition ${tab === 'my' ? 'bg-emerald-500 text-white' : 'text-slate-400 hover:text-white'}`}
        >
          My Leagues ({myLeagues.length})
        </button>
      </div>

      {tab === 'global' && !selectedLeague && (
        <StandingsTable standings={globalStandings} title="Global Leaderboard" onViewTeam={viewUserTeam} isLive={isLive} />
      )}

      {tab === 'my' && !selectedLeague && (
        <div className="space-y-3">
          {myLeagues.length === 0 ? (
            <p className="text-slate-400 text-center py-8">No leagues yet. Create or join one!</p>
          ) : (
            myLeagues.map((league) => (
              <div
                key={league.id}
                onClick={() => { viewLeague(league.id); setTab('my'); }}
                className="bg-slate-800 rounded-xl p-4 flex justify-between items-center cursor-pointer hover:bg-slate-700/50 transition"
              >
                <div>
                  <p className="text-white font-medium">{league.name}</p>
                  <p className="text-slate-400 text-xs">Code: {league.joinCode}</p>
                </div>
                <span className="text-slate-500 text-sm">View &rsaquo;</span>
              </div>
            ))
          )}
        </div>
      )}

      {selectedLeague && (
        <div>
          <button onClick={() => setSelectedLeague(null)} className="text-slate-400 hover:text-white text-sm mb-4 transition">
            &lsaquo; Back to list
          </button>
          <div className="mb-4 flex items-center gap-3">
            <div>
              <h2 className="text-lg font-semibold text-white">{selectedLeague.name}</h2>
              <p className="text-slate-400 text-sm">Join code: <span className="text-emerald-400 font-mono">{selectedLeague.joinCode}</span></p>
            </div>
            {isLive && <LiveBadge />}
          </div>
          <StandingsTable standings={selectedLeague.standings} title="" onViewTeam={viewUserTeam} isLive={isLive} />
        </div>
      )}

      {/* GW History Modal */}
      {viewingTeam && (
        <div className="fixed inset-0 bg-black/60 z-50 flex items-center justify-center p-4" onClick={() => setViewingTeam(null)}>
          <div className="bg-slate-900 rounded-xl max-w-md w-full max-h-[80vh] overflow-y-auto p-4 sm:p-6 border border-slate-700" onClick={(e) => e.stopPropagation()}>
            <div className="flex justify-between items-center mb-4">
              <div>
                <h2 className="text-lg font-bold text-white">{viewingTeam.teamName}</h2>
                <p className="text-slate-400 text-sm">{viewingTeam.managerName}</p>
              </div>
              <button onClick={() => setViewingTeam(null)} className="text-slate-400 hover:text-white text-2xl w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-700 transition">&times;</button>
            </div>

            {viewingTeam.history.length === 0 ? (
              <p className="text-slate-500 text-center py-8">No gameweek history yet.</p>
            ) : (
              <div className="bg-slate-800 rounded-xl overflow-hidden">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-slate-500 text-xs uppercase border-b border-slate-700">
                      <th className="text-left px-4 py-2">GW</th>
                      <th className="text-right px-4 py-2">Pts</th>
                      <th className="text-right px-4 py-2">Total</th>
                      <th className="text-right px-4 py-2 w-16"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {viewingTeam.history.map(h => (
                      <tr key={h.gameweekNumber} className="border-b border-slate-700/40 hover:bg-slate-700/30">
                        <td className="px-4 py-2 text-white font-medium">GW{h.gameweekNumber}</td>
                        <td className="px-4 py-2 text-right font-bold text-white tabular-nums">{h.points}</td>
                        <td className="px-4 py-2 text-right text-slate-400 tabular-nums">{h.cumulativePoints}</td>
                        <td className="px-4 py-2 text-right">
                          <button
                            onClick={() => openUserGwTeam(viewingTeam.userId, h.gameweekNumber)}
                            className="text-xs px-2.5 py-1 rounded font-medium bg-slate-700 text-slate-300 hover:bg-slate-600 hover:text-white transition"
                          >
                            View
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {/* GW Team Pitch Modal */}
      {viewingGwTeam && viewingTeam && (
        <div className="fixed inset-0 bg-black/75 backdrop-blur-sm z-[60] flex items-center justify-center p-4" onClick={() => setViewingGwTeam(null)}>
          <div className="bg-slate-900 rounded-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto p-4 sm:p-6 border border-slate-700" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-lg font-bold text-white">GW{viewingGwTeam.gwNumber} — {viewingTeam.teamName}</h2>
                <p className="text-slate-400 text-sm">
                  {viewingTeam.managerName} &middot;{' '}
                  <span className="text-emerald-400 font-bold">{viewingGwTeam.team.gameweekPoints} pts</span>
                </p>
              </div>
              <button
                onClick={() => setViewingGwTeam(null)}
                className="text-slate-400 hover:text-white text-2xl w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-700 transition"
              >
                &times;
              </button>
            </div>
            <PitchView
              picks={viewingGwTeam.team.picks}
              formation={viewGwFormation}
              onFormationChange={setViewGwFormation}
              readOnly
            />
          </div>
        </div>
      )}
    </div>
  );
}

function LiveBadge() {
  return (
    <span className="inline-flex items-center gap-1.5 bg-red-500/15 text-red-400 text-xs font-bold uppercase tracking-wider px-2.5 py-1 rounded-full border border-red-500/30">
      <span className="relative flex h-2 w-2">
        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75" />
        <span className="relative inline-flex rounded-full h-2 w-2 bg-red-500" />
      </span>
      Live
    </span>
  );
}

function StandingsTable({
  standings, title, onViewTeam, isLive,
}: {
  standings: LeagueStanding[];
  title: string;
  onViewTeam: (userId: string, name: string, teamName: string) => void;
  isLive: boolean;
}) {
  return (
    <div className="bg-slate-800 rounded-xl overflow-hidden">
      {title && <div className="px-4 py-3 border-b border-slate-700"><h3 className="text-sm font-semibold text-slate-400">{title}</h3></div>}
      <table className="w-full">
        <thead>
          <tr className="text-slate-400 text-xs uppercase border-b border-slate-700">
            <th className="text-left px-4 py-3 w-12">#</th>
            <th className="text-left px-4 py-3">Manager</th>
            <th className="text-left px-4 py-3">Team</th>
            {isLive && <th className="text-right px-4 py-3 w-16">GW</th>}
            <th className="text-right px-4 py-3">Total</th>
            <th className="text-right px-4 py-3 w-20"></th>
          </tr>
        </thead>
        <tbody>
          {standings.length === 0 ? (
            <tr><td colSpan={isLive ? 6 : 5} className="text-center py-8 text-slate-500">No standings yet</td></tr>
          ) : (
            standings.map((s) => (
              <tr key={s.userId} className="border-b border-slate-700/50 hover:bg-slate-700/30 transition-colors">
                <td className="px-4 py-3 text-slate-500 font-medium">{s.rank}</td>
                <td className="px-4 py-3 text-white">{s.displayName}</td>
                <td className="px-4 py-3 text-slate-400">{s.teamName}</td>
                {isLive && (
                  <td className="px-4 py-3 text-right">
                    <span className={`font-bold tabular-nums ${s.gameweekPoints > 0 ? 'text-green-400' : 'text-slate-500'}`}>
                      {s.gameweekPoints > 0 ? '+' : ''}{s.gameweekPoints}
                    </span>
                  </td>
                )}
                <td className="px-4 py-3 text-right font-semibold text-emerald-400 tabular-nums">{s.totalPoints}</td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => onViewTeam(s.userId, s.displayName, s.teamName)}
                    className="text-xs px-3 py-1 rounded font-medium bg-slate-700 text-slate-300 hover:bg-slate-600 hover:text-white transition"
                  >
                    View
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
