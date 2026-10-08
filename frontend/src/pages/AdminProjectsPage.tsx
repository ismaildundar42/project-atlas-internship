import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import {
  Search,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  Eye,
  CheckCircle2,
  FileEdit,
  Star,
  ChevronLeft,
  ChevronRight,
  X,
  Plus,
  Pencil,
  Archive,
  RotateCcw,
  AlertTriangle,
  Loader2,
  FolderKanban,
  Layers,
  Globe,
  EyeOff,
  Clock,
  XCircle,
  Check,
  Send,
  FileSpreadsheet,
  Download,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import {
  useAdminProjects,
  useSetProjectPublished,
  useApproveProject,
  useRejectProject,
  useSubmitProjectForReview,
} from '../hooks/useAdmin';
import { useAuth } from '../hooks/useAuth';
import { adminService } from '../services/adminService';
import { projectImportService } from '../services/projectImportService';
import { extractErrorMessage } from '../utils/urlUtils';
import { isPendingReview, isApproved, isRejected } from '../utils/approvalUtils';
import { getProjectWorkflowActions } from '../utils/workflowPolicy';
import { formatDate } from '../utils/formatters';
import type { AdminProjectQueryParams, AdminProjectListItem, ProjectApprovalStatus } from '../types/admin';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Badge from '../components/ui/Badge';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import EmptyState from '../components/ui/EmptyState';

function AdminProjectsPage() {
  const { t, i18n } = useTranslation(['projects', 'workflow', 'navigation', 'common', 'excel']);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { isAdmin, user } = useAuth();

  // Query Params state
  const [params, setParams] = useState<AdminProjectQueryParams>({
    search: '',
    publicationState: 'all',
    approvalState: 'all',
    lifecycleState: 'active',
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'updatedat',
    sortDirection: 'desc',
  });

  // Local search input state for smooth typing
  const [searchInput, setSearchInput] = useState('');

  // Soft delete confirmation modal state
  const [archiveTarget, setArchiveTarget] = useState<{ id: number; name: string } | null>(null);
  const [isProcessingArchive, setIsProcessingArchive] = useState(false);

  // Reject modal state
  const [rejectTarget, setRejectTarget] = useState<{ id: number; name: string } | null>(null);
  const [rejectionReasonInput, setRejectionReasonInput] = useState('');
  const [rejectionError, setRejectionError] = useState<string | null>(null);

  const [actionError, setActionError] = useState<string | null>(null);

  // Excel Export state
  const [isExporting, setIsExporting] = useState(false);
  const [exportMessage, setExportMessage] = useState<{ type: 'error' | 'warning'; text: string } | null>(null);

  const { data: pagedData, isLoading, isError, refetch } = useAdminProjects(params);
  const { data: pendingProjectsData } = useAdminProjects({
    approvalState: 'pending_review',
    lifecycleState: 'active',
    pageSize: 1,
  });
  const pendingReviewCount = pendingProjectsData?.totalCount ?? 0;

  const setPublishedMutation = useSetProjectPublished();
  const approveMutation = useApproveProject();
  const rejectMutation = useRejectProject();
  const submitForReviewMutation = useSubmitProjectForReview();

  const [publishingId, setPublishingId] = useState<number | null>(null);
  const [approvingId, setApprovingId] = useState<number | null>(null);
  const [submittingId, setSubmittingId] = useState<number | null>(null);

  const handleTogglePublish = async (project: AdminProjectListItem, targetState: boolean) => {
    try {
      setPublishingId(project.id);
      setActionError(null);
      await setPublishedMutation.mutateAsync({ id: project.id, isPublished: targetState });
    } catch (err) {
      setActionError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to change publication status.' : 'Yayın durumu değiştirilirken bir hata oluştu.'));
    } finally {
      setPublishingId(null);
    }
  };

  const handleApprove = async (project: AdminProjectListItem) => {
    try {
      setApprovingId(project.id);
      setActionError(null);
      await approveMutation.mutateAsync(project.id);
    } catch (err) {
      setActionError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to approve and publish project.' : 'Proje onaylanıp yayınlanırken bir hata oluştu.'));
    } finally {
      setApprovingId(null);
    }
  };

  const handleSubmitReview = async (project: AdminProjectListItem) => {
    try {
      setSubmittingId(project.id);
      setActionError(null);
      await submitForReviewMutation.mutateAsync(project.id);
    } catch (err) {
      setActionError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to submit project for review.' : 'Proje incelemeye gönderilirken bir hata oluştu.'));
    } finally {
      setSubmittingId(null);
    }
  };

  const handleConfirmReject = async () => {
    if (!rejectTarget) return;
    if (!rejectionReasonInput.trim()) {
      setRejectionError(t('modals.rejectReasonRequired', { ns: 'workflow' }));
      return;
    }

    try {
      setRejectionError(null);
      await rejectMutation.mutateAsync({ id: rejectTarget.id, rejectionReason: rejectionReasonInput.trim() });
      setRejectTarget(null);
      setRejectionReasonInput('');
    } catch (err) {
      setRejectionError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to send revision request.' : 'Düzeltme talebi iletilirken bir hata oluştu.'));
    }
  };

  const handleExport = async () => {
    if (isExporting) return;
    setExportMessage(null);

    if (pagedData && pagedData.totalCount === 0) {
      setExportMessage({
        type: 'warning',
        text: i18n.language === 'en' ? 'No projects found to export.' : 'Dışa aktarılacak proje bulunamadı.',
      });
      return;
    }

    try {
      setIsExporting(true);
      const exportParams = {
        search: params.search || undefined,
        statusId: params.statusId,
        publicationState: params.publicationState,
        approvalState: params.approvalState,
        lifecycleState: params.lifecycleState,
        sortBy: params.sortBy,
        sortDirection: params.sortDirection,
      };
      await projectImportService.exportProjects(exportParams);
    } catch (err: any) {
      setExportMessage({
        type: 'error',
        text: extractErrorMessage(err, t('export.exportError', { ns: 'excel' })),
      });
    } finally {
      setIsExporting(false);
    }
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setParams((prev) => ({
      ...prev,
      search: searchInput,
      pageNumber: 1,
    }));
  };

  const handleClearSearch = () => {
    setSearchInput('');
    setParams((prev) => ({
      ...prev,
      search: '',
      pageNumber: 1,
    }));
  };

  const handleApprovalFilter = (appState: 'all' | 'pending_review' | 'approved' | 'rejected' | 'draft') => {
    setParams((prev) => ({
      ...prev,
      approvalState: appState,
      pageNumber: 1,
    }));
  };

  const handlePublicationFilter = (pubState: 'all' | 'published' | 'draft') => {
    setParams((prev) => ({
      ...prev,
      publicationState: pubState,
      pageNumber: 1,
    }));
  };

  const handleLifecycleFilter = (lifecycle: 'active' | 'archived') => {
    setParams((prev) => ({
      ...prev,
      lifecycleState: lifecycle,
      pageNumber: 1,
    }));
  };

  const renderApprovalBadge = (status?: ProjectApprovalStatus | string | number) => {
    if (isPendingReview(status)) {
      return <Badge variant="info" size="sm" icon={<Clock size={12} />}>{t('enums.approvalStatus.pendingReview', { ns: 'common' })}</Badge>;
    }
    if (isApproved(status)) {
      return <Badge variant="success" size="sm" icon={<CheckCircle2 size={12} />}>{t('enums.approvalStatus.approved', { ns: 'common' })}</Badge>;
    }
    if (isRejected(status)) {
      return <Badge variant="danger" size="sm" icon={<XCircle size={12} />}>{t('enums.approvalStatus.rejected', { ns: 'common' })}</Badge>;
    }
    return <Badge variant="warning" size="sm" icon={<FileEdit size={12} />}>{t('enums.approvalStatus.draft', { ns: 'common' })}</Badge>;
  };

  const handlePageChange = (newPage: number) => {
    setParams((prev) => ({
      ...prev,
      pageNumber: newPage,
    }));
  };

  const handleSortChange = (sortBy: string) => {
    setParams((prev) => ({
      ...prev,
      sortBy,
      sortDirection: prev.sortBy === sortBy && prev.sortDirection === 'desc' ? 'asc' : 'desc',
      pageNumber: 1,
    }));
  };

  const getAriaSort = (columnKey: string): 'ascending' | 'descending' | 'none' => {
    if (params.sortBy?.toLowerCase() !== columnKey.toLowerCase()) {
      return 'none';
    }
    return params.sortDirection === 'asc' ? 'ascending' : 'descending';
  };

  const handleConfirmArchive = async () => {
    if (!archiveTarget) return;
    setIsProcessingArchive(true);
    setActionError(null);
    try {
      await adminService.deleteProject(archiveTarget.id);
      setArchiveTarget(null);
      await queryClient.invalidateQueries({ queryKey: ['admin'] });
      await queryClient.invalidateQueries({ queryKey: ['projects'] });
      await queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      await refetch();
    } catch (err: any) {
      setActionError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to archive project.' : 'Proje arşivlenirken bir hata oluştu.'));
    } finally {
      setIsProcessingArchive(false);
    }
  };

  const handleRestore = async (project: AdminProjectListItem) => {
    setActionError(null);
    try {
      await adminService.restoreProject(project.id);
      await queryClient.invalidateQueries({ queryKey: ['admin'] });
      await queryClient.invalidateQueries({ queryKey: ['projects'] });
      await queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      await refetch();
    } catch (err: any) {
      alert(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to restore project.' : 'Proje geri yüklenirken bir hata oluştu.'));
    }
  };

  return (
    <PageLayout
      title={t('admin.title', { ns: 'projects' })}
      description={t('admin.subtitle', { ns: 'projects' })}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
        { label: t('admin.title', { ns: 'projects' }) },
      ]}
      actions={
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
          <Button
            variant="secondary"
            size="md"
            onClick={handleExport}
            disabled={isExporting}
            title={t('export.exportBtn', { ns: 'excel' })}
          >
            {isExporting ? (
              <>
                <Loader2 size={16} className="animate-spin" aria-hidden="true" /> {t('export.exporting', { ns: 'excel' })}
              </>
            ) : (
              <>
                <Download size={16} aria-hidden="true" /> {t('export.exportBtn', { ns: 'excel' })}
              </>
            )}
          </Button>
          <Button variant="secondary" size="md" onClick={() => navigate('/admin/projects/import')}>
            <FileSpreadsheet size={16} aria-hidden="true" /> {t('admin.importExcelBtn', { ns: 'projects' })}
          </Button>
          <Button variant="primary" size="md" onClick={() => navigate('/admin/projects/new')}>
            <Plus size={16} aria-hidden="true" /> {t('admin.newProjectBtn', { ns: 'projects' })}
          </Button>
        </div>
      }
    >
      <div className="admin-projects-page">
        {exportMessage && (
          <div
            style={{
              padding: '12px 16px',
              marginBottom: '16px',
              borderRadius: '8px',
              backgroundColor: exportMessage.type === 'error' ? '#fef2f2' : '#fffbeb',
              border: `1px solid ${exportMessage.type === 'error' ? '#fecaca' : '#fde68a'}`,
              color: exportMessage.type === 'error' ? '#991b1b' : '#92400e',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              fontSize: '14px',
              fontWeight: 500,
            }}
          >
            <span>{exportMessage.text}</span>
            <button
              type="button"
              onClick={() => setExportMessage(null)}
              style={{
                background: 'transparent',
                border: 'none',
                cursor: 'pointer',
                color: 'inherit',
                padding: '4px',
                display: 'inline-flex',
                alignItems: 'center',
              }}
              aria-label={t('actions.close', { ns: 'common' })}
            >
              <X size={16} />
            </button>
          </div>
        )}
        {/* Controls & Filter Bar */}
        <Card padding="md" className="admin-filters-card">
          <form onSubmit={handleSearchSubmit} className="admin-filters-form">
            <div className="admin-search-box">
              <Search size={18} className="admin-search-box__icon" aria-hidden="true" />
              <input
                type="text"
                className="admin-search-box__input"
                placeholder={t('admin.searchPlaceholder', { ns: 'projects' })}
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                aria-label={t('admin.searchPlaceholder', { ns: 'projects' })}
              />
              {searchInput && (
                <button
                  type="button"
                  className="admin-search-box__clear"
                  onClick={handleClearSearch}
                  aria-label={t('actions.clear', { ns: 'common' })}
                >
                  <X size={16} />
                </button>
              )}
              <Button type="submit" variant="secondary" size="sm">
                {t('actions.search', { ns: 'common' })}
              </Button>
            </div>

            {/* Lifecycle & Publication Filter Control Bar */}
            <div className="admin-filter-bar">
              <div className="admin-filter-group">
                <span className="admin-filter-group-label">{i18n.language === 'en' ? 'Lifecycle View' : 'Görünüm Modu'}</span>
                <div className="admin-filter-tabs" role="tablist" aria-label="Lifecycle filters">
                  <button
                    type="button"
                    role="tab"
                    aria-selected={params.lifecycleState === 'active'}
                    className={`admin-filter-tab ${params.lifecycleState === 'active' ? 'admin-filter-tab--active' : ''}`}
                    onClick={() => handleLifecycleFilter('active')}
                  >
                    <FolderKanban size={15} />
                    <span>{i18n.language === 'en' ? 'Active Projects' : 'Aktif Projeler'}</span>
                  </button>
                  <button
                    type="button"
                    role="tab"
                    aria-selected={params.lifecycleState === 'archived'}
                    className={`admin-filter-tab ${params.lifecycleState === 'archived' ? 'admin-filter-tab--active admin-filter-tab--archived-active' : ''}`}
                    onClick={() => handleLifecycleFilter('archived')}
                  >
                    <Archive size={15} />
                    <span>{i18n.language === 'en' ? 'Archived Projects' : 'Arşivlenmiş Projeler'}</span>
                  </button>
                </div>
              </div>

              {params.lifecycleState === 'active' && (
                <>
                  <div className="admin-filter-group">
                    <span className="admin-filter-group-label">{t('admin.columns.approval', { ns: 'projects' })}</span>
                    <div className="admin-filter-tabs" role="tablist" aria-label="Approval filters">
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.approvalState === 'all'}
                        className={`admin-filter-tab ${params.approvalState === 'all' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handleApprovalFilter('all')}
                      >
                        <Layers size={14} />
                        <span>{t('status.all', { ns: 'common' })}</span>
                      </button>
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.approvalState === 'pending_review'}
                        className={`admin-filter-tab ${params.approvalState === 'pending_review' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handleApprovalFilter('pending_review')}
                        style={params.approvalState === 'pending_review' ? { backgroundColor: '#eff6ff', color: '#2563eb', borderColor: '#bfdbfe' } : {}}
                      >
                        <Clock size={14} />
                        <span>{t('admin.tabs.pendingReview', { ns: 'projects' })}</span>
                        {pendingReviewCount > 0 && (
                          <span
                            style={{
                              marginLeft: '6px',
                              padding: '1px 7px',
                              backgroundColor: params.approvalState === 'pending_review' ? '#2563eb' : '#dc2626',
                              color: '#ffffff',
                              borderRadius: '999px',
                              fontSize: '11px',
                              fontWeight: 700,
                              lineHeight: '16px',
                              display: 'inline-flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                            }}
                          >
                            {pendingReviewCount}
                          </span>
                        )}
                      </button>
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.approvalState === 'approved'}
                        className={`admin-filter-tab ${params.approvalState === 'approved' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handleApprovalFilter('approved')}
                      >
                        <CheckCircle2 size={14} />
                        <span>{t('enums.approvalStatus.approved', { ns: 'common' })}</span>
                      </button>
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.approvalState === 'rejected'}
                        className={`admin-filter-tab ${params.approvalState === 'rejected' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handleApprovalFilter('rejected')}
                      >
                        <XCircle size={14} />
                        <span>{t('enums.approvalStatus.rejected', { ns: 'common' })}</span>
                      </button>
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.approvalState === 'draft'}
                        className={`admin-filter-tab ${params.approvalState === 'draft' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handleApprovalFilter('draft')}
                      >
                        <FileEdit size={14} />
                        <span>{t('admin.tabs.drafts', { ns: 'projects' })}</span>
                      </button>
                    </div>
                  </div>

                  <div className="admin-filter-group">
                    <span className="admin-filter-group-label">{t('admin.columns.published', { ns: 'projects' })}</span>
                    <div className="admin-filter-tabs" role="tablist" aria-label="Publication filters">
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.publicationState === 'all'}
                        className={`admin-filter-tab ${params.publicationState === 'all' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handlePublicationFilter('all')}
                      >
                        <Layers size={14} />
                        <span>{t('status.all', { ns: 'common' })}</span>
                      </button>
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.publicationState === 'published'}
                        className={`admin-filter-tab ${params.publicationState === 'published' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handlePublicationFilter('published')}
                      >
                        <CheckCircle2 size={14} />
                        <span>{t('enums.publicationStatus.published', { ns: 'common' })}</span>
                      </button>
                      <button
                        type="button"
                        role="tab"
                        aria-selected={params.publicationState === 'draft'}
                        className={`admin-filter-tab ${params.publicationState === 'draft' ? 'admin-filter-tab--active' : ''}`}
                        onClick={() => handlePublicationFilter('draft')}
                      >
                        <FileEdit size={14} />
                        <span>{t('enums.publicationStatus.unpublished', { ns: 'common' })}</span>
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>
          </form>
        </Card>

        {/* Loading State */}
        {isLoading && (
          <Card padding="md">
            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              <Skeleton height="40px" />
              <Skeleton height="40px" />
              <Skeleton height="40px" />
              <Skeleton height="40px" />
              <Skeleton height="40px" />
            </div>
          </Card>
        )}

        {/* Error State */}
        {isError && !isLoading && (
          <ErrorState
            title={t('errors.genericTitle', { ns: 'common' })}
            description={t('errors.genericDesc', { ns: 'common' })}
            onRetry={refetch}
          />
        )}

        {/* Content View */}
        {!isLoading && !isError && pagedData && (
          <>
            {pagedData.items.length === 0 ? (
              <EmptyState
                title={params.lifecycleState === 'archived' ? (i18n.language === 'en' ? 'No archived projects found' : 'Arşivlenmiş proje bulunmuyor') : t('admin.emptyTitle', { ns: 'projects' })}
                description={
                  params.search
                    ? `"${params.search}" ${i18n.language === 'en' ? 'did not match any project records.' : 'araması ile eşleşen proje kaydı bulunamadı.'}`
                    : t('admin.emptyDesc', { ns: 'projects' })
                }
                actionLabel={params.search ? t('actions.clear', { ns: 'common' }) : undefined}
                onAction={params.search ? handleClearSearch : undefined}
              />
            ) : (
              <>
                {/* Desktop View Table */}
                <div className="admin-table-card card">
                  <div className="admin-table-wrapper">
                    <table className="admin-table">
                      <thead>
                        <tr>
                          <th scope="col" aria-sort={getAriaSort('name')}>
                            <button
                              type="button"
                              className="admin-table-sort-btn"
                              onClick={() => handleSortChange('name')}
                              aria-label={`${t('admin.columns.name', { ns: 'projects' })} - ${
                                getAriaSort('name') === 'ascending'
                                  ? t('library.sortAsc', { ns: 'projects', defaultValue: 'Artan sıralama' })
                                  : getAriaSort('name') === 'descending'
                                  ? t('library.sortDesc', { ns: 'projects', defaultValue: 'Azalan sıralama' })
                                  : t('library.sortBy', { ns: 'projects', defaultValue: 'Sırala' })
                              }`}
                            >
                              {t('admin.columns.name', { ns: 'projects' })}{' '}
                              {params.sortBy?.toLowerCase() === 'name' ? (
                                params.sortDirection === 'asc' ? (
                                  <ArrowUp size={14} aria-hidden="true" />
                                ) : (
                                  <ArrowDown size={14} aria-hidden="true" />
                                )
                              ) : (
                                <ArrowUpDown size={14} aria-hidden="true" />
                              )}
                            </button>
                          </th>
                          <th scope="col">{t('admin.columns.category', { ns: 'projects' })}</th>
                          <th scope="col">{t('admin.columns.status', { ns: 'projects' })}</th>
                          <th scope="col">{t('admin.columns.approval', { ns: 'projects' })}</th>
                          <th scope="col">{t('admin.columns.published', { ns: 'projects' })}</th>
                          <th scope="col">{t('library.featured', { ns: 'projects' })}</th>
                          <th scope="col">{t('admin.columns.primaryTeam', { ns: 'projects' })}</th>
                          <th scope="col" aria-sort={getAriaSort('updatedat')}>
                            <button
                              type="button"
                              className="admin-table-sort-btn"
                              onClick={() => handleSortChange('updatedat')}
                              aria-label={`${t('admin.columns.updatedAt', { ns: 'projects' })} - ${
                                getAriaSort('updatedat') === 'ascending'
                                  ? t('library.sortAsc', { ns: 'projects', defaultValue: 'Artan sıralama' })
                                  : getAriaSort('updatedat') === 'descending'
                                  ? t('library.sortDesc', { ns: 'projects', defaultValue: 'Azalan sıralama' })
                                  : t('library.sortBy', { ns: 'projects', defaultValue: 'Sırala' })
                              }`}
                            >
                              {t('admin.columns.updatedAt', { ns: 'projects' })}{' '}
                              {params.sortBy?.toLowerCase() === 'updatedat' ? (
                                params.sortDirection === 'asc' ? (
                                  <ArrowUp size={14} aria-hidden="true" />
                                ) : (
                                  <ArrowDown size={14} aria-hidden="true" />
                                )
                              ) : (
                                <ArrowUpDown size={14} aria-hidden="true" />
                              )}
                            </button>
                          </th>
                          <th scope="col" style={{ textAlign: 'right' }}>{t('admin.columns.actions', { ns: 'projects' })}</th>
                        </tr>
                      </thead>
                      <tbody>
                        {pagedData.items.map((project) => {
                          const actions = getProjectWorkflowActions({
                            isAdmin,
                            currentUserId: user?.id,
                            createdByUserId: project.createdByUserId,
                            approvalStatus: project.approvalStatus,
                            isPublished: project.isPublished,
                            isDeleted: project.isDeleted,
                            mode: 'edit',
                          });

                          return (
                            <tr key={project.id} style={{ opacity: project.isDeleted ? 0.75 : 1 }}>
                              {/* Proje Adı + Kısa Açıklama */}
                              <td className="admin-table__cell-main">
                                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                  <span className="admin-table__project-name">{project.name}</span>
                                  {project.isDeleted && (
                                    <Badge variant="danger" size="sm">
                                      {t('enums.lifecycleState.archived', { ns: 'common' })}
                                    </Badge>
                                  )}
                                </div>
                                {project.shortDescription && (
                                  <span className="admin-table__project-desc">
                                    {project.shortDescription.length > 60
                                      ? `${project.shortDescription.slice(0, 60)}...`
                                      : project.shortDescription}
                                  </span>
                                )}
                              </td>

                              {/* Kategori */}
                              <td>
                                <span className="admin-table__text-subtle">
                                  {project.category?.name || '-'}
                                </span>
                              </td>

                              {/* Durum */}
                              <td>
                                <Badge variant="default" size="sm">
                                  {project.status?.name || (i18n.language === 'en' ? 'Unspecified' : 'Belirtilmedi')}
                                </Badge>
                              </td>

                              {/* Onay Durumu */}
                              <td>
                                {renderApprovalBadge(project.approvalStatus)}
                              </td>

                              {/* Yayın */}
                              <td>
                                {project.isPublished ? (
                                  <Badge variant="success" size="sm" icon={<CheckCircle2 size={12} />}>
                                    {t('enums.publicationStatus.published', { ns: 'common' })}
                                  </Badge>
                                ) : (
                                  <Badge variant="warning" size="sm" icon={<FileEdit size={12} />}>
                                    {t('enums.publicationStatus.unpublished', { ns: 'common' })}
                                  </Badge>
                                )}
                              </td>

                              {/* Öne Çıkan */}
                              <td>
                                {project.isFeatured ? (
                                  <Badge variant="info" size="sm" icon={<Star size={12} />}>
                                    {t('actions.yes', { ns: 'common' })}
                                  </Badge>
                                ) : (
                                  <span className="admin-table__text-muted">{t('actions.no', { ns: 'common' })}</span>
                                )}
                              </td>

                              {/* Sorumlu Ekip */}
                              <td className="admin-table__cell-muted">
                                {project.primaryTeamName || (i18n.language === 'en' ? 'Unassigned' : 'Atanmadı')}
                              </td>

                              {/* Son Güncelleme */}
                              <td className="admin-table__cell-muted">
                                {formatDate(project.updatedAt || project.createdAt, i18n.language)}
                              </td>

                              {/* İşlem */}
                              <td style={{ textAlign: 'right' }}>
                                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '6px', flexWrap: 'wrap' }}>
                                  {project.isDeleted ? (
                                    actions.canRestore && (
                                      <Button
                                        variant="secondary"
                                        size="sm"
                                        onClick={() => handleRestore(project)}
                                        title={t('admin.actions.restore', { ns: 'projects' })}
                                      >
                                        <RotateCcw size={14} aria-hidden="true" /> {t('admin.actions.restore', { ns: 'projects' })}
                                      </Button>
                                    )
                                  ) : (
                                    <>
                                      {project.isPublished && (
                                        <Button
                                          variant="ghost"
                                          size="sm"
                                          onClick={() => navigate(`/projects/${project.slug}`)}
                                          title={t('admin.actions.view', { ns: 'projects' })}
                                        >
                                          <Eye size={14} aria-hidden="true" /> {t('admin.actions.view', { ns: 'projects' })}
                                        </Button>
                                      )}
                                      <Button
                                        variant="secondary"
                                        size="sm"
                                        onClick={() => navigate(`/admin/projects/${project.id}/edit`)}
                                        title={isPendingReview(project.approvalStatus) ? (i18n.language === 'en' ? 'Review' : 'İncele') : t('admin.actions.edit', { ns: 'projects' })}
                                      >
                                        <Pencil size={14} aria-hidden="true" /> {isPendingReview(project.approvalStatus) ? (i18n.language === 'en' ? 'Review' : 'İncele') : t('admin.actions.edit', { ns: 'projects' })}
                                      </Button>

                                      {/* Creator Workflow Action: İncelemeye Gönder (Draft) */}
                                      {actions.canSubmitForReview && (
                                        <Button
                                          variant="secondary"
                                          size="sm"
                                          style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
                                          disabled={submittingId === project.id}
                                          onClick={() => handleSubmitReview(project)}
                                          title={t('actions.submitForReview', { ns: 'workflow' })}
                                        >
                                          {submittingId === project.id ? (
                                            <Loader2 size={14} className="animate-spin" />
                                          ) : (
                                            <Send size={14} aria-hidden="true" />
                                          )}
                                          {' '}{t('actions.submitForReview', { ns: 'workflow' })}
                                        </Button>
                                      )}

                                      {/* Creator Workflow Action: İncelemeye Tekrar Gönder (Rejected) */}
                                      {actions.canResubmitForReview && (
                                        <Button
                                          variant="secondary"
                                          size="sm"
                                          style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
                                          disabled={submittingId === project.id}
                                          onClick={() => handleSubmitReview(project)}
                                          title={t('actions.resubmitForReview', { ns: 'workflow' })}
                                        >
                                          {submittingId === project.id ? (
                                            <Loader2 size={14} className="animate-spin" />
                                          ) : (
                                            <Send size={14} aria-hidden="true" />
                                          )}
                                          {' '}{t('actions.resubmitForReview', { ns: 'workflow' })}
                                        </Button>
                                      )}

                                      {/* Admin Review Action: Onayla ve Yayınla */}
                                      {actions.canApprove && (
                                        <Button
                                          variant="primary"
                                          size="sm"
                                          style={{ backgroundColor: '#16a34a', borderColor: '#16a34a' }}
                                          disabled={approvingId === project.id}
                                          onClick={() => handleApprove(project)}
                                          title={t('actions.approveAndPublish', { ns: 'workflow' })}
                                        >
                                          {approvingId === project.id ? (
                                            <Loader2 size={14} className="animate-spin" />
                                          ) : (
                                            <Check size={14} aria-hidden="true" />
                                          )}
                                          {' '}{t('actions.approveAndPublish', { ns: 'workflow' })}
                                        </Button>
                                      )}

                                      {/* Admin Review Action: Düzeltme İste */}
                                      {actions.canRequestCorrection && (
                                        <Button
                                          variant="ghost"
                                          size="sm"
                                          style={{ color: '#dc2626' }}
                                          onClick={() => {
                                            setRejectTarget({ id: project.id, name: project.name });
                                            setRejectionReasonInput('');
                                            setRejectionError(null);
                                          }}
                                          title={t('actions.requestChanges', { ns: 'workflow' })}
                                        >
                                          <XCircle size={14} aria-hidden="true" /> {t('actions.requestChanges', { ns: 'workflow' })}
                                        </Button>
                                      )}

                                      {/* Admin Unpublish (Taslağa Çek) */}
                                      {actions.canUnpublish && (
                                        <Button
                                          variant="ghost"
                                          size="sm"
                                          disabled={publishingId === project.id}
                                          onClick={() => handleTogglePublish(project, false)}
                                          title={t('admin.actions.unpublish', { ns: 'projects' })}
                                        >
                                          {publishingId === project.id ? (
                                            <Loader2 size={14} className="animate-spin" />
                                          ) : (
                                            <EyeOff size={14} aria-hidden="true" />
                                          )}
                                          {' '}{t('admin.actions.unpublish', { ns: 'projects' })}
                                        </Button>
                                      )}

                                      {/* Admin Publish (Yayına Al - Only for Approved Unpublished) */}
                                      {actions.canPublish && (
                                        <Button
                                          variant="secondary"
                                          size="sm"
                                          style={{ color: '#16a34a', borderColor: '#bbf7d0', backgroundColor: '#f0fdf4' }}
                                          disabled={publishingId === project.id}
                                          onClick={() => handleTogglePublish(project, true)}
                                          title={t('admin.actions.publish', { ns: 'projects' })}
                                        >
                                          {publishingId === project.id ? (
                                            <Loader2 size={14} className="animate-spin" />
                                          ) : (
                                            <Globe size={14} aria-hidden="true" />
                                          )}
                                          {' '}{t('admin.actions.publish', { ns: 'projects' })}
                                        </Button>
                                      )}

                                      {/* Admin Archive */}
                                      {actions.canArchive && (
                                        <Button
                                          variant="ghost"
                                          size="sm"
                                          style={{ color: '#ef4444' }}
                                          onClick={() => setArchiveTarget({ id: project.id, name: project.name })}
                                          title={t('admin.actions.archive', { ns: 'projects' })}
                                        >
                                          <Archive size={14} aria-hidden="true" /> {t('admin.actions.archive', { ns: 'projects' })}
                                        </Button>
                                      )}
                                    </>
                                  )}
                                </div>
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Mobile View Stacked Cards */}
                <div className="admin-mobile-cards">
                  {pagedData.items.map((project) => {
                    const actions = getProjectWorkflowActions({
                      isAdmin,
                      currentUserId: user?.id,
                      createdByUserId: project.createdByUserId,
                      approvalStatus: project.approvalStatus,
                      isPublished: project.isPublished,
                      isDeleted: project.isDeleted,
                      mode: 'edit',
                    });

                    return (
                      <Card key={project.id} padding="md" className="admin-mobile-card">
                        <div className="admin-mobile-card__header">
                          <h3 className="admin-mobile-card__title">{project.name}</h3>
                          <div className="admin-mobile-card__badges">
                            {project.isDeleted ? (
                              <Badge variant="danger" size="sm">{t('enums.lifecycleState.archived', { ns: 'common' })}</Badge>
                            ) : (
                              <>
                                {renderApprovalBadge(project.approvalStatus)}
                                {project.isPublished ? (
                                  <Badge variant="success" size="sm">{t('enums.publicationStatus.published', { ns: 'common' })}</Badge>
                                ) : (
                                  <Badge variant="warning" size="sm">{t('enums.publicationStatus.unpublished', { ns: 'common' })}</Badge>
                                )}
                                {project.isFeatured && (
                                  <Badge variant="info" size="sm">{t('library.featured', { ns: 'projects' })}</Badge>
                                )}
                              </>
                            )}
                          </div>
                        </div>

                        {project.shortDescription && (
                          <p className="admin-mobile-card__desc">{project.shortDescription}</p>
                        )}

                        <div className="admin-mobile-card__meta">
                          <div>
                            <span className="admin-mobile-card__meta-label">{t('admin.columns.category', { ns: 'projects' })}:</span>{' '}
                            <span>{project.category?.name || '-'}</span>
                          </div>
                          <div>
                            <span className="admin-mobile-card__meta-label">{t('admin.columns.status', { ns: 'projects' })}:</span>{' '}
                            <span>{project.status?.name || '-'}</span>
                          </div>
                          <div>
                            <span className="admin-mobile-card__meta-label">{t('admin.columns.primaryTeam', { ns: 'projects' })}:</span>{' '}
                            <span>{project.primaryTeamName || (i18n.language === 'en' ? 'Unassigned' : 'Atanmadı')}</span>
                          </div>
                          <div>
                            <span className="admin-mobile-card__meta-label">{t('admin.columns.updatedAt', { ns: 'projects' })}:</span>{' '}
                            <span>{formatDate(project.updatedAt || project.createdAt, i18n.language)}</span>
                          </div>
                        </div>

                        <div className="admin-mobile-card__actions" style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end', flexWrap: 'wrap' }}>
                          {project.isDeleted ? (
                            actions.canRestore && (
                              <Button
                                variant="secondary"
                                size="sm"
                                onClick={() => handleRestore(project)}
                              >
                                <RotateCcw size={14} aria-hidden="true" /> {t('admin.actions.restore', { ns: 'projects' })}
                              </Button>
                            )
                          ) : (
                            <>
                              {project.isPublished && (
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  onClick={() => navigate(`/projects/${project.slug}`)}
                                >
                                  <Eye size={14} aria-hidden="true" /> {t('admin.actions.view', { ns: 'projects' })}
                                </Button>
                              )}
                              <Button
                                variant="secondary"
                                size="sm"
                                onClick={() => navigate(`/admin/projects/${project.id}/edit`)}
                              >
                                <Pencil size={14} aria-hidden="true" /> {isPendingReview(project.approvalStatus) ? (i18n.language === 'en' ? 'Review' : 'İncele') : t('admin.actions.edit', { ns: 'projects' })}
                              </Button>

                              {/* Creator Submit for Review (Draft) */}
                              {actions.canSubmitForReview && (
                                <Button
                                  variant="secondary"
                                  size="sm"
                                  style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
                                  disabled={submittingId === project.id}
                                  onClick={() => handleSubmitReview(project)}
                                >
                                  {submittingId === project.id ? (
                                    <Loader2 size={14} className="animate-spin" />
                                  ) : (
                                    <Send size={14} aria-hidden="true" />
                                  )}
                                  {' '}{t('actions.submitForReview', { ns: 'workflow' })}
                                </Button>
                              )}

                              {/* Creator Resubmit for Review (Rejected) */}
                              {actions.canResubmitForReview && (
                                <Button
                                  variant="secondary"
                                  size="sm"
                                  style={{ color: '#2563eb', borderColor: '#bfdbfe', backgroundColor: '#eff6ff' }}
                                  disabled={submittingId === project.id}
                                  onClick={() => handleSubmitReview(project)}
                                >
                                  {submittingId === project.id ? (
                                    <Loader2 size={14} className="animate-spin" />
                                  ) : (
                                    <Send size={14} aria-hidden="true" />
                                  )}
                                  {' '}{t('actions.resubmitForReview', { ns: 'workflow' })}
                                </Button>
                              )}

                              {/* Admin Approve */}
                              {actions.canApprove && (
                                <Button
                                  variant="primary"
                                  size="sm"
                                  style={{ backgroundColor: '#16a34a', borderColor: '#16a34a' }}
                                  disabled={approvingId === project.id}
                                  onClick={() => handleApprove(project)}
                                >
                                  {approvingId === project.id ? (
                                    <Loader2 size={14} className="animate-spin" />
                                  ) : (
                                    <Check size={14} aria-hidden="true" />
                                  )}
                                  {' '}{t('actions.approveAndPublish', { ns: 'workflow' })}
                                </Button>
                              )}

                              {/* Admin Request Correction */}
                              {actions.canRequestCorrection && (
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  style={{ color: '#dc2626' }}
                                  onClick={() => {
                                    setRejectTarget({ id: project.id, name: project.name });
                                    setRejectionReasonInput('');
                                    setRejectionError(null);
                                  }}
                                >
                                  <XCircle size={14} aria-hidden="true" /> {t('actions.requestChanges', { ns: 'workflow' })}
                                </Button>
                              )}

                              {/* Admin Unpublish */}
                              {actions.canUnpublish && (
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  disabled={publishingId === project.id}
                                  onClick={() => handleTogglePublish(project, false)}
                                >
                                  {publishingId === project.id ? (
                                    <Loader2 size={14} className="animate-spin" />
                                  ) : (
                                    <EyeOff size={14} aria-hidden="true" />
                                  )}
                                  {' '}{t('admin.actions.unpublish', { ns: 'projects' })}
                                </Button>
                              )}

                              {/* Admin Publish */}
                              {actions.canPublish && (
                                <Button
                                  variant="secondary"
                                  size="sm"
                                  style={{ color: '#16a34a', borderColor: '#bbf7d0', backgroundColor: '#f0fdf4' }}
                                  disabled={publishingId === project.id}
                                  onClick={() => handleTogglePublish(project, true)}
                                >
                                  {publishingId === project.id ? (
                                    <Loader2 size={14} className="animate-spin" />
                                  ) : (
                                    <Globe size={14} aria-hidden="true" />
                                  )}
                                  {' '}{t('admin.actions.publish', { ns: 'projects' })}
                                </Button>
                              )}

                              {/* Admin Archive */}
                              {actions.canArchive && (
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  style={{ color: '#ef4444' }}
                                  onClick={() => setArchiveTarget({ id: project.id, name: project.name })}
                                >
                                  <Archive size={14} aria-hidden="true" /> {t('admin.actions.archive', { ns: 'projects' })}
                                </Button>
                              )}
                            </>
                          )}
                        </div>
                      </Card>
                    );
                  })}
                </div>

                {/* Pagination Controls */}
                <div className="admin-pagination">
                  <div className="admin-pagination__info">
                    {t('pagination.info', {
                      total: pagedData.totalCount,
                      start: (pagedData.pageNumber - 1) * pagedData.pageSize + 1,
                      end: Math.min(pagedData.pageNumber * pagedData.pageSize, pagedData.totalCount),
                      ns: 'common',
                    })}
                  </div>

                  <div className="admin-pagination__controls">
                    <Button
                      variant="secondary"
                      size="sm"
                      disabled={!pagedData.hasPreviousPage}
                      onClick={() => handlePageChange(pagedData.pageNumber - 1)}
                      aria-label={t('pagination.previous', { ns: 'common' })}
                    >
                      <ChevronLeft size={16} aria-hidden="true" /> {t('pagination.previous', { ns: 'common' })}
                    </Button>

                    <span className="admin-pagination__page-badge">
                      {t('pagination.pageOf', { current: pagedData.pageNumber, total: pagedData.totalPages, ns: 'common' })}
                    </span>

                    <Button
                      variant="secondary"
                      size="sm"
                      disabled={!pagedData.hasNextPage}
                      onClick={() => handlePageChange(pagedData.pageNumber + 1)}
                      aria-label={t('pagination.next', { ns: 'common' })}
                    >
                      {t('pagination.next', { ns: 'common' })} <ChevronRight size={16} aria-hidden="true" />
                    </Button>
                  </div>
                </div>
              </>
            )}
          </>
        )}

        {/* Soft Delete / Archive Confirmation Modal */}
        {archiveTarget && (
          <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="archive-modal-title">
            <div className="editor-modal" style={{ maxWidth: '480px' }}>
              <div className="editor-modal__header">
                <h3 id="archive-modal-title" className="editor-modal__title" style={{ display: 'flex', alignItems: 'center', gap: '8px', color: '#dc2626' }}>
                  <AlertTriangle size={20} /> {i18n.language === 'en' ? 'Archive Project' : 'Projeyi Arşivle'}
                </h3>
                <button
                  type="button"
                  className="editor-modal__close-btn"
                  onClick={() => setArchiveTarget(null)}
                  aria-label={t('actions.close', { ns: 'common', defaultValue: 'Kapat' })}
                >
                  ✕
                </button>
              </div>

              <div className="editor-modal__body">
                <p style={{ fontSize: '14px', color: 'var(--color-text-primary)', lineHeight: 1.5, margin: 0 }}>
                  <strong>"{archiveTarget.name}"</strong> {i18n.language === 'en' ? 'Are you sure you want to archive this project?' : 'isimli projeyi arşivlemek istediğinize emin misiniz?'}
                </p>
                <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)', margin: 0 }}>
                  {i18n.language === 'en'
                    ? 'Archived projects are removed from the public library and active list. Data remains preserved and can be restored from the Archived Projects tab.'
                    : 'Arşivlenen proje kamu kütüphanesinden ve aktif yönetim listesinden kaldırılır. Proje verileri ve geçmiş kayıtları bozulmadan saklanır, istenildiğinde "Arşivlenmiş Projeler" sekmesinden tekrar geri yüklenebilir.'}
                </p>

                {actionError && (
                  <div style={{ color: '#dc2626', fontSize: '13px', padding: '8px', backgroundColor: '#fef2f2', borderRadius: '4px', border: '1px solid #fecaca' }}>
                    {actionError}
                  </div>
                )}
              </div>

              <div className="editor-modal__footer">
                <Button variant="ghost" size="sm" onClick={() => setArchiveTarget(null)} disabled={isProcessingArchive}>
                  {t('actions.cancel', { ns: 'common' })}
                </Button>
                <Button variant="primary" size="sm" style={{ backgroundColor: '#dc2626', borderColor: '#dc2626' }} onClick={handleConfirmArchive} disabled={isProcessingArchive}>
                  {isProcessingArchive ? <Loader2 size={14} className="spin" /> : <Archive size={14} />} {t('admin.actions.archive', { ns: 'projects' })}
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Rejection / Correction Request Modal */}
        {rejectTarget && (
          <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="reject-modal-title">
            <div className="editor-modal" style={{ maxWidth: '480px' }}>
              <div className="editor-modal__header">
                <h3 id="reject-modal-title" className="editor-modal__title" style={{ display: 'flex', alignItems: 'center', gap: '8px', color: '#dc2626' }}>
                  <XCircle size={20} /> {t('modals.rejectTitle', { ns: 'workflow' })}
                </h3>
                <button
                  type="button"
                  className="editor-modal__close-btn"
                  onClick={() => {
                    setRejectTarget(null);
                    setRejectionReasonInput('');
                    setRejectionError(null);
                  }}
                  aria-label={t('actions.close', { ns: 'common', defaultValue: 'Kapat' })}
                >
                  ✕
                </button>
              </div>

              <div className="editor-modal__body">
                <p style={{ fontSize: '14px', color: 'var(--color-text-primary)', lineHeight: 1.5, margin: 0 }}>
                  {t('modals.rejectMessage', { ns: 'workflow' })}
                </p>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '6px', marginTop: '12px' }}>
                  <label style={{ fontSize: '13px', fontWeight: 600 }}>{t('modals.rejectReasonLabel', { ns: 'workflow' })}</label>
                  <textarea
                    rows={3}
                    className="editor-form-textarea"
                    placeholder={t('modals.rejectReasonPlaceholder', { ns: 'workflow' })}
                    value={rejectionReasonInput}
                    onChange={(e) => setRejectionReasonInput(e.target.value)}
                  />
                </div>

                {rejectionError && (
                  <div style={{ color: '#dc2626', fontSize: '13px', padding: '8px', backgroundColor: '#fef2f2', borderRadius: '4px', border: '1px solid #fecaca', marginTop: '10px' }}>
                    {rejectionError}
                  </div>
                )}
              </div>

              <div className="editor-modal__footer">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setRejectTarget(null);
                    setRejectionReasonInput('');
                    setRejectionError(null);
                  }}
                >
                  {t('actions.cancel', { ns: 'common' })}
                </Button>
                <Button
                  variant="primary"
                  size="sm"
                  style={{ backgroundColor: '#dc2626', borderColor: '#dc2626' }}
                  onClick={handleConfirmReject}
                  disabled={rejectMutation.isPending}
                >
                  {rejectMutation.isPending ? <Loader2 size={14} className="animate-spin" /> : <Send size={14} />} {t('actions.requestChanges', { ns: 'workflow' })}
                </Button>
              </div>
            </div>
          </div>
        )}
      </div>
    </PageLayout>
  );
}

export default AdminProjectsPage;
