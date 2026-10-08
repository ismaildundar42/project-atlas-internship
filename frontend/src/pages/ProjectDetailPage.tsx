import { useMemo, useState, useRef } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  AlertTriangle,
  ArrowLeft,
  Building2,
  Calendar,
  Clock,
  Code,
  ExternalLink,
  FileText,
  FolderKanban,
  Globe,
  HardHat,
  HelpCircle,
  Layers,
  Lightbulb,
  Mail,
  MapPin,
  ShieldCheck,
  Star,
  Target,
  Users,
  Wrench,
} from 'lucide-react';
import { useProjectBySlug } from '../hooks/useProjects';
import PageLayout from '../layouts/PageLayout';
import Badge from '../components/ui/Badge';
import Button from '../components/ui/Button';
import Card from '../components/ui/Card';
import ErrorState from '../components/ui/ErrorState';
import { ImageWithFallback } from '../components/ui/ImageWithFallback';
import Skeleton from '../components/ui/Skeleton';
import { formatDate, formatDevelopmentType, getStatusVariant } from '../utils/formatters';
import { resolveResourceUrl, normalizeExternalUrl } from '../utils/urlUtils';
import { ImageLightbox } from '../components/common/ImageLightbox';
import ProjectAiSummaryCard from '../components/projects/ProjectAiSummaryCard';

