import { useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { FolderKanban, SearchX } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { ProjectCard } from '../components/projects/ProjectCard';
import { ProjectFilters } from '../components/projects/ProjectFilters';
import { ProjectGridSkeleton } from '../components/projects/ProjectGridSkeleton';
import { ProjectPagination } from '../components/projects/ProjectPagination';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import {
  useLocations,
  useProjectCategories,
  useProjectStatuses,
  useTeams,
  useTechnologies,
} from '../hooks/useLookups';
import { useProjects } from '../hooks/useProjects';
import PageLayout from '../layouts/PageLayout';
import type { ProjectQueryParams } from '../types/project';

export function ProjectsPage() {
  const { t } = useTranslation(['projects', 'navigation', 'common']);
  const [searchParams, setSearchParams] = useSearchParams();

  // 1. Parse URL query params into ProjectQueryParams state
  const queryParams: ProjectQueryParams = useMemo(() => {
    const search = searchParams.get('search') || undefined;
    const status = searchParams.get('status') || undefined;
    const category = searchParams.get('category') || undefined;
    const developmentType = searchParams.get('developmentType') || undefined;
    const teamIdStr = searchParams.get('teamId');
    const locationIdStr = searchParams.get('locationId');
    const techIdStr = searchParams.get('technologyId');
    const tagIdStr = searchParams.get('tagId');
    const pageStr = searchParams.get('page');
    const sortBy = (searchParams.get('sortBy') as ProjectQueryParams['sortBy']) || 'updatedAt';
    const sortDirection = (searchParams.get('sortDirection') as ProjectQueryParams['sortDirection']) || 'desc';

    return {
      search,
      status,
      category,
      developmentType,
      teamId: teamIdStr ? Number(teamIdStr) : undefined,
      locationId: locationIdStr ? Number(locationIdStr) : undefined,
      technologyId: techIdStr ? Number(techIdStr) : undefined,
      tagId: tagIdStr ? Number(tagIdStr) : undefined,
      pageNumber: pageStr ? Number(pageStr) : 1,
      pageSize: 12,
      sortBy,
      sortDirection,
    };
  }, [searchParams]);

  // 2. Fetch data via TanStack Query hooks
  const { data: pagedProjects, isLoading, isError, refetch } = useProjects(queryParams);

  const { data: statuses = [] } = useProjectStatuses();
  const { data: categories = [] } = useProjectCategories();
  const { data: technologies = [] } = useTechnologies();
  const { data: locations = [] } = useLocations();
  const { data: teams = [] } = useTeams();

  // 3. Helper to update search params in URL
  const updateQueryParams = (updates: Partial<ProjectQueryParams>) => {
    const nextParams = new URLSearchParams(searchParams);

    Object.entries(updates).forEach(([key, value]) => {
      if (value === undefined || value === null || value === '' || value === false) {
        if (key === 'pageNumber') nextParams.delete('page');
        else nextParams.delete(key);
      } else {
        if (key === 'pageNumber') {
          if (value === 1) nextParams.delete('page');
          else nextParams.set('page', String(value));
        } else {
          nextParams.set(key, String(value));
        }
      }
    });

    setSearchParams(nextParams, { replace: true });
  };

  const handleClearFilters = () => {
    setSearchParams(new URLSearchParams(), { replace: true });
  };

  const hasActiveFilters = Boolean(
    queryParams.search ||
      queryParams.status ||
      queryParams.category ||
      queryParams.technologyId ||
      queryParams.locationId ||
      queryParams.teamId ||
      queryParams.developmentType ||
      queryParams.isFeatured
  );

  return (
    <PageLayout
      title={t('projects.library.title', 'Proje Kütüphanesi')}
      description={t('projects.library.description', 'Demir Export bünyesinde geliştirilen ve kullanılan projeleri keşfedin.')}
      breadcrumbs={[{ label: t('navigation.dashboard', 'Ana Sayfa'), href: '/' }, { label: t('projects.library.title', 'Proje Kütüphanesi') }]}
    >
      <div className="projects-container">
        {/* Search & Filter Toolbar */}
        <ProjectFilters
          queryParams={queryParams}
          onFilterChange={updateQueryParams}
          onClearFilters={handleClearFilters}
          statuses={statuses}
          categories={categories}
          technologies={technologies}
          locations={locations}
          teams={teams}
        />

        {/* Results Header Info & Sorting */}
        <div className="projects-header-info">
          <div className="projects-result-count">
            {isLoading ? (
              <span>{t('projects.library.loadingProjects', 'Projeler yükleniyor...')}</span>
            ) : pagedProjects ? (
              <span>
                {t('projects.library.totalProjectsListed', {
                  count: pagedProjects.totalCount,
                  defaultValue: `Toplam ${pagedProjects.totalCount} proje listeleniyor`,
                })}
              </span>
            ) : null}
          </div>

          <div className="projects-sort-wrapper">
            <label htmlFor="sort-select" className="projects-sort-label">
              {t('projects.library.sortBy', 'Sırala:')}
            </label>
            <select
              id="sort-select"
              value={`${queryParams.sortBy}_${queryParams.sortDirection}`}
              onChange={(e) => {
                const [sortBy, sortDirection] = e.target.value.split('_') as [
                  ProjectQueryParams['sortBy'],
                  ProjectQueryParams['sortDirection']
                ];
                updateQueryParams({ sortBy, sortDirection, pageNumber: 1 });
              }}
              className="projects-sort-select"
              aria-label={t('projects.library.sortProjectsAria', 'Projeleri sırala')}
            >
              <option value="updatedAt_desc">{t('projects.library.sortUpdatedDesc', 'Son Güncellenen')}</option>
              <option value="createdAt_desc">{t('projects.library.sortCreatedDesc', 'En Yeni')}</option>
              <option value="name_asc">{t('projects.library.sortNameAsc', 'Ad (A - Z)')}</option>
              <option value="name_desc">{t('projects.library.sortNameDesc', 'Ad (Z - A)')}</option>
            </select>
          </div>
        </div>

        {/* Content Area */}
        {isLoading ? (
          <ProjectGridSkeleton count={6} />
        ) : isError ? (
          <ErrorState
            title={t('projects.library.errorTitle', 'Projeler Yüklenemedi')}
            description={t('projects.library.errorDescription', 'Proje kütüphanesi verileri sunucudan alınırken bir hata oluştu. Lütfen bağlantınızı kontrol edip tekrar deneyin.')}
            onRetry={() => refetch()}
          />
        ) : pagedProjects && pagedProjects.items.length > 0 ? (
          <>
            {/* Project Cards Grid */}
            <div className="project-grid" role="region" aria-label={t('projects.library.cardsGridAria', 'Proje Kartları Listesi')}>
              {pagedProjects.items.map((project) => (
                <ProjectCard key={project.id} project={project} />
              ))}
            </div>

            {/* Server-side Pagination */}
            <ProjectPagination
              currentPage={pagedProjects.pageNumber}
              totalPages={pagedProjects.totalPages}
              totalCount={pagedProjects.totalCount}
              pageSize={pagedProjects.pageSize}
              onPageChange={(page) => updateQueryParams({ pageNumber: page })}
              hasPreviousPage={pagedProjects.hasPreviousPage}
              hasNextPage={pagedProjects.hasNextPage}
            />
          </>
        ) : hasActiveFilters ? (
          /* Empty State B: No matches for active filters */
          <EmptyState
            icon={<SearchX size={48} strokeWidth={1.25} />}
            title={t('projects.library.noMatchesTitle', 'Aramanızla Eşleşen Proje Bulunamadı')}
            description={t('projects.library.noMatchesDesc', 'Seçtiğiniz filtreler veya arama terimi ile eşleşen proje bulunmuyor. Filtrelerinizi genişleterek tekrar deneyebilirsiniz.')}
            actionLabel={t('projects.library.clearFilters', 'Filtreleri Temizle')}
            onAction={handleClearFilters}
          />
        ) : (
          /* Empty State A: No published projects in database */
          <EmptyState
            icon={<FolderKanban size={48} strokeWidth={1.25} />}
            title={t('projects.library.noProjectsTitle', 'Henüz Yayınlanmış Proje Bulunmuyor')}
            description={t('projects.library.noProjectsDesc', 'Sistemde henüz yayınlanmış bir Ar-Ge projesi kaydı bulunmamaktadır.')}
          />
        )}
      </div>
    </PageLayout>
  );
}

export default ProjectsPage;

