import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  FolderKanban,
  Globe,
  FileEdit,
  Star,
  AlertTriangle,
  ArrowRight,
  UserCheck,
  Building2,
  Users,
  FileText,
  Clock,
} from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import { useAuth } from '../hooks/useAuth';
import { useAdminDashboard } from '../hooks/useAdmin';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Badge from '../components/ui/Badge';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import { formatDate } from '../utils/formatters';

function AdminOverviewPage() {
  const { t, i18n } = useTranslation(['projects', 'navigation', 'common', 'workflow']);
  const { user } = useAuth();
  const navigate = useNavigate();
  const { data: dashboard, isLoading, isError, refetch } = useAdminDashboard();

  if (isLoading) {
    return (
      <PageLayout
        title={t('admin.title', { ns: 'projects' })}
        description={t('admin.subtitle', { ns: 'projects' })}
        breadcrumbs={[
          { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
          { label: t('admin.overview', { ns: 'navigation' }) },
        ]}
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
          <Skeleton height="100px" />
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 'var(--space-4)' }}>
            <Skeleton height="120px" />
            <Skeleton height="120px" />
            <Skeleton height="120px" />
            <Skeleton height="120px" />
          </div>
          <Skeleton height="200px" />
        </div>
      </PageLayout>
    );
  }

  if (isError || !dashboard) {
    return (
      <PageLayout
        title={t('admin.title', { ns: 'projects' })}
        description={t('admin.subtitle', { ns: 'projects' })}
        breadcrumbs={[
          { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
          { label: t('admin.overview', { ns: 'navigation' }) },
        ]}
      >
        <ErrorState
          title={t('errors.genericTitle', { ns: 'common' })}
          description={t('errors.genericDesc', { ns: 'common' })}
          onRetry={refetch}
        />
      </PageLayout>
    );
  }

  return (
    <PageLayout
      title={t('admin.title', { ns: 'projects' })}
      description={t('admin.subtitle', { ns: 'projects' })}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation' }) },
      ]}
      actions={
        <Button variant="primary" size="md" onClick={() => navigate('/admin/projects')}>
          <FolderKanban size={16} aria-hidden="true" /> {t('admin.title', { ns: 'projects' })}
        </Button>
      }
    >
      <div className="admin-overview">
        {/* Identity & Welcome Bar */}
        <Card padding="md" className="admin-overview__user-card">
          <div className="admin-overview__user-info">
            <div className="admin-overview__avatar">
              <UserCheck size={24} aria-hidden="true" />
            </div>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                <h2 className="admin-overview__user-name">
                  {user?.firstName} {user?.lastName}
                </h2>
                {user?.roles?.map((role) => (
                  <Badge key={role} variant="navy" size="sm">
                    {role === 'Admin' ? t('auth.roleAdmin', { ns: 'common' }) : role}
                  </Badge>
                ))}
              </div>
              <p className="admin-overview__user-email">{user?.email}</p>
            </div>
          </div>
          <div className="admin-overview__system-badge">
            <Badge variant="success" size="sm">
              {i18n.language === 'en' ? 'Authorized Session Active' : 'Yetkili Oturum Aktif'}
            </Badge>
          </div>
        </Card>

        {/* Management Metrics / KPI Grid */}
        <section aria-labelledby="kpi-heading" className="admin-overview__section">
          <h2 id="kpi-heading" className="sr-only">{i18n.language === 'en' ? 'Management Metrics' : 'Yönetim Metrikleri'}</h2>
          <div className="admin-overview__kpi-grid">
            {/* Total Projects */}
            <Card padding="md" className="admin-kpi-card">
              <div className="admin-kpi-card__header">
                <span className="admin-kpi-card__icon admin-kpi-card__icon--navy">
                  <FolderKanban size={20} aria-hidden="true" />
                </span>
                <span className="admin-kpi-card__title">{i18n.language === 'en' ? 'Total Projects' : 'Toplam Proje'}</span>
              </div>
              <div className="admin-kpi-card__value">{dashboard.totalProjects}</div>
              <p className="admin-kpi-card__sub">{i18n.language === 'en' ? 'All active projects recorded' : 'Kayıtlı tüm aktif projeler'}</p>
            </Card>

            {/* Published Projects */}
            <Card padding="md" className="admin-kpi-card">
              <div className="admin-kpi-card__header">
                <span className="admin-kpi-card__icon admin-kpi-card__icon--success">
                  <Globe size={20} aria-hidden="true" />
                </span>
                <span className="admin-kpi-card__title">{t('enums.publicationStatus.published', { ns: 'common' })}</span>
              </div>
              <div className="admin-kpi-card__value">{dashboard.publishedProjects}</div>
              <p className="admin-kpi-card__sub">{i18n.language === 'en' ? 'Projects visible to users' : 'Kullanıcılara görünür projeler'}</p>
            </Card>

            {/* Draft Projects */}
            <Card padding="md" className="admin-kpi-card">
              <div className="admin-kpi-card__header">
                <span className="admin-kpi-card__icon admin-kpi-card__icon--warning">
                  <FileEdit size={20} aria-hidden="true" />
                </span>
                <span className="admin-kpi-card__title">{t('enums.approvalStatus.draft', { ns: 'common' })}</span>
              </div>
              <div className="admin-kpi-card__value">{dashboard.draftProjects}</div>
              <p className="admin-kpi-card__sub">{i18n.language === 'en' ? 'Records not yet live' : 'Yayına hazır olmayan kayıtlar'}</p>
            </Card>

            {/* Featured Projects */}
            <Card padding="md" className="admin-kpi-card">
              <div className="admin-kpi-card__header">
                <span className="admin-kpi-card__icon admin-kpi-card__icon--info">
                  <Star size={20} aria-hidden="true" />
                </span>
                <span className="admin-kpi-card__title">{t('library.featured', { ns: 'projects' })}</span>
              </div>
              <div className="admin-kpi-card__value">{dashboard.featuredProjects}</div>
              <p className="admin-kpi-card__sub">{i18n.language === 'en' ? 'Marked as showcase projects' : 'Vitrin projesi olarak işaretli'}</p>
            </Card>
          </div>
        </section>

        {/* Content Attention Area ("İçerik Durumu") */}
        <section aria-labelledby="attention-heading" className="admin-overview__section">
          <Card padding="md" className="admin-attention-card">
            <div className="admin-section-header">
              <div className="admin-section-header__title-group">
                <AlertTriangle size={18} className="admin-section-header__icon" aria-hidden="true" />
                <h2 id="attention-heading" className="admin-section-header__title">{i18n.language === 'en' ? 'Content Attention' : 'İçerik Durumu'}</h2>
              </div>
              <span className="admin-section-header__hint">{i18n.language === 'en' ? 'Signals requiring review or completion' : 'İnceleme gerektiren içerik sinyalleri'}</span>
            </div>

            <div className="admin-attention-grid">
              {/* Signal 1: Draft Projects */}
              <div className="admin-attention-item">
                <div className="admin-attention-item__icon admin-attention-item__icon--warning">
                  <FileText size={18} aria-hidden="true" />
                </div>
                <div className="admin-attention-item__content">
                  <span className="admin-attention-item__label">{i18n.language === 'en' ? 'Draft Projects' : 'Taslak Projeler'}</span>
                  <span className="admin-attention-item__desc">{i18n.language === 'en' ? 'Records waiting for publishing' : 'Yayınlanmayı bekleyen kayıtlar'}</span>
                </div>
                <Badge variant={dashboard.contentAttention.draftProjectsCount > 0 ? 'warning' : 'default'} size="md">
                  {dashboard.contentAttention.draftProjectsCount}
                </Badge>
              </div>

              {/* Signal 2: Missing Description */}
              <div className="admin-attention-item">
                <div className="admin-attention-item__icon admin-attention-item__icon--info">
                  <FileText size={18} aria-hidden="true" />
                </div>
                <div className="admin-attention-item__content">
                  <span className="admin-attention-item__label">{i18n.language === 'en' ? 'Missing Summary' : 'Eksik Açıklama'}</span>
                  <span className="admin-attention-item__desc">{i18n.language === 'en' ? 'Short description undefined' : 'Kısa açıklaması tanımlanmamış'}</span>
                </div>
                <Badge variant={dashboard.contentAttention.missingDescriptionCount > 0 ? 'danger' : 'default'} size="md">
                  {dashboard.contentAttention.missingDescriptionCount}
                </Badge>
              </div>

              {/* Signal 3: Missing Team */}
              <div className="admin-attention-item">
                <div className="admin-attention-item__icon admin-attention-item__icon--navy">
                  <Users size={18} aria-hidden="true" />
                </div>
                <div className="admin-attention-item__content">
                  <span className="admin-attention-item__label">{i18n.language === 'en' ? 'Missing Lead Team' : 'Sorumlu Ekip Eksik'}</span>
                  <span className="admin-attention-item__desc">{i18n.language === 'en' ? 'Projects without assigned team' : 'Ekipleri atanmamış projeler'}</span>
                </div>
                <Badge variant={dashboard.contentAttention.missingTeamCount > 0 ? 'warning' : 'default'} size="md">
                  {dashboard.contentAttention.missingTeamCount}
                </Badge>
              </div>

              {/* Signal 4: Missing Location */}
              <div className="admin-attention-item">
                <div className="admin-attention-item__icon admin-attention-item__icon--navy">
                  <Building2 size={18} aria-hidden="true" />
                </div>
                <div className="admin-attention-item__content">
                  <span className="admin-attention-item__label">{i18n.language === 'en' ? 'Missing Location' : 'Lokasyon Eksik'}</span>
                  <span className="admin-attention-item__desc">{i18n.language === 'en' ? 'Site / location not assigned' : 'Saha/lokasyon eşleşmesi yapılmamış'}</span>
                </div>
                <Badge variant={dashboard.contentAttention.missingLocationCount > 0 ? 'info' : 'default'} size="md">
                  {dashboard.contentAttention.missingLocationCount}
                </Badge>
              </div>
            </div>
          </Card>
        </section>

        {/* Recent Project Activity ("Son Güncellenen Projeler") */}
        <section aria-labelledby="recent-heading" className="admin-overview__section">
          <Card padding="md" className="admin-recent-card">
            <div className="admin-section-header">
              <div className="admin-section-header__title-group">
                <Clock size={18} className="admin-section-header__icon" aria-hidden="true" />
                <h2 id="recent-heading" className="admin-section-header__title">{i18n.language === 'en' ? 'Recently Updated Projects' : 'Son Güncellenen Projeler'}</h2>
              </div>
              <Button variant="ghost" size="sm" onClick={() => navigate('/admin/projects')}>
                {t('actions.viewAll', { ns: 'common' })} <ArrowRight size={14} aria-hidden="true" />
              </Button>
            </div>

            {dashboard.recentProjects.length === 0 ? (
              <p className="admin-recent-empty">{i18n.language === 'en' ? 'No project records found yet.' : 'Henüz proje kaydı bulunmuyor.'}</p>
            ) : (
              <div className="admin-recent-table-wrapper">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th scope="col">{t('admin.columns.name', { ns: 'projects' })}</th>
                      <th scope="col">{t('admin.columns.published', { ns: 'projects' })}</th>
                      <th scope="col">{t('admin.columns.status', { ns: 'projects' })}</th>
                      <th scope="col">{t('admin.columns.primaryTeam', { ns: 'projects' })}</th>
                      <th scope="col">{t('admin.columns.updatedAt', { ns: 'projects' })}</th>
                      <th scope="col" style={{ textAlign: 'right' }}>{t('admin.columns.actions', { ns: 'projects' })}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.recentProjects.map((project) => (
                      <tr key={project.id}>
                        <td className="admin-table__cell-main">
                          <span className="admin-table__project-name">{project.name}</span>
                        </td>
                        <td>
                          {project.isPublished ? (
                            <Badge variant="success" size="sm">{t('enums.publicationStatus.published', { ns: 'common' })}</Badge>
                          ) : (
                            <Badge variant="warning" size="sm">{t('enums.approvalStatus.draft', { ns: 'common' })}</Badge>
                          )}
                        </td>
                        <td>
                          <Badge variant="default" size="sm">{project.statusName}</Badge>
                        </td>
                        <td className="admin-table__cell-muted">
                          {project.primaryTeamName || (i18n.language === 'en' ? 'Unassigned' : 'Atanmadı')}
                        </td>
                        <td className="admin-table__cell-muted">
                          {formatDate(project.updatedAt || project.createdAt, i18n.language)}
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => navigate('/admin/projects')}
                            title={t('admin.title', { ns: 'projects' })}
                          >
                            {t('admin.actions.edit', { ns: 'projects' })}
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </section>
      </div>
    </PageLayout>
  );
}

export default AdminOverviewPage;
