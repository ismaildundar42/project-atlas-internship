import apiClient from './apiClient';
import type { Location, Member, ProjectCategory, ProjectStatus, Tag, Team, Technology } from '../types/lookup';

export const lookupService = {
  getProjectStatuses: async (): Promise<ProjectStatus[]> => {
    const response = await apiClient.get<ProjectStatus[]>('/api/project-statuses');
    return response.data;
  },

  getProjectCategories: async (): Promise<ProjectCategory[]> => {
    const response = await apiClient.get<ProjectCategory[]>('/api/project-categories');
    return response.data;
  },

  getTechnologies: async (): Promise<Technology[]> => {
    const response = await apiClient.get<Technology[]>('/api/technologies');
    return response.data;
  },

  getLocations: async (): Promise<Location[]> => {
    const response = await apiClient.get<Location[]>('/api/locations');
    return response.data;
  },

  getTeams: async (): Promise<Team[]> => {
    const response = await apiClient.get<Team[]>('/api/lookups/teams');
    return response.data;
  },

  getMembers: async (): Promise<Member[]> => {
    const response = await apiClient.get<Member[]>('/api/members');
    return response.data;
  },

  getTags: async (): Promise<Tag[]> => {
    const response = await apiClient.get<Tag[]>('/api/tags');
    return response.data;
  },
};
