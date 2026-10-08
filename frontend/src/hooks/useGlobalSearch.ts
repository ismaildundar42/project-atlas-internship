import { useQuery } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { searchService } from '../services/searchService';
import type { SearchResultResponse } from '../types/search';

export const SEARCH_QUERY_KEY = ['globalSearch'] as const;

/**
 * Belirtilen gecikme süresi boyunca değeri debounce eden hook.
 */
export function useDebounce<T>(value: T, delay: number = 300): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

/**
 * Genel proje aramasını yürüten TanStack Query hook'u.
 * 280ms debounce ile klavye vuruşlarını optimize eder.
 */
export function useGlobalSearch(rawQuery: string, limit: number = 8) {
  const debouncedQuery = useDebounce(rawQuery.trim(), 280);
  const isQueryValid = debouncedQuery.length >= 2;

  const queryResult = useQuery<SearchResultResponse>({
    queryKey: [...SEARCH_QUERY_KEY, debouncedQuery, limit],
    queryFn: ({ signal }) => searchService.searchProjects(debouncedQuery, limit, signal),
    enabled: isQueryValid,
    staleTime: 60 * 1000, // 1 dakika boyunca önbellekte taze kalır
    placeholderData: (previousData) => previousData,
  });

  return {
    ...queryResult,
    debouncedQuery,
    isQueryValid,
    isDebouncing: rawQuery.trim() !== debouncedQuery && rawQuery.trim().length >= 2,
  };
}
