import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import api from '../../api/client';
import { LeagueType, type MyTeamSummary } from '../../api/types';

interface Props {
  /** The currently-selected leagueId from the URL. null = Global team. */
  current: number | null;
}

/**
 * Dropdown showing all of the user's teams (global + per-league).
 * Switching a team updates the `?leagueId=` query param on the current route,
 * which the page reads to load the right team.
 */
export default function TeamSelector({ current }: Props) {
  const [teams, setTeams] = useState<MyTeamSummary[]>([]);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    api.get<MyTeamSummary[]>('/team/mine')
      .then((r) => setTeams(r.data))
      .catch(() => setTeams([]));
  }, []);

  // Hide the selector entirely if the user only has 0 or 1 team — no choice to make.
  if (teams.length <= 1) return null;

  const onChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    const params = new URLSearchParams(location.search);
    if (value === 'global') {
      params.delete('leagueId');
    } else {
      params.set('leagueId', value);
    }
    const qs = params.toString();
    navigate(`${location.pathname}${qs ? `?${qs}` : ''}`);
  };

  const selectedValue = current == null ? 'global' : String(current);

  return (
    <div className="flex items-center gap-2">
      <label className="text-xs text-slate-400 uppercase tracking-wide">Team</label>
      <select
        value={selectedValue}
        onChange={onChange}
        className="bg-slate-800 border border-slate-700 rounded-lg px-3 py-1.5 text-white text-sm focus:outline-none focus:border-emerald-400"
      >
        {teams.map((t) => {
          const value = t.leagueId == null ? 'global' : String(t.leagueId);
          const tag = t.leagueContext === LeagueType.Draft ? ' (Draft)'
            : t.leagueContext === LeagueType.Classic ? ' (League)'
            : '';
          return (
            <option key={value} value={value}>
              {t.teamName} — {t.leagueName}{tag}
            </option>
          );
        })}
      </select>
    </div>
  );
}
