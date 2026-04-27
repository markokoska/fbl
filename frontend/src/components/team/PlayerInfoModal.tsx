import { useEffect, useState } from 'react';
import type { PlayerDetail } from '../../api/types';
import { PlayerPosition } from '../../api/types';
import api from '../../api/client';
import { getTeamLogo, getTeamShort, getJerseySvg, getFitnessColor, getFitnessLabel, getTeamDisplayName } from '../../utils/teamAssets';

interface Fixture {
  id: number;
  gameweekNumber: number;
  homeTeam: string;
  awayTeam: string;
  homeGoals: number | null;
  awayGoals: number | null;
  isFinished: boolean;
}

const posLabel = (p: PlayerPosition) => ['', 'GK', 'DEF', 'MID', 'FWD'][p];
const posBg = (p: PlayerPosition) => ['', 'bg-yellow-500', 'bg-blue-500', 'bg-green-500', 'bg-red-500'][p];

interface Props {
  playerId: number;
  onClose: () => void;
}

export default function PlayerInfoModal({ playerId, onClose }: Props) {
  const [player, setPlayer] = useState<PlayerDetail | null>(null);
  const [fixtures, setFixtures] = useState<Fixture[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<'history' | 'fixtures'>('history');

  useEffect(() => {
    api.get<PlayerDetail>(`/players/${playerId}`)
      .then(res => {
        setPlayer(res.data);
        return api.get<Fixture[]>(`/fixtures/team?team=${encodeURIComponent(res.data.team)}`);
      })
      .then(res => setFixtures(res.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [playerId]);

  if (loading) {
    return (
      <div className="fixed inset-0 bg-black/70 z-50 flex items-center justify-center" onClick={onClose}>
        <div className="text-white text-lg">Loading...</div>
      </div>
    );
  }

  if (!player) {
    return (
      <div className="fixed inset-0 bg-black/70 z-50 flex items-center justify-center" onClick={onClose}>
        <div className="text-red-400 text-lg">Player not found</div>
      </div>
    );
  }

  const fitnessColor = getFitnessColor(player.fitness);
  const fitnessLabel = getFitnessLabel(player.fitness);
  const isGk = player.position === PlayerPosition.GK;
  const jersey = getJerseySvg(player.team, isGk);
  const teamLogo = getTeamLogo(player.team);

  // Calculate form (avg points over last 5 GWs)
  const recentGws = player.gameweekHistory.slice(-5);
  const form = recentGws.length > 0
    ? (recentGws.reduce((s, g) => s + g.points, 0) / recentGws.length).toFixed(1)
    : '0.0';

  // Latest GW points
  const latestGw = player.gameweekHistory.length > 0
    ? player.gameweekHistory[player.gameweekHistory.length - 1]
    : null;

  return (
    <div className="fixed inset-0 bg-black/70 z-50 flex items-center justify-center p-4" onClick={onClose}>
      <div
        className="bg-slate-900 rounded-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto shadow-2xl"
        onClick={e => e.stopPropagation()}
      >
        {/* Fitness banner */}
        {player.fitness < 100 && (
          <div
            className="px-4 py-2 text-sm font-medium text-white flex items-center justify-between rounded-t-2xl"
            style={{ backgroundColor: fitnessColor + 'CC' }}
          >
            <span>{fitnessLabel} - {player.fitness}% chance of playing</span>
            <button onClick={onClose} className="text-white/80 hover:text-white text-xl font-bold">&times;</button>
          </div>
        )}

        {/* Header */}
        <div className="relative bg-gradient-to-br from-slate-800 to-slate-900 p-5 flex gap-4 items-center">
          {player.fitness >= 100 && (
            <button
              onClick={onClose}
              className="absolute top-3 right-3 text-slate-400 hover:text-white text-2xl font-bold"
            >
              &times;
            </button>
          )}

          {/* Player photo or jersey + team logo */}
          <div className="relative flex-shrink-0">
            <img
              src={`/players/${playerId}.png`}
              alt=""
              className="w-20 h-24 object-cover rounded-lg drop-shadow-lg"
              onError={(e) => { (e.target as HTMLImageElement).src = jersey; }}
            />
            <img
              src={teamLogo}
              alt=""
              className="absolute -bottom-1 -right-1 w-8 h-8 rounded-full bg-white p-0.5 shadow"
            />
          </div>

          {/* Name + position */}
          <div className="flex-1 min-w-0">
            <span className={`inline-block px-2 py-0.5 rounded text-xs font-bold text-white ${posBg(player.position)}`}>
              {posLabel(player.position)}
            </span>
            <h2 className="text-xl font-bold text-white mt-1 truncate">{player.name}</h2>
            <p className="text-slate-400 text-sm">{player.team}</p>
            <p className="text-emerald-400 text-sm font-semibold mt-0.5">{player.price.toFixed(1)}M</p>
          </div>
        </div>

        {/* Stats summary row */}
        <div className="grid grid-cols-4 divide-x divide-slate-700 bg-slate-800/50">
          <div className="text-center py-3">
            <p className="text-slate-400 text-[10px] uppercase tracking-wide">Form</p>
            <p className="text-white font-bold text-lg">{form}</p>
          </div>
          <div className="text-center py-3">
            <p className="text-slate-400 text-[10px] uppercase tracking-wide">
              {latestGw ? `GW ${latestGw.gameweekNumber}` : 'GW'}
            </p>
            <p className="text-white font-bold text-lg">{latestGw?.points ?? '-'}pts</p>
          </div>
          <div className="text-center py-3">
            <p className="text-slate-400 text-[10px] uppercase tracking-wide">Total</p>
            <p className="text-white font-bold text-lg">{player.totalPoints}pts</p>
          </div>
          <div className="text-center py-3">
            <p className="text-slate-400 text-[10px] uppercase tracking-wide">Owned</p>
            <p className="text-white font-bold text-lg">{player.ownershipPercent}%</p>
          </div>
        </div>

        {/* Season stats box */}
        <div className="mx-4 mt-4 bg-slate-800 rounded-xl p-4">
          <h3 className="text-xs font-semibold text-slate-400 uppercase tracking-wide mb-3">Season Stats</h3>
          <div className="grid grid-cols-3 gap-y-3 gap-x-4">
            <StatItem label="Matches" value={player.stats.matchesPlayed} />
            <StatItem label="Goals" value={player.stats.goals} />
            <StatItem label="Assists" value={player.stats.assists} />
            <StatItem label="Clean Sheets" value={player.stats.cleanSheets} />
            <StatItem label="Yellow Cards" value={player.stats.yellowCards} />
            <StatItem label="Red Cards" value={player.stats.redCards} />
            <StatItem label="Bonus" value={player.stats.bonusPoints} />
            <StatItem label="xG" value={player.stats.expectedGoals.toFixed(1)} />
            <StatItem label="xA" value={player.stats.expectedAssists.toFixed(1)} />
            {isGk && <StatItem label="Pen Saves" value={player.stats.penaltySaves} />}
            {player.stats.penaltiesMissed > 0 && <StatItem label="Pen Missed" value={player.stats.penaltiesMissed} />}
            {player.stats.ownGoals > 0 && <StatItem label="Own Goals" value={player.stats.ownGoals} />}
          </div>
        </div>

        {/* Tabs */}
        <div className="flex mx-4 mt-4 bg-slate-800 rounded-lg overflow-hidden">
          <button
            onClick={() => setTab('history')}
            className={`flex-1 text-sm font-semibold py-2.5 transition ${
              tab === 'history' ? 'bg-emerald-500 text-white' : 'text-slate-400 hover:text-white'
            }`}
          >
            History
          </button>
          <button
            onClick={() => setTab('fixtures')}
            className={`flex-1 text-sm font-semibold py-2.5 transition ${
              tab === 'fixtures' ? 'bg-emerald-500 text-white' : 'text-slate-400 hover:text-white'
            }`}
          >
            Fixtures
          </button>
        </div>

        {/* History / Fixtures content */}
        <div className="mx-4 mt-3 mb-4">
          {tab === 'history' ? (
            <HistoryTable history={player.gameweekHistory} fixtures={fixtures} team={player.team} />
          ) : (
            <FixturesTable fixtures={fixtures} team={player.team} />
          )}
        </div>

        {/* Fitness bar */}
        <div className="mx-4 mb-4">
          <div className="flex items-center justify-between text-xs mb-1">
            <span className="text-slate-400">Fitness</span>
            <span style={{ color: fitnessColor }} className="font-semibold">{fitnessLabel} ({player.fitness}%)</span>
          </div>
          <div className="w-full h-2 bg-slate-700 rounded-full overflow-hidden">
            <div
              className="h-full rounded-full transition-all"
              style={{ width: `${player.fitness}%`, backgroundColor: fitnessColor }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function StatItem({ label, value }: { label: string; value: string | number }) {
  return (
    <div>
      <p className="text-slate-500 text-[10px] uppercase">{label}</p>
      <p className="text-white font-semibold text-sm">{value}</p>
    </div>
  );
}

function getOpponent(fixture: Fixture, team: string) {
  const isHome = fixture.homeTeam === team;
  const opp = isHome ? fixture.awayTeam : fixture.homeTeam;
  const venue = isHome ? 'H' : 'A';
  const score = fixture.isFinished && fixture.homeGoals !== null
    ? `${fixture.homeGoals}-${fixture.awayGoals}`
    : '';
  return { opp, venue, score, short: getTeamShort(opp) };
}

function HistoryTable({ history, fixtures, team }: {
  history: PlayerDetail['gameweekHistory'];
  fixtures: Fixture[];
  team: string;
}) {
  // Build a map of GW number → fixture for this team (finished only)
  const fixtureMap = new Map<number, Fixture>();
  for (const f of fixtures) {
    if (f.isFinished) fixtureMap.set(f.gameweekNumber, f);
  }

  if (history.length === 0) {
    return <p className="text-slate-500 text-sm text-center py-4">No gameweek history yet</p>;
  }

  const totals = history.reduce(
    (acc, gw) => ({
      points: acc.points + gw.points,
      goals: acc.goals + gw.stats.goals,
      assists: acc.assists + gw.stats.assists,
      cs: acc.cs + gw.stats.cleanSheets,
      yc: acc.yc + gw.stats.yellowCards,
      rc: acc.rc + gw.stats.redCards,
      og: acc.og + gw.stats.ownGoals,
      ps: acc.ps + gw.stats.penaltySaves,
      pm: acc.pm + gw.stats.penaltiesMissed,
      bonus: acc.bonus + gw.stats.bonusPoints,
    }),
    { points: 0, goals: 0, assists: 0, cs: 0, yc: 0, rc: 0, og: 0, ps: 0, pm: 0, bonus: 0 }
  );

  return (
    <div className="bg-slate-800 rounded-xl overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-xs">
          <thead>
            <tr className="text-slate-400 uppercase border-b border-slate-700">
              <th className="text-left px-3 py-2">GW</th>
              <th className="text-left px-2 py-2">OPP</th>
              <th className="text-right px-2 py-2">PTS</th>
              <th className="text-right px-2 py-2">MIN</th>
              <th className="text-right px-2 py-2">GS</th>
              <th className="text-right px-2 py-2">A</th>
              <th className="text-right px-2 py-2">CS</th>
              <th className="text-right px-2 py-2">YC</th>
              <th className="text-right px-2 py-2">RC</th>
              <th className="text-right px-2 py-2">OG</th>
              <th className="text-right px-2 py-2">PS</th>
              <th className="text-right px-2 py-2">PM</th>
              <th className="text-right px-2 py-2">B</th>
            </tr>
          </thead>
          <tbody>
            {history.map(gw => {
              const fix = fixtureMap.get(gw.gameweekNumber);
              const opp = fix ? getOpponent(fix, team) : null;
              return (
                <tr key={gw.gameweekNumber} className="border-b border-slate-700/30 hover:bg-slate-700/20">
                  <td className="px-3 py-2 text-slate-300 font-medium">{gw.gameweekNumber}</td>
                  <td className="px-2 py-2 text-slate-300 text-[10px]">
                    {opp ? (
                      <span>
                        {opp.short} ({opp.venue}) <span className="text-slate-500">{opp.score}</span>
                      </span>
                    ) : '-'}
                  </td>
                  <td className="px-2 py-2 text-right text-white font-semibold">{gw.points}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.minutesPlayed}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.goals}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.assists}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.cleanSheets}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.yellowCards}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.redCards}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.ownGoals}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.penaltySaves}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.penaltiesMissed}</td>
                  <td className="px-2 py-2 text-right text-slate-400">{gw.stats.bonusPoints}</td>
                </tr>
              );
            })}
            <tr className="bg-slate-700/40 font-semibold">
              <td className="px-3 py-2 text-emerald-400" colSpan={2}>Total</td>
              <td className="px-2 py-2 text-right text-white">{totals.points}</td>
              <td className="px-2 py-2 text-right text-slate-400"></td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.goals}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.assists}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.cs}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.yc}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.rc}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.og}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.ps}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.pm}</td>
              <td className="px-2 py-2 text-right text-slate-400">{totals.bonus}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}

