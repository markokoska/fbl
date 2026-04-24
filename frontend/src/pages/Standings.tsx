import { useEffect, useState } from 'react';
import api from '../api/client';
import { getTeamLogo, getTeamDisplayName } from '../utils/teamAssets';

interface Standing {
  position: number;
  team: string;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
  form: string[];
}

function getPositionColor(pos: number): string {
  if (pos <= 1) return 'bg-blue-600';        // Champions League (league phase)
  if (pos <= 4) return 'bg-blue-500';        // Champions League
  if (pos <= 5) return 'bg-orange-500';      // Europa League
  if (pos <= 6) return 'bg-green-600';       // Conference League (qualification)
  if (pos >= 17) return 'bg-red-600';        // Relegation (2. Bundesliga)
  if (pos === 16) return 'bg-red-400';       // Relegation playoff
  return 'bg-transparent';
}

function FormBadge({ result }: { result: string }) {
  const bg = result === 'W' ? 'bg-emerald-500' : result === 'D' ? 'bg-amber-500' : 'bg-red-500';
  return (
    <span className={`${bg} text-white text-[10px] font-bold w-5 h-5 rounded-sm flex items-center justify-center`}>
      {result}
    </span>
  );
}

export default function Standings() {
  const [standings, setStandings] = useState<Standing[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/fixtures/standings')
      .then(r => setStandings(r.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="text-center py-16 text-slate-400">Loading standings...</div>;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-white mb-6">Bundesliga Standings</h1>

      <div className="bg-slate-800 rounded-xl overflow-x-auto border border-slate-700">
        {/* Header */}
        <div className="grid grid-cols-[36px_1fr_36px_36px_36px_36px_60px_44px_44px_130px] gap-1 px-4 py-3 bg-slate-700/50 text-slate-400 text-xs font-semibold uppercase">
          <span className="text-center">#</span>
          <span>Team</span>
          <span className="text-center">MP</span>
          <span className="text-center">W</span>
          <span className="text-center">D</span>
          <span className="text-center">L</span>
          <span className="text-center">G</span>
          <span className="text-center">GD</span>
          <span className="text-center font-bold">PTS</span>
          <span className="text-center">Form</span>
        </div>

        {/* Rows */}
        {standings.map((s) => (
          <div
            key={s.team}
            className="grid grid-cols-[36px_1fr_36px_36px_36px_36px_60px_44px_44px_130px] gap-1 px-4 py-2.5 border-t border-slate-700/50 hover:bg-slate-700/30 items-center"
          >
            {/* Position with colored indicator */}
            <div className="flex items-center justify-center gap-1">
              <div className={`w-1 h-6 rounded-full ${getPositionColor(s.position)}`} />
              <span className="text-white font-semibold text-sm">{s.position}</span>
            </div>

            {/* Team */}
            <div className="flex items-center gap-2">
              <img src={getTeamLogo(s.team)} alt="" className="w-6 h-6 object-contain" />
              <span className="text-white font-medium text-sm truncate">
                {getTeamDisplayName(s.team)}
              </span>
            </div>

            <span className="text-center text-slate-300 text-sm">{s.played}</span>
            <span className="text-center text-slate-300 text-sm">{s.won}</span>
            <span className="text-center text-slate-300 text-sm">{s.drawn}</span>
            <span className="text-center text-slate-300 text-sm">{s.lost}</span>
            <span className="text-center text-slate-300 text-sm">{s.goalsFor}:{s.goalsAgainst}</span>
            <span className={`text-center text-sm font-medium ${s.goalDifference > 0 ? 'text-emerald-400' : s.goalDifference < 0 ? 'text-red-400' : 'text-slate-300'}`}>
              {s.goalDifference > 0 ? '+' : ''}{s.goalDifference}
            </span>
            <span className="text-center text-white font-bold text-sm">{s.points}</span>

            {/* Form */}
            <div className="flex items-center justify-center gap-1">
              {s.form.map((f, i) => (
                <FormBadge key={i} result={f} />
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* Legend */}
      <div className="mt-4 flex flex-wrap gap-4 text-xs text-slate-400">
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-blue-500" />
          Champions League
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-orange-500" />
          Europa League
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-green-600" />
          Conference League (Qual.)
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-red-400" />
          Relegation Playoff
        </div>
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-red-600" />
          Relegation
        </div>
      </div>
    </div>
  );
}
