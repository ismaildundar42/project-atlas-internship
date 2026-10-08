import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { projectService } from '../services/projectService';
import type { ProjectQueryParams } from '../types/project';

export const PROJECT_QUERY_KEYS = {
  list: (params: ProjectQueryParams) => ['projects', 'list', params] as const,
  bySlug: (slug: string) => ['projects', 'detail', 'slug', slug] as const,
  byId: (id: number) => ['projects', 'detail', 'id', id] as const,
};

export function useProjects(params: ProjectQueryParams) {
  return useQuery({
    queryKey: PROJECT_QUERY_KEYS.list(params),
    queryFn: () => projectService.getProjects(params),
    placeholderData: keepPreviousData,
    staleTime: 60 * 1000, // 1 dakika
  });
}

export function useProjectBySlug(slug: string, enabled = true) {
  return useQuery({
    queryKey: PROJECT_QUERY_KEYS.bySlug(slug),
    queryFn: () => projectService.getProjectBySlug(slug),
    enabled: Boolean(slug) && enabled,
    staleTime: 5 * 60 * 1000,
  });
}
