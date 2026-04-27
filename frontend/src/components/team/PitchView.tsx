import { useState } from 'react';
import type { Pick } from '../../api/types';
import { PlayerPosition } from '../../api/types';
import { getJerseySvg, getTeamShort, getFitnessColor } from '../../utils/teamAssets';
import api from '../../api/client';
import PlayerInfoModal from './PlayerInfoModal';

type Formation = '3-4-3' | '3-5-2' | '4-3-3' | '4-4-2' | '4-5-1' | '5-3-2' | '5-4-1';

const FORMATIONS: Record<Formation, [number, number, number]> = {
  '3-4-3': [3, 4, 3],
  '3-5-2': [3, 5, 2],
  '4-3-3': [4, 3, 3],
  '4-4-2': [4, 4, 2],
  '4-5-1': [4, 5, 1],
  '5-3-2': [5, 3, 2],
  '5-4-1': [5, 4, 1],
};

function detectFormation(starting: Pick[]): Formation | null {
  const defs = starting.filter(p => p.position === PlayerPosition.DEF).length;
  const mids = starting.filter(p => p.position === PlayerPosition.MID).length;
  const fwds = starting.filter(p => p.position === PlayerPosition.FWD).length;
  for (const [name, [d, m, f]] of Object.entries(FORMATIONS)) {
    if (d === defs && m === mids && f === fwds) return name as Formation;
  }
  return null;
}

interface Props {
  picks: Pick[];
  formation: Formation;
  onFormationChange: (f: Formation) => void;
  onPicksUpdated?: (picks: Pick[]) => void;
  readOnly?: boolean;
  /** League context for the team being managed. null = global team. */
  leagueId?: number | null;
}

type Mode = 'idle' | 'swap' | 'menu';

function PlayerCard({ pick, selected, swapTarget, onClick }: {
  pick: Pick; selected: boolean; swapTarget: boolean; onClick?: () => void;
}) {
  const isGk = pick.position === PlayerPosition.GK;
  const jersey = getJerseySvg(pick.team, isGk);
  const lastName = pick.playerName.split(' ').slice(-1)[0];

  return (
    <div
      className={`flex flex-col items-center w-16 sm:w-20 relative cursor-pointer transition-transform ${
        selected ? 'scale-110' : swapTarget ? 'scale-105 opacity-90' : 'hover:scale-105'
      }`}
      onClick={onClick}
    >
      {selected && (
        <div className="absolute -inset-1 rounded-xl border-2 border-amber-400 bg-amber-400/10 z-0 animate-pulse" />
      )}
      {swapTarget && !selected && (
        <div className="absolute -inset-1 rounded-xl border-2 border-sky-400/60 bg-sky-400/10 z-0" />
      )}

      {pick.isCaptain && (
        <div className="absolute -top-1 -right-1 z-10 w-5 h-5 rounded-full bg-amber-500 text-black text-[10px] font-bold flex items-center justify-center shadow">C</div>
      )}
      {pick.isViceCaptain && (
        <div className="absolute -top-1 -right-1 z-10 w-5 h-5 rounded-full bg-slate-400 text-black text-[10px] font-bold flex items-center justify-center shadow">V</div>
      )}

      <img src={jersey} alt="" className="w-10 h-12 sm:w-12 sm:h-14 drop-shadow-lg relative z-[1]" />

      <div
        className="absolute top-0 left-0 w-2.5 h-2.5 rounded-full border border-white/30 z-[2]"
        style={{ backgroundColor: getFitnessColor(100) }}
        title="Fit"
      />

      <div className="mt-0.5 bg-slate-900/80 backdrop-blur rounded px-1.5 py-0.5 text-center w-full relative z-[1]">
        <p className="text-[10px] sm:text-xs text-white font-medium truncate leading-tight">{lastName}</p>
        <p className="text-[9px] text-slate-400 leading-tight">{getTeamShort(pick.team)}</p>
      </div>

      <div className="mt-0.5 bg-emerald-600/90 rounded px-1.5 py-0.5 text-[10px] sm:text-xs font-bold text-white relative z-[1]">
        {pick.gameweekPoints}
      </div>
    </div>
  );
}

