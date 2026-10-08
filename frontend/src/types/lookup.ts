/**
 * Backend LookupsController DTO'ları ile birebir uyumlu TypeScript tipleri.
 */

export interface ProjectStatus {
  id: number;
  name: string;
  code: string;
  description?: string;
  displayOrder: number;
}

export interface ProjectCategory {
  id: number;
  name: string;
  code: string;
  description?: string;
  displayOrder: number;
}

export interface Technology {
  id: number;
  name: string;
  category: string;
}

export interface Location {
  id: number;
  name: string;
  description?: string;
  city?: string;
  type?: string;
}

export interface Team {
  id: number;
  name: string;
  code?: string;
  departmentName?: string;
}

export interface Tag {
  id: number;
  name: string;
  slug: string;
}

export interface Member {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string;
  email?: string;
}
