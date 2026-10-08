import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  FolderKanban,
  CheckCircle2,
  Clock,
  Star,
  ArrowRight,
  Layers,
  Cpu,
  Sparkles,
} from 'lucide-react';
import { useDashboardSummary } from '../hooks/useDashboardSummary';
import { useHealthCheck } from '../hooks/useHealthCheck';
import PageLayout from '../layouts/PageLayout';
import Card from '../components/ui/Card';
import Badge from '../components/ui/Badge';
import Button from '../components/ui/Button';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import EmptyState from '../components/ui/EmptyState';
import { ImageWithFallback } from '../components/ui/ImageWithFallback';
import { resolveResourceUrl } from '../utils/urlUtils';
import { formatDate } from '../utils/formatters';

// ─── Status Variant Helper ───────────────────────────────────────────────────

function getStatusVariant(code: string): 'default' | 'success' | 'warning' | 'danger' | 'info' | 'navy' {
  switch (code) {
    case 'ACTIVE':
      return 'success';
    case 'ACTIVE_DEVELOPMENT':
      return 'info';
    case 'PLANNING':
    case 'PROOF_OF_CONCEPT':
    case 'PILOT':
      return 'warning';
    case 'COMPLETED':
      return 'navy';
    case 'ON_HOLD':
      return 'danger';
    default:
      return 'default';
  }
}

/**
 * Dashboard / Ana Sayfa Bileşeni.
 * Demir Export Ar-Ge portföyünün canlı metriklerini, durum/kategori dağılımlarını,
 * öne çıkan ve son eklenen projeleri sunar.
 */