function BenchCard({ pick, selected, swapTarget, onClick }: {
  pick: Pick; selected: boolean; swapTarget: boolean; onClick?: () => void;
}) {
  const isGk = pick.position === PlayerPosition.GK;
  const jersey = getJerseySvg(pick.team, isGk);
  const lastName = pick.playerName.split(' ').slice(-1)[0];

  return (
    <div
      className={`flex flex-col items-center w-16 sm:w-20 relative cursor-pointer transition-transform ${
        selected ? 'scale-110' : swapTarget ? 'scale-105' : 'hover:scale-105'
      }`}
      onClick={onClick}
    >
      {selected && (
        <div className="absolute -inset-1 rounded-xl border-2 border-amber-400 bg-amber-400/10 z-0 animate-pulse" />
      )}
      {swapTarget && !selected && (
        <div className="absolute -inset-1 rounded-xl border-2 border-sky-400/60 bg-sky-400/10 z-0" />
      )}
      <img src={jersey} alt="" className="w-8 h-10 sm:w-10 sm:h-12 opacity-70 relative z-[1]" />
      <div className="mt-0.5 bg-slate-800/80 rounded px-1.5 py-0.5 text-center w-full relative z-[1]">
        <p className="text-[10px] sm:text-xs text-slate-300 truncate leading-tight">{lastName}</p>
        <p className="text-[9px] text-slate-500 leading-tight">{getTeamShort(pick.team)}</p>
      </div>
      <div className="mt-0.5 bg-slate-700/80 rounded px-1.5 py-0.5 text-[10px] sm:text-xs text-slate-300 relative z-[1]">
        {pick.gameweekPoints}
      </div>
    </div>
  );
}

