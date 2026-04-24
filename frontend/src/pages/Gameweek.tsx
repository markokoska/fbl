import { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import api from '../api/client';
import { GameweekStatus, type Gameweek as GwType } from '../api/types';

interface PlayerScore {
  playerId: number;
  playerName: string;
  team: string;
  totalPoints: number;
  events: { eventType: string; minute: number | null; points: number }[];
}

interface LiveEvent {
  playerName: string;
  team: string;
  eventType: string;
  minute: number | null;
  points: number;
}

export default function GameweekPage() {
  const [gameweeks, setGameweeks] = useState<GwType[]>([]);
  const [selectedGw, setSelectedGw] = useState<number | null>(null);
  const [scores, setScores] = useState<PlayerScore[]>([]);
  const [liveEvents, setLiveEvents] = useState<LiveEvent[]>([]);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    api.get<GwType[]>('/gameweek').then((r) => {
      setGameweeks(r.data);
      const current = r.data.find((g) => g.status === GameweekStatus.Live || g.status === GameweekStatus.Upcoming);
      if (current) setSelectedGw(current.id);
    });
  }, []);

  useEffect(() => {
    if (selectedGw) {
      api.get(`/gameweek/${selectedGw}/scores`).then((r) => {
        setScores(r.data.scores || []);
      });

      // Connect SignalR
      const token = localStorage.getItem('fbl_token');
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/livescore?access_token=${token}`)
        .withAutomaticReconnect()
        .build();

      connection.start().then(() => {
        connection.invoke('JoinGameweek', selectedGw);
      });

      connection.on('MatchEvent', (evt: LiveEvent) => {
        setLiveEvents((prev) => [evt, ...prev].slice(0, 50));
        // Refresh scores
        api.get(`/gameweek/${selectedGw}/scores`).then((r) => {
          setScores(r.data.scores || []);
        });
      });

      connection.on('BatchUpdate', () => {
        api.get(`/gameweek/${selectedGw}/scores`).then((r) => {
          setScores(r.data.scores || []);
        });
      });

      connectionRef.current = connection;

      return () => {
        connection.stop();
      };
    }
  }, [selectedGw]);

  const selectedGwData = gameweeks.find((g) => g.id === selectedGw);
  const statusLabel = (s: GameweekStatus) => ['Upcoming', 'Live', 'Finished'][s];
  const statusColor = (s: GameweekStatus) => ['text-slate-400', 'text-green-400', 'text-slate-500'][s];

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-white mb-6">Live Scores</h1>

      {/* GW selector */}
      <div className="flex gap-2 mb-6 overflow-x-auto pb-2">
        {gameweeks.map((gw) => (
          <button
            key={gw.id}
            onClick={() => { setSelectedGw(gw.id); setLiveEvents([]); }}
            className={`px-3 py-1.5 rounded text-sm font-medium whitespace-nowrap transition ${
              selectedGw === gw.id
                ? 'bg-emerald-500 text-white'
                : 'bg-slate-800 text-slate-400 hover:text-white'
            }`}
          >
            GW{gw.number}
          </button>
        ))}
      </div>

      {selectedGwData && (
        <div className="bg-slate-800 rounded-xl p-4 mb-6 flex items-center justify-between">
          <div>
            <h2 className="text-lg font-semibold text-white">Gameweek {selectedGwData.number}</h2>
            <p className="text-slate-400 text-sm">
              Kickoff: {new Date(selectedGwData.kickoffTime).toLocaleString()}
            </p>
          </div>
          <span className={`text-sm font-semibold ${statusColor(selectedGwData.status)}`}>
            {statusLabel(selectedGwData.status)}
          </span>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Live feed */}
        <div className="lg:col-span-1">
          <h3 className="text-sm font-semibold text-slate-400 mb-3">Live Feed</h3>
          <div className="bg-slate-800 rounded-xl p-4 max-h-96 overflow-y-auto space-y-2">
            {liveEvents.length === 0 ? (
              <p className="text-slate-500 text-sm text-center py-4">Waiting for events...</p>
            ) : (
              liveEvents.map((evt, i) => (
                <div key={i} className="bg-slate-700/50 rounded-lg p-2 text-sm">
                  <span className="text-white font-medium">{evt.playerName}</span>
                  <span className="text-slate-400"> ({evt.team})</span>
                  <br />
                  <span className="text-emerald-400">{evt.eventType}</span>
                  {evt.minute && <span className="text-slate-500"> {evt.minute}'</span>}
                  <span className={`ml-2 font-semibold ${evt.points >= 0 ? 'text-green-400' : 'text-red-400'}`}>
                    {evt.points > 0 ? '+' : ''}{evt.points} pts
                  </span>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Scores table */}
        <div className="lg:col-span-2">
          <h3 className="text-sm font-semibold text-slate-400 mb-3">Player Scores</h3>
          <div className="bg-slate-800 rounded-xl overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="text-slate-400 text-xs uppercase border-b border-slate-700">
                  <th className="text-left px-4 py-3">Player</th>
                  <th className="text-left px-4 py-3">Team</th>
                  <th className="text-right px-4 py-3">Points</th>
                </tr>
              </thead>
              <tbody>
                {scores.length === 0 ? (
                  <tr><td colSpan={3} className="text-center py-8 text-slate-500">No scores yet</td></tr>
                ) : (
                  scores.map((s) => (
                    <tr key={s.playerId} className="border-b border-slate-700/50 hover:bg-slate-700/30">
                      <td className="px-4 py-3 text-white">{s.playerName}</td>
                      <td className="px-4 py-3 text-slate-400 text-sm">{s.team}</td>
                      <td className="px-4 py-3 text-right font-semibold text-emerald-400">{s.totalPoints}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
