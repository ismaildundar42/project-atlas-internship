import apiClient from './apiClient';
import type { AuthUser, CaptchaChallenge, LoginCredentials } from '../types/auth';

export const authService = {
  getCaptcha: async (): Promise<CaptchaChallenge> => {
    const response = await apiClient.get<CaptchaChallenge>('/api/auth/captcha');
    return response.data;
  },

  login: async (credentials: LoginCredentials): Promise<AuthUser> => {
    const response = await apiClient.post<AuthUser>('/api/auth/login', credentials);
    return response.data;
  },

  logout: async (): Promise<void> => {
    await apiClient.post('/api/auth/logout');
  },

  getCurrentUser: async (): Promise<AuthUser> => {
    const response = await apiClient.get<AuthUser>('/api/auth/me');
    return response.data;
  },
};
