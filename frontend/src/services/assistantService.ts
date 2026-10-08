import apiClient from './apiClient';
import type { ProjectAssistantRequest, ProjectAssistantResponse } from '../types/assistant';

/**
 * Proje Kütüphanesi RAG Asistanı API istemci servisi.
 */
export const assistantService = {
  /**
   * Doğal dildeki soru için yetkilendirilmiş RAG asistan yanıtı üretir.
   */
  async ask(request: ProjectAssistantRequest): Promise<ProjectAssistantResponse> {
    const response = await apiClient.post<ProjectAssistantResponse>('/api/ai/assistant', request, {
      timeout: 120000, // 120 saniye (LLM üretimi için güvenli pay)
    });
    return response.data;
  },
};

export default assistantService;
