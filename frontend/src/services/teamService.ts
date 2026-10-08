import apiClient from './apiClient';
import type { TeamDetail, TeamListItem, TeamsSummary } from '../types/team';

export const teamService = {
  getSummary: async (): Promise<TeamsSummary> => {
    const response = await apiClient.get<TeamsSummary>('/api/teams/summary');
    return response.data;
  },

  getTeams: async (): Promise<TeamListItem[]> => {
    const response = await apiClient.get<TeamListItem[]>('/api/teams');
    return response.data;
  },

  getTeamDetail: async (id: number): Promise<TeamDetail> => {
    const response = await apiClient.get<TeamDetail>(`/api/teams/${id}`);
    return response.data;
  },
};
