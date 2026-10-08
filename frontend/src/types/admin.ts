import type { ProjectStatus, ProjectCategory } from './lookup';

export type ProjectApprovalStatus = 'Draft' | 'PendingReview' | 'Approved' | 'Rejected';

export interface AdminContentAttention {
  draftProjectsCount: number;
  pendingReviewProjectsCount?: number;
  missingDescriptionCount: number;
  missingTeamCount: number;
  missingLocationCount: number;
}

export interface AdminRecentProject {
  id: number;
  name: string;
  slug: string;
  statusName: string;
  statusCode: string;
  isPublished: boolean;
  isFeatured: boolean;
  primaryTeamName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AdminDashboard {
  totalProjects: number;
  publishedProjects: number;
  draftProjects: number;
  featuredProjects: number;
  contentAttention: AdminContentAttention;
  recentProjects: AdminRecentProject[];
}

export interface AdminProjectQueryParams {
  search?: string;
  publicationState?: 'all' | 'published' | 'draft';
  approvalState?: 'all' | 'draft' | 'pending_review' | 'approved' | 'rejected';
  lifecycleState?: 'active' | 'archived' | 'all';
  statusId?: number;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface AdminProjectListItem {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  status: ProjectStatus;
  category: ProjectCategory;
  developmentType: string;
  isPublished: boolean;
  isFeatured: boolean;
  approvalStatus: ProjectApprovalStatus;
  createdByUserId?: string;
  creatorDisplayName?: string;
  submittedForReviewAt?: string;
  reviewedAt?: string;
  rejectionReason?: string;
  coverImageUrl?: string;
  isDeleted?: boolean;
  deletedAt?: string;
  primaryTeamName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ProjectTeamRequest {
  teamId: number;
  isPrimary: boolean;
}

export interface ProjectMemberRequest {
  memberId: number;
  projectRole?: string;
}

export interface ProjectIntegrationRequest {
  name: string;
  description?: string | null;
  integrationType: string;
}

export interface ProjectDocumentRequest {
  name: string;
  description?: string | null;
  fileName: string;
  fileUrl: string;
  documentType?: string | null;
}

export interface ProjectMediaRequest {
  mediaType: string;
  fileName: string;
  fileUrl: string;
  altText?: string | null;
  caption?: string | null;
  displayOrder: number;
}

export interface CreateProjectRequest {
  name: string;
  slug: string;
  shortDescription: string;
  description?: string | null;
  purpose?: string | null;
  problemSolved?: string | null;
  nonTechnicalDescription?: string | null;
  technicalDescription?: string | null;
  businessImpact?: string | null;
  targetAudience?: string | null;
  accessInstructions?: string | null;
  applicationUrl?: string | null;
  repositoryUrl?: string | null;
  coverImageUrl?: string | null;

  statusId: number;
  categoryId: number;
  developmentType: string;

  startDate?: string | null;
  endDate?: string | null;

  isPublished: boolean;
  isFeatured: boolean;

  teams: ProjectTeamRequest[];
  members: ProjectMemberRequest[];
  locationIds: number[];
  technologyIds: number[];
  tagIds: number[];

  integrations: ProjectIntegrationRequest[];
  documents: ProjectDocumentRequest[];
  mediaItems: ProjectMediaRequest[];
}

export type UpdateProjectRequest = CreateProjectRequest;

export interface AdminProjectTeamEdit {
  teamId: number;
  teamName: string;
  isPrimary: boolean;
}

export interface AdminProjectMemberEdit {
  memberId: number;
  memberName: string;
  projectRole?: string;
}

export interface AdminProjectIntegrationEdit {
  id: number;
  name: string;
  description?: string;
  integrationType: string;
}

export interface AdminProjectDocumentEdit {
  id: number;
  name: string;
  description?: string;
  fileName: string;
  fileUrl: string;
  documentType?: string;
}

export interface AdminProjectMediaEdit {
  id: number;
  mediaType: string;
  fileName: string;
  fileUrl: string;
  altText?: string;
  caption?: string;
  displayOrder: number;
}

export interface AdminProjectEdit {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  description?: string;
  purpose?: string;
  problemSolved?: string;
  nonTechnicalDescription?: string;
  technicalDescription?: string;
  businessImpact?: string;
  targetAudience?: string;
  accessInstructions?: string;
  applicationUrl?: string;
  repositoryUrl?: string;
  coverImageUrl?: string;

  statusId: number;
  categoryId: number;
  developmentType: string;

  startDate?: string;
  endDate?: string;

  isPublished: boolean;
  isFeatured: boolean;
  approvalStatus: ProjectApprovalStatus;
  createdByUserId?: string;
  creatorDisplayName?: string;
  submittedForReviewAt?: string;
  submittedForReviewByUserId?: string;
  submittedForReviewByDisplayName?: string;
  reviewedAt?: string;
  reviewedByUserId?: string;
  reviewedByDisplayName?: string;
  rejectionReason?: string;

  createdAt: string;
  updatedAt?: string;

  teams: AdminProjectTeamEdit[];
  members: AdminProjectMemberEdit[];
  locationIds: number[];
  technologyIds: number[];
  tagIds: number[];

  integrations: AdminProjectIntegrationEdit[];
  documents: AdminProjectDocumentEdit[];
  mediaItems: AdminProjectMediaEdit[];
}

export interface RejectProjectRequest {
  reason?: string;
  rejectionReason: string;
}

export interface AuditLog {
  id: number;
  occurredAtUtc: string;
  actorUserId?: number | string;
  actorDisplayName?: string;
  actorDisplayNameSnapshot?: string;
  actorEmail?: string;
  actorEmailSnapshot?: string;
  action: string;
  entityType: string;
  entityId?: string;
  entityDisplayName?: string;
  entityDisplayNameSnapshot?: string;
  description: string;
  metadataJson?: string;
}

export interface AuditLogQueryParams {
  search?: string;
  action?: string;
  entityType?: string;
  actorUserId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface UploadResult {
  originalFileName: string;
  storedFileName: string;
  relativePath: string;
  fileUrl: string;
  fileSize: number;
  contentType: string;
}

export interface CreateTechnologyPayload {
  name: string;
  category: number;
}

export interface CreateLocationPayload {
  name: string;
  locationType: number;
  description?: string;
}
