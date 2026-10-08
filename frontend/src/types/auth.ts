export interface AuthUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  isAdmin: boolean;
  isSuperAdmin: boolean;
  canCreateProjects: boolean;
  roles: string[];
}

export interface CaptchaChallenge {
  challengeId: string;
  imageDataUrl: string;
  expiresInSeconds: number;
}

export interface LoginCredentials {
  email: string;
  password: string;
  captchaChallengeId?: string;
  captchaAnswer?: string;
}

export interface AuthState {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

export interface UserProjectEntryItem {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  isAdmin: boolean;
  canCreateProjects: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface UpdateProjectEntryAccessRequest {
  canCreateProjects: boolean;
}
