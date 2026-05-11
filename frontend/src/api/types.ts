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

export const LeagueType = {
  Global: 0,
  Classic: 1,
  Draft: 2,
} as const;
export type LeagueType = (typeof LeagueType)[keyof typeof LeagueType];

export const DraftStatus = {
  Pending: 0,
  InProgress: 1,
  Completed: 2,
} as const;
export type DraftStatus = (typeof DraftStatus)[keyof typeof DraftStatus];

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
  leagueId?: number | null;
  leagueName?: string | null;
  leagueType?: LeagueType | null;
}

/** Summary entry used by the team-switcher dropdown. */
export interface MyTeamSummary {
  teamId: number;
  teamName: string;
  leagueId: number | null;
  leagueName: string;
  leagueContext: LeagueType;
  totalPoints: number;
  gameweekPoints: number;
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
  type: LeagueType;
  memberCount: number;
  maxMembers: number;
  draftStatus: DraftStatus;
  hasMyTeam: boolean;
  isCreator: boolean;
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

// ---- Draft mode ----

export interface DraftMember {
  userId: string;
  displayName: string;
  orderIndex: number;
  picksMade: number;
}

export interface DraftPickEntry {
  pickNumber: number;
  round: number;
  userId: string;
  displayName: string;
  playerId: number;
  playerName: string;
  playerTeam: string;
  position: PlayerPosition;
  wasAutoPick: boolean;
  pickedAt: string;
}

export interface DraftState {
  leagueId: number;
  leagueName: string;
  status: DraftStatus;
  maxMembers: number;
  totalPicks: number;
  currentPickNumber: number;
  currentRound: number;
  currentPickerUserId: string | null;
  currentPickerName: string | null;
  currentPickDeadline: string | null;
  pickSeconds: number;
  myTurn: boolean;
  isCreator: boolean;
  members: DraftMember[];
  picks: DraftPickEntry[];
}

export interface AvailablePlayer {
  id: number;
  name: string;
  team: string;
  position: PlayerPosition;
  price: number;
  totalPoints: number;
}

export interface PickBroadcast {
  leagueId: number;
  pick: DraftPickEntry;
  nextPickNumber: number;
  nextPickerUserId: string | null;
  nextPickDeadline: string | null;
  status: DraftStatus;
}

// ---- Waivers ----

export const WaiverPhase = {
  Queue: 0,
  FreeAgency: 1,
  Locked: 2,
  NoDraftYet: 3,
} as const;
export type WaiverPhase = (typeof WaiverPhase)[keyof typeof WaiverPhase];

export const WaiverClaimStatus = {
  Pending: 0,
  Succeeded: 1,
  Failed: 2,
} as const;
export type WaiverClaimStatus = (typeof WaiverClaimStatus)[keyof typeof WaiverClaimStatus];

export interface WaiverClaim {
  id: number;
  playerOutId: number;
  playerOutName: string;
  playerOutTeam: string;
  playerOutPosition: PlayerPosition;
  playerInId: number;
  playerInName: string;
  playerInTeam: string;
  playerInPosition: PlayerPosition;
  priority: number;
  status: WaiverClaimStatus;
  failureReason: string | null;
  createdAt: string;
  processedAt: string | null;
}

export interface WaiverState {
  leagueId: number;
  leagueName: string;
  phase: WaiverPhase;
  upcomingGameweekNumber: number | null;
  upcomingDeadline: string | null;
  processAt: string | null;
  isLocked: boolean;
  myClaims: WaiverClaim[];
  recentResolved: WaiverClaim[];
}

export interface FreeAgent {
  id: number;
  name: string;
  team: string;
  position: PlayerPosition;
  price: number;
  totalPoints: number;
  isLocked: boolean;
  droppedByName: string | null;
}
