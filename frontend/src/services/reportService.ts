import apiClient from './apiClient';
import type { ReportOverview } from '../types/report';

/**
 * Raporlama ve İçgörüler API servis metodları.
 */
export const reportService = {
  /**
   * Portföy geneli için raporlama özet ve dağılım verilerini getirir.
   */
  async getReportOverview(): Promise<ReportOverview> {
    const response = await apiClient.get<ReportOverview>('/api/reports/overview');
    return response.data;
  },
};
