import { useEffect, useState } from 'react';
import api from '../api/client';
import { getTeamLogo, getTeamDisplayName } from '../utils/teamAssets';

interface MatchFixture {
  id: number;
  gameweekNumber: number;
  homeTeam: string;
  awayTeam: string;
  homeGoals: number | null;
  awayGoals: number | null;
  isFinished: boolean;
}

export default function Matches() {
  const [matches, setMatches] = useState<MatchFixture[]>([]);
  const [currentGw, setCurrentGw] = useState(28); // default
  const [loading, setLoading] = useState(true);
  const [totalGws] = useState(34);

  useEffect(() => {
    // Find the current/next GW on mount
    api.get('/gameweek/current')
      .then(res => setCurrentGw(res.data.number))
      .catch(() => {});
  }, []);

  useEffect(() => {
    setLoading(true);
    api.get<MatchFixture[]>(`/fixtures/gameweek/${currentGw}`)
      .then(res => setMatches(res.data))
      .catch(() => setMatches([]))
      .finally(() => setLoading(false));
  }, [currentGw]);

  const allFinished = matches.length > 0 && matches.every(m => m.isFinished);
  const allUpcoming = matches.length > 0 && matches.every(m => !m.isFinished);

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      {/* GW Navigator */}
      <div className="flex items-center justify-center gap-4 mb-8">
        <button
          onClick={() => setCurrentGw(g => Math.max(1, g - 1))}
          disabled={currentGw <= 1}
          className="w-10 h-10 rounded-full bg-slate-800 hover:bg-slate-700 disabled:opacity-30 text-white flex items-center justify-center transition text-lg font-bold"
        >
          &lsaquo;
        </button>
        <div className="text-center">
          <h1 className="text-xl font-bold text-white">Matchweek {currentGw}</h1>
          <p className="text-slate-400 text-xs mt-0.5">
            {allFinished ? 'All matches finished' : allUpcoming ? 'Upcoming' : 'In progress'}
          </p>
        </div>
        <button
          onClick={() => setCurrentGw(g => Math.min(totalGws, g + 1))}
          disabled={currentGw >= totalGws}
          className="w-10 h-10 rounded-full bg-slate-800 hover:bg-slate-700 disabled:opacity-30 text-white flex items-center justify-center transition text-lg font-bold"
        >
          &rsaquo;
        </button>
      </div>

      {/* GW Quick Select */}
      <div className="flex justify-center gap-1 mb-6 flex-wrap">
        {Array.from({ length: totalGws }, (_, i) => i + 1).map(gw => (
          <button
            key={gw}
            onClick={() => setCurrentGw(gw)}
            className={`w-8 h-8 rounded text-xs font-medium transition ${
              gw === currentGw
                ? 'bg-emerald-500 text-white'
                : 'bg-slate-800 text-slate-400 hover:bg-slate-700 hover:text-white'
            }`}
          >
            {gw}
          </button>
        ))}
      </div>

      {/* Matches */}
      {loading ? (
        <div className="text-center py-16 text-slate-400">Loading...</div>
      ) : matches.length === 0 ? (
        <div className="text-center py-16 text-slate-500">No matches for this gameweek</div>
      ) : (
        <div className="bg-slate-800 rounded-2xl overflow-hidden divide-y divide-slate-700/50">
          {matches.map(match => (
            <MatchRow key={match.id} match={match} />
          ))}
        </div>
      )}
    </div>
  );
}

function MatchRow({ match }: { match: MatchFixture }) {
  const isFinished = match.isFinished;

  return (
    <div className="flex items-center px-4 py-4 hover:bg-slate-700/20 transition">
      {/* Status */}
      <div className="w-10 flex-shrink-0">
        <span className={`text-[10px] font-bold uppercase ${isFinished ? 'text-slate-500' : 'text-emerald-400'}`}>
          {isFinished ? 'FT' : 'TBD'}
        </span>
      </div>

      {/* Home team */}
      <div className="flex-1 flex items-center justify-end gap-2">
        <span className="text-white text-sm font-medium text-right">{getTeamDisplayName(match.homeTeam)}</span>
        <img src={getTeamLogo(match.homeTeam)} alt="" className="w-7 h-7 object-contain flex-shrink-0" />
      </div>

      {/* Score */}
      <div className="w-20 text-center flex-shrink-0 mx-2">
        {isFinished ? (
          <span className="text-white font-bold text-lg">
            {match.homeGoals} <span className="text-slate-500">-</span> {match.awayGoals}
          </span>
        ) : (
          <span className="text-slate-500 text-sm font-medium">vs</span>
        )}
      </div>

      {/* Away team */}
      <div className="flex-1 flex items-center gap-2">
        <img src={getTeamLogo(match.awayTeam)} alt="" className="w-7 h-7 object-contain flex-shrink-0" />
        <span className="text-white text-sm font-medium">{getTeamDisplayName(match.awayTeam)}</span>
      </div>
    </div>
  );
}
