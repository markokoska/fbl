import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/client';
import type { Team, Gameweek, GameweekHistory, Pick } from '../api/types';
import { useCountdown } from '../hooks/useCountdown';
import { useLeagueId, leagueQuery } from '../hooks/useLeagueId';
import PitchView, { type Formation } from '../components/team/PitchView';
import TeamSelector from '../components/team/TeamSelector';

export default function MyTeam() {
  const leagueId = useLeagueId();
  const [team, setTeam] = useState<Team | null>(null);
  const [gw, setGw] = useState<Gameweek | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [formation, setFormation] = useState<Formation>('4-4-2');
  const [history, setHistory] = useState<GameweekHistory[]>([]);
  const [viewGw, setViewGw] = useState<{ gwNumber: number; team: Team } | null>(null);
  const [viewFormation, setViewFormation] = useState<Formation>('4-4-2');
  const countdown = useCountdown(gw?.deadline);

  useEffect(() => {
    setLoading(true);
    setError('');
    setTeam(null);
    loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [leagueId]);

  const loadData = async () => {
    try {
      const [teamRes, gwRes] = await Promise.all([
        api.get<Team>(`/team${leagueQuery(leagueId)}`),
        api.get<Gameweek>('/gameweek/current'),
      ]);
      setTeam(teamRes.data);
      setGw(gwRes.data);
      api.get<GameweekHistory[]>(`/team/history${leagueQuery(leagueId)}`)
        .then(r => setHistory(r.data))
        .catch(() => setHistory([]));
    } catch (err: any) {
      if (err.response?.status === 404) {
        setError('no-team');
      } else {
        setError(err.response?.data || 'Failed to load team');
      }
    } finally {
      setLoading(false);
    }
  };

  const openGwView = async (gwNumber: number) => {
    try {
      const res = await api.get<Team>(`/team/gameweek/${gwNumber}${leagueQuery(leagueId)}`);
      setViewGw({ gwNumber, team: res.data });
    } catch { /* ignore */ }
  };

  if (loading) return <div className="text-center py-16 text-slate-400">Loading...</div>;

  if (error === 'no-team') {
    const isLeague = leagueId != null;
    return (
      <div className="max-w-2xl mx-auto px-4 py-16 text-center">
        <h1 className="text-3xl font-bold text-white mb-4">
          {isLeague ? 'Create Your Team for This League' : 'Create Your Squad'}
        </h1>
        <p className="text-slate-400 mb-8">Pick 15 players to start your Fantasy Bundesliga journey</p>
        <Link
          to={`/transfers${leagueQuery(leagueId)}`}
          className="inline-block bg-emerald-500 hover:bg-emerald-600 text-white font-semibold px-8 py-3 rounded-lg transition"
        >
          Pick Your Team
        </Link>
      </div>
    );
  }

  if (!team) return <div className="text-center py-16 text-red-400">{error}</div>;

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      {/* Team selector — only renders if user has multiple teams */}
      <div className="flex justify-end mb-3">
        <TeamSelector current={leagueId} />
      </div>

      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h1 className="text-2xl font-bold text-white">
            {team.name}
            {team.leagueName && team.leagueId != null && (
              <span className="ml-2 text-xs uppercase tracking-wide bg-slate-700 text-slate-300 px-2 py-0.5 rounded">
                {team.leagueName}
              </span>
            )}
          </h1>
          <p className="text-slate-400 text-sm">
            Budget: <span className="text-emerald-400">{team.budget.toFixed(1)}M</span> | Free Transfers:{' '}
            <span className="text-emerald-400">{team.freeTransfers}</span>
          </p>
        </div>
        <div className="text-right">
          <p className="text-sm text-slate-400">
            GW{gw?.number} Deadline: <span className={countdown === 'LOCKED' ? 'text-red-400' : 'text-emerald-400'}>{countdown}</span>
          </p>
        </div>
      </div>

      {/* Points summary */}
      <div className="grid grid-cols-2 gap-4 mb-6">
        <div className="bg-slate-800 rounded-xl p-4 text-center">
          <p className="text-slate-400 text-sm">GW Points</p>
          <p className="text-3xl font-bold text-white">{team.gameweekPoints}</p>
        </div>
        <div className="bg-slate-800 rounded-xl p-4 text-center">
          <p className="text-slate-400 text-sm">Total Points</p>
          <p className="text-3xl font-bold text-emerald-400">{team.totalPoints}</p>
        </div>
      </div>

      {/* Pitch View */}
      <PitchView
        picks={team.picks}
        formation={formation}
        onFormationChange={setFormation}
        onPicksUpdated={(newPicks) => setTeam({ ...team, picks: newPicks })}
        leagueId={leagueId}
      />

      {/* GW History */}
      {history.length > 0 && <GwHistoryTable history={history} onViewGw={openGwView} />}

      {/* Past GW Modal */}
      {viewGw && (
        <GwDetailModal
          gwNumber={viewGw.gwNumber}
          team={viewGw.team}
          formation={viewFormation}
          onFormationChange={setViewFormation}
          onClose={() => setViewGw(null)}
        />
      )}
    </div>
  );
}

