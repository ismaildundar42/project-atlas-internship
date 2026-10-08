/**
 * Uygulama genelinde kullanılan genel TypeScript tip tanımları.
 *
 * Özelliğe özgü tipler kendi feature/types dosyalarında tanımlanmalıdır.
 * Yalnızca gerçekten cross-cutting tipler buraya eklenmeli.
 */

/** API yanıtlarının genel sarmalayıcı tipi (ileride kullanılacak) */
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

/** Sayfalandırılmış API yanıtı için genel tip */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/** Seçim listelerinde (dropdown vb.) kullanılan temel option tipi */
export interface SelectOption {
  value: string | number;
  label: string;
}
