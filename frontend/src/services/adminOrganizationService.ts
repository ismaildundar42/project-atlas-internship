import apiClient from './apiClient';
import type {
  DepartmentAdmin,
  TeamAdmin,
  MemberAdmin,
  CreateDepartmentPayload,
  UpdateDepartmentPayload,
  CreateTeamPayload,
  UpdateTeamPayload,
  CreateMemberPayload,
  UpdateMemberPayload,
} from '../types/organization';

export const adminOrganizationService = {
  // Departments
  getDepartments: async (): Promise<DepartmentAdmin[]> => {
    const response = await apiClient.get<DepartmentAdmin[]>('/api/admin/organization/departments');
    return response.data;
  },

  createDepartment: async (payload: CreateDepartmentPayload): Promise<DepartmentAdmin> => {
    const response = await apiClient.post<DepartmentAdmin>('/api/admin/organization/departments', payload);
    return response.data;
  },

  updateDepartment: async (id: number, payload: UpdateDepartmentPayload): Promise<DepartmentAdmin> => {
    const response = await apiClient.put<DepartmentAdmin>(`/api/admin/organization/departments/${id}`, payload);
    return response.data;
  },

  deleteDepartment: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/admin/organization/departments/${id}`);
  },

  // Teams
  getTeams: async (): Promise<TeamAdmin[]> => {
    const response = await apiClient.get<TeamAdmin[]>('/api/admin/organization/teams');
    return response.data;
  },

  createTeam: async (payload: CreateTeamPayload): Promise<TeamAdmin> => {
    const response = await apiClient.post<TeamAdmin>('/api/admin/organization/teams', payload);
    return response.data;
  },

  updateTeam: async (id: number, payload: UpdateTeamPayload): Promise<TeamAdmin> => {
    const response = await apiClient.put<TeamAdmin>(`/api/admin/organization/teams/${id}`, payload);
    return response.data;
  },

  deleteTeam: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/admin/organization/teams/${id}`);
  },

  // Members
  getMembers: async (): Promise<MemberAdmin[]> => {
    const response = await apiClient.get<MemberAdmin[]>('/api/admin/organization/members');
    return response.data;
  },

  createMember: async (payload: CreateMemberPayload): Promise<MemberAdmin> => {
    const response = await apiClient.post<MemberAdmin>('/api/admin/organization/members', payload);
    return response.data;
  },

  updateMember: async (id: number, payload: UpdateMemberPayload): Promise<MemberAdmin> => {
    const response = await apiClient.put<MemberAdmin>(`/api/admin/organization/members/${id}`, payload);
    return response.data;
  },

  deleteMember: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/admin/organization/members/${id}`);
  },

  // Member Identity & Project Access (Phase 13.2)
  updateMemberProjectAccess: async (
    memberId: number,
    payload: { canCreateProjects: boolean }
  ): Promise<MemberAdmin> => {
    const response = await apiClient.put<MemberAdmin>(
      `/api/admin/organization/members/${memberId}/project-access`,
      payload
    );
    return response.data;
  },

  createMemberAccount: async (
    memberId: number,
    payload: { password: string; confirmPassword: string }
  ): Promise<MemberAdmin> => {
    const response = await apiClient.post<MemberAdmin>(
      `/api/admin/organization/members/${memberId}/account`,
      payload
    );
    return response.data;
  },

  linkMemberAccount: async (
    memberId: number,
    payload?: { userId?: number }
  ): Promise<MemberAdmin> => {
    const response = await apiClient.post<MemberAdmin>(
      `/api/admin/organization/members/${memberId}/link-account`,
      payload || {}
    );
    return response.data;
  },

  // Member Admin Role Assignment (Phase 19.7 - SuperAdmin Only)
  updateMemberAdminRole: async (
    memberId: number,
    payload: { isAdmin: boolean }
  ): Promise<MemberAdmin> => {
    const response = await apiClient.put<MemberAdmin>(
      `/api/admin/organization/members/${memberId}/admin-role`,
      payload
    );
    return response.data;
  },
};

