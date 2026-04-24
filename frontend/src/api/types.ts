export const PlayerPosition = {
  GK: 1,
  DEF: 2,
  MID: 3,
  FWD: 4,
} as const;
export type PlayerPosition = (typeof PlayerPosition)[keyof typeof PlayerPosition];

export const GameweekStatus = {
  Upcoming: 0,
  Live: 1,
  Finished: 2,
} as const;
export type GameweekStatus = (typeof GameweekStatus)[keyof typeof GameweekStatus];

export const ChipType = {
  Wildcard: 0,
  BenchBoost: 1,
  TripleCaptain: 2,
  FreeHit: 3,
} as const;
export type ChipType = (typeof ChipType)[keyof typeof ChipType];

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  userId: string;
  displayName: string;
  email: string;
  roles: string[];
}

export interface PlayerStats {
  matchesPlayed: number;
  goals: number;
  assists: number;
  cleanSheets: number;
  yellowCards: number;
  redCards: number;
  penaltiesMissed: number;
  penaltySaves: number;
  ownGoals: number;
  bonusPoints: number;
  expectedGoals: number;
  expectedAssists: number;
}

export interface GameweekStats {
  minutesPlayed: number;
  goals: number;
  assists: number;
  cleanSheets: number;
  yellowCards: number;
  redCards: number;
  penaltiesMissed: number;
  penaltySaves: number;
  ownGoals: number;
  bonusPoints: number;
}

export interface GameweekPoints {
  gameweekNumber: number;
  points: number;
  stats: GameweekStats;
  events: string[];
}

export interface Player {
  id: number;
  name: string;
  team: string;
  position: PlayerPosition;
  price: number;
  totalPoints: number;
  photoUrl?: string;
  fitness: number;
  stats: PlayerStats;
}

export interface PlayerDetail extends Player {
  gameweekHistory: GameweekPoints[];
  ownershipPercent: number;
}

export interface Pick {
  playerId: number;
  playerName: string;
  team: string;
  position: PlayerPosition;
  price: number;
  squadPosition: number;
  isCaptain: boolean;
  isViceCaptain: boolean;
  gameweekPoints: number;
}

export interface Team {
  id: number;
  name: string;
  budget: number;
  freeTransfers: number;
  totalPoints: number;
  gameweekPoints: number;
  picks: Pick[];
  activeChip?: ChipType;
}

export interface Gameweek {
  id: number;
  number: number;
  kickoffTime: string;
  deadline: string;
  status: GameweekStatus;
  isLocked: boolean;
}

export interface League {
  id: number;
  name: string;
  joinCode: string;
  isGlobal: boolean;
  standings: LeagueStanding[];
}

export interface LeagueStanding {
  rank: number;
  userId: string;
  displayName: string;
  teamName: string;
  totalPoints: number;
  gameweekPoints: number;
}

export interface TransferResult {
  success: boolean;
  message: string;
  remainingBudget: number;
  freeTransfersLeft: number;
}

export interface GameweekHistory {
  gameweekNumber: number;
  points: number;
  cumulativePoints: number;
  overallRank: number;
}

export interface ChipsAvailable {
  wildcard: boolean;
  benchBoost: boolean;
  tripleCaptain: boolean;
  freeHit: boolean;
}
