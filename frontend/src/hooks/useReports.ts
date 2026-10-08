import { useQuery } from '@tanstack/react-query';
import { reportService } from '../services/reportService';

export const REPORTS_OVERVIEW_QUERY_KEY = ['reports', 'overview'] as const;

/**
 * Portföy raporlama ve içgörü verilerini çekmek ve önbelleğe almak için TanStack Query hook'u.
 */
export function useReportOverview() {
  return useQuery({
    queryKey: REPORTS_OVERVIEW_QUERY_KEY,
    queryFn: () => reportService.getReportOverview(),
    staleTime: 5 * 60 * 1000, // 5 dakika önbellek
  });
}
