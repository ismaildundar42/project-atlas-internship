import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import {
  KeyRound,
  Search,
  CheckCircle2,
  XCircle,
  Clock,
  Filter,
  User,
  AlertCircle,
  ChevronLeft,
  ChevronRight,
  MessageSquare,
  ShieldOff,
  X,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Badge from '../components/ui/Badge';
import Skeleton from '../components/ui/Skeleton';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import { formatDateTime } from '../utils/formatters';
import {
  getAdminAccessRequests,
  approveAccessRequest,
  rejectAccessRequest,
  revokeModuleAccess,
  MODULE_ACCESS_CHANGED_EVENT,
} from '../services/moduleAccessService';
import type {
  ModuleAccessRequestDto,
  AccessRequestQueryParams,
} from '../types/moduleAccess';
import type { PagedResult } from '../types/project';

export function AdminModuleAccessRequestsPage() {
  const { t, i18n } = useTranslation(['access', 'common', 'navigation']);

  const [params, setParams] = useState<AccessRequestQueryParams>({
    status: 'pending',
    module: 'all',
    search: '',
    pageNumber: 1,
    pageSize: 15,
  });

  const [searchInput, setSearchInput] = useState('');
  const [data, setData] = useState<PagedResult<ModuleAccessRequestDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Review Modal state
  const [activeRequest, setActiveRequest] = useState<ModuleAccessRequestDto | null>(null);
  const [reviewAction, setReviewAction] = useState<'approve' | 'reject' | null>(null);
  const [reviewNote, setReviewNote] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Revocation Modal state
  const [revokeTarget, setRevokeTarget] = useState<ModuleAccessRequestDto | null>(null);

  const fetchRequests = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const result = await getAdminAccessRequests({
        ...params,
        status: params.status === 'all' ? undefined : params.status,
        module: params.module === 'all' ? undefined : params.module,
      });
      setData(result);
    } catch (err: any) {
      const serverDetail = err?.response?.data?.detail || err?.response?.data?.message;
      if (serverDetail && typeof serverDetail === 'string' && !serverDetail.startsWith('Request failed')) {
        setError(serverDetail);
      } else {
        setError(t('access.fetchFailed', 'Erişim talepleri yüklenirken bir hata oluştu.'));
      }
    } finally {
      setIsLoading(false);
    }
  }, [params, t]);

  useEffect(() => {
    fetchRequests();
    window.addEventListener(MODULE_ACCESS_CHANGED_EVENT, fetchRequests);
    return () => {
      window.removeEventListener(MODULE_ACCESS_CHANGED_EVENT, fetchRequests);
    };
  }, [fetchRequests]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setParams((prev) => ({
      ...prev,
      search: searchInput.trim(),
      pageNumber: 1,
    }));
  };

  const handleStatusChange = (status: string) => {
    setParams((prev) => ({
      ...prev,
      status,
      pageNumber: 1,
    }));
  };

  const handleModuleChange = (module: string) => {
    setParams((prev) => ({
      ...prev,
      module,
      pageNumber: 1,
    }));
  };

  const openReviewModal = (request: ModuleAccessRequestDto, action: 'approve' | 'reject') => {
    setActiveRequest(request);
    setReviewAction(action);
    setReviewNote('');
    setActionError(null);
  };

  const closeReviewModal = () => {
    setActiveRequest(null);
    setReviewAction(null);
    setReviewNote('');
    setActionError(null);
    setIsProcessing(false);
  };

  const handleConfirmReview = async () => {
    if (!activeRequest || !reviewAction) return;

    try {
      setIsProcessing(true);
      setActionError(null);

      if (reviewAction === 'approve') {
        await approveAccessRequest(activeRequest.id, { reviewNote: reviewNote.trim() || undefined });
      } else {
        await rejectAccessRequest(activeRequest.id, { reviewNote: reviewNote.trim() || undefined });
      }

      closeReviewModal();
      await fetchRequests();
    } catch (err: any) {
      const serverDetail = err?.response?.data?.detail || err?.response?.data?.message;
      if (serverDetail && typeof serverDetail === 'string' && !serverDetail.startsWith('Request failed')) {
        setActionError(serverDetail);
      } else {
        setActionError(t('access.actionFailed', 'İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'));
      }
    } finally {
      setIsProcessing(false);
    }
  };

  const handleConfirmRevoke = async () => {
    if (!revokeTarget) return;

    try {
      setIsProcessing(true);
      setActionError(null);

      const moduleKey = (
        revokeTarget.moduleKey ||
        (revokeTarget.module === 1 || revokeTarget.module === 'Reports' ? 'reports' : 'teams')
      ).toLowerCase();

      await revokeModuleAccess(revokeTarget.requestedByUserId, moduleKey);
      setRevokeTarget(null);
      await fetchRequests();
    } catch (err: any) {
      const serverDetail = err?.response?.data?.detail || err?.response?.data?.message;
      if (serverDetail && typeof serverDetail === 'string' && !serverDetail.startsWith('Request failed')) {
        setActionError(serverDetail);
      } else {
        setActionError(t('access.actionFailed', 'İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'));
      }
    } finally {
      setIsProcessing(false);
    }
  };

  const isPendingFilter = params.status === 'pending';

  return (
    <PageLayout
      title={t('access.adminPageTitle', 'Modül Erişim Talepleri')}
      description={t('access.adminPageSubtitle', 'Kullanıcıların Raporlama ve Ekipler modüllerine erişim taleplerini inceleyin ve yönetin.')}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation', defaultValue: 'Ana Sayfa' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation', defaultValue: 'Yönetim Merkezi' }), href: '/admin' },
        { label: t('access.adminPageTitle', 'Modül Erişim Talepleri') },
      ]}
    >
      <div className="admin-module-access-page" style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
        {/* Filters Card */}
        <Card padding="md">
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px', alignItems: 'center', justifyContent: 'space-between' }}>
            {/* Status Tabs */}
            <div className="admin-filter-tabs">
              {[
                { id: 'pending', label: t('access.status.pending', 'İnceleme Bekleyen') },
                { id: 'approved', label: t('access.status.approved', 'Onaylanan') },
                { id: 'rejected', label: t('access.status.rejected', 'Reddedilen') },
                { id: 'all', label: t('common.all', 'Tümü') },
              ].map((tab) => (
                <button
                  key={tab.id}
                  type="button"
                  className={`admin-filter-tab ${params.status === tab.id ? 'admin-filter-tab--active' : ''}`}
                  onClick={() => handleStatusChange(tab.id)}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            {/* Module Filter & Search */}
            <div style={{ display: 'flex', gap: '12px', alignItems: 'center', flexWrap: 'wrap' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Filter size={15} style={{ color: 'var(--color-text-secondary)' }} />
                <select
                  className="form-select"
                  style={{ minWidth: '150px', fontSize: '13px', padding: '8px 12px' }}
                  value={params.module || 'all'}
                  onChange={(e) => handleModuleChange(e.target.value)}
                  aria-label="Filter by module"
                >
                  <option value="all">{t('access.allModules', 'Tüm Modüller')}</option>
                  <option value="reports">{t('access.modules.reports', 'Raporlama')}</option>
                  <option value="teams">{t('access.modules.teams', 'Ekipler')}</option>
                </select>
              </div>

              <form onSubmit={handleSearchSubmit} style={{ display: 'flex', gap: '8px' }}>
                <div className="admin-search-box" style={{ minWidth: '220px' }}>
                  <Search size={16} className="admin-search-box__icon" aria-hidden="true" />
                  <input
                    type="text"
                    className="admin-search-box__input"
                    placeholder={t('common.search', 'Kullanıcı veya gerekçe ara...')}
                    value={searchInput}
                    onChange={(e) => setSearchInput(e.target.value)}
                    aria-label={t('common.search', 'Kullanıcı veya gerekçe ara...')}
                  />
                  {searchInput && (
                    <button
                      type="button"
                      className="admin-search-box__clear"
                      onClick={() => {
                        setSearchInput('');
                        setParams((prev) => ({ ...prev, search: '', pageNumber: 1 }));
                      }}
                      aria-label={t('actions.clear', { ns: 'common', defaultValue: 'Temizle' })}
                    >
                      <X size={15} />
                    </button>
                  )}
                </div>
                <Button type="submit" variant="secondary" size="sm">
                  {t('common.search', 'Ara')}
                </Button>
              </form>
            </div>
          </div>
        </Card>

        {/* Main Content */}
        {isLoading ? (
          <Card padding="md">
            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              <Skeleton height="50px" />
              <Skeleton height="50px" />
              <Skeleton height="50px" />
            </div>
          </Card>
        ) : error ? (
          <ErrorState description={error} onRetry={fetchRequests} />
        ) : !data || data.items.length === 0 ? (
          <EmptyState
            icon={<KeyRound size={48} />}
            title={
              isPendingFilter
                ? t('access.noPendingRequests', 'İnceleme bekleyen erişim talebi bulunmuyor.')
                : t('access.noRequestsFound', 'Erişim talebi bulunamadı.')
            }
            description={
              isPendingFilter
                ? t('access.noPendingDesc', 'Tüm modül erişim talepleri sonuçlandırılmıştır.')
                : t('access.tryChangingFilters', 'Filtreleri değiştirerek tekrar arayabilirsiniz.')
            }
          />
        ) : (
          <>
            <div className="admin-table-card card">
              <div className="admin-table-wrapper">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th scope="col" style={{ minWidth: '200px' }}>
                        {t('access.table.requester', 'Talep Eden')}
                      </th>
                      <th scope="col" style={{ width: '130px' }}>
                        {t('access.table.module', 'Modül')}
                      </th>
                      <th scope="col" style={{ minWidth: '220px' }}>
                        {t('access.table.reason', 'Gerekçe')}
                      </th>
                      <th scope="col" style={{ width: '160px' }}>
                        {t('access.table.date', 'Tarih')}
                      </th>
                      <th scope="col" style={{ width: '150px', textAlign: 'center' }}>
                        {t('access.table.status', 'Durum')}
                      </th>
                      <th scope="col" style={{ width: '180px', textAlign: 'right' }}>
                        {t('access.table.actions', 'İşlemler')}
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((req: ModuleAccessRequestDto) => {
                      const isPending = req.status === 'Pending' || req.status === 1;
                      const isApproved = req.status === 'Approved' || req.status === 2;

                      return (
                        <tr key={req.id}>
                          {/* Requester */}
                          <td>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                              <div
                                style={{
                                  width: '34px',
                                  height: '34px',
                                  borderRadius: '50%',
                                  backgroundColor: 'rgba(56, 189, 248, 0.12)',
                                  color: 'var(--color-brand-primary, #0284c7)',
                                  display: 'flex',
                                  alignItems: 'center',
                                  justifyContent: 'center',
                                  fontSize: '13px',
                                  fontWeight: 600,
                                  flexShrink: 0,
                                }}
                              >
                                <User size={16} aria-hidden="true" />
                              </div>
                              <div>
                                <div style={{ fontWeight: 600, color: 'var(--color-text-primary)', fontSize: '13px' }}>
                                  {req.requesterName}
                                </div>
                                <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>
                                  {req.requesterEmail}
                                </div>
                              </div>
                            </div>
                          </td>

                          {/* Module */}
                          <td>
                            <Badge variant="navy">{req.moduleName}</Badge>
                          </td>

                          {/* Reason */}
                          <td>
                            {req.reason ? (
                              <div style={{ fontSize: '13px', color: 'var(--color-text-primary)', lineHeight: 1.4 }}>
                                {req.reason}
                              </div>
                            ) : (
                              <span style={{ fontSize: '12px', color: 'var(--color-text-muted)', fontStyle: 'italic' }}>
                                {t('access.noReasonProvided', 'Belirtilmedi')}
                              </span>
                            )}

                            {req.reviewNote && (
                              <div
                                style={{
                                  marginTop: '6px',
                                  padding: '4px 8px',
                                  borderRadius: 'var(--radius-sm)',
                                  backgroundColor: 'var(--color-surface-subtle)',
                                  border: '1px solid var(--color-border-subtle)',
                                  fontSize: '12px',
                                  color: 'var(--color-text-secondary)',
                                  display: 'flex',
                                  alignItems: 'flex-start',
                                  gap: '6px',
                                }}
                              >
                                <MessageSquare size={13} style={{ marginTop: '2px', flexShrink: 0, color: 'var(--color-text-muted)' }} aria-hidden="true" />
                                <span>
                                  <strong>{req.reviewerName || t('access.admin', 'Yönetici')}:</strong> {req.reviewNote}
                                </span>
                              </div>
                            )}
                          </td>

                          {/* Date */}
                          <td style={{ whiteSpace: 'nowrap', fontSize: '12.5px', color: 'var(--color-text-muted)' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                              <Clock size={13} aria-hidden="true" />
                              <span>{formatDateTime(req.requestedAt, i18n.language)}</span>
                            </div>
                          </td>

                          {/* Status */}
                          <td style={{ textAlign: 'center' }}>
                            {isPending ? (
                              <Badge variant="warning">{t('access.status.pending', 'İnceleme Bekliyor')}</Badge>
                            ) : isApproved ? (
                              <Badge variant="success">{t('access.status.approved', 'Onaylandı')}</Badge>
                            ) : (
                              <Badge variant="danger">{t('access.status.rejected', 'Reddedildi')}</Badge>
                            )}
                          </td>

                          {/* Actions */}
                          <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                            {isPending ? (
                              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
                                <Button
                                  variant="primary"
                                  size="sm"
                                  onClick={() => openReviewModal(req, 'approve')}
                                >
                                  <CheckCircle2 size={14} aria-hidden="true" />
                                  {t('access.actions.approve', 'Onayla')}
                                </Button>
                                <Button
                                  variant="danger"
                                  size="sm"
                                  onClick={() => openReviewModal(req, 'reject')}
                                >
                                  <XCircle size={14} aria-hidden="true" />
                                  {t('access.actions.reject', 'Reddet')}
                                </Button>
                              </div>
                            ) : isApproved ? (
                              <div style={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', gap: '10px' }}>
                                <span style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>
                                  {req.reviewedAt ? formatDateTime(req.reviewedAt, i18n.language) : '-'}
                                </span>
                                <Button
                                  variant="danger"
                                  size="sm"
                                  onClick={() => {
                                    setRevokeTarget(req);
                                    setActionError(null);
                                  }}
                                >
                                  <ShieldOff size={14} aria-hidden="true" />
                                  {t('access.actions.revoke', 'Erişimi Kaldır')}
                                </Button>
                              </div>
                            ) : (
                              <span style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>
                                {req.reviewedAt ? formatDateTime(req.reviewedAt, i18n.language) : '-'}
                              </span>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Pagination Controls */}
            {data && data.totalPages > 1 && (
              <div className="admin-pagination">
                <div className="admin-pagination__info">
                  {t('pagination.info', {
                    total: data.totalCount,
                    start: (params.pageNumber! - 1) * params.pageSize! + 1,
                    end: Math.min(params.pageNumber! * params.pageSize!, data.totalCount),
                    ns: 'common',
                    defaultValue: `Toplam ${data.totalCount} kayıttan ${(params.pageNumber! - 1) * params.pageSize! + 1}-${Math.min(params.pageNumber! * params.pageSize!, data.totalCount)} arası`,
                  })}
                </div>

                <div className="admin-pagination__controls">
                  <Button
                    variant="secondary"
                    size="sm"
                    disabled={params.pageNumber === 1}
                    onClick={() => setParams((p) => ({ ...p, pageNumber: (p.pageNumber || 1) - 1 }))}
                    aria-label={t('pagination.previous', { ns: 'common', defaultValue: 'Önceki' })}
                  >
                    <ChevronLeft size={16} aria-hidden="true" /> {t('pagination.previous', { ns: 'common', defaultValue: 'Önceki' })}
                  </Button>

                  <span className="admin-pagination__page-badge">
                    {t('pagination.pageOf', { current: params.pageNumber, total: data.totalPages, ns: 'common', defaultValue: `Sayfa ${params.pageNumber} / ${data.totalPages}` })}
                  </span>

                  <Button
                    variant="secondary"
                    size="sm"
                    disabled={params.pageNumber === data.totalPages}
                    onClick={() => setParams((p) => ({ ...p, pageNumber: (p.pageNumber || 1) + 1 }))}
                    aria-label={t('pagination.next', { ns: 'common', defaultValue: 'Sonraki' })}
                  >
                    {t('pagination.next', { ns: 'common', defaultValue: 'Sonraki' })} <ChevronRight size={16} aria-hidden="true" />
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </div>

      {/* Review Modal (Approve or Reject) */}
      {activeRequest && reviewAction && (
        <div
          className="editor-modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="review-modal-title"
          onClick={(e) => {
            if (e.target === e.currentTarget && !isProcessing) closeReviewModal();
          }}
        >
          <div className="editor-modal" style={{ maxWidth: '480px' }}>
            <div className="editor-modal__header">
              <h3 id="review-modal-title" className="editor-modal__title">
                {reviewAction === 'approve'
                  ? t('access.modal.approveTitle', 'Erişim Talebini Onayla')
                  : t('access.modal.rejectTitle', 'Erişim Talebini Reddet')}
              </h3>
            </div>

            <div className="editor-modal__body">
              <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', margin: 0, lineHeight: 1.5 }}>
                <strong>{activeRequest.requesterName}</strong> ({activeRequest.requesterEmail}) kullanıcısının{' '}
                <strong>{activeRequest.moduleName}</strong> modülüne erişim talebini{' '}
                {reviewAction === 'approve' ? 'onaylamak üzeresiniz.' : 'reddetmek üzeresiniz.'}
              </p>

              {actionError && (
                <div className="editor-alert editor-alert--error">
                  <AlertCircle size={16} aria-hidden="true" />
                  <span>{actionError}</span>
                </div>
              )}

              <div className="form-group">
                <label
                  htmlFor="review-note"
                  className="form-label"
                >
                  {reviewAction === 'approve'
                    ? t('access.modal.approveNoteLabel', 'Onay Notu (İsteğe bağlı)')
                    : t('access.modal.rejectNoteLabel', 'Ret Gerekçesi / Not (İsteğe bağlı)')}
                </label>
                <textarea
                  id="review-note"
                  value={reviewNote}
                  onChange={(e) => setReviewNote(e.target.value)}
                  placeholder={
                    reviewAction === 'approve'
                      ? t('access.modal.approveNotePlaceholder', 'Örn: Talep onaylandı.')
                      : t('access.modal.rejectNotePlaceholder', 'Örn: Şu an için yetkilendirme uygun görülmemiştir.')
                  }
                  rows={3}
                  disabled={isProcessing}
                  className="form-textarea"
                />
              </div>
            </div>

            <div className="editor-modal__footer">
              <Button
                variant="ghost"
                size="md"
                onClick={closeReviewModal}
                disabled={isProcessing}
              >
                {t('common.cancel', 'Vazgeç')}
              </Button>
              <Button
                variant={reviewAction === 'approve' ? 'primary' : 'danger'}
                size="md"
                onClick={handleConfirmReview}
                isLoading={isProcessing}
              >
                {reviewAction === 'approve' ? (
                  <>
                    <CheckCircle2 size={16} aria-hidden="true" />
                    {t('access.actions.confirmApprove', 'Onayla ve Yetki Ver')}
                  </>
                ) : (
                  <>
                    <XCircle size={16} aria-hidden="true" />
                    {t('access.actions.confirmReject', 'Talebi Reddet')}
                  </>
                )}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Revocation Confirmation Modal */}
      {revokeTarget && (
        <div
          className="editor-modal-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="revoke-modal-title"
          onClick={(e) => {
            if (e.target === e.currentTarget && !isProcessing) setRevokeTarget(null);
          }}
        >
          <div className="editor-modal" style={{ maxWidth: '480px' }}>
            <div className="editor-modal__header">
              <h3 id="revoke-modal-title" className="editor-modal__title" style={{ color: 'var(--color-danger)' }}>
                {t('access.modal.revokeTitle', 'Modül Erişim Yetkisini Kaldır')}
              </h3>
            </div>

            <div className="editor-modal__body">
              <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', margin: 0, lineHeight: 1.5 }}>
                <strong>{revokeTarget.requesterName}</strong> ({revokeTarget.requesterEmail}) {t('access.modal.revokeDesc', { module: revokeTarget.moduleName, defaultValue: `kullanıcısının ${revokeTarget.moduleName} modülü erişim yetkisini kaldırmak üzeresiniz.` })}
              </p>

              {actionError && (
                <div className="editor-alert editor-alert--error">
                  <AlertCircle size={16} aria-hidden="true" />
                  <span>{actionError}</span>
                </div>
              )}
            </div>

            <div className="editor-modal__footer">
              <Button
                variant="ghost"
                size="md"
                onClick={() => setRevokeTarget(null)}
                disabled={isProcessing}
              >
                {t('common.cancel', 'Vazgeç')}
              </Button>
              <Button
                variant="danger"
                size="md"
                onClick={handleConfirmRevoke}
                isLoading={isProcessing}
              >
                <ShieldOff size={16} aria-hidden="true" />
                {t('access.actions.confirmRevoke', 'Yetkiyi Kaldır')}
              </Button>
            </div>
          </div>
        </div>
      )}
    </PageLayout>
  );
}

export default AdminModuleAccessRequestsPage;
