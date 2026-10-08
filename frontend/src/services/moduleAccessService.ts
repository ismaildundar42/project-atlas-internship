import apiClient from './apiClient';
import type {
  ModuleAccessStatusDto,
  CreateModuleAccessRequestDto,
  ModuleAccessRequestDto,
  AccessRequestQueryParams,
  ReviewModuleAccessRequestDto,
  ApplicationModule,
} from '../types/moduleAccess';
import type { PagedResult } from '../types/project';

export const MODULE_ACCESS_CHANGED_EVENT = 'de-module-access-changed';

export function notifyModuleAccessChanged() {
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent(MODULE_ACCESS_CHANGED_EVENT));
  }
}

export async function getMyModuleAccess(): Promise<Record<string, ModuleAccessStatusDto>> {
  const response = await apiClient.get<Record<string, ModuleAccessStatusDto>>('/api/module-access/my-access');
  return response.data;
}

export async function createModuleAccessRequest(
  data: CreateModuleAccessRequestDto
): Promise<ModuleAccessRequestDto> {
  const response = await apiClient.post<ModuleAccessRequestDto>('/api/module-access/requests', data);
  notifyModuleAccessChanged();
  return response.data;
}

export async function getAdminAccessRequests(
  params?: AccessRequestQueryParams
): Promise<PagedResult<ModuleAccessRequestDto>> {
  const response = await apiClient.get<PagedResult<ModuleAccessRequestDto>>('/api/admin/module-access/requests', {
    params,
  });
  return response.data;
}

export async function getAdminPendingCount(): Promise<number> {
  const response = await apiClient.get<{ count: number }>('/api/admin/module-access/pending-count');
  return response.data.count;
}

export async function approveAccessRequest(
  id: number,
  data?: ReviewModuleAccessRequestDto
): Promise<ModuleAccessRequestDto> {
  const response = await apiClient.post<ModuleAccessRequestDto>(`/api/admin/module-access/requests/${id}/approve`, data || {});
  notifyModuleAccessChanged();
  return response.data;
}

export async function rejectAccessRequest(
  id: number,
  data?: ReviewModuleAccessRequestDto
): Promise<ModuleAccessRequestDto> {
  const response = await apiClient.post<ModuleAccessRequestDto>(`/api/admin/module-access/requests/${id}/reject`, data || {});
  notifyModuleAccessChanged();
  return response.data;
}

export async function grantDirectModuleAccess(
  userId: number,
  module: ApplicationModule | string
): Promise<void> {
  await apiClient.post(`/api/admin/module-access/users/${userId}/grant`, { module });
  notifyModuleAccessChanged();
}

export async function revokeModuleAccess(
  userId: number,
  module: ApplicationModule | string
): Promise<void> {
  await apiClient.delete(`/api/admin/module-access/users/${userId}/revoke/${module}`);
  notifyModuleAccessChanged();
}
