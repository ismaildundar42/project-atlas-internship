import type { ProjectApprovalStatus } from '../types/admin';

/**
 * Proje Onay Durumu yardımcı kontrol fonksiyonları.
 * Backend'in string ("PendingReview", "Draft", ...) veya int (1, 0, ...) dönmesi
 * durumlarının her ikisinde de tip güvenliğini ve runtime tutarlılığını garanti eder.
 */

export function isPendingReview(status?: ProjectApprovalStatus | string | number | null): boolean {
  if (status === undefined || status === null) return false;
  return (
    status === 'PendingReview' ||
    status === 1 ||
    String(status).toLowerCase() === 'pendingreview' ||
    String(status).toLowerCase() === 'pending_review'
  );
}

export function isApproved(status?: ProjectApprovalStatus | string | number | null): boolean {
  if (status === undefined || status === null) return false;
  return (
    status === 'Approved' ||
    status === 2 ||
    String(status).toLowerCase() === 'approved'
  );
}

export function isRejected(status?: ProjectApprovalStatus | string | number | null): boolean {
  if (status === undefined || status === null) return false;
  return (
    status === 'Rejected' ||
    status === 3 ||
    String(status).toLowerCase() === 'rejected'
  );
}

export function isDraft(status?: ProjectApprovalStatus | string | number | null): boolean {
  if (status === undefined || status === null) return true;
  return (
    status === 'Draft' ||
    status === 0 ||
    String(status).toLowerCase() === 'draft'
  );
}
