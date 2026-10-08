import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';

export const DASHBOARD_QUERY_KEY = ['dashboard', 'summary'] as const;

/**
 * Dashboard özet verilerini çekmek ve önbelleğe almak için TanStack Query hook'u.
 */
export function useDashboardSummary() {
  return useQuery({
    queryKey: DASHBOARD_QUERY_KEY,
    queryFn: () => dashboardService.getDashboardSummary(),
    staleTime: 5 * 60 * 1000, // 5 dakika stale kalır
  });
}
