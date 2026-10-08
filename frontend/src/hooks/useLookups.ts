import { useQuery } from '@tanstack/react-query';
import { lookupService } from '../services/lookupService';

export const LOOKUP_QUERY_KEYS = {
  statuses: ['lookups', 'statuses'] as const,
  categories: ['lookups', 'categories'] as const,
  technologies: ['lookups', 'technologies'] as const,
  locations: ['lookups', 'locations'] as const,
  teams: ['lookups', 'teams'] as const,
  members: ['lookups', 'members'] as const,
  tags: ['lookups', 'tags'] as const,
};

const STALE_TIME_LOOKUPS = 30 * 60 * 1000; // 30 dakika stale kalır (referans veriler seyrek değişir)

export function useProjectStatuses() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.statuses,
    queryFn: () => lookupService.getProjectStatuses(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}

export function useProjectCategories() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.categories,
    queryFn: () => lookupService.getProjectCategories(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}

export function useTechnologies() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.technologies,
    queryFn: () => lookupService.getTechnologies(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}

export function useLocations() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.locations,
    queryFn: () => lookupService.getLocations(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}

export function useTeams() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.teams,
    queryFn: () => lookupService.getTeams(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}

export function useMembers() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.members,
    queryFn: () => lookupService.getMembers(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}

export function useTags() {
  return useQuery({
    queryKey: LOOKUP_QUERY_KEYS.tags,
    queryFn: () => lookupService.getTags(),
    staleTime: STALE_TIME_LOOKUPS,
  });
}
