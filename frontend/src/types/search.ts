/**
/// Global Search TypeScript Definitions
*/

export interface ProjectSearchResult {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  categoryName: string;
  categoryCode: string;
  statusName: string;
  statusCode: string;
  coverImageUrl?: string | null;
  primaryTeamName?: string | null;
  technologies: string[];
  tags: string[];
  locations: string[];
  teams: string[];
  matchReason?: string | null;
  updatedAt?: string | null;
}

export interface SearchResultResponse {
  query: string;
  totalCount: number;
  items: ProjectSearchResult[];
}