export default function PitchView({ picks, formation, onFormationChange, onPicksUpdated, readOnly, leagueId }: Props) {
  const leagueQs = leagueId == null ? '' : `?leagueId=${leagueId}`;
  const [mode, setMode] = useState<Mode>('idle');
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [msg, setMsg] = useState('');
  const [saving, setSaving] = useState(false);
  const [infoPlayerId, setInfoPlayerId] = useState<number | null>(null);

  const starting = picks.filter(p => p.squadPosition <= 11).sort((a, b) => a.squadPosition - b.squadPosition);
  const bench = picks.filter(p => p.squadPosition > 11).sort((a, b) => a.squadPosition - b.squadPosition);

  const gks = starting.filter(p => p.position === PlayerPosition.GK);
  const defs = starting.filter(p => p.position === PlayerPosition.DEF);
  const mids = starting.filter(p => p.position === PlayerPosition.MID);
  const fwds = starting.filter(p => p.position === PlayerPosition.FWD);

  // Auto-detect formation from actual starting XI
  const detectedFormation = detectFormation(starting) || formation;

  const rows = [
    { players: fwds, label: 'FWD' },
    { players: mids, label: 'MID' },
    { players: defs, label: 'DEF' },
    { players: gks, label: 'GK' },
  ];

  const showMsg = (text: string, duration = 2000) => {
    setMsg(text);
    setTimeout(() => setMsg(''), duration);
  };

  const savePicks = async (newPicks: Pick[]) => {
    setSaving(true);
    try {
      await api.put(`/team/picks${leagueQs}`, {
        picks: newPicks.map(p => ({
          playerId: p.playerId,
          squadPosition: p.squadPosition,
          isCaptain: p.isCaptain,
          isViceCaptain: p.isViceCaptain,
        })),
      });
      if (onPicksUpdated) onPicksUpdated(newPicks);
      return true;
    } catch (err: any) {
      showMsg(err.response?.data || 'Save failed');
      return false;
    } finally {
      setSaving(false);
    }
  };

  const setCaptain = async (playerId: number) => {
    const pick = picks.find(p => p.playerId === playerId);
    if (!pick || pick.squadPosition > 11) return;

    const newPicks = picks.map(p => ({
      ...p,
      isCaptain: p.playerId === playerId,
      isViceCaptain: p.playerId === playerId ? false : p.isViceCaptain,
    }));

    if (await savePicks(newPicks)) showMsg('Captain set!');
    reset();
  };

  const setViceCaptain = async (playerId: number) => {
    const pick = picks.find(p => p.playerId === playerId);
    if (!pick || pick.squadPosition > 11) return;

    const newPicks = picks.map(p => ({
      ...p,
      isViceCaptain: p.playerId === playerId,
      isCaptain: p.playerId === playerId ? false : p.isCaptain,
    }));

    if (await savePicks(newPicks)) showMsg('Vice Captain set!');
    reset();
  };

  const handleSwap = async (clickedId: number) => {
    const pickA = picks.find(p => p.playerId === selectedId)!;
    const pickB = picks.find(p => p.playerId === clickedId)!;

    const newPicks = picks.map(p => {
      if (p.playerId === pickA.playerId) return { ...p, squadPosition: pickB.squadPosition };
      if (p.playerId === pickB.playerId) return { ...p, squadPosition: pickA.squadPosition };
      return p;
    });

    const newStarting = newPicks.filter(p => p.squadPosition <= 11);
    const gkCount = newStarting.filter(p => p.position === PlayerPosition.GK).length;
    if (gkCount !== 1) {
      showMsg('Must have exactly 1 GK in starting XI');
      reset();
      return;
    }

    const newFormation = detectFormation(newStarting);
    if (!newFormation) {
      showMsg('Invalid formation — need 3-5 DEF, 2-5 MID, 1-3 FWD');
      reset();
      return;
    }

    if (await savePicks(newPicks)) {
      if (newFormation !== formation) onFormationChange(newFormation);
      showMsg('Swap saved!');
    }
    reset();
  };

  const reset = () => {
    setMode('idle');
    setSelectedId(null);
  };

  const handleClick = (clickedId: number) => {
    if (readOnly) {
      setInfoPlayerId(clickedId);
      return;
    }

    // In swap mode — complete the swap
    if (mode === 'swap' && selectedId !== null) {
      if (clickedId === selectedId) { reset(); return; }
      handleSwap(clickedId);
      return;
    }

    // Click on already-selected player — deselect
    if (selectedId === clickedId) { reset(); return; }

    // First click — show menu
    setSelectedId(clickedId);
    setMode('menu');
    setMsg('');
  };

  const isStarter = (id: number) => picks.find(p => p.playerId === id)?.squadPosition! <= 11;

  return (
    <div>
      {/* Formation display */}
      <div className="flex items-center justify-center mb-4">
        <span className="px-4 py-1.5 rounded-lg text-sm font-semibold bg-emerald-500 text-white shadow-lg shadow-emerald-500/30">
          {detectedFormation}
        </span>
      </div>

      {/* Action bar: menu or message */}
      {!readOnly && mode === 'menu' && selectedId !== null && (
        <div className="flex items-center justify-center gap-2 mb-3">
          <span className="text-sm text-slate-300 mr-1">
            {picks.find(p => p.playerId === selectedId)?.playerName}:
          </span>
          <button
            onClick={() => { setInfoPlayerId(selectedId); reset(); }}
            className="px-3 py-1 rounded-lg text-xs font-semibold bg-purple-500/20 text-purple-400 hover:bg-purple-500/30 transition"
          >
            Info
          </button>
          <button
            onClick={() => { setMode('swap'); setMsg('Select a player to swap with'); }}
            className="px-3 py-1 rounded-lg text-xs font-semibold bg-sky-500/20 text-sky-400 hover:bg-sky-500/30 transition"
          >
            Swap
          </button>
          {isStarter(selectedId) && (
            <>
              <button
                onClick={() => setCaptain(selectedId)}
                className="px-3 py-1 rounded-lg text-xs font-semibold bg-amber-500/20 text-amber-400 hover:bg-amber-500/30 transition"
              >
                Captain
              </button>
              <button
                onClick={() => setViceCaptain(selectedId)}
                className="px-3 py-1 rounded-lg text-xs font-semibold bg-slate-500/20 text-slate-300 hover:bg-slate-500/30 transition"
              >
                Vice Captain
              </button>
            </>
          )}
          <button
            onClick={reset}
            className="px-2 py-1 rounded-lg text-xs text-slate-500 hover:text-white transition"
          >
            Cancel
          </button>
        </div>
      )}

      {msg && (
        <div className={`text-center text-sm mb-3 py-1.5 px-3 rounded-lg ${
          msg.includes('saved') || msg.includes('set') ? 'bg-emerald-500/20 text-emerald-400' :
          msg.includes('Invalid') || msg.includes('Must') || msg.includes('failed') ? 'bg-red-500/20 text-red-400' :
          'bg-amber-500/20 text-amber-400'
        }`}>
          {saving ? 'Saving...' : msg}
        </div>
      )}

      {/* Pitch */}
      <div className="relative rounded-2xl overflow-hidden shadow-2xl">
        <div className="bg-gradient-to-b from-emerald-700 via-emerald-600 to-emerald-700 relative">
          <svg className="absolute inset-0 w-full h-full" viewBox="0 0 400 520" preserveAspectRatio="none">
            <rect x="10" y="10" width="380" height="500" fill="none" stroke="rgba(255,255,255,0.2)" strokeWidth="2" />
            <line x1="10" y1="260" x2="390" y2="260" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <circle cx="200" cy="260" r="50" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <circle cx="200" cy="260" r="3" fill="rgba(255,255,255,0.15)" />
            <rect x="100" y="10" width="200" height="80" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <rect x="150" y="10" width="100" height="30" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <rect x="100" y="430" width="200" height="80" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <rect x="150" y="480" width="100" height="30" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <path d="M10,20 A10,10 0 0,1 20,10" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <path d="M380,10 A10,10 0 0,1 390,20" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <path d="M10,500 A10,10 0 0,0 20,510" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
            <path d="M380,510 A10,10 0 0,0 390,500" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="1.5" />
          </svg>

          <div className="relative z-10 flex flex-col items-center py-6 gap-4 sm:gap-6 min-h-[420px] sm:min-h-[480px]">
            {rows.map((row, i) => (
              <div key={i} className="flex items-start justify-center gap-2 sm:gap-4 w-full px-2">
                {row.players.map(p => (
                  <PlayerCard
                    key={p.playerId}
                    pick={p}
                    selected={selectedId === p.playerId}
                    swapTarget={mode === 'swap' && selectedId !== p.playerId}
                    onClick={() => handleClick(p.playerId)}
                  />
                ))}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Bench */}
      <div className="mt-4 bg-slate-800/60 rounded-xl p-4">
        <p className="text-xs text-slate-500 uppercase tracking-wide mb-3 font-semibold">Substitutes</p>
        <div className="flex items-start justify-center gap-3 sm:gap-6">
          {bench.map(p => (
            <BenchCard
              key={p.playerId}
              pick={p}
              selected={selectedId === p.playerId}
              swapTarget={mode === 'swap' && selectedId !== p.playerId}
              onClick={() => handleClick(p.playerId)}
            />
          ))}
        </div>
      </div>

      {/* Player Info Modal */}
      {infoPlayerId && (
        <PlayerInfoModal
          playerId={infoPlayerId}
          onClose={() => setInfoPlayerId(null)}
        />
      )}
    </div>
  );
}

export type { Formation };
