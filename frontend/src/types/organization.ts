export interface DepartmentAdmin {
  id: number;
  name: string;
  code?: string;
  description?: string;
  teamCount: number;
}

export interface TeamAdmin {
  id: number;
  name: string;
  code?: string;
  departmentId?: number;
  departmentName?: string;
  description?: string;
  projectCount: number;
}

export interface MemberAdmin {
  id: number;
  firstName: string;
  lastName: string;
  title?: string;
  email?: string;
  projectCount: number;

  // ─── Organization Placement (Phase 19.7) ──────────────────────────────────
  teamId?: number | null;
  teamName?: string | null;
  departmentId?: number | null;
  departmentName?: string | null;

  // ─── Identity & Project Capability (Phase 13.2 & 19.7) ────────────────────
  hasApplicationAccount: boolean;
  applicationUserId?: number;
  applicationUserEmail?: string;
  applicationUserIsActive?: boolean;
  canCreateProjects: boolean;
  isAdmin: boolean;
  isSuperAdmin: boolean;
  hasMatchingUnlinkedAccount?: boolean;
  matchingUnlinkedUserId?: number;
}

export interface CreateDepartmentPayload {
  name: string;
  code?: string;
  description?: string;
}

export interface UpdateDepartmentPayload {
  name: string;
  code?: string;
  description?: string;
}

export interface CreateTeamPayload {
  name: string;
  code?: string;
  departmentId?: number;
  description?: string;
}

export interface UpdateTeamPayload {
  name: string;
  code?: string;
  departmentId?: number;
  description?: string;
}

export interface CreateMemberPayload {
  firstName: string;
  lastName: string;
  title?: string;
  email?: string;
  teamId?: number | null;
  departmentId?: number | null;
}

export interface UpdateMemberPayload {
  firstName: string;
  lastName: string;
  title?: string;
  email?: string;
  teamId?: number | null;
  departmentId?: number | null;
}

export interface UpdateMemberProjectAccessPayload {
  canCreateProjects: boolean;
}

export interface UpdateMemberAdminRolePayload {
  isAdmin: boolean;
}

export interface CreateMemberAccountPayload {
  password: string;
  confirmPassword: string;
}

export interface LinkMemberAccountPayload {
  userId?: number;
}

