import apiClient from './apiClient';
import type { InAppNotification, UnreadCountResponse } from '../types/notification';

export const notificationService = {
  /**
   * Oturum açmış kullanıcının son bildirimlerini getirir.
   */
  getNotifications: async (take: number = 20): Promise<InAppNotification[]> => {
    const response = await apiClient.get<InAppNotification[]>('/api/notifications', {
      params: { take },
    });
    return response.data;
  },

  /**
   * Okunmamış bildirim sayısını getirir.
   */
  getUnreadCount: async (): Promise<number> => {
    const response = await apiClient.get<UnreadCountResponse>('/api/notifications/unread-count');
    return response.data.unreadCount;
  },

  /**
   * Bildirimi okundu olarak işaretler.
   */
  markAsRead: async (id: number): Promise<void> => {
    await apiClient.post(`/api/notifications/${id}/read`);
  },

  /**
   * Tüm bildirimleri okundu olarak işaretler.
   */
  markAllAsRead: async (): Promise<void> => {
    await apiClient.post('/api/notifications/read-all');
  },

  /**
   * Belirli bir bildirimi siler.
   */
  deleteNotification: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/notifications/${id}`);
  },

  /**
   * Tüm okunmuş bildirimleri siler.
   */
  clearReadNotifications: async (): Promise<{ deletedCount: number; message: string }> => {
    const response = await apiClient.delete<{ deletedCount: number; message: string }>('/api/notifications/read');
    return response.data;
  },
};
