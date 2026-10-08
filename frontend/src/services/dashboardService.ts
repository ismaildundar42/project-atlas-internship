import apiClient from './apiClient';
import type { DashboardSummary } from '../types/dashboard';

/**
/// Dashboard API servis metodları.
*/
export const dashboardService = {
  /**
   * Dashboard özet metriklerini getirir.
   */
  async getDashboardSummary(): Promise<DashboardSummary> {
    const response = await apiClient.get<DashboardSummary>('/api/dashboard/summary');
    return response.data;
  },
};
