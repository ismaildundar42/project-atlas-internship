import apiClient from './apiClient';
import type { PagedResult, ProjectDetail, ProjectListItem, ProjectQueryParams } from '../types/project';

export const projectService = {
  getProjects: async (params: ProjectQueryParams = {}): Promise<PagedResult<ProjectListItem>> => {
    const response = await apiClient.get<PagedResult<ProjectListItem>>('/api/projects', {
      params,
    });
    return response.data;
  },

  getProjectById: async (id: number): Promise<ProjectDetail> => {
    const response = await apiClient.get<ProjectDetail>(`/api/projects/${id}`);
    return response.data;
  },

  getProjectBySlug: async (slug: string): Promise<ProjectDetail> => {
    const response = await apiClient.get<ProjectDetail>(`/api/projects/slug/${encodeURIComponent(slug)}`);
    return response.data;
  },
};
