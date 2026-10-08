import { useState, useEffect, useCallback } from 'react';
import { useAuth } from './useAuth';
import { getMyModuleAccess, createModuleAccessRequest, MODULE_ACCESS_CHANGED_EVENT } from '../services/moduleAccessService';
import type { ModuleAccessStatusDto, ApplicationModule } from '../types/moduleAccess';

export function useModuleAccess() {
  const { isAuthenticated, isAdmin } = useAuth();
  const [accessMap, setAccessMap] = useState<Record<string, ModuleAccessStatusDto>>({});
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAccess = useCallback(async () => {
    if (!isAuthenticated) {
      setAccessMap({});
      setIsLoading(false);
      return;
    }

    try {
      setIsLoading(true);
      setError(null);
      const data = await getMyModuleAccess();
      setAccessMap(data);
    } catch (err: any) {
      setError(err?.response?.data?.detail || err?.message || 'Modül erişim bilgisi alınamadı.');
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    fetchAccess();
    window.addEventListener(MODULE_ACCESS_CHANGED_EVENT, fetchAccess);
    return () => {
      window.removeEventListener(MODULE_ACCESS_CHANGED_EVENT, fetchAccess);
    };
  }, [fetchAccess]);

  const hasModuleAccess = useCallback(
    (moduleKey: 'reports' | 'teams' | string): boolean => {
      if (isAdmin) return true;
      const key = moduleKey.toLowerCase();
      return Boolean(accessMap[key]?.hasAccess);
    },
    [isAdmin, accessMap]
  );

  const isModulePending = useCallback(
    (moduleKey: 'reports' | 'teams' | string): boolean => {
      if (isAdmin) return false;
      const key = moduleKey.toLowerCase();
      return accessMap[key]?.requestStatus === 'Pending';
    },
    [isAdmin, accessMap]
  );

  const requestAccess = useCallback(
    async (module: ApplicationModule, reason?: string) => {
      const result = await createModuleAccessRequest({ module, reason });
      await fetchAccess();
      return result;
    },
    [fetchAccess]
  );

  return {
    accessMap,
    isLoading,
    error,
    hasModuleAccess,
    isModulePending,
    requestAccess,
    refetchAccess: fetchAccess,
  };
}
