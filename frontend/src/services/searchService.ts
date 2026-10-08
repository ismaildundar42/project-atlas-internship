import apiClient from './apiClient';
import type { SearchResultResponse } from '../types/search';

/**
 * Global Search API Service
 */
export const searchService = {
  /**
   * Projeler üzerinde çok boyutlu genel arama sorgusu yürütür.
   * @param query Arama metni
   * @param limit Döndürülecek azami proje sayısı
   * @param signal AbortController iptal sinyali
   */
  async searchProjects(query: string, limit: number = 8, signal?: AbortSignal): Promise<SearchResultResponse> {
    const trimmed = query.trim();
    if (!trimmed || trimmed.length < 2) {
      return { query: trimmed, totalCount: 0, items: [] };
    }

    const response = await apiClient.get<SearchResultResponse>('/api/search', {
      params: { q: trimmed, limit },
      signal,
    });
    return response.data;
  },
};
