import { useSearchParams } from 'react-router-dom';

/**
 * Reads the `?leagueId=` query param from the URL.
 * Returns null when absent (= the user's global team).
 */
export function useLeagueId(): number | null {
  const [params] = useSearchParams();
  const raw = params.get('leagueId');
  if (raw == null || raw === '') return null;
  const parsed = parseInt(raw, 10);
  return Number.isFinite(parsed) ? parsed : null;
}

/**
 * Build a query string suffix (`?leagueId=X` or '') for axios calls.
 * Lets us write `api.get(\`/team\${leagueQuery(leagueId)}\`)` safely.
 */
export function leagueQuery(leagueId: number | null): string {
  return leagueId == null ? '' : `?leagueId=${leagueId}`;
}
