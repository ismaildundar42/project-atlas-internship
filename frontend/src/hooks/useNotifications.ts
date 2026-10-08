import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationService } from '../services/notificationService';
import { useAuth } from './useAuth';
import type { InAppNotification } from '../types/notification';

export const NOTIFICATION_QUERY_KEYS = {
  all: ['notifications'] as const,
  list: () => [...NOTIFICATION_QUERY_KEYS.all, 'list'] as const,
  unreadCount: () => [...NOTIFICATION_QUERY_KEYS.all, 'unread-count'] as const,
};

export function useNotifications(take: number = 20) {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: [...NOTIFICATION_QUERY_KEYS.list(), take],
    queryFn: () => notificationService.getNotifications(take),
    enabled: isAuthenticated,
    staleTime: 15 * 1000,
    refetchInterval: 30 * 1000, // 30 sn periyodik sorgulama
    refetchOnWindowFocus: true,
  });
}

export function useUnreadNotificationCount() {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: NOTIFICATION_QUERY_KEYS.unreadCount(),
    queryFn: () => notificationService.getUnreadCount(),
    enabled: isAuthenticated,
    staleTime: 15 * 1000,
    refetchInterval: 30 * 1000,
    refetchOnWindowFocus: true,
  });
}

export function useMarkNotificationAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => notificationService.markAsRead(id),
    onMutate: async (id: number) => {
      await queryClient.cancelQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });

      queryClient.setQueriesData<number>(
        { queryKey: NOTIFICATION_QUERY_KEYS.unreadCount() },
        (old) => (typeof old === 'number' && old > 0 ? old - 1 : 0)
      );

      queryClient.setQueriesData<InAppNotification[]>(
        { queryKey: NOTIFICATION_QUERY_KEYS.all },
        (old) => {
          if (!Array.isArray(old)) return old;
          return old.map((n) =>
            n.id === id ? { ...n, isRead: true, readAtUtc: new Date().toISOString() } : n
          );
        }
      );
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });
    },
  });
}

export function useMarkAllNotificationsAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => notificationService.markAllAsRead(),
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });

      queryClient.setQueriesData<number>(
        { queryKey: NOTIFICATION_QUERY_KEYS.unreadCount() },
        () => 0
      );

      queryClient.setQueriesData<InAppNotification[]>(
        { queryKey: NOTIFICATION_QUERY_KEYS.all },
        (old) => {
          if (!Array.isArray(old)) return old;
          return old.map((n) => ({ ...n, isRead: true, readAtUtc: new Date().toISOString() }));
        }
      );
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });
    },
  });
}

export function useDeleteNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => notificationService.deleteNotification(id),
    onMutate: async (id: number) => {
      await queryClient.cancelQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });

      // Find if deleted notification was unread
      let wasUnread = false;
      queryClient.setQueriesData<InAppNotification[]>(
        { queryKey: NOTIFICATION_QUERY_KEYS.all },
        (old) => {
          if (!Array.isArray(old)) return old;
          const target = old.find((n) => n.id === id);
          if (target && !target.isRead) {
            wasUnread = true;
          }
          return old.filter((n) => n.id !== id);
        }
      );

      if (wasUnread) {
        queryClient.setQueriesData<number>(
          { queryKey: NOTIFICATION_QUERY_KEYS.unreadCount() },
          (old) => (typeof old === 'number' && old > 0 ? old - 1 : 0)
        );
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });
    },
  });
}

export function useClearReadNotifications() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => notificationService.clearReadNotifications(),
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });

      queryClient.setQueriesData<InAppNotification[]>(
        { queryKey: NOTIFICATION_QUERY_KEYS.all },
        (old) => {
          if (!Array.isArray(old)) return old;
          return old.filter((n) => !n.isRead);
        }
      );
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATION_QUERY_KEYS.all });
    },
  });
}
