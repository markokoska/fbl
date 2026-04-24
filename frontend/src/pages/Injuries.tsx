import { useEffect, useMemo, useState } from 'react';
import api from '../api/client';
import type { Player } from '../api/types';
import {
  getTeamLogo,
  getTeamDisplayName,
  getFitnessColor,
  getFitnessLabel,
} from '../utils/teamAssets';

type FilterKey = 'all' | 'injured' | 'doubtful' | 'knock';

export default function Injuries() {
  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<FilterKey>('all');
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    api
      .get<Player[]>('/players?sortBy=name&pageSize=1000')
      .then((r) => {
        if (!cancelled) setPlayers(r.data);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  // Only players who are not fully fit
  const injuredPlayers = useMemo(() => {
    return players.filter((p) => p.fitness < 100);
  }, [players]);

  // Apply filter
  const filtered = useMemo(() => {
    switch (filter) {
      case 'injured':
        return injuredPlayers.filter((p) => p.fitness <= 25);
      case 'doubtful':
        return injuredPlayers.filter((p) => p.fitness > 25 && p.fitness <= 50);
      case 'knock':
        return injuredPlayers.filter((p) => p.fitness > 50 && p.fitness < 100);
      default:
        return injuredPlayers;
    }
  }, [injuredPlayers, filter]);

  // Group by team
  const grouped = useMemo(() => {
    const map: Record<string, Player[]> = {};
    for (const p of filtered) {
      (map[p.team] ??= []).push(p);
    }
    // Sort each team by severity (lowest fitness first)
    for (const t of Object.keys(map)) {
      map[t].sort((a, b) => a.fitness - b.fitness);
    }
    return map;
  }, [filtered]);

  const sortedTeams = useMemo(
    () => Object.keys(grouped).sort((a, b) => getTeamDisplayName(a).localeCompare(getTeamDisplayName(b))),
    [grouped]
  );

  const counts = useMemo(
    () => ({
      all: injuredPlayers.length,
      injured: injuredPlayers.filter((p) => p.fitness <= 25).length,
      doubtful: injuredPlayers.filter((p) => p.fitness > 25 && p.fitness <= 50).length,
      knock: injuredPlayers.filter((p) => p.fitness > 50 && p.fitness < 100).length,
    }),
    [injuredPlayers]
  );

  const toggle = (team: string) =>
    setCollapsed((prev) => ({ ...prev, [team]: !prev[team] }));

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-white">Injury News</h1>
        <p className="text-slate-400 text-sm mt-1">Bundesliga 2025/26 — team-by-team fitness status</p>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-2 mb-6 flex-wrap">
        <FilterPill active={filter === 'all'} onClick={() => setFilter('all')} color="slate">
          All ({counts.all})
        </FilterPill>
        <FilterPill active={filter === 'injured'} onClick={() => setFilter('injured')} color="red">
          Injured ({counts.injured})
        </FilterPill>
        <FilterPill active={filter === 'doubtful'} onClick={() => setFilter('doubtful')} color="orange">
          Doubtful ({counts.doubtful})
        </FilterPill>
        <FilterPill active={filter === 'knock'} onClick={() => setFilter('knock')} color="yellow">
          Knock ({counts.knock})
        </FilterPill>
      </div>

      {loading ? (
        <div className="text-center py-16 text-slate-400">Loading...</div>
      ) : sortedTeams.length === 0 ? (
        <div className="text-center py-16 text-slate-500 text-sm bg-slate-800 rounded-2xl">
          No injury news for this filter
        </div>
      ) : (
        <div className="space-y-4">
          {sortedTeams.map((team) => (
            <TeamBlock
              key={team}
              team={team}
              players={grouped[team]}
              collapsed={!!collapsed[team]}
              onToggle={() => toggle(team)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function FilterPill({
  active,
  onClick,
  color,
  children,
}: {
  active: boolean;
  onClick: () => void;
  color: 'slate' | 'red' | 'orange' | 'yellow';
  children: React.ReactNode;
}) {
  const colorMap = {
    slate: active ? 'bg-slate-600 text-white' : 'bg-slate-800 text-slate-400 hover:bg-slate-700',
    red: active ? 'bg-red-500 text-white' : 'bg-slate-800 text-red-400 hover:bg-slate-700',
    orange: active ? 'bg-orange-500 text-white' : 'bg-slate-800 text-orange-400 hover:bg-slate-700',
    yellow: active ? 'bg-yellow-500 text-slate-900' : 'bg-slate-800 text-yellow-400 hover:bg-slate-700',
  };
  return (
    <button
      onClick={onClick}
      className={`px-3 py-1.5 rounded-full text-xs font-semibold transition ${colorMap[color]}`}
    >
      {children}
    </button>
  );
}

function TeamBlock({
  team,
  players,
  collapsed,
  onToggle,
}: {
  team: string;
  players: Player[];
  collapsed: boolean;
  onToggle: () => void;
}) {
  return (
    <div className="bg-slate-800 rounded-xl overflow-hidden border border-slate-700/50">
      {/* Team header */}
      <button
        onClick={onToggle}
        className="w-full flex items-center gap-3 px-4 py-3 hover:bg-slate-700/30 transition"
      >
        <img src={getTeamLogo(team)} alt="" className="w-8 h-8 object-contain flex-shrink-0" />
        <h2 className="text-white font-bold text-lg flex-1 text-left">{getTeamDisplayName(team)}</h2>
        <span className="text-slate-400 text-xs">
          {players.length} player{players.length === 1 ? '' : 's'}
        </span>
        <span className={`text-slate-500 text-sm transition-transform ${collapsed ? '' : 'rotate-90'}`}>
          &rsaquo;
        </span>
      </button>

      {/* Table */}
      {!collapsed && (
        <div className="border-t border-slate-700/50">
          {/* Column headers */}
          <div className="grid grid-cols-[1fr,140px,100px] gap-3 px-4 py-2 text-xs uppercase tracking-wide text-slate-500 font-semibold border-b border-slate-700/50">
            <div>Player</div>
            <div>Injury</div>
            <div className="text-right">Latest</div>
          </div>

          {players.map((p) => (
            <InjuryRow key={p.id} player={p} />
          ))}
        </div>
      )}
    </div>
  );
}

function InjuryRow({ player }: { player: Player }) {
  const color = getFitnessColor(player.fitness);
  const label = getFitnessLabel(player.fitness);

  return (
    <div className="grid grid-cols-[1fr,140px,100px] gap-3 px-4 py-2.5 items-center hover:bg-slate-700/20 transition border-b border-slate-700/30 last:border-b-0">
      {/* Player */}
      <div className="flex items-center gap-3 min-w-0">
        <div className="w-8 h-8 rounded-full overflow-hidden flex-shrink-0 bg-slate-700 flex items-center justify-center">
          <img
            src={`/players/${player.id}.png`}
            alt=""
            className="w-full h-full object-cover object-top"
            onError={(e) => {
              (e.currentTarget as HTMLImageElement).style.display = 'none';
            }}
          />
        </div>
        <span className="text-white text-sm font-medium truncate">{player.name}</span>
      </div>

      {/* Injury status */}
      <div className="flex items-center gap-2">
        <span className="w-2 h-2 rounded-full flex-shrink-0" style={{ backgroundColor: color }} />
        <span className="text-sm font-medium" style={{ color }}>
          {label}
        </span>
      </div>

      {/* Latest (fitness %) */}
      <div className="text-right">
        <span className="text-slate-400 text-xs font-semibold tabular-nums">{player.fitness}%</span>
      </div>
    </div>
  );
}
