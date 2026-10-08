import { isPendingReview, isApproved, isRejected } from './approvalUtils';
import type { ProjectApprovalStatus } from '../types/admin';

export interface WorkflowContext {
  isAdmin: boolean;
  currentUserId?: number | string | null;
  canCreateProjects?: boolean;
  createdByUserId?: number | string | null;
  approvalStatus?: ProjectApprovalStatus | string | number | null;
  isPublished?: boolean;
  isDeleted?: boolean;
  mode?: 'create' | 'edit';
}

export interface WorkflowActions {
  statusLabel: string;
  statusVariant: 'warning' | 'info' | 'success' | 'danger';
  canEdit: boolean;
  canSubmitForReview: boolean;
  canResubmitForReview: boolean;
  canApprove: boolean;
  canRequestCorrection: boolean;
  canPublish: boolean;
  canUnpublish: boolean;
  canArchive: boolean;
  canRestore: boolean;
}

/**
 * Tek ve yetkili iş akışı politika fonksiyonu.
 * AdminProjectsPage ve ProjectEditorPage aynı kuralları çalıştırır.
 */
export function getProjectWorkflowActions(ctx: WorkflowContext): WorkflowActions {
  const {
    isAdmin,
    currentUserId,
    createdByUserId,
    approvalStatus,
    isPublished = false,
    isDeleted = false,
    mode = 'edit',
  } = ctx;

  const currentUserIdNum = currentUserId ? Number(currentUserId) : 0;
  const createdByUserIdNum = createdByUserId ? Number(createdByUserId) : 0;
  const isOwner = Boolean(
    currentUserIdNum > 0 &&
    createdByUserIdNum > 0 &&
    currentUserIdNum === createdByUserIdNum
  );

  const pendingReview = isPendingReview(approvalStatus);
  const approved = isApproved(approvalStatus);
  const rejected = isRejected(approvalStatus);

  // Kullanıcıya görünen durum etiketi ve rozet varyantı
  let statusLabel = 'Taslak';
  let statusVariant: 'warning' | 'info' | 'success' | 'danger' = 'warning';

  if (pendingReview) {
    statusLabel = 'İnceleme Bekliyor';
    statusVariant = 'info';
  } else if (approved) {
    statusLabel = 'Onaylandı';
    statusVariant = 'success';
  } else if (rejected) {
    statusLabel = 'Düzeltme İstendi';
    statusVariant = 'danger';
  } else {
    statusLabel = 'Taslak';
    statusVariant = 'warning';
  }

  // Arşivlenmiş projeler
  if (isDeleted) {
    return {
      statusLabel,
      statusVariant,
      canEdit: false,
      canSubmitForReview: false,
      canResubmitForReview: false,
      canApprove: false,
      canRequestCorrection: false,
      canPublish: false,
      canUnpublish: false,
      canArchive: false,
      canRestore: isAdmin,
    };
  }

  // ─── 1. OLUŞTURUCU (NON-ADMIN CREATOR / OWNER) ─────────────────────────────
  if (!isAdmin) {
    if (pendingReview) {
      return {
        statusLabel,
        statusVariant,
        canEdit: false, // Form kesinlikle kilitli
        canSubmitForReview: false,
        canResubmitForReview: false,
        canApprove: false,
        canRequestCorrection: false,
        canPublish: false,
        canUnpublish: false,
        canArchive: false,
        canRestore: false,
      };
    }

    if (rejected) {
      return {
        statusLabel,
        statusVariant,
        canEdit: true,
        canSubmitForReview: false,
        canResubmitForReview: mode === 'edit',
        canApprove: false,
        canRequestCorrection: false,
        canPublish: false,
        canUnpublish: false,
        canArchive: false,
        canRestore: false,
      };
    }

    if (approved) {
      return {
        statusLabel,
        statusVariant,
        canEdit: true, // Düzenlerse onay sıfırlanma uyarısı alır
        canSubmitForReview: false,
        canResubmitForReview: false,
        canApprove: false,
        canRequestCorrection: false,
        canPublish: false,
        canUnpublish: false,
        canArchive: false,
        canRestore: false,
      };
    }

    // Varsayılan: Taslak
    return {
      statusLabel,
      statusVariant,
      canEdit: true,
      canSubmitForReview: mode === 'edit',
      canResubmitForReview: false,
      canApprove: false,
      canRequestCorrection: false,
      canPublish: false,
      canUnpublish: false,
      canArchive: false,
      canRestore: false,
    };
  }

  // ─── 2. YÖNETİCİ (ADMIN) ───────────────────────────────────────────────────
  if (pendingReview) {
    // Kritik onay bekleyen durum
    return {
      statusLabel,
      statusVariant,
      canEdit: true,
      canSubmitForReview: false,
      canResubmitForReview: false,
      canApprove: true, // [Onayla ve Yayınla]
      canRequestCorrection: true, // [Düzeltme İste]
      canPublish: false, // Jenerik 'Yayına Al' gösterilmez
      canUnpublish: false,
      canArchive: true,
      canRestore: false,
    };
  }

  if (rejected) {
    return {
      statusLabel,
      statusVariant,
      canEdit: true,
      canSubmitForReview: false,
      canResubmitForReview: false,
      canApprove: false,
      canRequestCorrection: false,
      canPublish: false,
      canUnpublish: false,
      canArchive: true,
      canRestore: false,
    };
  }

  if (approved) {
    return {
      statusLabel,
      statusVariant,
      canEdit: true,
      canSubmitForReview: false,
      canResubmitForReview: false,
      canApprove: false,
      canRequestCorrection: false,
      canPublish: !isPublished, // Yalnızca yayında değilse 'Yayına Al'
      canUnpublish: isPublished, // Yayındaysa 'Taslağa Çek'
      canArchive: true,
      canRestore: false,
    };
  }

  // Admin Taslak:
  // Admin kendi oluşturduğu taslak için doğrudan onaylayabilir; başka kullanıcının taslağı için Düzeltme İste veya Yayına Al gösterilmez
  const isAdminSelfProject = isOwner || (!createdByUserId && mode === 'create');
  return {
    statusLabel,
    statusVariant,
    canEdit: true,
    canSubmitForReview: false,
    canResubmitForReview: false,
    canApprove: isAdminSelfProject && mode === 'edit',
    canRequestCorrection: false,
    canPublish: false,
    canUnpublish: false,
    canArchive: true,
    canRestore: false,
  };
}
