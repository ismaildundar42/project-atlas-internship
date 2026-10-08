export interface ReportSummary {
  totalProjects: number;
  activeProjects: number;
  inProgressProjects: number;
  completedProjects: number;
  featuredProjects: number;
}

export interface ReportStatusDistribution {
  statusId: number;
  name: string;
  code: string;
  count: number;
  percentage: number;
}

export interface ReportCategoryDistribution {
  categoryId: number;
  name: string;
  code: string;
  count: number;
  percentage: number;
}

export interface ReportDevelopmentTypeDistribution {
  developmentType: number;
  name: string;
  count: number;
  percentage: number;
}

export interface ReportTechnology {
  id: number;
  name: string;
  category: string;
  projectCount: number;
}

export interface ReportLocation {
  id: number;
  name: string;
  type: string;
  projectCount: number;
}

export interface ReportTeam {
  id: number;
  name: string;
  departmentName: string;
  projectCount: number;
  primaryProjectCount: number;
}

export interface ReportTimeline {
  year: number;
  month: number;
  periodLabel: string;
  projectCount: number;
}

export interface ReportOverview {
  summary: ReportSummary;
  statusDistribution: ReportStatusDistribution[];
  categoryDistribution: ReportCategoryDistribution[];
  developmentTypeDistribution: ReportDevelopmentTypeDistribution[];
  topTechnologies: ReportTechnology[];
  locationDistribution: ReportLocation[];
  teamDistribution: ReportTeam[];
  projectTimeline: ReportTimeline[];
}
