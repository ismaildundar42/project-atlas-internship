import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Search,
  X,
  ChevronLeft,
  ChevronRight,
  Filter,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import { useAdminAuditLogs } from '../hooks/useAdmin';
import type { AuditLogQueryParams } from '../types/admin';
import type { BadgeVariant } from '../components/ui/Badge';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Badge from '../components/ui/Badge';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import EmptyState from '../components/ui/EmptyState';
import { formatDateTime } from '../utils/formatters';

const ACTION_VARIANTS: Record<string, BadgeVariant> = {
  ProjectCreated: 'default',
  ProjectUpdated: 'default',
  ProjectSubmittedForReview: 'info',
  ProjectApproved: 'success',
  ProjectRejected: 'danger',
  ProjectPublished: 'success',
  ProjectUnpublished: 'warning',
  ProjectArchived: 'danger',
  ProjectRestored: 'info',
  ProjectDeleted: 'danger',
  MemberAccountCreated: 'navy',
  MemberAccountLinked: 'info',
  ProjectAccessGranted: 'success',
  ProjectAccessRevoked: 'warning',
  ProjectBatchImported: 'navy',
  ProjectImported: 'navy',
  ModuleAccessRequested: 'info',
  ModuleAccessApproved: 'success',
  ModuleAccessRejected: 'danger',
  ModuleAccessRevoked: 'warning',
  ModuleAccessGranted: 'success',
};

