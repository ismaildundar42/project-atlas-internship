import React, { useEffect, useState } from 'react';
import { Filter, RotateCcw, Search, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { Location, ProjectCategory, ProjectStatus, Team, Technology } from '../../types/lookup';
import type { ProjectQueryParams } from '../../types/project';
import { formatDevelopmentType } from '../../utils/formatters';
import Button from '../ui/Button';

interface ProjectFiltersProps {
  queryParams: ProjectQueryParams;
  onFilterChange: (updates: Partial<ProjectQueryParams>) => void;
  onClearFilters: () => void;
  statuses: ProjectStatus[];
  categories: ProjectCategory[];
  technologies: Technology[];
  locations: Location[];
  teams: Team[];
  isLoadingLookups?: boolean;
}

export const ProjectFilters: React.FC<ProjectFiltersProps> = ({
  queryParams,
  onFilterChange,
  onClearFilters,
  statuses,
  categories,
  technologies,
  locations,
  teams,
}) => {
  const { t } = useTranslation(['projects', 'common']);
  const [searchInput, setSearchInput] = useState(queryParams.search || '');
  const [isMobileFiltersOpen, setIsMobileFiltersOpen] = useState(false);

  // Sync internal search input state when queryParams.search changes externally
  useEffect(() => {
    setSearchInput(queryParams.search || '');
  }, [queryParams.search]);

  // Debounce search input (400ms)
  useEffect(() => {
    const timer = setTimeout(() => {
      const trimmed = searchInput.trim();
      if ((queryParams.search || '') !== trimmed) {
        onFilterChange({ search: trimmed || undefined, pageNumber: 1 });
      }
    }, 400);

    return () => clearTimeout(timer);
  }, [searchInput, queryParams.search, onFilterChange]);

  // Determine active filters
  const activeStatus = statuses.find((s) => s.code === queryParams.status);
  const activeCategory = categories.find((c) => c.code === queryParams.category);
  const activeTech = technologies.find((t) => t.id === queryParams.technologyId);
  const activeLocation = locations.find((l) => l.id === queryParams.locationId);
  const activeTeam = teams.find((tm) => tm.id === queryParams.teamId);
  const activeDevType = queryParams.developmentType;

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
    <div className="project-filters">
      {/* Top Search & Filter Actions Toolbar */}
      <div className="project-filters__toolbar">
        <div className="project-filters__search-wrapper">
          <Search size={18} className="project-filters__search-icon" aria-hidden="true" />
          <input
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder={t('projects.filters.searchPlaceholder', 'Proje adı veya açıklama ara...')}
            className="project-filters__search-input"
            aria-label={t('projects.filters.searchAria', 'Proje ara')}
          />
          {searchInput && (
            <button
              type="button"
              onClick={() => {
                setSearchInput('');
                onFilterChange({ search: undefined, pageNumber: 1 });
              }}
              className="project-filters__search-clear"
              aria-label={t('projects.filters.clearSearchText', 'Arama metnini temizle')}
            >
              <X size={16} />
            </button>
          )}
        </div>

        {/* Mobile Filter Toggle Button */}
        <Button
          variant="secondary"
          size="md"
          className="project-filters__mobile-toggle"
          onClick={() => setIsMobileFiltersOpen(!isMobileFiltersOpen)}
          aria-expanded={isMobileFiltersOpen}
          aria-controls="project-filters-panel"
        >
          <Filter size={16} aria-hidden="true" />
          <span>{t('projects.filters.filters', 'Filtreler')}</span>
          {hasActiveFilters && <span className="project-filters__badge-dot" />}
        </Button>
      </div>

      {/* Select Controls Panel (Desktop always visible, Mobile collapsible) */}
      <div
        id="project-filters-panel"
        className={`project-filters__panel ${isMobileFiltersOpen ? 'project-filters__panel--open' : ''}`}
      >
        <div className="project-filters__grid">
          {/* Status Select */}
          <div className="project-filters__group">
            <label htmlFor="filter-status" className="project-filters__label">
              {t('projects.filters.status', 'Durum')}
            </label>
            <select
              id="filter-status"
              value={queryParams.status || ''}
              onChange={(e) => onFilterChange({ status: e.target.value || undefined, pageNumber: 1 })}
              className="project-filters__select"
            >
              <option value="">{t('projects.filters.allStatuses', 'Tüm Durumlar')}</option>
              {statuses.map((st) => (
                <option key={st.id} value={st.code}>
                  {st.name}
                </option>
              ))}
            </select>
          </div>

          {/* Category Select */}
          <div className="project-filters__group">
            <label htmlFor="filter-category" className="project-filters__label">
              {t('projects.filters.category', 'Kategori')}
            </label>
            <select
              id="filter-category"
              value={queryParams.category || ''}
              onChange={(e) => onFilterChange({ category: e.target.value || undefined, pageNumber: 1 })}
              className="project-filters__select"
            >
              <option value="">{t('projects.filters.allCategories', 'Tüm Kategoriler')}</option>
              {categories.map((cat) => (
                <option key={cat.id} value={cat.code}>
                  {cat.name}
                </option>
              ))}
            </select>
          </div>

          {/* Technology Select */}
          <div className="project-filters__group">
            <label htmlFor="filter-technology" className="project-filters__label">
              {t('projects.filters.technology', 'Teknoloji')}
            </label>
            <select
              id="filter-technology"
              value={queryParams.technologyId || ''}
              onChange={(e) =>
                onFilterChange({
                  technologyId: e.target.value ? Number(e.target.value) : undefined,
                  pageNumber: 1,
                })
              }
              className="project-filters__select"
            >
              <option value="">{t('projects.filters.allTechnologies', 'Tüm Teknolojiler')}</option>
              {technologies.map((tech) => (
                <option key={tech.id} value={tech.id}>
                  {tech.name} ({tech.category})
                </option>
              ))}
            </select>
          </div>

          {/* Development Type Select */}
          <div className="project-filters__group">
            <label htmlFor="filter-devtype" className="project-filters__label">
              {t('projects.filters.developmentType', 'Geliştirme Tipi')}
            </label>
            <select
              id="filter-devtype"
              value={queryParams.developmentType || ''}
              onChange={(e) => onFilterChange({ developmentType: e.target.value || undefined, pageNumber: 1 })}
              className="project-filters__select"
            >
              <option value="">{t('projects.filters.allTypes', 'Tüm Tipler')}</option>
              <option value="Internal">{t('projects.developmentType.internal', 'İç Geliştirme')}</option>
              <option value="External">{t('projects.developmentType.external', 'Dış Hizmet')}</option>
              <option value="Hybrid">{t('projects.developmentType.hybrid', 'Karma / Ortak')}</option>
            </select>
          </div>

          {/* Location Select */}
          <div className="project-filters__group">
            <label htmlFor="filter-location" className="project-filters__label">
              {t('projects.filters.location', 'Lokasyon')}
            </label>
            <select
              id="filter-location"
              value={queryParams.locationId || ''}
              onChange={(e) =>
                onFilterChange({
                  locationId: e.target.value ? Number(e.target.value) : undefined,
                  pageNumber: 1,
                })
              }
              className="project-filters__select"
            >
              <option value="">{t('projects.filters.allLocations', 'Tüm Lokasyonlar')}</option>
              {locations.map((loc) => (
                <option key={loc.id} value={loc.id}>
                  {loc.name}
                </option>
              ))}
            </select>
          </div>

          {/* Team Select */}
          <div className="project-filters__group">
            <label htmlFor="filter-team" className="project-filters__label">
              {t('projects.filters.team', 'Ekip')}
            </label>
            <select
              id="filter-team"
              value={queryParams.teamId || ''}
              onChange={(e) =>
                onFilterChange({
                  teamId: e.target.value ? Number(e.target.value) : undefined,
                  pageNumber: 1,
                })
              }
              className="project-filters__select"
            >
              <option value="">{t('projects.filters.allTeams', 'Tüm Ekipler')}</option>
              {teams.map((tItem) => (
                <option key={tItem.id} value={tItem.id}>
                  {tItem.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Active Filter Badges Bar */}
      {hasActiveFilters && (
        <div className="project-filters__active-bar">
          <span className="project-filters__active-title">{t('projects.filters.activeFilters', 'Aktif Filtreler:')}</span>
          <div className="project-filters__chips">
            {queryParams.search && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.searchFilterPrefix', 'Arama')}: "{queryParams.search}"</span>
                <button
                  type="button"
                  onClick={() => {
                    setSearchInput('');
                    onFilterChange({ search: undefined, pageNumber: 1 });
                  }}
                  aria-label={t('projects.filters.removeSearchFilter', 'Arama filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}
            {activeStatus && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.statusFilterPrefix', 'Durum')}: {activeStatus.name}</span>
                <button
                  type="button"
                  onClick={() => onFilterChange({ status: undefined, pageNumber: 1 })}
                  aria-label={t('projects.filters.removeStatusFilter', 'Durum filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}
            {activeCategory && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.categoryFilterPrefix', 'Kategori')}: {activeCategory.name}</span>
                <button
                  type="button"
                  onClick={() => onFilterChange({ category: undefined, pageNumber: 1 })}
                  aria-label={t('projects.filters.removeCategoryFilter', 'Kategori filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}
            {activeTech && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.technologyFilterPrefix', 'Teknoloji')}: {activeTech.name}</span>
                <button
                  type="button"
                  onClick={() => onFilterChange({ technologyId: undefined, pageNumber: 1 })}
                  aria-label={t('projects.filters.removeTechnologyFilter', 'Teknoloji filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}
            {activeDevType && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.devTypeFilterPrefix', 'Tip')}: {formatDevelopmentType(activeDevType, t)}</span>
                <button
                  type="button"
                  onClick={() => onFilterChange({ developmentType: undefined, pageNumber: 1 })}
                  aria-label={t('projects.filters.removeDevTypeFilter', 'Geliştirme tipi filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}
            {activeLocation && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.locationFilterPrefix', 'Lokasyon')}: {activeLocation.name}</span>
                <button
                  type="button"
                  onClick={() => onFilterChange({ locationId: undefined, pageNumber: 1 })}
                  aria-label={t('projects.filters.removeLocationFilter', 'Lokasyon filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}
            {activeTeam && (
              <span className="project-filters__chip">
                <span>{t('projects.filters.teamFilterPrefix', 'Ekip')}: {activeTeam.name}</span>
                <button
                  type="button"
                  onClick={() => onFilterChange({ teamId: undefined, pageNumber: 1 })}
                  aria-label={t('projects.filters.removeTeamFilter', 'Ekip filtresini kaldır')}
                >
                  <X size={14} />
                </button>
              </span>
            )}

            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setSearchInput('');
                onClearFilters();
              }}
              className="project-filters__clear-btn"
            >
              <RotateCcw size={14} aria-hidden="true" />
              <span>{t('projects.filters.clearFilters', 'Filtreleri Temizle')}</span>
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectFilters;

