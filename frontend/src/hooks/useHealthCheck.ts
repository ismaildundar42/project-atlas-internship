import { useQuery } from '@tanstack/react-query';
import apiClient from '../services/apiClient';

interface HealthStatus {
  status: string;
  timestamp: string;
  version: string;
}

/**
 * Backend API sağlık durumunu sorgulayan custom hook.
 *
 * TanStack Query kullanılarak:
 * - Otomatik cache yönetimi
 * - Loading/error state yönetimi
 * - Retry mantığı sağlanır.
 *
 * Bu hook yalnızca development amaçlıdır; production'da
 * health check'i ayrı bir monitoring sistemine taşımak daha uygundur.
 */
export function useHealthCheck() {
  return useQuery<HealthStatus>({
    queryKey: ['health'],
    queryFn: async () => {
      const response = await apiClient.get<HealthStatus>('/api/health');
      return response.data;
    },
    // Health check her 60 saniyede bir yenilenir
    refetchInterval: 60 * 1000,
    // Bu sorgunun "stale" sayılması için 30 saniye yeterli
    staleTime: 30 * 1000,
  });
}
