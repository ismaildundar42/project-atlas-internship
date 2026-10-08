import apiClient from './apiClient';
import type { ProjectAiSummaryRequest, ProjectAiSummaryResponse } from '../types/aiSummary';

/**
/// AI Proje Özeti API Servisi
/// Yetkilendirilmiş proje detay sayfasında doğrudan ve sınırlı SQL bağlamından AI özeti talep eder.
*/
export const aiSummaryService = {
  getProjectSummary: async (
    projectId: number,
    request?: ProjectAiSummaryRequest,
    signal?: AbortSignal
  ): Promise<ProjectAiSummaryResponse> => {
    const response = await apiClient.post<ProjectAiSummaryResponse>(
      `/api/projects/${projectId}/ai-summary`,
      request ?? {},
      { signal, timeout: 120000 }
    );
    return response.data;
  },
};

export default aiSummaryService;