function GwHistoryTable({ history, onViewGw }: { history: GameweekHistory[]; onViewGw: (gw: number) => void }) {
  const best = Math.max(...history.map(h => h.points));
  return (
    <div className="mt-8">
      <h2 className="text-lg font-semibold text-white mb-3">Gameweek History</h2>
      <div className="bg-slate-800 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-slate-400 text-xs uppercase border-b border-slate-700">
              <th className="text-left px-4 py-3">GW</th>
              <th className="text-right px-4 py-3">Points</th>
              <th className="text-right px-4 py-3">Cumulative</th>
              <th className="text-right px-4 py-3 w-20"></th>
            </tr>
          </thead>
          <tbody>
            {history.map(h => (
              <tr key={h.gameweekNumber} className="border-b border-slate-700/50 hover:bg-slate-700/30 transition-colors">
                <td className="px-4 py-2.5 text-white font-medium">GW{h.gameweekNumber}</td>
                <td className="px-4 py-2.5 text-right">
                  <span className={`font-bold tabular-nums ${h.points === best ? 'text-emerald-400' : h.points === 0 ? 'text-slate-500' : 'text-white'}`}>
                    {h.points}
                  </span>
                </td>
                <td className="px-4 py-2.5 text-right text-slate-400 tabular-nums">{h.cumulativePoints}</td>
                <td className="px-4 py-2.5 text-right">
                  <button
                    onClick={() => onViewGw(h.gameweekNumber)}
                    className="text-xs px-2.5 py-1 rounded font-medium bg-slate-700 text-slate-300 hover:bg-slate-600 hover:text-white transition"
                  >
                    View
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr className="border-t border-slate-600">
              <td className="px-4 py-3 text-white font-semibold">Total</td>
              <td className="px-4 py-3 text-right font-bold text-emerald-400 tabular-nums">
                {history.reduce((s, h) => s + h.points, 0)}
              </td>
              <td />
              <td />
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  );
}

function GwDetailModal({
  gwNumber,
  team,
  formation,
  onFormationChange,
  onClose,
}: {
  gwNumber: number;
  team: Team;
  formation: Formation;
  onFormationChange: (f: Formation) => void;
  onClose: () => void;
}) {
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  // Sort picks for display: starters first by position, then bench
  const starters = team.picks.filter(p => p.squadPosition <= 11);
  const bench = team.picks.filter(p => p.squadPosition > 11);

  return (
    <div className="fixed inset-0 bg-black/70 backdrop-blur-sm z-50 flex items-center justify-center p-4" onClick={onClose}>
      <div
        className="bg-slate-900 rounded-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto p-4 sm:p-6 border border-slate-700"
        onClick={e => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-lg font-bold text-white">Gameweek {gwNumber}</h2>
            <p className="text-slate-400 text-sm">
              {team.name} &middot; <span className="text-emerald-400 font-bold">{team.gameweekPoints} pts</span>
            </p>
          </div>
          <button onClick={onClose} className="text-slate-400 hover:text-white text-2xl w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-700 transition">
            &times;
          </button>
        </div>

        {/* Pitch */}
        <PitchView
          picks={team.picks}
          formation={formation}
          onFormationChange={onFormationChange}
          readOnly
        />

        {/* Points breakdown table */}
        <div className="mt-4 bg-slate-800 rounded-xl overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-slate-500 text-xs uppercase border-b border-slate-700">
                <th className="text-left px-4 py-2">Player</th>
                <th className="text-center px-2 py-2">Role</th>
                <th className="text-right px-4 py-2">Pts</th>
              </tr>
            </thead>
            <tbody>
              {starters.map(p => (
                <PointsRow key={p.playerId} pick={p} />
              ))}
              {bench.length > 0 && (
                <tr><td colSpan={3} className="px-4 py-1.5 text-slate-500 text-xs uppercase font-semibold bg-slate-700/30">Substitutes</td></tr>
              )}
              {bench.map(p => (
                <PointsRow key={p.playerId} pick={p} isBench />
              ))}
            </tbody>
            <tfoot>
              <tr className="border-t border-slate-600">
                <td className="px-4 py-2.5 text-white font-semibold" colSpan={2}>Total</td>
                <td className="px-4 py-2.5 text-right font-bold text-emerald-400 tabular-nums">{team.gameweekPoints}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
  );
}

function PointsRow({ pick, isBench }: { pick: Pick; isBench?: boolean }) {
  const posLabel = ['', 'GK', 'DEF', 'MID', 'FWD'][pick.position] || '';
  const posColor = ['', 'text-yellow-400', 'text-blue-400', 'text-green-400', 'text-red-400'][pick.position] || '';
  const multiplier = pick.isCaptain ? 2 : 1;
  const displayPts = isBench ? '-' : pick.gameweekPoints * multiplier;

  return (
    <tr className={`border-b border-slate-700/40 ${isBench ? 'opacity-60' : ''} hover:bg-slate-700/20`}>
      <td className="px-4 py-2 text-white flex items-center gap-2">
        <img
          src={`/players/${pick.playerId}.png`}
          alt=""
          className="w-6 h-6 rounded-full object-cover object-top bg-slate-700 flex-shrink-0"
          onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = 'none'; }}
        />
        <span className="truncate">{pick.playerName}</span>
        {pick.isCaptain && <span className="text-[10px] bg-emerald-500 text-white px-1.5 py-0.5 rounded font-bold flex-shrink-0">C</span>}
        {pick.isViceCaptain && <span className="text-[10px] bg-slate-600 text-white px-1.5 py-0.5 rounded font-bold flex-shrink-0">V</span>}
      </td>
      <td className={`px-2 py-2 text-center text-xs font-semibold ${posColor}`}>{posLabel}</td>
      <td className={`px-4 py-2 text-right font-bold tabular-nums ${
        typeof displayPts === 'number'
          ? displayPts > 0 ? 'text-emerald-400' : displayPts < 0 ? 'text-red-400' : 'text-slate-500'
          : 'text-slate-500'
      }`}>
        {displayPts}
      </td>
    </tr>
  );
}
