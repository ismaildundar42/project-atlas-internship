export interface TeamsSummary {
  totalTeams: number;
  totalDepartments: number;
  activeTeamsInPublishedProjects: number;
  totalMembersInProjects: number;
}

export interface TeamListItem {
  id: number;
  name: string;
  departmentId?: number;
  departmentName?: string;
  description?: string;
  publishedProjectCount: number;
  memberCount: number;
  technologies: string[];
  locations: string[];
}

export interface TeamProject {
  id: number;
  name: string;
  slug: string;
  shortDescription?: string;
  coverImageUrl?: string;
  statusName?: string;
  categoryName?: string;
  isPrimaryTeam: boolean;
  projectRole?: string;
}

export interface TeamMember {
  id: number;
  firstName: string;
  lastName: string;
  title?: string;
  email?: string;
  projectRoles: string[];
}

export interface TeamDetail {
  id: number;
  name: string;
  departmentId?: number;
  departmentName?: string;
  description?: string;
  publishedProjectCount: number;
  memberCount: number;
  technologyCount: number;
  locationCount: number;
  projects: TeamProject[];
  technologies: string[];
  locations: string[];
  members: TeamMember[];
}