function FixturesTable({ fixtures, team }: { fixtures: Fixture[]; team: string }) {
  const upcoming = fixtures.filter(f => !f.isFinished);

  if (upcoming.length === 0) {
    return <p className="text-slate-500 text-sm text-center py-4">No upcoming fixtures</p>;
  }

  return (
    <div className="bg-slate-800 rounded-xl overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-xs">
          <thead>
            <tr className="text-slate-400 uppercase border-b border-slate-700">
              <th className="text-left px-3 py-2">GW</th>
              <th className="text-left px-2 py-2">Opponent</th>
              <th className="text-center px-2 py-2">Venue</th>
            </tr>
          </thead>
          <tbody>
            {upcoming.map(f => {
              const opp = getOpponent(f, team);
              return (
                <tr key={f.id} className="border-b border-slate-700/30 hover:bg-slate-700/20">
                  <td className="px-3 py-2.5 text-slate-300 font-medium">{f.gameweekNumber}</td>
                  <td className="px-2 py-2.5">
                    <div className="flex items-center gap-2">
                      <img src={getTeamLogo(opp.opp)} alt="" className="w-5 h-5 rounded-full" />
                      <span className="text-white">{getTeamDisplayName(opp.opp)}</span>
                    </div>
                  </td>
                  <td className="px-2 py-2.5 text-center">
                    <span className={`px-2 py-0.5 rounded text-[10px] font-bold ${
                      opp.venue === 'H' ? 'bg-emerald-500/20 text-emerald-400' : 'bg-red-500/20 text-red-400'
                    }`}>
                      {opp.venue === 'H' ? 'HOME' : 'AWAY'}
                    </span>
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
