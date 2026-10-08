import i18n from '../i18n';
import type { TFunction } from 'i18next';

/**
 * Proje kütüphanesi ve gösterge paneli için biçimlendirme ve görünüm yardımcıları.
 */

export function getStatusVariant(code: string): 'default' | 'success' | 'warning' | 'danger' | 'info' | 'navy' {
  switch (code) {
    case 'ACTIVE':
    case 'Aktif':
      return 'success';
    case 'ACTIVE_DEVELOPMENT':
    case 'Aktif Geliştirme':
      return 'info';
    case 'PLANNING':
    case 'Fikir':
    case 'PROOF_OF_CONCEPT':
    case 'Kavram Kanıtlama (PoC)':
    case 'PILOT':
    case 'Pilot':
      return 'warning';
    case 'COMPLETED':
    case 'Tamamlandı':
      return 'navy';
    case 'ON_HOLD':
    case 'Durduruldu':
      return 'danger';
    case 'ARCHIVED':
    case 'Arşivlendi':
      return 'default';
    default:
      return 'default';
  }
}

export function formatDevelopmentType(type?: string | number, t?: TFunction): string {
  if (type === undefined || type === null || type === '') return t ? t('detail.notSpecified', { ns: 'projects' }) : 'Belirtilmedi';
  const norm = String(type).toLowerCase();
  if (norm === 'internal' || norm === '1' || norm === 'ic_kaynak' || norm === 'iç kaynak') {
    return t ? t('enums.developmentType.internal', { ns: 'common' }) : 'İç Kaynak';
  }
  if (norm === 'external' || norm === '2' || norm === 'dis_kaynak' || norm === 'dış kaynak') {
    return t ? t('enums.developmentType.external', { ns: 'common' }) : 'Dış Kaynak';
  }
  if (norm === 'hybrid' || norm === '3' || norm === 'hibrit' || norm === 'karma') {
    return t ? t('enums.developmentType.hybrid', { ns: 'common' }) : 'Hibrit';
  }
  return String(type);
}

export function formatDate(
  dateString?: string | Date | null,
  customLocale?: string,
  options?: Intl.DateTimeFormatOptions
): string {
  if (!dateString) return '';
  try {
    let date: Date;
    if (typeof dateString === 'string') {
      let str = dateString.trim();
      // If ISO format without timezone indicator (no 'Z' and no offset +/-HH:mm), append 'Z' so it is parsed as UTC
      if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?$/.test(str)) {
        str = `${str}Z`;
      }
      date = new Date(str);
    } else {
      date = dateString;
    }

    if (isNaN(date.getTime())) return String(dateString);

    const lang = customLocale || i18n.language || 'tr';
    const localeStr = lang.startsWith('en') ? 'en-US' : 'tr-TR';

    return new Intl.DateTimeFormat(localeStr, options || {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(date);
  } catch {
    return String(dateString);
  }
}

export function formatDateTime(
  dateString?: string | Date | null,
  customLocale?: string
): string {
  if (!dateString) return '';
  return formatDate(dateString, customLocale, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function formatNumber(
  value: number,
  customLocale?: string,
  options?: Intl.NumberFormatOptions
): string {
  const lang = customLocale || i18n.language || 'tr';
  const localeStr = lang.startsWith('en') ? 'en-US' : 'tr-TR';
  return new Intl.NumberFormat(localeStr, options).format(value);
}

export function formatRelativeTime(
  dateString?: string | Date | null,
  customLocale?: string
): string {
  if (!dateString) return '';
  try {
    let date: Date;
    if (typeof dateString === 'string') {
      let str = dateString.trim();
      if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?$/.test(str)) {
        str = `${str}Z`;
      }
      date = new Date(str);
    } else {
      date = dateString;
    }

    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHours = Math.floor(diffMin / 60);
    const diffDays = Math.floor(diffHours / 24);

    const lang = customLocale || i18n.language || 'tr';
    const isEn = lang.startsWith('en');

    if (diffMin < 1) {
      return isEn ? 'Just now' : 'Az önce';
    }
    if (diffHours < 1) {
      return isEn ? `${diffMin} min${diffMin > 1 ? 's' : ''} ago` : `${diffMin} dk önce`;
    }
    if (diffDays < 1) {
      return isEn ? `${diffHours} hour${diffHours > 1 ? 's' : ''} ago` : `${diffHours} sa önce`;
    }
    if (diffDays < 30) {
      return isEn ? `${diffDays} day${diffDays > 1 ? 's' : ''} ago` : `${diffDays} gün önce`;
    }

    return formatDate(date, lang);
  } catch {
    return String(dateString);
  }
}
