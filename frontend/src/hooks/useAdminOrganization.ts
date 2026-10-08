import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adminOrganizationService } from '../services/adminOrganizationService';
import type {
  CreateDepartmentPayload,
  UpdateDepartmentPayload,
  CreateTeamPayload,
  UpdateTeamPayload,
  CreateMemberPayload,
  UpdateMemberPayload,
} from '../types/organization';

export const ADMIN_ORG_QUERY_KEYS = {
  departments: () => ['admin', 'organization', 'departments'] as const,
  teams: () => ['admin', 'organization', 'teams'] as const,
  members: () => ['admin', 'organization', 'members'] as const,
};

export function useAdminDepartments() {
  return useQuery({
    queryKey: ADMIN_ORG_QUERY_KEYS.departments(),
    queryFn: () => adminOrganizationService.getDepartments(),
    staleTime: 30 * 1000,
  });
}

export function useCreateDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateDepartmentPayload) => adminOrganizationService.createDepartment(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useUpdateDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateDepartmentPayload }) =>
      adminOrganizationService.updateDepartment(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useDeleteDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminOrganizationService.deleteDepartment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useAdminTeams() {
  return useQuery({
    queryKey: ADMIN_ORG_QUERY_KEYS.teams(),
    queryFn: () => adminOrganizationService.getTeams(),
    staleTime: 30 * 1000,
  });
}

export function useCreateTeam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateTeamPayload) => adminOrganizationService.createTeam(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useUpdateTeam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateTeamPayload }) =>
      adminOrganizationService.updateTeam(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useDeleteTeam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminOrganizationService.deleteTeam(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useAdminMembers() {
  return useQuery({
    queryKey: ADMIN_ORG_QUERY_KEYS.members(),
    queryFn: () => adminOrganizationService.getMembers(),
    staleTime: 30 * 1000,
  });
}

export function useCreateMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateMemberPayload) => adminOrganizationService.createMember(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useUpdateMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateMemberPayload }) =>
      adminOrganizationService.updateMember(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useDeleteMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => adminOrganizationService.deleteMember(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['teams'] });
      queryClient.invalidateQueries({ queryKey: ['lookups'] });
    },
  });
}

export function useUpdateMemberProjectAccess() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      memberId,
      payload,
    }: {
      memberId: number;
      payload: { canCreateProjects: boolean };
    }) => adminOrganizationService.updateMemberProjectAccess(memberId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
    },
  });
}

export function useCreateMemberAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      memberId,
      payload,
    }: {
      memberId: number;
      payload: { password: string; confirmPassword: string };
    }) => adminOrganizationService.createMemberAccount(memberId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
    },
  });
}

export function useLinkMemberAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      memberId,
      payload,
    }: {
      memberId: number;
      payload?: { userId?: number };
    }) => adminOrganizationService.linkMemberAccount(memberId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
    },
  });
}

export function useUpdateMemberAdminRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      memberId,
      payload,
    }: {
      memberId: number;
      payload: { isAdmin: boolean };
    }) => adminOrganizationService.updateMemberAdminRole(memberId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organization'] });
      queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
    },
  });
}