function DashboardPage() {
  const { t, i18n } = useTranslation(['projects', 'navigation', 'common']);
  const navigate = useNavigate();
  const { data: summary, isLoading, isError, refetch } = useDashboardSummary();
  const { data: health } = useHealthCheck();

  // 1. Error State
  if (isError) {
    return (
      <PageLayout
        title={t('projects.dashboard.pageTitle', 'Demir Export Proje Kütüphanesi')}
        description={t('projects.dashboard.pageDescription', 'Ar-Ge projelerini keşfedin, inceleyin ve takip edin.')}
      >
        <Card>
          <ErrorState
            title={t('projects.dashboard.errorTitle', 'Dashboard Verileri Yüklenemedi')}
            description={t('projects.dashboard.errorDescription', 'Sunucuyla iletişim kurulurken bir sorun oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.')}
            retryLabel={t('common.retry', 'Yeniden Dene')}
            onRetry={() => { void refetch(); }}
          />
        </Card>
      </PageLayout>
    );
  }

  // 2. Loading State (Skeleton)
  if (isLoading || !summary) {
    return (
      <PageLayout
        title={t('projects.dashboard.pageTitle', 'Demir Export Proje Kütüphanesi')}
        description={t('projects.dashboard.pageDescription', 'Ar-Ge projelerini keşfedin, inceleyin ve takip edin.')}
      >
        <div className="dashboard" aria-busy="true" aria-label={t('projects.dashboard.loadingAria', 'Dashboard verileri yükleniyor')}>
          {/* KPI Skeleton */}
          <div className="dashboard-kpi-grid">
            {[1, 2, 3, 4].map((i) => (
              <Card key={i}>
                <div className="kpi-card">
                  <div className="kpi-card__content" style={{ width: '100%' }}>
                    <Skeleton width={100} height={16} />
                    <Skeleton width={60} height={36} />
                  </div>
                  <Skeleton variant="circle" width={48} height={48} />
                </div>
              </Card>
            ))}
          </div>

          {/* Section Skeleton */}
          <div className="dashboard-grid-2col">
            <Card>
              <Skeleton width={180} height={24} />
              <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 16 }}>
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} width="100%" height={24} />
                ))}
              </div>
            </Card>
            <Card>
              <Skeleton width={180} height={24} />
              <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 16 }}>
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} width="100%" height={24} />
                ))}
              </div>
            </Card>
          </div>

          {/* Featured Cards Skeleton */}
          <div>
            <Skeleton width={200} height={28} />
            <div className="featured-grid" style={{ marginTop: 16 }}>
              <Skeleton.Card />
              <Skeleton.Card />
            </div>
          </div>
        </div>
      </PageLayout>
    );
  }

  // 3. Empty State (Yayınlanmış proje sıfırsa)
  if (summary.totalProjects === 0) {
    return (
      <PageLayout
        title={t('projects.dashboard.pageTitle', 'Demir Export Proje Kütüphanesi')}
        description={t('projects.dashboard.pageDescription', 'Ar-Ge projelerini keşfedin, inceleyin ve takip edin.')}
      >
        <Card>
          <EmptyState
            title={t('projects.dashboard.noPublishedProjects', 'Yayınlanmış Proje Bulunmuyor')}
            description={t('projects.dashboard.noPublishedProjectsDesc', 'Henüz sisteme eklenmiş ve yayınlanmış bir Ar-Ge projesi bulunmamaktadır.')}
            actionLabel={t('projects.dashboard.goToLibrary', 'Proje Kütüphanesine Git')}
            onAction={() => navigate('/projects')}
          />
        </Card>
      </PageLayout>
    );
  }

  return (
    <PageLayout
      title={t('projects.dashboard.pageTitle', 'Demir Export Proje Kütüphanesi')}
      description={t('projects.dashboard.pageDescription', 'Ar-Ge projelerini keşfedin, inceleyin ve takip edin.')}
      actions={
        <Button
          variant="primary"
          rightIcon={<ArrowRight size={16} />}
          onClick={() => navigate('/projects')}
        >
          {t('projects.dashboard.goToLibrary', 'Proje Kütüphanesine Git')}
        </Button>
      }
    >
      <div className="dashboard">
        {/* ─── 1. KPI Cards ───────────────────────────────────────────────────── */}
        <section aria-label={t('projects.dashboard.portfolioMetricsAria', 'Portföy Özet Metrikleri')}>
          <div className="dashboard-kpi-grid">
            {/* Toplam Proje */}
            <Card>
              <div className="kpi-card">
                <div className="kpi-card__content">
                  <span className="kpi-card__label">{t('projects.dashboard.totalProjects', 'Toplam Proje')}</span>
                  <span className="kpi-card__value">{summary.totalProjects}</span>
                </div>
                <div className="kpi-card__icon-wrapper kpi-card__icon-wrapper--navy" aria-hidden="true">
                  <FolderKanban size={24} />
                </div>
              </div>
            </Card>

            {/* Aktif Proje */}
            <Card>
              <div className="kpi-card">
                <div className="kpi-card__content">
                  <span className="kpi-card__label">{t('projects.dashboard.activeProjects', 'Aktif Proje')}</span>
                  <span className="kpi-card__value">{summary.activeProjects}</span>
                </div>
                <div className="kpi-card__icon-wrapper kpi-card__icon-wrapper--success" aria-hidden="true">
                  <CheckCircle2 size={24} />
                </div>
              </div>
            </Card>

            {/* Devam Eden */}
            <Card>
              <div className="kpi-card">
                <div className="kpi-card__content">
                  <span className="kpi-card__label">{t('projects.dashboard.inProgressProjects', 'Devam Eden')}</span>
                  <span className="kpi-card__value">{summary.inProgressProjects}</span>
                </div>
                <div className="kpi-card__icon-wrapper kpi-card__icon-wrapper--warning" aria-hidden="true">
                  <Clock size={24} />
                </div>
              </div>
            </Card>

            {/* Öne Çıkan */}
            <Card>
              <div className="kpi-card">
                <div className="kpi-card__content">
                  <span className="kpi-card__label">{t('projects.dashboard.featuredProjects', 'Öne Çıkan')}</span>
                  <span className="kpi-card__value">{summary.featuredProjectsCount}</span>
                </div>
                <div className="kpi-card__icon-wrapper kpi-card__icon-wrapper--red" aria-hidden="true">
                  <Star size={24} />
                </div>
              </div>
            </Card>
          </div>
        </section>

        {/* ─── 2. Distributions (Visualizations) ────────────────────────────── */}
        <section aria-label={t('projects.dashboard.distributionsAria', 'Proje Dağılımları')}>
          <div className="dashboard-grid-2col">
            {/* Durum Dağılımı */}
            <Card as="article">
              <div className="section-header">
                <h2 className="section-header__title">{t('projects.dashboard.statusDistribution', 'Proje Durum Dağılımı')}</h2>
                <Badge variant="navy" size="sm" icon={<Clock size={12} />}>
                  {summary.statusDistribution.length} {t('projects.dashboard.statusCountSuffix', 'Durum')}
                </Badge>
              </div>

              {/* Visual Horizontal Bars */}
              <div className="distribution-list">
                {summary.statusDistribution.map((item) => {
                  const percentage = Math.round((item.count / summary.totalProjects) * 100);
                  const isCompleted = item.code === 'COMPLETED';
                  const isActive = item.code === 'ACTIVE';
                  const isDev = item.code === 'ACTIVE_DEVELOPMENT' || item.code === 'PILOT';

                  let fillClass = 'distribution-bar__fill';
                  if (isActive) fillClass += ' distribution-bar__fill--active';
                  else if (isDev) fillClass += ' distribution-bar__fill--in-progress';
                  else if (isCompleted) fillClass += ' distribution-bar__fill--completed';

                  return (
                    <div key={item.statusId} className="distribution-item">
                      <div className="distribution-item__header">
                        <span className="distribution-item__label">{item.name}</span>
                        <span className="distribution-item__count">
                          {t('projects.dashboard.projectDistributionCount', { count: item.count, percentage })}
                        </span>
                      </div>
                      <div className="distribution-bar" role="progressbar" aria-valuenow={item.count} aria-valuemin={0} aria-valuemax={summary.totalProjects} aria-label={`${item.name}: ${item.count} ${t('projects.projectCountSuffix', 'proje')}`}>
                        <div
                          className={fillClass}
                          style={{ width: `${percentage}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
            </Card>

            {/* Kategori Dağılımı */}
            <Card as="article">
              <div className="section-header">
                <h2 className="section-header__title">{t('projects.dashboard.categoryDistribution', 'Kategori Dağılımı')}</h2>
                <Badge variant="navy" size="sm" icon={<Layers size={12} />}>
                  {summary.categoryDistribution.length} {t('projects.dashboard.categoryCountSuffix', 'Kategori')}
                </Badge>
              </div>

              <div className="distribution-list">
                {summary.categoryDistribution.map((cat) => {
                  const percentage = Math.round((cat.count / summary.totalProjects) * 100);
                  return (
                    <div key={cat.categoryId} className="distribution-item">
                      <div className="distribution-item__header">
                        <span className="distribution-item__label">{cat.name}</span>
                        <span className="distribution-item__count">
                          {t('projects.dashboard.projectDistributionCount', { count: cat.count, percentage })}
                        </span>
                      </div>
                      <div className="distribution-bar" role="progressbar" aria-valuenow={cat.count} aria-valuemin={0} aria-valuemax={summary.totalProjects} aria-label={`${cat.name}: ${cat.count} ${t('projects.projectCountSuffix', 'proje')}`}>
                        <div
                          className="distribution-bar__fill"
                          style={{ width: `${percentage}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
            </Card>
          </div>
        </section>

        {/* ─── 3. Featured Projects ─────────────────────────────────────────── */}
        {summary.featuredProjects.length > 0 && (
          <section aria-label={t('projects.dashboard.featuredProjectsAria', 'Öne Çıkan Projeler')}>
            <div className="section-header">
              <div>
                <h2 className="section-header__title">{t('projects.dashboard.featuredProjectsTitle', 'Öne Çıkan Ar-Ge Projeleri')}</h2>
                <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-muted)', marginTop: 2 }}>
                  {t('projects.dashboard.featuredProjectsSubtitle', 'Stratejik önceliğe sahip ve etkisi yüksek projeler')}
                </p>
              </div>
              <Button variant="ghost" size="sm" onClick={() => navigate('/projects')}>
                {t('projects.dashboard.viewAll', 'Tümünü Gör')}
              </Button>
            </div>

            <div className="featured-grid">
              {summary.featuredProjects.map((proj) => (
                <Card key={proj.id} padding="none" clickable className="featured-card">
                  <div className="featured-card__image-container">
                    <ImageWithFallback
                      src={resolveResourceUrl(proj.coverImageUrl)}
                      alt={`${proj.name} ${t('projects.card.coverImageSuffix', 'kapak görseli')}`}
                      fallbackText={proj.name}
                      className="featured-card__image"
                    />
                  </div>

                  <div className="featured-card__body" style={{ padding: 'var(--space-4)' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 'var(--space-2)' }}>
                      <Badge variant="navy" size="sm">
                        {proj.category.name}
                      </Badge>
                      <Badge variant={getStatusVariant(proj.status.code)} size="sm">
                        {proj.status.name}
                      </Badge>
                    </div>

                    <h3 className="featured-card__title">{proj.name}</h3>
                    <p className="featured-card__desc">{proj.shortDescription}</p>

                    {proj.technologies.length > 0 && (
                      <div className="tech-tag-list" style={{ marginTop: 'var(--space-1)' }}>
                        {proj.technologies.slice(0, 3).map((tech, idx) => (
                          <Badge key={idx} variant="default" size="sm">
                            {tech}
                          </Badge>
                        ))}
                      </div>
                    )}

                    <div className="featured-card__footer">
                      <span style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)' }}>
                        {t('projects.card.projectDetail', 'Proje Detayı')}
                      </span>
                      <Button
                        variant="ghost"
                        size="sm"
                        rightIcon={<ArrowRight size={14} />}
                        onClick={(e) => {
                          e.stopPropagation();
                          navigate('/projects');
                        }}
                      >
                        {t('projects.card.inspect', 'İncele')}
                      </Button>
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          </section>
        )}

        {/* ─── 4. Recent Projects & Top Technologies ───────────────────────── */}
        <div className="dashboard-grid-2col">
          {/* Son Güncellenen Projeler */}
          <section aria-label={t('projects.dashboard.recentProjectsAria', 'Son Güncellenen Projeler')}>
            <Card as="article">
              <div className="section-header">
                <h2 className="section-header__title">{t('projects.dashboard.recentProjectsTitle', 'Son Güncellenen Projeler')}</h2>
                <Link to="/projects" style={{ fontSize: 'var(--font-size-xs)', fontWeight: 'var(--font-weight-medium)', color: 'var(--color-brand-navy-light)' }}>
                  {t('projects.dashboard.all', 'Tümü')}
                </Link>
              </div>

              <div className="recent-list">
                {summary.recentProjects.map((proj) => (
                  <div
                    key={proj.id}
                    className="recent-item"
                    tabIndex={0}
                    role="button"
                    aria-label={`${proj.name} ${t('projects.card.goToDetailSuffix', 'detayına git')}`}
                    onClick={() => navigate('/projects')}
                    onKeyDown={(e) => e.key === 'Enter' && navigate('/projects')}
                  >
                    <div className="recent-item__left">
                      <span className="recent-item__name">{proj.name}</span>
                      <div className="recent-item__meta">
                        <span>{proj.category.name}</span>
                        <span>•</span>
                        <span>
                          {formatDate(proj.updatedAt, i18n.language)}
                        </span>
                      </div>
                    </div>
                    <Badge variant={getStatusVariant(proj.status.code)} size="sm">
                      {proj.status.name}
                    </Badge>
                  </div>
                ))}
              </div>
            </Card>
          </section>

          {/* Öne Çıkan Teknolojiler */}
          <section aria-label={t('projects.dashboard.topTechnologiesAria', 'Öne Çıkan Teknolojiler')}>
            <Card as="article">
              <div className="section-header">
                <h2 className="section-header__title">{t('projects.dashboard.topTechnologiesTitle', 'Öne Çıkan Teknolojiler')}</h2>
                <Badge variant="navy" size="sm" icon={<Cpu size={12} />}>
                  {summary.topTechnologies.length} {t('projects.dashboard.technologyCountSuffix', 'Teknoloji')}
                </Badge>
              </div>

              <div className="top-tech-grid">
                {summary.topTechnologies.map((tech) => (
                  <div key={tech.id} className="top-tech-card">
                    <span className="top-tech-card__name">{tech.name}</span>
                    <span className="top-tech-card__badge">{tech.projectCount} {t('projects.projectCountSuffix', 'Proje')}</span>
                  </div>
                ))}
              </div>
            </Card>
          </section>
        </div>

        {/* ─── 5. Discovery Call to Action ───────────────────────────────────── */}
        <Card elevated style={{ backgroundColor: 'var(--color-surface)', borderLeft: '4px solid var(--color-brand-navy)' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-4)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
              <div className="kpi-card__icon-wrapper kpi-card__icon-wrapper--navy" aria-hidden="true">
                <Sparkles size={24} />
              </div>
              <div>
                <h3 style={{ fontSize: 'var(--font-size-md)', fontWeight: 'var(--font-weight-semibold)', color: 'var(--color-text-primary)' }}>
                  {t('projects.dashboard.ctaTitle', 'Tüm Ar-Ge Projelerini Keşfedin')}
                </h3>
                <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-muted)', marginTop: 2 }}>
                  {t('projects.dashboard.ctaDesc', 'Maden sahalarında geliştirilen yazılım, yapay zeka, IoT ve otomasyon teknolojilerinin tamamını listeleyin.')}
                </p>
              </div>
            </div>

            <Button
              variant="primary"
              rightIcon={<ArrowRight size={16} />}
              onClick={() => navigate('/projects')}
            >
              {t('projects.dashboard.ctaButton', 'Tüm Projeleri İncele')} ({summary.totalProjects})
            </Button>
          </div>
        </Card>

        {/* ─── Dev Health Footer Indicator ──────────────────────────────────── */}
        {health && (
          <footer style={{ marginTop: 'var(--space-4)', display: 'flex', justifyContent: 'flex-end' }}>
            <span style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-disabled)', display: 'inline-flex', alignItems: 'center', gap: 6 }}>
              <span className="health-dot health-dot--ok" style={{ width: 6, height: 6 }} />
              API v{health.version} ({health.status})
            </span>
          </footer>
        )}
      </div>
    </PageLayout>
  );
}

export default DashboardPage;

