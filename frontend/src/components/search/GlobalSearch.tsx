import { useState, useRef, useEffect, useId, type KeyboardEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Search,
  X,
  Loader2,
  FolderKanban,
  ArrowRight,
  Sparkles,
  MapPin,
  Users,
  AlertCircle,
} from 'lucide-react';
import { useGlobalSearch } from '../../hooks/useGlobalSearch';
import { resolveResourceUrl } from '../../utils/urlUtils';
import type { ProjectSearchResult } from '../../types/search';

interface GlobalSearchProps {
  className?: string;
  onNavigate?: () => void;
}

export function GlobalSearch({ className = '', onNavigate }: GlobalSearchProps) {
  const { t } = useTranslation(['projects', 'common']);
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState<number>(-1);

  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const listboxRef = useRef<HTMLUListElement>(null);
  const inputId = useId();
  const listboxId = useId();
  const navigate = useNavigate();

  const { data, isLoading, isError, isDebouncing } = useGlobalSearch(query, 8);

  const results: ProjectSearchResult[] = data?.items || [];
  const totalCount = data?.totalCount || 0;
  const isSearching = isDebouncing || (isLoading && query.trim().length >= 2);
  const showDropdown = isOpen && query.trim().length >= 2;

  // Total selectable items = results count + (totalCount > 0 ? 1 : 0) (for footer link)
  const totalSelectableCount = results.length + (results.length > 0 ? 1 : 0);

  // Click outside to close dropdown
  useEffect(() => {
    const handlePointerDown = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
        setActiveIndex(-1);
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
    };
  }, []);

  // Reset active index when results change
  useEffect(() => {
    setActiveIndex(-1);
  }, [results]);

  // Scroll active item into view
  useEffect(() => {
    if (activeIndex >= 0 && listboxRef.current) {
      const activeEl = listboxRef.current.children[activeIndex] as HTMLElement;
      if (activeEl) {
        activeEl.scrollIntoView({ block: 'nearest' });
      }
    }
  }, [activeIndex]);

  const handleSelectResult = (project: ProjectSearchResult) => {
    setIsOpen(false);
    setQuery('');
    setActiveIndex(-1);
    onNavigate?.();
    navigate(`/projects/${project.slug}`);
  };

  const handleViewAllInLibrary = () => {
    const term = query.trim();
    setIsOpen(false);
    setQuery('');
    setActiveIndex(-1);
    onNavigate?.();
    navigate(`/projects?search=${encodeURIComponent(term)}`);
  };

  const handleClear = () => {
    setQuery('');
    setIsOpen(false);
    setActiveIndex(-1);
    inputRef.current?.focus();
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (!showDropdown) {
      if (e.key === 'ArrowDown' && query.trim().length >= 2) {
        setIsOpen(true);
        e.preventDefault();
      }
      if (e.key === 'Enter' && query.trim().length >= 2) {
        e.preventDefault();
        handleViewAllInLibrary();
      }
      return;
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setActiveIndex((prev) => (prev + 1 >= totalSelectableCount ? 0 : prev + 1));
        break;

      case 'ArrowUp':
        e.preventDefault();
        setActiveIndex((prev) => (prev <= 0 ? totalSelectableCount - 1 : prev - 1));
        break;

      case 'Enter':
        e.preventDefault();
        if (activeIndex >= 0 && activeIndex < results.length) {
          handleSelectResult(results[activeIndex]);
        } else if (activeIndex === results.length || activeIndex === -1) {
          handleViewAllInLibrary();
        }
        break;

      case 'Escape':
        e.preventDefault();
        setIsOpen(false);
        setActiveIndex(-1);
        inputRef.current?.blur();
        break;

      case 'Tab':
        setIsOpen(false);
        setActiveIndex(-1);
        break;

      default:
        break;
    }
  };

  return (
    <div ref={containerRef} className={`global-search ${className}`}>
      {/* Search Input Box */}
      <div
        className={`global-search__input-wrapper ${isOpen ? 'global-search__input-wrapper--focused' : ''}`}
        role="combobox"
        aria-expanded={showDropdown}
        aria-haspopup="listbox"
        aria-controls={listboxId}
        aria-owns={showDropdown ? listboxId : undefined}
      >
        <label htmlFor={inputId} className="sr-only">
          {t('projects.searchPlaceholder', 'Proje, teknoloji veya ekip ara...')}
        </label>

        <span className="global-search__icon global-search__icon--left" aria-hidden="true">
          {isSearching ? (
            <Loader2 size={16} className="global-search__spinner" />
          ) : (
            <Search size={16} />
          )}
        </span>

        <input
          ref={inputRef}
          id={inputId}
          type="text"
          className="global-search__input"
          placeholder={t('projects.searchPlaceholder', 'Proje, teknoloji veya ekip ara...')}
          value={query}
          autoComplete="off"
          spellCheck="false"
          aria-autocomplete="list"
          aria-activedescendant={
            activeIndex >= 0 && activeIndex < results.length
              ? `search-opt-${results[activeIndex].id}`
              : activeIndex === results.length
                ? 'search-opt-all'
                : undefined
          }
          onChange={(e) => {
            setQuery(e.target.value);
            setIsOpen(true);
          }}
          onFocus={() => {
            if (query.trim().length >= 2) {
              setIsOpen(true);
            }
          }}
          onKeyDown={handleKeyDown}
        />

        {query.length > 0 && (
          <button
            type="button"
            className="global-search__clear"
            aria-label={t('projects.clearSearch', 'Aramayı temizle')}
            onClick={handleClear}
          >
            <X size={14} aria-hidden="true" />
          </button>
        )}
      </div>

      {/* Search Results Dropdown Panel */}
      {showDropdown && (
        <div className="global-search__dropdown" role="region" aria-label={t('projects.searchResultsAria', 'Arama sonuçları')}>
          {/* 1. Loading State */}
          {isSearching && results.length === 0 && (
            <div className="global-search__state global-search__state--loading">
              <Loader2 size={20} className="global-search__spinner" aria-hidden="true" />
              <span>{t('projects.searchingHint', 'Projeler ve kurumsal bilgiler taranıyor...')}</span>
            </div>
          )}

          {/* 2. Error State */}
          {!isSearching && isError && (
            <div className="global-search__state global-search__state--error">
              <AlertCircle size={18} aria-hidden="true" />
              <span>{t('projects.searchError', 'Arama sonuçları alınamadı. Lütfen tekrar deneyin.')}</span>
            </div>
          )}

          {/* 3. Empty State */}
          {!isSearching && !isError && results.length === 0 && (
            <div className="global-search__empty">
              <div className="global-search__empty-title">
                {t('projects.noResultsFor', { query: query.trim() })}
              </div>
              <div className="global-search__empty-desc">
                {t('projects.noResultsHint', 'Farklı bir proje adı, teknoloji (ör. React, .NET), lokasyon veya ekip adı deneyebilirsiniz.')}
              </div>
            </div>
          )}

          {/* 4. Results List */}
          {results.length > 0 && (
            <>
              <div className="global-search__header">
                <span className="global-search__header-title">{t('projects.matchingProjects', 'Eşleşen Projeler')}</span>
                <span className="global-search__header-count">
                  {t('projects.resultsCount', { count: totalCount, defaultValue: `${totalCount} sonuç` })}
                </span>
              </div>

              <ul
                ref={listboxRef}
                id={listboxId}
                className="global-search__list"
                role="listbox"
                aria-label={t('projects.searchResultsListAria', 'Arama sonuçları listesi')}
              >
                {results.map((project, index) => {
                  const isSelected = activeIndex === index;
                  const coverUrl = resolveResourceUrl(project.coverImageUrl);

                  return (
                    <li
                      key={project.id}
                      id={`search-opt-${project.id}`}
                      role="option"
                      aria-selected={isSelected}
                      className={`global-search__item ${isSelected ? 'global-search__item--active' : ''}`}
                      onClick={() => handleSelectResult(project)}
                      onMouseEnter={() => setActiveIndex(index)}
                    >
                      {/* Project Thumbnail / Icon */}
                      <div className="global-search__item-thumb">
                        {coverUrl ? (
                          <img
                            src={coverUrl}
                            alt=""
                            className="global-search__item-img"
                            onError={(e) => {
                              // Fallback on broken image
                              (e.currentTarget as HTMLElement).style.display = 'none';
                            }}
                          />
                        ) : (
                          <FolderKanban size={20} className="global-search__item-thumb-icon" aria-hidden="true" />
                        )}
                      </div>

                      {/* Main Information */}
                      <div className="global-search__item-body">
                        <div className="global-search__item-top">
                          <span className="global-search__item-name">{project.name}</span>
                          <span className="global-search__badge global-search__badge--category">
                            {project.categoryName}
                          </span>
                        </div>

                        {project.shortDescription && (
                          <p className="global-search__item-desc">
                            {project.shortDescription}
                          </p>
                        )}

                        {/* Match Context / Reason or Chips */}
                        <div className="global-search__item-meta">
                          {project.matchReason ? (
                            <span className="global-search__match-chip">
                              <Sparkles size={11} aria-hidden="true" />
                              <span>{project.matchReason}</span>
                            </span>
                          ) : project.technologies.length > 0 ? (
                            <span className="global-search__meta-item">
                              {project.technologies.slice(0, 3).join(' · ')}
                            </span>
                          ) : null}

                          {project.locations.length > 0 && (
                            <span className="global-search__meta-item">
                              <MapPin size={11} aria-hidden="true" />
                              <span>{project.locations[0]}</span>
                            </span>
                          )}

                          {project.primaryTeamName && (
                            <span className="global-search__meta-item">
                              <Users size={11} aria-hidden="true" />
                              <span>{project.primaryTeamName}</span>
                            </span>
                          )}
                        </div>
                      </div>
                    </li>
                  );
                })}
              </ul>

              {/* 5. Dropdown Footer: Full Library Discovery Action */}
              <div
                id="search-opt-all"
                role="option"
                aria-selected={activeIndex === results.length}
                className={`global-search__footer ${
                  activeIndex === results.length ? 'global-search__footer--active' : ''
                }`}
                onClick={handleViewAllInLibrary}
                onMouseEnter={() => setActiveIndex(results.length)}
              >
                <span>
                  <strong>"{query.trim()}"</strong> {t('projects.viewAllInLibrarySuffix', { count: totalCount })}
                </span>
                <ArrowRight size={15} aria-hidden="true" />
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default GlobalSearch;
