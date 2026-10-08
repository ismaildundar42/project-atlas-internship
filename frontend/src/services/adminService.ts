import apiClient from './apiClient';
import type { PagedResult } from '../types/project';
import type {
  AdminDashboard,
  AdminProjectEdit,
  AdminProjectListItem,
  AdminProjectQueryParams,
  CreateProjectRequest,
  UpdateProjectRequest,
  UploadResult,
  CreateTechnologyPayload,
  CreateLocationPayload,
  AuditLog,
  AuditLogQueryParams,
  RejectProjectRequest,
} from '../types/admin';
import type { Technology, Location } from '../types/lookup';
import type { UserProjectEntryItem, UpdateProjectEntryAccessRequest } from '../types/auth';

export const adminService = {
  getDashboard: async (): Promise<AdminDashboard> => {
    const response = await apiClient.get<AdminDashboard>('/api/admin/dashboard');
    return response.data;
  },

  getProjects: async (params: AdminProjectQueryParams = {}): Promise<PagedResult<AdminProjectListItem>> => {
    const response = await apiClient.get<PagedResult<AdminProjectListItem>>('/api/admin/projects', {
      params,
    });
    return response.data;
  },

  getProjectForEdit: async (id: number): Promise<AdminProjectEdit> => {
    const response = await apiClient.get<AdminProjectEdit>(`/api/admin/projects/${id}`);
    return response.data;
  },

  createProject: async (data: CreateProjectRequest): Promise<{ id: number; slug: string }> => {
    const response = await apiClient.post<{ id: number; slug: string }>('/api/admin/projects', data);
    return response.data;
  },

  updateProject: async (id: number, data: UpdateProjectRequest): Promise<void> => {
    await apiClient.put(`/api/admin/projects/${id}`, data);
  },

  deleteProject: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/admin/projects/${id}`);
  },

  restoreProject: async (id: number): Promise<void> => {
    await apiClient.post(`/api/admin/projects/${id}/restore`);
  },

  setProjectPublished: async (id: number, isPublished: boolean): Promise<void> => {
    await apiClient.put(`/api/admin/projects/${id}/publish`, { isPublished });
  },

  // ─── Approval Workflow Methods ──────────────────────────────────────────

  submitProjectForReview: async (id: number): Promise<void> => {
    await apiClient.post(`/api/admin/projects/${id}/submit-for-review`);
  },

  approveProject: async (id: number): Promise<void> => {
    await apiClient.post(`/api/admin/projects/${id}/approve`);
  },

  rejectProject: async (id: number, rejectionReason: string): Promise<void> => {
    const payload: RejectProjectRequest = { reason: rejectionReason, rejectionReason };
    await apiClient.post(`/api/admin/projects/${id}/reject`, payload);
  },

  // ─── Audit Log Methods ──────────────────────────────────────────────────

  getAuditLogs: async (params: AuditLogQueryParams = {}): Promise<PagedResult<AuditLog>> => {
    const response = await apiClient.get<PagedResult<AuditLog>>('/api/admin/audit-logs', { params });
    return response.data;
  },

  getProjectAuditLogs: async (projectId: number): Promise<AuditLog[]> => {
    const response = await apiClient.get<AuditLog[]>(`/api/admin/projects/${projectId}/audit-logs`);
    return response.data;
  },

  uploadDocument: async (projectId: number, file: File): Promise<UploadResult> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<UploadResult>(
      `/api/admin/projects/${projectId}/documents/upload`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return response.data;
  },

  uploadMedia: async (projectId: number, file: File): Promise<UploadResult> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<UploadResult>(
      `/api/admin/projects/${projectId}/media/upload`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return response.data;
  },

  createTechnology: async (payload: CreateTechnologyPayload): Promise<Technology> => {
    const response = await apiClient.post<Technology>('/api/admin/technologies', payload);
    return response.data;
  },

  createLocation: async (payload: CreateLocationPayload): Promise<Location> => {
    const response = await apiClient.post<Location>('/api/admin/locations', payload);
    return response.data;
  },

  // ─── Proje Giriş Yetkisi Yönetimi (Admin Only) ───────────────────────────

  getUsersForProjectEntry: async (): Promise<UserProjectEntryItem[]> => {
    const response = await apiClient.get<UserProjectEntryItem[]>('/api/admin/users');
    return response.data;
  },

  updateProjectEntryAccess: async (userId: number, request: UpdateProjectEntryAccessRequest): Promise<void> => {
    await apiClient.put(`/api/admin/users/${userId}/project-entry-access`, request);
  },
};
