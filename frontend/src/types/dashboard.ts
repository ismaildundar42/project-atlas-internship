export interface StatusDistribution {
  statusId: number;
  name: string;
  code: string;
  count: number;
}

export interface CategoryDistribution {
  categoryId: number;
  name: string;
  code: string;
  count: number;
}

export interface TopTechnology {
  id: number;
  name: string;
  category: string;
  projectCount: number;
}

export interface RecentProject {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  status: {
    id: number;
    name: string;
    code: string;
    description?: string;
    displayOrder: number;
  };
  category: {
    id: number;
    name: string;
    code: string;
    description?: string;
    displayOrder: number;
  };
  updatedAt: string;
  coverImageUrl?: string | null;
}

export interface FeaturedProject {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  status: {
    id: number;
    name: string;
    code: string;
    description?: string;
    displayOrder: number;
  };
  category: {
    id: number;
    name: string;
    code: string;
    description?: string;
    displayOrder: number;
  };
  technologies: string[];
  coverImageUrl?: string | null;
}

export interface DashboardSummary {
  totalProjects: number;
  activeProjects: number;
  inProgressProjects: number;
  featuredProjectsCount: number;
  statusDistribution: StatusDistribution[];
  categoryDistribution: CategoryDistribution[];
  recentProjects: RecentProject[];
  featuredProjects: FeaturedProject[];
  topTechnologies: TopTechnology[];
}
