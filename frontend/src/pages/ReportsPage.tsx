import {
  Clock,
  Layers,
  TrendingUp,
  Cpu,
  MapPin,
  Users2,
  Boxes,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import PageLayout from '../layouts/PageLayout';
import Card from '../components/ui/Card';
import Badge from '../components/ui/Badge';
import Skeleton from '../components/ui/Skeleton';
import ErrorState from '../components/ui/ErrorState';
import EmptyState from '../components/ui/EmptyState';
import { useReportOverview } from '../hooks/useReports';
import { ReportSummaryStrip } from '../components/reports/ReportSummaryStrip';
import { DonutChart } from '../components/reports/DonutChart';
import { CategoryBarChart } from '../components/reports/CategoryBarChart';
import { TimelineBarChart } from '../components/reports/TimelineBarChart';
import { TechnologyLandscapeChart } from '../components/reports/TechnologyLandscapeChart';
import { LocationDistributionChart } from '../components/reports/LocationDistributionChart';
import { TeamParticipationChart } from '../components/reports/TeamParticipationChart';
import { getStatusColor, getDevTypeColor, type ChartSegment } from '../components/reports/reportColors';
import { useTheme } from '../context/ThemeContext';
import { formatDevelopmentType } from '../utils/formatters';

/**
 * Phase 11.1 — Raporlama ve Portföy İçgörüleri Deneyimi
 * Kurumsal Ar-Ge proje portföyünün derin analitik ve görsel keşif sayfası.
 */
function ReportsPage() {
  const { t } = useTranslation(['reports', 'navigation', 'common', 'projects']);
  const { resolvedTheme } = useTheme();
  const isDark = resolvedTheme === 'dark';
  const { data, isLoading, isError, refetch } = useReportOverview();

  // 1. Error State
  if (isError) {
    return (
      <PageLayout
        title={t('reports.pageTitle', 'Raporlama ve İçgörüler')}
        description={t('reports.pageDescription', 'Demir Export Ar-Ge proje portföyünün analitik dağılımını, teknoloji ekosistemini ve gelişim trendlerini inceleyin.')}
        breadcrumbs={[{ label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' }, { label: t('reports.pageTitle', 'Raporlama ve İçgörüler') }]}
      >
        <Card>
          <ErrorState
            title={t('reports.errorTitle', 'Raporlama Verileri Yüklenemedi')}
            description={t('reports.errorDescription', 'Portföy raporları sunucudan alınırken bir hata oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.')}
            retryLabel={t('common.retry', 'Yeniden Dene')}
            onRetry={() => { void refetch(); }}
          />
        </Card>
      </PageLayout>
    );
  }

  // 2. Loading State (Skeletons)
  if (isLoading || !data) {
    return (
      <PageLayout
        title={t('reports.pageTitle', 'Raporlama ve İçgörüler')}
        description={t('reports.pageDescription', 'Demir Export Ar-Ge proje portföyünün analitik dağılımını, teknoloji ekosistemini ve gelişim trendlerini inceleyin.')}
        breadcrumbs={[{ label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' }, { label: t('reports.pageTitle', 'Raporlama ve İçgörüler') }]}
      >
        <div className="reports-container" aria-busy="true" aria-label={t('reports.loadingAria', 'Raporlama verileri yükleniyor')}>
          {/* Summary Strip Skeleton */}
          <Card>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '4px 0' }}>
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                  <Skeleton variant="circle" width={36} height={36} />
                  <div>
                    <Skeleton width={40} height={20} />
                    <div style={{ marginTop: 4 }}>
                      <Skeleton width={70} height={12} />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>

          {/* 2-Col Donut + Bar Skeleton */}
          <div className="reports-grid-2col">
            <Card>
              <Skeleton width={180} height={20} />
              <div style={{ marginTop: 24, display: 'flex', justifyContent: 'space-around', alignItems: 'center' }}>
                <Skeleton variant="circle" width={160} height={160} />
                <div style={{ display: 'flex', flexDirection: 'column', gap: 12, width: '40%' }}>
                  {[1, 2, 3].map((i) => (
                    <Skeleton key={i} width="100%" height={24} />
                  ))}
                </div>
              </div>
            </Card>
            <Card>
              <Skeleton width={180} height={20} />
              <div style={{ marginTop: 24, display: 'flex', flexDirection: 'column', gap: 14 }}>
                {[1, 2, 3, 4].map((i) => (
                  <Skeleton key={i} width="100%" height={26} />
                ))}
              </div>
            </Card>
          </div>

          {/* Timeline Skeleton */}
          <Card>
            <Skeleton width={200} height={20} />
            <div style={{ marginTop: 20 }}>
              <Skeleton width="100%" height={200} />
            </div>
          </Card>
        </div>
      </PageLayout>
    );
  }

  const { summary, statusDistribution, categoryDistribution, developmentTypeDistribution, topTechnologies, locationDistribution, teamDistribution, projectTimeline } = data;

  // 3. Empty State (Yayınlanmış hiç proje yoksa)
  if (summary.totalProjects === 0) {
    return (
      <PageLayout
        title={t('reports.pageTitle', 'Raporlama ve İçgörüler')}
        description={t('reports.pageDescription', 'Demir Export Ar-Ge proje portföyünün analitik dağılımını, teknoloji ekosistemini ve gelişim trendlerini inceleyin.')}
        breadcrumbs={[{ label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' }, { label: t('reports.pageTitle', 'Raporlama ve İçgörüler') }]}
      >
        <Card>
          <EmptyState
            title={t('reports.noProjectsTitle', 'Raporlanacak Proje Bulunmuyor')}
            description={t('reports.noProjectsDesc', 'Portföyde henüz yayınlanmış bir proje bulunmadığı için raporlama metrikleri oluşturulamadı.')}
          />
        </Card>
      </PageLayout>
    );
  }

  // ─── Status Donut Segments (Deterministic Colors) ─────────────────────────
  const statusSegments: ChartSegment[] = statusDistribution.map((item) => ({
    id: item.statusId,
    label: item.name,
    value: item.count,
    percentage: item.percentage,
    color: getStatusColor(item.code, isDark),
  }));

  // ─── Development Type Donut Segments (Deterministic Colors) ───────────────
  const devTypeSegments: ChartSegment[] = developmentTypeDistribution.map((item) => ({
    id: item.developmentType,
    label: formatDevelopmentType(item.developmentType, t),
    value: item.count,
    percentage: item.percentage,
    color: getDevTypeColor(item.developmentType, isDark),
  }));

  return (
    <PageLayout
      title={t('reports.pageTitle', 'Raporlama ve İçgörüler')}
      description={t('reports.pageDescription', 'Demir Export Ar-Ge proje portföyünün analitik dağılımını, teknoloji ekosistemini ve gelişim trendlerini inceleyin.')}
      breadcrumbs={[{ label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' }, { label: t('reports.pageTitle', 'Raporlama ve İçgörüler') }]}
    >
      <div className="reports-container">
        {/* ─── 1. COMPACT PORTFOLIO SUMMARY STRIP ───────────────────────────── */}
        <section aria-label={t('reports.portfolioSummaryAria', 'Portföy Özet Şeridi')}>
          <ReportSummaryStrip summary={summary} />
        </section>

        {/* ─── 2. PRIMARY VISUAL SECTION: STATUS DONUT & CATEGORY BARS ───────── */}
        <section aria-label={t('reports.statusAndCategoryAnalysisAria', 'Portföy Durum ve Kategori Analizi')}>
          <div className="reports-grid-2col">
            {/* Status Donut Chart */}
            <Card as="article">
              <div className="report-card-header">
                <div className="report-card-header__left">
                  <h2 className="report-card-header__title">{t('reports.sections.statusDistributionTitle', 'Portföy Durum Dağılımı')}</h2>
                  <span className="report-card-header__subtitle">
                    {t('reports.sections.statusDistributionSubtitle', 'Yayınlanmış projelerin yaşam döngüsü aşamaları')}
                  </span>
                </div>
                <Badge variant="navy" size="sm" icon={<Clock size={12} />}>
                  {statusDistribution.length} {t('reports.sections.statusStageSuffix', 'Aşama')}
                </Badge>
              </div>

              <DonutChart
                segments={statusSegments}
                totalCount={summary.totalProjects}
                centerLabel={t('projects.projectCountSuffix', 'Proje')}
                title={t('reports.sections.statusDistributionTitle', 'Proje Durum Dağılımı')}
              />
            </Card>

            {/* Category Comparison Bars */}
            <Card as="article">
              <div className="report-card-header">
                <div className="report-card-header__left">
                  <h2 className="report-card-header__title">{t('reports.sections.categoryDistributionTitle', 'Kategori Dağılımı')}</h2>
                  <span className="report-card-header__subtitle">
                    {t('reports.sections.categoryDistributionSubtitle', 'Tematik alanlara göre proje yoğunlaşması')}
                  </span>
                </div>
                <Badge variant="navy" size="sm" icon={<Layers size={12} />}>
                  {categoryDistribution.length} {t('reports.sections.categorySuffix', 'Kategori')}
                </Badge>
              </div>

              <CategoryBarChart
                categories={categoryDistribution}
              />
            </Card>
          </div>
        </section>

        {/* ─── 3. PROJECT TIMELINE ANALYTICS ────────────────────────────────── */}
        <section aria-label={t('reports.sections.timelineAria', 'Proje Portföyü Zaman Çizgisi')}>
          <Card as="article">
            <div className="report-card-header">
              <div className="report-card-header__left">
                <h2 className="report-card-header__title">{t('reports.sections.timelineTitle', 'Proje Zaman Çizgisi')}</h2>
                <span className="report-card-header__subtitle">
                  {t('reports.sections.timelineSubtitle', 'Başlangıç tarihlerine göre projelerin aylık sisteme katılma ve başlama dağılımı')}
                </span>
              </div>
              <Badge variant="navy" size="sm" icon={<TrendingUp size={12} />}>
                {projectTimeline.length} {t('reports.sections.periodSuffix', 'Dönem')}
              </Badge>
            </div>

            <TimelineBarChart timeline={projectTimeline} />
          </Card>
        </section>

        {/* ─── 4. TECHNOLOGY LANDSCAPE & LOCATION DISTRIBUTION ──────────────── */}
        <section aria-label={t('reports.sections.techAndLocationAria', 'Teknoloji Ekosistemi ve Lokasyon Dağılımı')}>
          <div className="reports-grid-2col">
            {/* Technology Ecosystem */}
            <Card as="article">
              <div className="report-card-header">
                <div className="report-card-header__left">
                  <h2 className="report-card-header__title">{t('reports.sections.technologyLandscapeTitle', 'Teknoloji Ekosistemi')}</h2>
                  <span className="report-card-header__subtitle">
                    {t('reports.sections.technologyLandscapeSubtitle', 'Portföyde en yaygın kullanılan yazılım ve donanım teknolojileri')}
                  </span>
                </div>
                <Badge variant="navy" size="sm" icon={<Cpu size={12} />}>
                  Top {topTechnologies.length}
                </Badge>
              </div>

              <TechnologyLandscapeChart
                technologies={topTechnologies}
                totalProjects={summary.totalProjects}
              />
            </Card>

            {/* Location Distribution */}
            <Card as="article">
              <div className="report-card-header">
                <div className="report-card-header__left">
                  <h2 className="report-card-header__title">{t('reports.sections.locationDistributionTitle', 'Proje Lokasyonları')}</h2>
                  <span className="report-card-header__subtitle">
                    {t('reports.sections.locationDistributionSubtitle', 'Maden sahaları, işletmeler ve merkez birimlerdeki uygulama dağılımı')}
                  </span>
                </div>
                <Badge variant="navy" size="sm" icon={<MapPin size={12} />}>
                  {locationDistribution.length} {t('reports.sections.locationSuffix', 'Lokasyon')}
                </Badge>
              </div>

              <LocationDistributionChart
                locations={locationDistribution}
                totalProjects={summary.totalProjects}
              />
            </Card>
          </div>
        </section>

        {/* ─── 5. TEAM PARTICIPATION & DEVELOPMENT MODEL ────────────────────── */}
        <section aria-label={t('reports.sections.teamAndModelAria', 'Ekip Katılımı ve Geliştirme Modeli')}>
          <div className="reports-grid-2col">
            {/* Team Participation */}
            <Card as="article">
              <div className="report-card-header">
                <div className="report-card-header__left">
                  <h2 className="report-card-header__title">{t('reports.sections.teamParticipationTitle', 'Projelerde Görev Alan Ekipler')}</h2>
                  <span className="report-card-header__subtitle">
                    {t('reports.sections.teamParticipationSubtitle', 'Portföy projelerinde görev ve sorumluluk alan departman ekipleri')}
                  </span>
                </div>
                <Badge variant="navy" size="sm" icon={<Users2 size={12} />}>
                  {teamDistribution.length} {t('reports.sections.teamSuffix', 'Ekip')}
                </Badge>
              </div>

              <TeamParticipationChart
                teams={teamDistribution}
                totalProjects={summary.totalProjects}
              />
            </Card>

            {/* Development Model Donut Chart */}
            <Card as="article">
              <div className="report-card-header">
                <div className="report-card-header__left">
                  <h2 className="report-card-header__title">{t('reports.sections.developmentModelTitle', 'Geliştirme Modeli')}</h2>
                  <span className="report-card-header__subtitle">
                    {t('reports.sections.developmentModelSubtitle', 'Kurum içi, dış kaynak ve hibrit geliştirme kaynak oranı')}
                  </span>
                </div>
                <Badge variant="navy" size="sm" icon={<Boxes size={12} />}>
                  {developmentTypeDistribution.length} {t('reports.sections.modelSuffix', 'Model')}
                </Badge>
              </div>

              <DonutChart
                segments={devTypeSegments}
                totalCount={summary.totalProjects}
                centerLabel={t('projects.projectCountSuffix', 'Proje')}
                title={t('reports.sections.developmentModelTitle', 'Geliştirme Modeli Dağılımı')}
              />
            </Card>
          </div>
        </section>
      </div>
    </PageLayout>
  );
}

export default ReportsPage;
