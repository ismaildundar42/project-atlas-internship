import { useQuery } from '@tanstack/react-query';
import { teamService } from '../services/teamService';

export const PUBLIC_TEAM_QUERY_KEYS = {
  summary: () => ['teams', 'summary'] as const,
  list: () => ['teams', 'list'] as const,
  detail: (id: number) => ['teams', 'detail', id] as const,
};

export function useTeamsSummary() {
  return useQuery({
    queryKey: PUBLIC_TEAM_QUERY_KEYS.summary(),
    queryFn: () => teamService.getSummary(),
    staleTime: 60 * 1000,
  });
}

export function usePublicTeams() {
  return useQuery({
    queryKey: PUBLIC_TEAM_QUERY_KEYS.list(),
    queryFn: () => teamService.getTeams(),
    staleTime: 60 * 1000,
  });
}

export function useTeamDetail(id: number) {
  return useQuery({
    queryKey: PUBLIC_TEAM_QUERY_KEYS.detail(id),
    queryFn: () => teamService.getTeamDetail(id),
    enabled: Boolean(id) && !isNaN(id),
    staleTime: 60 * 1000,
  });
}