export function ProjectDetailPage() {
  const { t, i18n } = useTranslation(['projects', 'navigation', 'common']);
  const { slug } = useParams<{ slug: string }>();
  const { data: project, isLoading, isError, error, refetch } = useProjectBySlug(slug || '');

  const [lightboxIndex, setLightboxIndex] = useState<number>(0);
  const [isLightboxOpen, setIsLightboxOpen] = useState<boolean>(false);
  const triggerRef = useRef<HTMLElement | null>(null);

  // Group technologies by category
  const groupedTechnologies = useMemo(() => {
    if (!project?.technologies) return {};
    return project.technologies.reduce((acc, tech) => {
      const cat = tech.category || t('projects.detail.otherCategory', 'Diğer');
      if (!acc[cat]) acc[cat] = [];
      acc[cat].push(tech);
      return acc;
    }, {} as Record<string, typeof project.technologies>);
  }, [project?.technologies, t]);

  const isNotFound = isError && (error as any)?.response?.status === 404;

  if (isLoading) {
    return (
      <PageLayout
        title={t('projects.detail.pageTitleLoading', 'Proje Detayı')}
        description={t('projects.detail.loadingDescription', 'Proje bilgileri yükleniyor...')}
        breadcrumbs={[
          { label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' },
          { label: t('projects.library.title', 'Proje Kütüphanesi'), href: '/projects' },
          { label: t('common.loading', 'Yükleniyor...') },
        ]}
      >
        <div className="project-detail">
          <Card padding="lg">
            <div style={{ marginBottom: '16px' }}>
              <Skeleton variant="text" width="40%" height={32} />
            </div>
            <div style={{ marginBottom: '24px' }}>
              <Skeleton variant="text" width="80%" height={20} />
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px' }}>
              <Skeleton variant="rect" height={60} />
              <Skeleton variant="rect" height={60} />
              <Skeleton variant="rect" height={60} />
              <Skeleton variant="rect" height={60} />
            </div>
          </Card>
          <div className="project-detail__body-grid">
            <div className="project-detail__main-column">
              <Card padding="lg">
                <div style={{ marginBottom: '16px' }}>
                  <Skeleton variant="text" width="30%" height={24} />
                </div>
                <Skeleton variant="rect" height={180} />
              </Card>
            </div>
            <div className="project-detail__sidebar-column">
              <Card padding="lg">
                <div style={{ marginBottom: '12px' }}>
                  <Skeleton variant="text" width="50%" height={20} />
                </div>
                <Skeleton variant="rect" height={120} />
              </Card>
            </div>
          </div>
        </div>
      </PageLayout>
    );
  }

  if (isNotFound || (!isLoading && !isError && !project)) {
    return (
      <PageLayout
        title={t('projects.detail.notFoundTitle', 'Proje Bulunamadı')}
        description={t('projects.detail.notFoundDescription', 'İstenen proje bulunamadı veya yayından kaldırılmış olabilir.')}
        breadcrumbs={[
          { label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' },
          { label: t('projects.library.title', 'Proje Kütüphanesi'), href: '/projects' },
          { label: t('projects.detail.notFoundBreadcrumb', 'Bulunamadı') },
        ]}
      >
        <ErrorState
          title={t('projects.detail.notFoundTitle', 'Proje Bulunamadı')}
          description={t('projects.detail.notFoundDetail', { slug: slug || '' })}
        />
        <div style={{ marginTop: 'var(--space-4)' }}>
          <Link to="/projects">
            <Button variant="secondary" size="md">
              <ArrowLeft size={16} aria-hidden="true" /> {t('projects.detail.backToLibrary', 'Proje Kütüphanesine Dön')}
            </Button>
          </Link>
        </div>
      </PageLayout>
    );
  }

  if (isError) {
    return (
      <PageLayout
        title={t('projects.detail.errorTitle', 'Proje Yüklenemedi')}
        description={t('projects.detail.errorDescription', 'Proje bilgileri sunucudan çekilirken hata oluştu.')}
        breadcrumbs={[
          { label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' },
          { label: t('projects.library.title', 'Proje Kütüphanesi'), href: '/projects' },
          { label: t('common.error', 'Hata') },
        ]}
      >
        <ErrorState
          title={t('projects.detail.errorTitle', 'Proje Detayı Yüklenemedi')}
          description={t('projects.detail.errorSubtitle', 'Sunucu bağlantısında geçici bir hata oluştu. Lütfen tekrar deneyiniz.')}
          onRetry={() => refetch()}
        />
        <div style={{ marginTop: 'var(--space-4)' }}>
          <Link to="/projects">
            <Button variant="secondary" size="md">
              <ArrowLeft size={16} aria-hidden="true" /> {t('projects.detail.backToLibrary', 'Proje Kütüphanesine Dön')}
            </Button>
          </Link>
        </div>
      </PageLayout>
    );
  }

  if (!project) return null;

  const primaryTeam = project.teams?.find((t) => t.isPrimary) || project.teams?.[0];
  const primaryLocation = project.locations?.[0];
  const supportContact = project.members?.find(
    (m) =>
      m.projectRole?.toLowerCase().includes('destek') ||
      m.projectRole?.toLowerCase().includes('lider') ||
      m.projectRole?.toLowerCase().includes('yönetici')
  ) || project.members?.[0];

  return (
    <PageLayout
      title={project.name}
      description={project.shortDescription}
      breadcrumbs={[
        { label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' },
        { label: t('projects.library.title', 'Proje Kütüphanesi'), href: '/projects' },
        { label: project.name },
      ]}
    >
      <div className="project-detail">
        {/* A. HERO / SUMMARY BANNER */}
        <section className="project-detail__hero" aria-label={t('projects.detail.heroAria', 'Proje Özeti ve Aksiyonlar')}>
          <div className="project-detail__hero-nav">
            <Link to="/projects" className="project-detail__back-link">
              <ArrowLeft size={16} aria-hidden="true" />
              <span>{t('projects.detail.backToLibrary', 'Proje Kütüphanesine Dön')}</span>
            </Link>

            <div className="project-detail__hero-actions">
              {project.repositoryUrl && (
                <a
                  href={normalizeExternalUrl(project.repositoryUrl)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn--secondary btn--sm"
                  aria-label={`${project.name} ${t('projects.detail.sourceCodeAriaSuffix', 'projesinin kaynak kodunu inceleyin')}`}
                >
                  <Code size={16} aria-hidden="true" />
                  <span>{t('projects.detail.sourceCode', 'Kaynak Kod')}</span>
                </a>
              )}
              {project.applicationUrl && (
                <a
                  href={normalizeExternalUrl(project.applicationUrl)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn--primary btn--md"
                  aria-label={`${project.name} ${t('projects.detail.launchAppAriaSuffix', 'canlı uygulamasını başlatın')}`}
                >
                  <ExternalLink size={18} aria-hidden="true" />
                  <span>{t('projects.detail.openApplication', 'Uygulamayı Aç')}</span>
                </a>
              )}
            </div>
          </div>

          <div className="project-detail__hero-header">
            <div className="project-detail__badges-bar">
              <Badge variant={getStatusVariant(project.status.code)} size="md">
                {project.status.name}
              </Badge>
              <Badge variant="default" size="md">
                {project.category.name}
              </Badge>
              <span className="project-card__dev-type" style={{ fontWeight: 600 }}>
                {formatDevelopmentType(project.developmentType, t)}
              </span>
              {project.isFeatured && (
                <span className="project-card__featured-pill">
                  <Star size={12} fill="currentColor" aria-hidden="true" />
                  <span>{t('projects.detail.featuredBadge', 'Öne Çıkan Ar-Ge Projesi')}</span>
                </span>
              )}
            </div>

            <h1 className="project-detail__title">{project.name}</h1>
            <p className="project-detail__short-desc">{project.shortDescription}</p>
          </div>

          {/* B. QUICK FACTS STRIP */}
          <div className="project-detail__quick-facts" aria-label={t('projects.detail.quickFactsAria', 'Hızlı Proje Bilgileri')}>
            <div className="project-detail__fact-item">
              <span className="project-detail__fact-label">{t('projects.filters.status', 'Durum')}</span>
              <span className="project-detail__fact-value">{project.status.name}</span>
            </div>
            <div className="project-detail__fact-item">
              <span className="project-detail__fact-label">{t('projects.filters.category', 'Kategori')}</span>
              <span className="project-detail__fact-value">{project.category.name}</span>
            </div>
            <div className="project-detail__fact-item">
              <span className="project-detail__fact-label">{t('projects.filters.developmentType', 'Geliştirme Tipi')}</span>
              <span className="project-detail__fact-value">{formatDevelopmentType(project.developmentType, t)}</span>
            </div>
            {primaryTeam && (
              <div className="project-detail__fact-item">
                <span className="project-detail__fact-label">{t('projects.card.primaryTeam', 'Sorumlu Ekip')}</span>
                <span className="project-detail__fact-value">
                  <Building2 size={14} aria-hidden="true" />
                  <span>{primaryTeam.name}</span>
                </span>
              </div>
            )}
            {primaryLocation && (
              <div className="project-detail__fact-item">
                <span className="project-detail__fact-label">{t('projects.detail.primaryLocation', 'Ana Saha / Lokasyon')}</span>
                <span className="project-detail__fact-value">
                  <MapPin size={14} aria-hidden="true" />
                  <span>{primaryLocation.name}</span>
                </span>
              </div>
            )}
            {project.startDate && (
              <div className="project-detail__fact-item">
                <span className="project-detail__fact-label">{t('projects.detail.startDate', 'Başlangıç Tarihi')}</span>
                <span className="project-detail__fact-value">
                  <Calendar size={14} aria-hidden="true" />
                  <span>{formatDate(project.startDate, i18n.language)}</span>
                </span>
              </div>
            )}
          </div>
        </section>

        {/* BODY LAYOUT GRID */}
        <div className="project-detail__body-grid">
          {/* MAIN COLUMN (8 cols) */}
          <div className="project-detail__main-column">
            {/* AI PROJECT SUMMARY CARD (Phase 20) */}
            <ProjectAiSummaryCard projectId={project.id} projectName={project.name} />

            {/* C. PROJECT OVERVIEW & VALUE */}
            <section className="project-detail__section" aria-labelledby="section-overview-title">
              <h2 id="section-overview-title" className="project-detail__section-title">
                <FolderKanban size={22} className="project-detail__section-title-icon" aria-hidden="true" />
                <span>{t('projects.detail.overviewAndImpact', 'Genel Bakış ve İş Etkisi')}</span>
              </h2>

              {project.purpose && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">
                    <Target size={18} className="project-detail__subblock-icon" style={{ color: 'var(--color-brand-red)' }} aria-hidden="true" />
                    <span>{t('projects.detail.projectPurpose', 'Projenin Amacı')}</span>
                  </h3>
                  <p className="project-detail__subblock-text">{project.purpose}</p>
                </div>
              )}

              {project.problemSolved && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">
                    <AlertTriangle size={18} className="project-detail__subblock-icon" style={{ color: '#d97706' }} aria-hidden="true" />
                    <span>{t('projects.detail.problemSolved', 'Çözülen Problem')}</span>
                  </h3>
                  <p className="project-detail__subblock-text">{project.problemSolved}</p>
                </div>
              )}

              {project.nonTechnicalDescription && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">
                    <Lightbulb size={18} className="project-detail__subblock-icon" style={{ color: '#2563eb' }} aria-hidden="true" />
                    <span>{t('projects.detail.solutionOffered', 'Nasıl Bir Çözüm Sunuyor?')}</span>
                  </h3>
                  <p className="project-detail__subblock-text">{project.nonTechnicalDescription}</p>
                </div>
              )}

              {project.businessImpact && (
                <div className="project-detail__highlight-box project-detail__highlight-box--red">
                  <h3 className="project-detail__subblock-title" style={{ color: 'var(--color-brand-red)' }}>
                    <ShieldCheck size={18} aria-hidden="true" /> {t('projects.detail.businessImpactAndGain', 'Sağladığı İş Kazancı ve Değer')}
                  </h3>
                  <p className="project-detail__subblock-text">{project.businessImpact}</p>
                </div>
              )}

              {project.targetAudience && (
                <div className="project-detail__highlight-box">
                  <h3 className="project-detail__subblock-title">
                    <Users size={18} aria-hidden="true" /> {t('projects.detail.targetAudience', 'Hedef Kullanıcı Kitlesi')}
                  </h3>
                  <p className="project-detail__subblock-text">{project.targetAudience}</p>
                </div>
              )}
            </section>

            {/* D. FIELD & OPERATIONAL INFORMATION */}
            <section className="project-detail__section" aria-labelledby="section-field-title">
              <h2 id="section-field-title" className="project-detail__section-title">
                <HardHat size={22} className="project-detail__section-title-icon" aria-hidden="true" />
                <span>{t('projects.detail.fieldAndUsageInfo', 'Saha ve Kullanım Bilgileri')}</span>
              </h2>

              {project.accessInstructions && (
                <div className="project-detail__field-box">
                  <Wrench size={20} className="project-detail__field-box-icon" aria-hidden="true" />
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                    <h3 className="project-detail__subblock-title" style={{ color: 'var(--color-info)' }}>
                      {t('projects.detail.accessInstructions', 'Erişim ve Kullanım Talimatları')}
                    </h3>
                    <p className="project-detail__subblock-text" style={{ fontSize: 'var(--font-size-sm)' }}>
                      {project.accessInstructions}
                    </p>
                  </div>
                </div>
              )}

              {project.locations && project.locations.length > 0 && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">{t('projects.detail.miningSitesAndFacilities', 'Kullanıldığı Maden Sahaları & Tesisler')}</h3>
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginTop: '4px' }}>
                    {project.locations.map((loc) => (
                      <span key={loc.id} className="project-detail__location-badge">
                        <MapPin size={12} style={{ display: 'inline', marginRight: '4px' }} />
                        {loc.name} {loc.type ? `(${loc.type})` : ''}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {supportContact && (
                <div className="project-detail__subblock" style={{ marginTop: '8px' }}>
                  <h3 className="project-detail__subblock-title">
                    <HelpCircle size={16} aria-hidden="true" /> {t('projects.detail.supportAndContact', 'Operasyonel Destek ve İletişim')}
                  </h3>
                  <p className="project-detail__subblock-text" style={{ fontSize: 'var(--font-size-sm)' }}>
                    {t('projects.detail.supportContactDesc', 'Bu sistemle ilgili saha sorunları, erişim talepleri veya teknik destek için sorumlu kontak:')}
                  </p>
                  <div className="project-detail__member-card" style={{ maxWidth: '380px', marginTop: '6px' }}>
                    <div className="project-detail__member-avatar">
                      {supportContact.firstName[0]}
                      {supportContact.lastName[0]}
                    </div>
                    <div className="project-detail__member-info">
                      <span className="project-detail__member-name">{supportContact.fullName}</span>
                      <span className="project-detail__member-role">{supportContact.projectRole || supportContact.title || t('projects.detail.supportLead', 'Destek Sorumlusu')}</span>
                      {supportContact.email && (
                        <a href={`mailto:${supportContact.email}`} className="project-detail__member-email">
                          <Mail size={12} style={{ display: 'inline', marginRight: '4px' }} />
                          {supportContact.email}
                        </a>
                      )}
                    </div>
                  </div>
                </div>
              )}
            </section>

            {/* F. TECHNICAL INFORMATION */}
            <section className="project-detail__section" aria-labelledby="section-tech-title">
              <h2 id="section-tech-title" className="project-detail__section-title">
                <Code size={22} className="project-detail__section-title-icon" aria-hidden="true" />
                <span>{t('projects.detail.techDetailsAndArchitecture', 'Teknik Detaylar ve Mimari')}</span>
              </h2>

              {project.technicalDescription && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">{t('projects.detail.architectureAndSystemDesign', 'Mimari ve Sistem Yapısı')}</h3>
                  <p className="project-detail__subblock-text">{project.technicalDescription}</p>
                </div>
              )}

              {Object.keys(groupedTechnologies).length > 0 && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">{t('projects.detail.technologiesUsed', 'Kullanılan Teknolojiler')}</h3>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', marginTop: '6px' }}>
                    {Object.entries(groupedTechnologies).map(([cat, techs]) => (
                      <div key={cat} className="project-detail__tech-group">
                        <span className="project-detail__tech-group-title">{cat}</span>
                        <div className="project-detail__tech-tags">
                          {techs.map((tItem) => (
                            <span key={tItem.id} className="project-detail__tech-badge">
                              {tItem.name}
                            </span>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {project.tags && project.tags.length > 0 && (
                <div className="project-detail__subblock">
                  <h3 className="project-detail__subblock-title">{t('projects.detail.tags', 'Etiketler')}</h3>
                  <div className="project-detail__tech-tags">
                    {project.tags.map((tag) => (
                      <span key={tag.id} className="project-filters__chip" style={{ fontSize: '11px' }}>
                        #{tag.name}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </section>

            {/* G. INTEGRATIONS */}
            {project.integrations && project.integrations.length > 0 && (
              <section className="project-detail__section" aria-labelledby="section-integrations-title">
                <h2 id="section-integrations-title" className="project-detail__section-title">
                  <Layers size={22} className="project-detail__section-title-icon" aria-hidden="true" />
                  <span>{t('projects.detail.integrations', 'Sistem Entegrasyonları')}</span>
                </h2>

                <div className="project-detail__integrations-list">
                  {project.integrations.map((item) => (
                    <div key={item.id} className="project-detail__integration-card">
                      <div className="project-detail__integration-header">
                        <span className="project-detail__integration-name">{item.name}</span>
                        <Badge variant="default" size="sm">
                          {item.integrationType}
                        </Badge>
                      </div>
                      {item.description && (
                        <p className="project-detail__subblock-text" style={{ fontSize: 'var(--font-size-sm)' }}>
                          {item.description}
                        </p>
                      )}
                    </div>
                  ))}
                </div>
              </section>
            )}

            {/* I. DOCUMENTS */}
            {project.documents && project.documents.length > 0 && (
              <section className="project-detail__section" aria-labelledby="section-docs-title">
                <h2 id="section-docs-title" className="project-detail__section-title">
                  <FileText size={22} className="project-detail__section-title-icon" aria-hidden="true" />
                  <span>{t('projects.detail.documentsAndManuals', 'Dokümanlar ve Kullanım Kılavuzları')}</span>
                </h2>

                <div className="project-detail__docs-grid">
                  {project.documents.map((doc) => (
                    <div key={doc.id} className="project-detail__doc-card">
                      <FileText size={24} className="project-detail__doc-icon" aria-hidden="true" />
                      <div className="project-detail__doc-content">
                        <a
                          href={resolveResourceUrl(doc.fileUrl)}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="project-detail__doc-title"
                        >
                          {doc.name}
                        </a>
                        {doc.documentType && (
                          <span className="project-detail__tech-badge" style={{ width: 'fit-content', fontSize: '10px' }}>
                            {doc.documentType}
                          </span>
                        )}
                        {doc.description && <span className="project-detail__doc-desc">{doc.description}</span>}
                      </div>
                    </div>
                  ))}
                </div>
              </section>
            )}

            {/* J. MEDIA GALLERY */}
            {project.media && project.media.length > 0 && (
              <section className="project-detail__section" aria-labelledby="section-media-title">
                <h2 id="section-media-title" className="project-detail__section-title">
                  <Globe size={22} className="project-detail__section-title-icon" aria-hidden="true" />
                  <span>{t('projects.detail.mediaGallery', 'Saha Görselleri ve Ekran Alıntıları')}</span>
                </h2>

                <div className="project-detail__media-grid">
                  {project.media.map((item, index) => (
                    <div
                      key={item.id}
                      className="project-detail__media-card"
                      style={{ cursor: 'pointer' }}
                      tabIndex={0}
                      role="button"
                      aria-label={`${item.caption || item.altText || item.fileName || t('projects.detail.mediaImage', 'Görsel')} ${t('projects.detail.openLightboxAriaSuffix', 'büyük boyutta aç')}`}
                      onClick={(e) => {
                        triggerRef.current = e.currentTarget;
                        setLightboxIndex(index);
                        setIsLightboxOpen(true);
                      }}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                          e.preventDefault();
                          triggerRef.current = e.currentTarget;
                          setLightboxIndex(index);
                          setIsLightboxOpen(true);
                        }
                      }}
                    >
                      <div className="project-detail__media-frame">
                        <ImageWithFallback
                          src={resolveResourceUrl(item.fileUrl)}
                          alt={item.altText || item.caption || project.name}
                          style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                        />
                      </div>
                      {item.caption && <div className="project-detail__media-caption">{item.caption}</div>}
                    </div>
                  ))}
                </div>
              </section>
            )}
          </div>

          {/* SIDEBAR COLUMN (4 cols) */}
          <div className="project-detail__sidebar-column">
            {/* E. RESPONSIBLE PEOPLE & TEAMS */}
            <section className="project-detail__section" aria-labelledby="sidebar-teams-title">
              <h2 id="sidebar-teams-title" className="project-detail__section-title" style={{ fontSize: 'var(--font-size-lg)' }}>
                <Users size={18} className="project-detail__section-title-icon" aria-hidden="true" />
                <span>{t('projects.detail.responsibleTeamsAndPeople', 'Sorumlu Ekipler ve Kişiler')}</span>
              </h2>

              {project.teams && project.teams.length > 0 && (
                <div className="project-detail__subblock">
                  <span className="project-detail__tech-group-title">{t('projects.detail.responsibleTeams', 'Sorumlu Ekipler')}</span>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    {project.teams.map((tTeam) => (
                      <div key={tTeam.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <span style={{ fontSize: 'var(--font-size-sm)', fontWeight: 600 }}>{tTeam.name}</span>
                        {tTeam.isPrimary && (
                          <Badge variant="navy" size="sm">
                            {t('projects.detail.primaryTeamBadge', 'Ana Ekip')}
                          </Badge>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {project.members && project.members.length > 0 && (
                <div className="project-detail__subblock" style={{ marginTop: '8px' }}>
                  <span className="project-detail__tech-group-title">{t('projects.detail.teamMembersAndRoles', 'Ekip Üyeleri & Roller')}</span>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    {project.members.map((m) => (
                      <div key={m.id} className="project-detail__member-card">
                        <div className="project-detail__member-avatar">
                          {m.firstName[0]}
                          {m.lastName[0]}
                        </div>
                        <div className="project-detail__member-info">
                          <span className="project-detail__member-name">{m.fullName}</span>
                          {m.projectRole && <span className="project-detail__member-role">{m.projectRole}</span>}
                          {m.title && !m.projectRole && <span className="project-detail__member-role">{m.title}</span>}
                          {m.email && (
                            <a href={`mailto:${m.email}`} className="project-detail__member-email">
                              <Mail size={12} style={{ display: 'inline', marginRight: '4px' }} />
                              {m.email}
                            </a>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </section>

            {/* H. LOCATIONS LIST */}
            {project.locations && project.locations.length > 0 && (
              <section className="project-detail__section" aria-labelledby="sidebar-locations-title">
                <h2 id="sidebar-locations-title" className="project-detail__section-title" style={{ fontSize: 'var(--font-size-lg)' }}>
                  <MapPin size={18} className="project-detail__section-title-icon" aria-hidden="true" />
                  <span>{t('projects.detail.usageLocations', 'Kullanım Sahaları')}</span>
                </h2>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                  {project.locations.map((loc) => (
                    <div key={loc.id} style={{ padding: '8px 12px', backgroundColor: 'var(--color-surface-subtle)', borderRadius: 'var(--radius-md)', border: '1px solid var(--color-border-subtle)' }}>
                      <div style={{ fontSize: 'var(--font-size-sm)', fontWeight: 600, color: 'var(--color-text-primary)' }}>{loc.name}</div>
                      {loc.description && <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', marginTop: '2px' }}>{loc.description}</div>}
                    </div>
                  ))}
                </div>
              </section>
            )}

            {/* K. METADATA & TIMELINE */}
            <section className="project-detail__section" aria-labelledby="sidebar-meta-title">
              <h2 id="sidebar-meta-title" className="project-detail__section-title" style={{ fontSize: 'var(--font-size-lg)' }}>
                <Clock size={18} className="project-detail__section-title-icon" aria-hidden="true" />
                <span>{t('projects.detail.timelineAndMeta', 'Zaman Çizelgesi & Künye')}</span>
              </h2>

              <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)' }}>
                {project.startDate && (
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <span>{t('projects.detail.startDateLabel', 'Başlangıç:')}</span>
                    <strong style={{ color: 'var(--color-text-primary)' }}>{formatDate(project.startDate, i18n.language)}</strong>
                  </div>
                )}
                {project.endDate && (
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <span>{t('projects.detail.endDateLabel', 'Bitiş:')}</span>
                    <strong style={{ color: 'var(--color-text-primary)' }}>{formatDate(project.endDate, i18n.language)}</strong>
                  </div>
                )}
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span>{t('projects.detail.createdAtLabel', 'Sisteme Eklendi:')}</span>
                  <strong>{formatDate(project.createdAt, i18n.language)}</strong>
                </div>
                {project.updatedAt && (
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <span>{t('projects.detail.updatedAtLabel', 'Son Güncelleme:')}</span>
                    <strong>{formatDate(project.updatedAt, i18n.language)}</strong>
                  </div>
                )}
              </div>
            </section>
          </div>
        </div>
      </div>

      <ImageLightbox
        images={project.media || []}
        currentIndex={lightboxIndex}
        isOpen={isLightboxOpen}
        onClose={() => setIsLightboxOpen(false)}
        onNavigate={(idx) => setLightboxIndex(idx)}
        triggerElementRef={triggerRef}
      />
    </PageLayout>
  );
}

export default ProjectDetailPage;
