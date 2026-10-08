export interface ChartSegment {
  id: string | number;
  label: string;
  value: number;
  percentage: number;
  color: string;
  sublabel?: string;
}

// ─── Status Color Palette ───────────────────────────────────────────────────
export const STATUS_COLORS: Record<string, string> = {
  ACTIVE: '#16a34a',             // Canlı / Aktif - Yeşil
  ACTIVE_DEVELOPMENT: '#2563eb', // Aktif Geliştirme - Mavi
  PILOT: '#0284c7',              // Pilot - Açık Mavi
  PROOF_OF_CONCEPT: '#0ea5e9',   // POC - Gökyüzü Mavisi
  PLANNING: '#d97706',           // Planlama - Kehribar/Amber
  COMPLETED: '#0f2744',          // Tamamlandı - Kurumsal Lacivert
  ON_HOLD: '#dc2626',            // Beklemede - Kırmızı
  ARCHIVED: '#9ca3af',           // Arşivlendi - Gri
};

export const DARK_STATUS_COLORS: Record<string, string> = {
  ACTIVE: '#4ade80',             // Canlı / Aktif - Parlak Yeşil
  ACTIVE_DEVELOPMENT: '#60a5fa', // Aktif Geliştirme - Parlak Mavi
  PILOT: '#38bdf8',              // Pilot - Açık Mavi
  PROOF_OF_CONCEPT: '#7dd3fc',   // POC - Gökyüzü Mavisi
  PLANNING: '#fbbf24',           // Planlama - Amber
  COMPLETED: '#38bdf8',          // Tamamlandı - Parlak Mavi
  ON_HOLD: '#f87171',            // Beklemede - Kırmızı
  ARCHIVED: '#94a3b8',           // Arşivlendi - Gri
};

export const DEFAULT_STATUS_COLOR = '#64748b';

/**
 * Normalize a status code string to the UPPER_SNAKE_CASE keys used in the
 * STATUS_COLORS / DARK_STATUS_COLORS maps, regardless of what casing/format
 * the API returns (e.g. "Active", "active", "ACTIVE", "active-development").
 */
function normalizeStatusCode(code: string): string {
  return code.toUpperCase().replace(/-/g, '_').replace(/\s+/g, '_');
}

export function getStatusColor(code: string, isDark?: boolean): string {
  const key = normalizeStatusCode(code);
  if (isDark) {
    return DARK_STATUS_COLORS[key] ?? '#94a3b8';
  }
  return STATUS_COLORS[key] ?? DEFAULT_STATUS_COLOR;
}

// ─── Development Type Color Palette ─────────────────────────────────────────
export const DEV_TYPE_COLORS: Record<number, string> = {
  1: '#0f2744', // İç Geliştirme - Kurumsal Lacivert
  2: '#d97706', // Dış Kaynak - Amber
  3: '#2563eb', // Karma - Mavi
};

export const DARK_DEV_TYPE_COLORS: Record<number, string> = {
  1: '#38bdf8', // İç Geliştirme - Açık Mavi
  2: '#fbbf24', // Dış Kaynak - Amber
  3: '#60a5fa', // Karma - Mavi
};

export function getDevTypeColor(type: number, isDark?: boolean): string {
  if (isDark) {
    return DARK_DEV_TYPE_COLORS[type] ?? '#38bdf8';
  }
  return DEV_TYPE_COLORS[type] ?? '#1e4a7c';
}

// ─── Category / Generic Qualitative Palette ─────────────────────────────────
export const CATEGORY_PALETTE = [
  '#0f2744', // Kurumsal Lacivert
  '#1e4a7c', // Lacivert Orta
  '#2563eb', // Mavi
  '#0284c7', // Açık Mavi
  '#059669', // Zümrüt Yeşili
  '#d97706', // Kehribar
  '#7c3aed', // Mor
  '#475569', // Slate
];

export const DARK_CATEGORY_PALETTE = [
  '#38bdf8', // Açık Mavi
  '#60a5fa', // Mavi
  '#818cf8', // İndigo
  '#a78bfa', // Mor
  '#34d399', // Zümrüt Yeşili
  '#fbbf24', // Kehribar
  '#f472b6', // Pembe
  '#94a3b8', // Slate
];

export function getCategoryColor(index: number, isDark?: boolean): string {
  const palette = isDark ? DARK_CATEGORY_PALETTE : CATEGORY_PALETTE;
  return palette[index % palette.length];
}
