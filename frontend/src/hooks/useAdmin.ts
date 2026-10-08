import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../services/adminService';
import type {
  AdminProjectQueryParams,
  CreateProjectRequest,
  UpdateProjectRequest,
  CreateTechnologyPayload,
  CreateLocationPayload,
  AuditLogQueryParams,
} from '../types/admin';

export const ADMIN_QUERY_KEYS = {
  dashboard: () => ['admin', 'dashboard'] as const,
  projects: (params: AdminProjectQueryParams) => ['admin', 'projects', params] as const,
  projectDetailForEdit: (id: number) => ['admin', 'projects', 'edit', id] as const,
  auditLogs: (params: AuditLogQueryParams) => ['admin', 'audit-logs', params] as const,
  projectAuditLogs: (projectId: number) => ['admin', 'projects', projectId, 'audit-logs'] as const,
};

export function useAdminDashboard() {
  return useQuery({
    queryKey: ADMIN_QUERY_KEYS.dashboard(),
    queryFn: () => adminService.getDashboard(),
    staleTime: 60 * 1000,
  });
}

export function useAdminProjects(params: AdminProjectQueryParams) {
  return useQuery({
    queryKey: ADMIN_QUERY_KEYS.projects(params),
    queryFn: () => adminService.getProjects(params),
    placeholderData: keepPreviousData,
    staleTime: 60 * 1000,
  });
}

export function useAdminProjectForEdit(id: number) {
  return useQuery({
    queryKey: ADMIN_QUERY_KEYS.projectDetailForEdit(id),
    queryFn: () => adminService.getProjectForEdit(id),
    enabled: !!id && id > 0,
    staleTime: 0, // Fresh data for editor
  });
}

export function useCreateProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProjectRequest) => adminService.createProject(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useUpdateProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateProjectRequest }) =>
      adminService.updateProject(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ADMIN_QUERY_KEYS.projectDetailForEdit(variables.id) });
    },
  });
}

export function useSetProjectPublished() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, isPublished }: { id: number; isPublished: boolean }) =>
      adminService.setProjectPublished(id, isPublished),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['global-search'] });
      queryClient.invalidateQueries({ queryKey: ['search'] });
      queryClient.invalidateQueries({ queryKey: ADMIN_QUERY_KEYS.projectDetailForEdit(variables.id) });
    },
  });
}

export function useSubmitProjectForReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => adminService.submitProjectForReview(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ADMIN_QUERY_KEYS.projectDetailForEdit(id) });
    },
  });
}

export function useApproveProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => adminService.approveProject(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['global-search'] });
      queryClient.invalidateQueries({ queryKey: ['search'] });
      queryClient.invalidateQueries({ queryKey: ADMIN_QUERY_KEYS.projectDetailForEdit(id) });
    },
  });
}

export function useRejectProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, rejectionReason }: { id: number; rejectionReason: string }) =>
      adminService.rejectProject(id, rejectionReason),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ADMIN_QUERY_KEYS.projectDetailForEdit(variables.id) });
    },
  });
}

export function useAdminAuditLogs(params: AuditLogQueryParams) {
  return useQuery({
    queryKey: ADMIN_QUERY_KEYS.auditLogs(params),
    queryFn: () => adminService.getAuditLogs(params),
    placeholderData: keepPreviousData,
    staleTime: 30 * 1000,
  });
}

export function useProjectAuditLogs(projectId: number) {
  return useQuery({
    queryKey: ADMIN_QUERY_KEYS.projectAuditLogs(projectId),
    queryFn: () => adminService.getProjectAuditLogs(projectId),
    enabled: !!projectId && projectId > 0,
    staleTime: 30 * 1000,
  });
}

export function useCreateTechnology() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTechnologyPayload) => adminService.createTechnology(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lookups', 'technologies'] });
    },
  });
}

export function useCreateLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateLocationPayload) => adminService.createLocation(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lookups', 'locations'] });
    },
  });
}
