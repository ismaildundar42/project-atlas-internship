export interface UserSummary {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  canCreateProjects: boolean;
  isSuperAdmin: boolean;
  isAdmin: boolean;
  roles: string[];
}

export interface MemberSummary {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string | null;
  email?: string | null;
  teamId?: number | null;
  teamName?: string | null;
  departmentId?: number | null;
  departmentName?: string | null;
  isLinked: boolean;
}

export interface ProfileProjectItem {
  id: number;
  name: string;
  slug: string;
  shortDescription?: string | null;
  coverImageUrl?: string | null;
  statusCode: string;
  statusName: string;
  categoryCode: string;
  categoryName: string;
  developmentType: string;
  isPublished: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface UserProfileResponse {
  user: UserSummary;
  member?: MemberSummary | null;
  ownedProjects: ProfileProjectItem[];
  contributedProjects: ProfileProjectItem[];
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
