export type ApplicationModule = 'Reports' | 'Teams' | 'reports' | 'teams';

export type AccessRequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface ModuleAccessStatusDto {
  module: number | string;
  moduleKey: 'reports' | 'teams' | string;
  moduleName: string;
  hasAccess: boolean;
  isDefaultAccess: boolean;
  requestStatus?: AccessRequestStatus | null;
  pendingRequestId?: number | null;
  requestedAt?: string | null;
}

export interface CreateModuleAccessRequestDto {
  module: ApplicationModule | number;
  reason?: string;
}

export interface ModuleAccessRequestDto {
  id: number;
  requestedByUserId: number;
  requesterName: string;
  requesterEmail: string;
  requesterTitle?: string | null;
  requesterTeamName?: string | null;
  module: number | string;
  moduleKey: string;
  moduleName: string;
  reason?: string | null;
  status: AccessRequestStatus | number;
  statusName: string;
  requestedAt: string;
  reviewedByUserId?: number | null;
  reviewerName?: string | null;
  reviewedAt?: string | null;
  reviewNote?: string | null;
}

export interface AccessRequestQueryParams {
  status?: string;
  module?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface ReviewModuleAccessRequestDto {
  reviewNote?: string;
}
