import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCountdown } from '../hooks/useCountdown';
import api from '../api/client';
import type { Gameweek } from '../api/types';
import { getTeamLogo } from '../utils/teamAssets';

const TEAMS = [
  'Bayern Munich', 'Borussia Dortmund', 'RB Leipzig', 'Bayer Leverkusen',
  'VfB Stuttgart', 'Eintracht Frankfurt', 'SC Freiburg', 'VfL Wolfsburg',
  'TSG Hoffenheim', 'Werder Bremen', 'FC Augsburg', '1. FC Union Berlin',
  'Borussia Mönchengladbach', '1. FSV Mainz 05', 'Hamburger SV',
  'FC St. Pauli', '1. FC Köln', '1. FC Heidenheim',
];

export default function Home() {
  const { user } = useAuth();
  const [currentGw, setCurrentGw] = useState<Gameweek | null>(null);
  const countdown = useCountdown(currentGw?.deadline);

  useEffect(() => {
    if (user) {
      api.get<Gameweek>('/gameweek/current').then((r) => setCurrentGw(r.data)).catch(() => {});
    }
  }, [user]);

  return (
    <div className="min-h-screen bg-slate-900">
      {/* Hero Section */}
      <div className="relative overflow-hidden" style={{
        background: 'linear-gradient(135deg, #1a0533 0%, #2d1b4e 30%, #1b3a4b 60%, #0f4c3a 100%)',
      }}>
        {/* Subtle animated gradient overlay */}
        <div className="absolute inset-0 opacity-30" style={{
          background: 'radial-gradient(ellipse at 70% 20%, rgba(16, 185, 129, 0.3) 0%, transparent 60%), radial-gradient(ellipse at 30% 80%, rgba(139, 92, 246, 0.3) 0%, transparent 60%)',
        }} />

        <div className="relative max-w-6xl mx-auto px-4 py-16 flex flex-col lg:flex-row items-center gap-8">
          {/* Left content */}
          <div className="flex-1 text-center lg:text-left">
            <img src="/bundesliga.webp" alt="Bundesliga" className="h-16 mb-6 mx-auto lg:mx-0" />
            <h1 className="text-4xl lg:text-5xl font-bold text-white mb-4 leading-tight">
              Fantasy<br />
              <span className="text-emerald-400">Bundesliga</span>
            </h1>
            <p className="text-lg text-slate-300 mb-3 max-w-md">
              Build your dream Bundesliga squad, compete with friends, and prove you're the ultimate manager.
            </p>
            <p className="text-slate-400 mb-8">
              It's FREE to play. Pick your 15 players within a 100M budget.
            </p>

            {!user ? (
              <div className="flex gap-4 justify-center lg:justify-start">
                <Link
                  to="/login"
                  className="px-8 py-3 rounded-full border-2 border-white text-white font-semibold hover:bg-white hover:text-slate-900 transition"
                >
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="px-8 py-3 rounded-full bg-emerald-500 text-white font-semibold hover:bg-emerald-600 transition"
                >
                  Register now
                </Link>
              </div>
            ) : (
              <div className="flex gap-4 justify-center lg:justify-start">
                <Link
                  to="/my-team"
                  className="px-8 py-3 rounded-full bg-emerald-500 text-white font-semibold hover:bg-emerald-600 transition"
                >
                  My Team
                </Link>
                <Link
                  to="/transfers"
                  className="px-8 py-3 rounded-full border-2 border-emerald-400 text-emerald-400 font-semibold hover:bg-emerald-400 hover:text-slate-900 transition"
                >
                  Transfers
                </Link>
              </div>
            )}
          </div>

          {/* Right side - team logos collage */}
          <div className="flex-1 hidden lg:flex flex-wrap justify-center gap-4 max-w-md opacity-80">
            {TEAMS.slice(0, 12).map((team) => (
              <img
                key={team}
                src={getTeamLogo(team)}
                alt={team}
                className="w-14 h-14 object-contain drop-shadow-lg hover:scale-110 transition-transform"
              />
            ))}
          </div>
        </div>
      </div>

      {/* Gameweek Deadline Banner */}
      {currentGw && (
        <div className="bg-gradient-to-r from-purple-900/60 to-emerald-900/60 border-y border-slate-700">
          <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
            <span className="text-slate-300 font-medium">Gameweek {currentGw.number} Deadline</span>
            <span className={`text-2xl font-bold ${countdown === 'LOCKED' ? 'text-red-400' : 'text-emerald-400'}`}>
              {countdown}
            </span>
          </div>
        </div>
      )}

      {/* Feature Cards */}
      <div className="max-w-6xl mx-auto px-4 py-16">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* Card 1 - Pick Your Squad */}
          <div className="group relative rounded-2xl overflow-hidden bg-gradient-to-br from-emerald-600 to-emerald-800 p-1">
            <div className="bg-slate-800 rounded-xl p-6 h-full">
              <div className="w-16 h-16 rounded-xl bg-emerald-500/20 flex items-center justify-center mb-4">
                <svg className="w-8 h-8 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Pick Your Squad</h3>
              <p className="text-slate-400">
                Use your budget of 100M to pick a squad of 15 players from the Bundesliga.
              </p>
            </div>
          </div>

          {/* Card 2 - Create and Join Leagues */}
          <div className="group relative rounded-2xl overflow-hidden bg-gradient-to-br from-violet-600 to-violet-800 p-1">
            <div className="bg-slate-800 rounded-xl p-6 h-full">
              <div className="w-16 h-16 rounded-xl bg-violet-500/20 flex items-center justify-center mb-4">
                <svg className="w-8 h-8 text-violet-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Create & Join Leagues</h3>
              <p className="text-slate-400">
                Play against friends and family in invitational leagues and compete for bragging rights.
              </p>
            </div>
          </div>

          {/* Card 3 - Compete Against Friends */}
          <div className="group relative rounded-2xl overflow-hidden bg-gradient-to-br from-amber-600 to-amber-800 p-1">
            <div className="bg-slate-800 rounded-xl p-6 h-full">
              <div className="w-16 h-16 rounded-xl bg-amber-500/20 flex items-center justify-center mb-4">
                <svg className="w-8 h-8 text-amber-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Live Scoring</h3>
              <p className="text-slate-400">
                Watch your points update in real-time on match days and track the Bundesliga standings.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Team Logos Strip */}
      <div className="border-t border-slate-700 bg-slate-800/50">
        <div className="max-w-6xl mx-auto px-4 py-8">
          <p className="text-center text-slate-500 text-sm font-semibold uppercase tracking-wider mb-6">
            18 Bundesliga Teams
          </p>
          <div className="flex flex-wrap justify-center gap-6">
            {TEAMS.map((team) => (
              <img
                key={team}
                src={getTeamLogo(team)}
                alt={team}
                className="w-10 h-10 object-contain opacity-70 hover:opacity-100 hover:scale-110 transition-all"
              />
            ))}
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer className="border-t border-slate-700 bg-slate-900">
        <div className="max-w-6xl mx-auto px-4 py-6 text-center text-slate-500 text-sm">
          Fantasy Bundesliga &copy; 2026. Built for fun.
        </div>
      </footer>
    </div>
  );
}
