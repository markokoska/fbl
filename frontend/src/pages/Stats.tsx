import { useEffect, useState } from 'react';
import api from '../api/client';
import type { Player } from '../api/types';
import { getTeamShort, getTeamDisplayName, getTeamColors } from '../utils/teamAssets';

type Category = {
  key: string;
  title: string;
  sortBy: string;
  statValue: (p: Player) => number | string;
  accent: string; // tailwind bg class
};

const CATEGORIES: Category[] = [
  {
    key: 'goals',
    title: 'Goals',
    sortBy: 'goals',
    statValue: (p) => p.stats.goals,
    accent: 'from-emerald-500/20 to-emerald-500/0',
  },
  {
    key: 'assists',
    title: 'Assists',
    sortBy: 'assists',
    statValue: (p) => p.stats.assists,
    accent: 'from-sky-500/20 to-sky-500/0',
  },
  {
    key: 'points',
    title: 'Total Points',
    sortBy: 'totalPoints',
    statValue: (p) => p.totalPoints,
    accent: 'from-fuchsia-500/20 to-fuchsia-500/0',
  },
  {
    key: 'cleanSheets',
    title: 'Clean Sheets',
    sortBy: 'cleanSheets',
    statValue: (p) => p.stats.cleanSheets,
    accent: 'from-amber-500/20 to-amber-500/0',
  },
];

const TOP_N = 10;

export default function Stats() {
  const [data, setData] = useState<Record<string, Player[]>>({});
  const [loading, setLoading] = useState(true);
  const [openCategory, setOpenCategory] = useState<Category | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const results = await Promise.all(
          CATEGORIES.map((c) =>
            api.get<Player[]>(`/players?sortBy=${c.sortBy}&pageSize=${TOP_N}`).then((r) => [c.key, r.data] as const)
          )
        );
        if (!cancelled) {
          setData(Object.fromEntries(results));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-white">Stats Centre</h1>
        <p className="text-slate-400 text-sm mt-1">Bundesliga 2025/26 Player Stats</p>
      </div>

      {loading ? (
        <div className="text-center py-16 text-slate-400">Loading...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
          {CATEGORIES.map((cat) => (
            <Leaderboard
              key={cat.key}
              category={cat}
              players={data[cat.key] ?? []}
              onViewAll={() => setOpenCategory(cat)}
            />
          ))}
        </div>
      )}

      {openCategory && (
        <FullListModal category={openCategory} onClose={() => setOpenCategory(null)} />
      )}
    </div>
  );
}

function Leaderboard({
  category,
  players,
  onViewAll,
}: {
  category: Category;
  players: Player[];
  onViewAll: () => void;
}) {
  return (
    <div className="bg-slate-800 rounded-2xl overflow-hidden border border-slate-700/50 flex flex-col">
      {/* Title bar (clickable) */}
      <button
        onClick={onViewAll}
        className={`bg-gradient-to-b ${category.accent} px-5 py-4 border-b border-slate-700/50 text-left hover:brightness-125 transition flex items-center justify-between group`}
      >
        <h2 className="text-white font-bold uppercase tracking-wide text-sm">{category.title}</h2>
        <span className="text-slate-400 text-xs group-hover:text-white transition">View all &rsaquo;</span>
      </button>

      {/* Rows */}
      <div className="divide-y divide-slate-700/40">
        {players.length === 0 ? (
          <div className="text-center text-slate-500 text-sm py-8">No data</div>
        ) : (
          players.map((p, idx) => <StatRow key={p.id} player={p} rank={idx + 1} category={category} />)
        )}
      </div>

      {/* Footer View all button */}
      <button
        onClick={onViewAll}
        className="border-t border-slate-700/50 px-4 py-3 text-emerald-400 hover:text-emerald-300 hover:bg-slate-700/30 transition text-xs font-semibold uppercase tracking-wide"
      >
        View all {category.title.toLowerCase()} &rsaquo;
      </button>
    </div>
  );
}

function StatRow({ player, rank, category }: { player: Player; rank: number; category: Category }) {
  const colors = getTeamColors(player.team);
  return (
    <div className="flex items-center gap-3 px-3 py-2.5 hover:bg-slate-700/30 transition">
      {/* Rank */}
      <div className="w-5 text-slate-500 text-xs font-semibold text-center flex-shrink-0">{rank}</div>

      {/* Photo with team-color background */}
      <div
        className="w-10 h-10 rounded-full overflow-hidden flex-shrink-0 flex items-center justify-center ring-2 ring-slate-900"
        style={{ backgroundColor: colors.primary === '#FFFFFF' ? colors.secondary : colors.primary }}
      >
        <img
          src={`/players/${player.id}.png`}
          alt=""
          className="w-full h-full object-cover object-top"
          onError={(e) => {
            (e.currentTarget as HTMLImageElement).style.display = 'none';
          }}
        />
      </div>

      {/* Name + Team */}
      <div className="flex-1 min-w-0">
        <div className="text-white text-sm font-medium truncate">{player.name}</div>
        <div className="text-slate-400 text-xs truncate">
          {getTeamShort(player.team)} · {getTeamDisplayName(player.team)}
        </div>
      </div>

      {/* Stat value */}
      <div className="text-white font-bold text-lg tabular-nums flex-shrink-0">
        {category.statValue(player)}
      </div>
    </div>
  );
}

function FullListModal({ category, onClose }: { category: Category; onClose: () => void }) {
  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    api
      .get<Player[]>(`/players?sortBy=${category.sortBy}&pageSize=1000`)
      .then((r) => {
        if (!cancelled) setPlayers(r.data);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [category.sortBy]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  const filtered = search
    ? players.filter(
        (p) =>
          p.name.toLowerCase().includes(search.toLowerCase()) ||
          p.team.toLowerCase().includes(search.toLowerCase())
      )
    : players;

  return (
    <div
      className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        className="bg-slate-800 rounded-2xl overflow-hidden border border-slate-700 w-full max-w-2xl max-h-[85vh] flex flex-col"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div
          className={`bg-gradient-to-b ${category.accent} px-6 py-4 border-b border-slate-700 flex items-center justify-between`}
        >
          <div>
            <h2 className="text-white font-bold text-xl">{category.title}</h2>
            <p className="text-slate-300 text-xs mt-0.5">Bundesliga 2025/26 — all players</p>
          </div>
          <button
            onClick={onClose}
            className="text-slate-300 hover:text-white text-2xl w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-700 transition"
          >
            &times;
          </button>
        </div>

        {/* Search */}
        <div className="px-4 py-3 border-b border-slate-700/50">
          <input
            type="text"
            placeholder="Search player or team..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full bg-slate-900 text-white text-sm rounded-lg px-3 py-2 border border-slate-700 focus:border-emerald-500 focus:outline-none"
          />
        </div>

        {/* List */}
        <div className="flex-1 overflow-y-auto divide-y divide-slate-700/40">
          {loading ? (
            <div className="text-center py-16 text-slate-400">Loading...</div>
          ) : filtered.length === 0 ? (
            <div className="text-center py-16 text-slate-500 text-sm">No players match</div>
          ) : (
            filtered.map((p, idx) => (
              <StatRow key={p.id} player={p} rank={idx + 1} category={category} />
            ))
          )}
        </div>

        {/* Footer */}
        <div className="px-4 py-2 border-t border-slate-700/50 text-slate-500 text-xs text-center">
          {filtered.length} player{filtered.length === 1 ? '' : 's'}
        </div>
      </div>
    </div>
  );
}
