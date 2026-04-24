// Team configuration: colors, logo filename, jersey filename, short name
export const TEAM_CONFIG: Record<string, {
  short: string;
  primary: string;
  secondary: string;
  accent?: string;
  logo: string;
  jersey: string;
}> = {
  'Bayern Munich': { short: 'BAY', primary: '#DC052D', secondary: '#0066B2', logo: 'bayern-munich.png', jersey: 'bayern-munich' },
  'Borussia Dortmund': { short: 'BVB', primary: '#FDE100', secondary: '#000000', logo: 'borussia-dortmund.png', jersey: 'borussia-dortmund' },
  'RB Leipzig': { short: 'RBL', primary: '#FFFFFF', secondary: '#DD0741', accent: '#001F47', logo: 'rb-leipzig.png', jersey: 'rb-leipzig' },
  'Bayer Leverkusen': { short: 'B04', primary: '#E32221', secondary: '#000000', logo: 'bayer-leverkusen.png', jersey: 'bayer-leverkusen' },
  'VfB Stuttgart': { short: 'VFB', primary: '#FFFFFF', secondary: '#E32219', accent: '#FFD700', logo: 'vfb-stuttgart.png', jersey: 'vfb-stuttgart' },
  'Eintracht Frankfurt': { short: 'SGE', primary: '#000000', secondary: '#FFFFFF', accent: '#E1000F', logo: 'eintracht-frankfurt.png', jersey: 'eintracht-frankfurt' },
  'SC Freiburg': { short: 'SCF', primary: '#000000', secondary: '#FFFFFF', accent: '#E2001A', logo: 'sc-freiburg.png', jersey: 'sc-freiburg' },
  'VfL Wolfsburg': { short: 'WOB', primary: '#65B32E', secondary: '#FFFFFF', logo: 'vfl-wolfsburg.png', jersey: 'vfl-wolfsburg' },
  'TSG Hoffenheim': { short: 'TSG', primary: '#1961B5', secondary: '#FFFFFF', logo: 'tsg-hoffenheim.png', jersey: 'tsg-hoffenheim' },
  'Werder Bremen': { short: 'SVW', primary: '#1D6C37', secondary: '#FFFFFF', logo: 'werder-bremen.png', jersey: 'werder-bremen' },
  'FC Augsburg': { short: 'FCA', primary: '#BA2632', secondary: '#FFFFFF', accent: '#006B3F', logo: 'fc-augsburg.png', jersey: 'fc-augsburg' },
  '1. FC Union Berlin': { short: 'FCU', primary: '#EB1923', secondary: '#FFFFFF', accent: '#FEE600', logo: 'union-berlin.png', jersey: 'union-berlin' },
  'Borussia Mönchengladbach': { short: 'BMG', primary: '#000000', secondary: '#FFFFFF', accent: '#1FA149', logo: 'gladbach.png', jersey: 'gladbach' },
  '1. FSV Mainz 05': { short: 'M05', primary: '#C3002F', secondary: '#FFFFFF', logo: 'mainz-05.png', jersey: 'mainz-05' },
  'Hamburger SV': { short: 'HSV', primary: '#005B9E', secondary: '#FFFFFF', accent: '#000000', logo: 'hamburger-sv.svg', jersey: 'hamburger-sv' },
  'FC St. Pauli': { short: 'STP', primary: '#6E4C2A', secondary: '#FFFFFF', logo: 'fc-st-pauli.png', jersey: 'fc-st-pauli' },
  '1. FC Köln': { short: 'KOE', primary: '#FFFFFF', secondary: '#ED1C24', accent: '#000000', logo: '1-fc-koeln.svg', jersey: '1-fc-koeln' },
  '1. FC Heidenheim': { short: 'FCH', primary: '#E30613', secondary: '#003E7E', accent: '#FFFFFF', logo: 'fc-heidenheim.png', jersey: 'fc-heidenheim' },
};

export function getTeamDisplayName(team: string): string {
  return team.replace(/^1\.\s*/, '');
}

export function getTeamLogo(team: string): string {
  const config = TEAM_CONFIG[team];
  return config ? `/teams/${config.logo}` : '/favicon.svg';
}

export function getTeamShort(team: string): string {
  return TEAM_CONFIG[team]?.short ?? team.slice(0, 3).toUpperCase();
}

export function getTeamColors(team: string) {
  return TEAM_CONFIG[team] ?? { primary: '#666', secondary: '#fff' };
}

// Get jersey image path (real jersey PNGs)
export function getJerseySvg(team: string, isGk = false): string {
  const config = TEAM_CONFIG[team];
  if (config) {
    const suffix = isGk ? '-gk' : '';
    return `/jerseys/${config.jersey}${suffix}.png`;
  }
  // Fallback: generate a simple SVG
  const colors = getTeamColors(team);
  const body = isGk ? '#333333' : colors.primary;
  const sleeves = isGk ? '#555555' : (colors.accent ?? colors.secondary);
  const collar = colors.secondary;

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 90">
    <path d="M5,15 L20,10 L20,35 L5,35 Z" fill="${sleeves}" stroke="${collar}" stroke-width="1"/>
    <path d="M75,15 L60,10 L60,35 L75,35 Z" fill="${sleeves}" stroke="${collar}" stroke-width="1"/>
    <path d="M20,10 L35,5 L45,5 L60,10 L60,85 L20,85 Z" fill="${body}" stroke="${collar}" stroke-width="1"/>
    <path d="M35,5 Q40,10 45,5" fill="none" stroke="${collar}" stroke-width="2"/>
  </svg>`;

  return `data:image/svg+xml,${encodeURIComponent(svg)}`;
}

// Fitness color helper
export function getFitnessColor(fitness: number): string {
  if (fitness <= 0) return '#EF4444';   // red - injured
  if (fitness <= 25) return '#EF4444';  // red - injured
  if (fitness <= 50) return '#F97316';  // orange - doubtful
  if (fitness <= 75) return '#EAB308';  // yellow - questionable
  return '#22C55E';                      // green - fit
}

export function getFitnessLabel(fitness: number): string {
  if (fitness <= 0) return 'Injured';
  if (fitness <= 25) return 'Injured';
  if (fitness <= 50) return 'Doubtful';
  if (fitness <= 75) return 'Minor Knock';
  return 'Fit';
}
