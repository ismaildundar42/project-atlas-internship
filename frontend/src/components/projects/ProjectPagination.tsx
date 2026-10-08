import React from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Button from '../ui/Button';

interface ProjectPaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const ProjectPagination: React.FC<ProjectPaginationProps> = ({
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  hasPreviousPage,
  hasNextPage,
}) => {
  const { t } = useTranslation(['projects', 'common']);

  if (totalCount === 0 || totalPages <= 1) {
    return null;
  }

  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);

  // Generate visible page number buttons
  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (currentPage > 3) {
        pages.push('...');
      }

      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) {
        pages.push('...');
      }
      pages.push(totalPages);
    }

    return pages;
  };

  return (
    <nav className="project-pagination" aria-label={t('projects.pagination.navAria', 'Proje listesi sayfalaması')}>
      <div className="project-pagination__info">
        {t('projects.pagination.showingRange', {
          total: totalCount,
          start: startItem,
          end: endItem,
          defaultValue: `${totalCount} projeden ${startItem}–${endItem} arası gösteriliyor`,
        })}
      </div>

      <div className="project-pagination__controls">
        <Button
          variant="secondary"
          size="sm"
          onClick={() => onPageChange(currentPage - 1)}
          disabled={!hasPreviousPage}
          aria-label={t('projects.pagination.previousPageAria', 'Önceki sayfaya git')}
        >
          <ChevronLeft size={16} aria-hidden="true" />
          <span>{t('projects.pagination.previous', 'Önceki')}</span>
        </Button>

        <div className="project-pagination__pages">
          {getPageNumbers().map((page, index) => {
            if (page === '...') {
              return (
                <span key={`ellipsis-${index}`} className="project-pagination__ellipsis">
                  …
                </span>
              );
            }

            const pageNum = page as number;
            const isCurrent = pageNum === currentPage;

            return (
              <button
                key={pageNum}
                type="button"
                onClick={() => onPageChange(pageNum)}
                className={`project-pagination__page-btn ${isCurrent ? 'project-pagination__page-btn--active' : ''}`}
                aria-current={isCurrent ? 'page' : undefined}
                aria-label={t('projects.pagination.pageNumberAria', { page: pageNum, defaultValue: `Sayfa ${pageNum}` })}
              >
                {pageNum}
              </button>
            );
          })}
        </div>

        <Button
          variant="secondary"
          size="sm"
          onClick={() => onPageChange(currentPage + 1)}
          disabled={!hasNextPage}
          aria-label={t('projects.pagination.nextPageAria', 'Sonraki sayfaya git')}
        >
          <span>{t('projects.pagination.next', 'Sonraki')}</span>
          <ChevronRight size={16} aria-hidden="true" />
        </Button>
      </div>
    </nav>
  );
};

export default ProjectPagination;

