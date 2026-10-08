import apiClient from './apiClient';
import type { UserProfileResponse, ChangePasswordRequest } from '../types/profile';

export const profileService = {
  getProfile: async (): Promise<UserProfileResponse> => {
    const response = await apiClient.get<UserProfileResponse>('/api/profile/me');
    return response.data;
  },

  changePassword: async (request: ChangePasswordRequest): Promise<{ message: string }> => {
    const response = await apiClient.post<{ message: string }>('/api/profile/change-password', request);
    return response.data;
  }
};
