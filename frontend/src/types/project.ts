import type { Location, ProjectCategory, ProjectStatus, Tag, Team, Technology } from './lookup';

/**
 * Backend PagedResult<T> ile uyumlu jenerik tip.
 */
export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Proje kartı ve liste görünümü için DTO.
 */
export interface ProjectListItem {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  status: ProjectStatus;
  category: ProjectCategory;
  developmentType: string;
  isFeatured: boolean;
  startDate?: string;
  endDate?: string;
  primaryTeam?: Team;
  locations: Location[];
  technologies: Technology[];
  tags: Tag[];
  coverImageUrl?: string;
}

export interface ProjectMemberDetail {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string;
  email?: string;
  projectRole?: string;
}

export interface ProjectIntegrationItem {
  id: number;
  name: string;
  description?: string;
  integrationType: string;
}

export interface ProjectMediaItem {
  id: number;
  mediaType: string;
  fileName: string;
  fileUrl: string;
  altText?: string;
  caption?: string;
  displayOrder: number;
}

export interface ProjectDocumentItem {
  id: number;
  name: string;
  description?: string;
  fileName: string;
  fileUrl: string;
  documentType?: string;
}

/**
 * Proje detay DTO'su (Backend ProjectDetailDto ile %100 uyumlu).
 */
export interface ProjectDetail {
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
  developmentType: string;
  startDate?: string;
  endDate?: string;
  isFeatured: boolean;
  coverImageUrl?: string;
  status: ProjectStatus;
  category: ProjectCategory;
  teams: (Team & { isPrimary?: boolean })[];
  members: ProjectMemberDetail[];
  locations: Location[];
  technologies: Technology[];
  tags: Tag[];
  integrations: ProjectIntegrationItem[];
  media: ProjectMediaItem[];
  documents: ProjectDocumentItem[];
  createdAt: string;
  updatedAt?: string;
}

/**
 * GET /api/projects endpoint'i için sorgu parametreleri.
 */
export interface ProjectQueryParams {
  search?: string;
  status?: string;
  category?: string;
  developmentType?: string;
  teamId?: number;
  departmentId?: number;
  department?: string;
  locationId?: number;
  technologyId?: number;
  tagId?: number;
  isFeatured?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: 'updatedAt' | 'createdAt' | 'name' | 'startDate';
  sortDirection?: 'asc' | 'desc';
}