function AdminAuditLogsPage() {
  const { t, i18n } = useTranslation(['workflow', 'navigation', 'common']);
  const [params, setParams] = useState<AuditLogQueryParams>({
    search: '',
    action: undefined,
    entityType: undefined,
    pageNumber: 1,
    pageSize: 15,
  });

  const [searchInput, setSearchInput] = useState('');

  const { data: pagedData, isLoading, isError, refetch } = useAdminAuditLogs(params);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setParams((prev) => ({
      ...prev,
      search: searchInput.trim(),
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

  const handleActionChange = (actionVal: string) => {
    setParams((prev) => ({
      ...prev,
      action: actionVal === 'all' ? undefined : actionVal,
      pageNumber: 1,
    }));
  };

  const handleEntityTypeChange = (entityVal: string) => {
    setParams((prev) => ({
      ...prev,
      entityType: entityVal === 'all' ? undefined : entityVal,
      pageNumber: 1,
    }));
  };

  const handlePageChange = (newPage: number) => {
    setParams((prev) => ({
      ...prev,
      pageNumber: newPage,
    }));
  };

  const renderActionBadge = (action: string) => {
    const label = t(`audit.actions.${action}`, { defaultValue: action, ns: 'workflow' });
    const variant = ACTION_VARIANTS[action] || 'default';
    return <Badge variant={variant} size="sm">{label}</Badge>;
  };

  return (
    <PageLayout
      title={t('audit.title', { ns: 'workflow' })}
      description={t('audit.subtitle', { ns: 'workflow' })}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
        { label: t('audit.title', { ns: 'workflow' }) },
      ]}
    >
      <div className="admin-audit-logs-page" style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
        {/* Filters Card */}
        <Card padding="md">
          <form onSubmit={handleSearchSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'center' }}>
              <div className="admin-search-box" style={{ flex: '1 1 300px' }}>
                <Search size={18} className="admin-search-box__icon" aria-hidden="true" />
                <input
                  type="text"
                  className="admin-search-box__input"
                  placeholder={t('audit.searchPlaceholder', { ns: 'workflow' })}
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  aria-label={t('audit.searchPlaceholder', { ns: 'workflow' })}
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

              {/* Action Filter Select */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Filter size={15} style={{ color: 'var(--color-text-secondary)' }} />
                <select
                  className="form-select"
                  style={{ minWidth: '180px', fontSize: '13px', padding: '8px 12px' }}
                  value={params.action || 'all'}
                  onChange={(e) => handleActionChange(e.target.value)}
                  aria-label="Filter by action type"
                >
                  <option value="all">{t('audit.allActionTypes', { ns: 'workflow' })}</option>
                  <option value="ModuleAccessRequested">{t('audit.actions.ModuleAccessRequested', { ns: 'workflow' })}</option>
                  <option value="ModuleAccessApproved">{t('audit.actions.ModuleAccessApproved', { ns: 'workflow' })}</option>
                  <option value="ModuleAccessRejected">{t('audit.actions.ModuleAccessRejected', { ns: 'workflow' })}</option>
                  <option value="ModuleAccessRevoked">{t('audit.actions.ModuleAccessRevoked', { ns: 'workflow' })}</option>
                  <option value="ModuleAccessGranted">{t('audit.actions.ModuleAccessGranted', { ns: 'workflow' })}</option>
                  <option value="ProjectBatchImported">{t('audit.actions.ProjectBatchImported', { ns: 'workflow' })}</option>
                  <option value="ProjectImported">{t('audit.actions.ProjectImported', { ns: 'workflow' })}</option>
                  <option value="ProjectSubmittedForReview">{t('audit.actions.ProjectSubmittedForReview', { ns: 'workflow' })}</option>
                  <option value="ProjectApproved">{t('audit.actions.ProjectApproved', { ns: 'workflow' })}</option>
                  <option value="ProjectRejected">{t('audit.actions.ProjectRejected', { ns: 'workflow' })}</option>
                  <option value="ProjectCreated">{t('audit.actions.ProjectCreated', { ns: 'workflow' })}</option>
                  <option value="ProjectUpdated">{t('audit.actions.ProjectUpdated', { ns: 'workflow' })}</option>
                  <option value="ProjectPublished">{t('audit.actions.ProjectPublished', { ns: 'workflow' })}</option>
                  <option value="ProjectUnpublished">{t('audit.actions.ProjectUnpublished', { ns: 'workflow' })}</option>
                  <option value="ProjectArchived">{t('audit.actions.ProjectArchived', { ns: 'workflow' })}</option>
                  <option value="ProjectRestored">{t('audit.actions.ProjectRestored', { ns: 'workflow' })}</option>
                  <option value="ProjectAccessGranted">{t('audit.actions.ProjectAccessGranted', { ns: 'workflow' })}</option>
                  <option value="ProjectAccessRevoked">{t('audit.actions.ProjectAccessRevoked', { ns: 'workflow' })}</option>
                  <option value="MemberAccountCreated">{t('audit.actions.MemberAccountCreated', { ns: 'workflow' })}</option>
                </select>
              </div>

              {/* Entity Type Filter Select */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <select
                  className="form-select"
                  style={{ minWidth: '150px', fontSize: '13px', padding: '8px 12px' }}
                  value={params.entityType || 'all'}
                  onChange={(e) => handleEntityTypeChange(e.target.value)}
                  aria-label="Filter by entity type"
                >
                  <option value="all">{t('audit.allEntityTypes', { ns: 'workflow' })}</option>
                  <option value="Project">{t('audit.entities.project', { ns: 'workflow' })}</option>
                  <option value="ProjectBatch">{t('audit.entities.projectBatch', { ns: 'workflow' })}</option>
                  <option value="Member">{t('audit.entities.member', { ns: 'workflow' })}</option>
                  <option value="ApplicationUser">{t('audit.entities.user', { ns: 'workflow' })}</option>
                  <option value="ModuleAccessRequest">{t('audit.entities.moduleAccessRequest', { ns: 'workflow' })}</option>
                  <option value="UserModulePermission">{t('audit.entities.userModulePermission', { ns: 'workflow' })}</option>
                </select>
              </div>
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
            title={t('audit.loadErrorTitle', { ns: 'workflow' })}
            description={t('audit.loadErrorDesc', { ns: 'workflow' })}
            onRetry={refetch}
          />
        )}

        {/* Data View */}
        {!isLoading && !isError && pagedData && (
          <>
            {pagedData.items.length === 0 ? (
              <EmptyState
                title={t('audit.emptyTitle', { ns: 'workflow' })}
                description={
                  params.search
                    ? t('audit.emptySearchDesc', { query: params.search, ns: 'workflow' })
                    : t('audit.emptyDesc', { ns: 'workflow' })
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
                          <th scope="col" style={{ width: '170px' }}>{t('audit.columns.date', { ns: 'workflow' })}</th>
                          <th scope="col" style={{ width: '180px' }}>{t('audit.columns.user', { ns: 'workflow' })}</th>
                          <th scope="col" style={{ width: '170px' }}>{t('audit.columns.action', { ns: 'workflow' })}</th>
                          <th scope="col" style={{ width: '200px' }}>{t('audit.columns.entity', { ns: 'workflow' })}</th>
                          <th scope="col">{t('audit.columns.description', { ns: 'workflow' })}</th>
                        </tr>
                      </thead>
                      <tbody>
                        {pagedData.items.map((log) => (
                          <tr key={log.id}>
                            <td className="admin-table__cell-muted" style={{ fontSize: '12.5px', whiteSpace: 'nowrap' }}>
                              {formatDateTime(log.occurredAtUtc, i18n.language)}
                            </td>
                            <td>
                              <span style={{ fontWeight: 600, fontSize: '13px', color: 'var(--color-text-primary)' }}>
                                {log.actorDisplayName || log.actorDisplayNameSnapshot || log.actorEmail || log.actorEmailSnapshot || t('audit.systemUser', { ns: 'workflow' })}
                              </span>
                            </td>
                            <td>
                              {renderActionBadge(log.action)}
                            </td>
                            <td>
                              <span style={{ fontSize: '13px', color: 'var(--color-text-primary)', fontWeight: 500 }}>
                                {log.entityDisplayName || log.entityDisplayNameSnapshot || (log.entityId ? `#${log.entityId}` : '-')}
                              </span>
                            </td>
                            <td style={{ fontSize: '13px', color: 'var(--color-text-primary)', lineHeight: 1.4 }}>
                              {log.description}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Mobile View Stacked List */}
                <div className="admin-mobile-cards">
                  {pagedData.items.map((log) => (
                    <Card key={log.id} padding="md" className="admin-mobile-card">
                      <div className="admin-mobile-card__header">
                        <span style={{ fontWeight: 600, fontSize: '14px' }}>
                          {log.actorDisplayName || log.actorDisplayNameSnapshot || log.actorEmail || log.actorEmailSnapshot || t('audit.systemUser', { ns: 'workflow' })}
                        </span>
                        {renderActionBadge(log.action)}
                      </div>
                      <p style={{ fontSize: '13px', color: 'var(--color-text-primary)', margin: '8px 0', lineHeight: 1.4 }}>
                        {log.description}
                      </p>
                      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '12px', color: 'var(--color-text-secondary)', marginTop: '8px' }}>
                        <span>{t('audit.recordLabel', { ns: 'workflow' })} <strong>{log.entityDisplayName || log.entityDisplayNameSnapshot || '-'}</strong></span>
                        <span>{formatDateTime(log.occurredAtUtc, i18n.language)}</span>
                      </div>
                    </Card>
                  ))}
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
      </div>
    </PageLayout>
  );
}

export default AdminAuditLogsPage;
