import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ReportTechnology } from '../../types/report';

interface TechnologyLandscapeChartProps {
  technologies: ReportTechnology[];
  totalProjects: number;
}

export function TechnologyLandscapeChart({ technologies, totalProjects }: TechnologyLandscapeChartProps) {
  const { t } = useTranslation(['reports', 'common', 'projects']);
  const [hoveredId, setHoveredId] = useState<number | null>(null);

  if (technologies.length === 0) {
    return (
      <div className="report-empty-inline">
        <p>{t('reports.techChart.empty', 'Teknoloji eşleştirmesi yapılmış proje bulunmamaktadır.')}</p>
      </div>
    );
  }

  const maxCount = Math.max(...technologies.map((tItem) => tItem.projectCount), 1);

  return (
    <div className="tech-landscape-wrapper" role="region" aria-label={t('reports.techChart.chartAria', 'Teknoloji Ekosistemi Kullanım Grafiği')}>
      <div className="tech-landscape-list" role="list">
        {technologies.map((tech) => {
          const widthPercent = Math.max((tech.projectCount / maxCount) * 100, 4);
          const portfolioPercent = totalProjects > 0 ? Math.round((tech.projectCount / totalProjects) * 100) : 0;
          const isHovered = hoveredId === tech.id;

          return (
            <div
              key={tech.id}
              role="listitem"
              className={`tech-landscape-row ${isHovered ? 'tech-landscape-row--active' : ''}`}
              onMouseEnter={() => setHoveredId(tech.id)}
              onMouseLeave={() => setHoveredId(null)}
              tabIndex={0}
              onFocus={() => setHoveredId(tech.id)}
              onBlur={() => setHoveredId(null)}
              aria-label={`${tech.name} (${tech.category}): ${tech.projectCount} ${t('reports.techChart.projectsUsedIn', 'projede kullanılıyor')} (${t('reports.techChart.portfolioShare', { percent: portfolioPercent })})`}
            >
              {/* Left Info */}
              <div className="tech-landscape-row__left">
                <span className="tech-landscape-row__name" title={tech.name}>
                  {tech.name}
                </span>
                <span className="tech-landscape-row__category">{tech.category}</span>
              </div>

              {/* Graphical Bar */}
              <div className="tech-landscape-row__track-wrapper">
                <div className="tech-landscape-row__track">
                  <div
                    className="tech-landscape-row__bar"
                    style={{ width: `${widthPercent}%` }}
                  />
                </div>
                <div className="tech-landscape-row__metrics">
                  <span className="tech-landscape-row__count">{tech.projectCount} {t('projects.projectCountSuffix', 'proje')}</span>
                  <span className="tech-landscape-row__pct">%{portfolioPercent}</span>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Screen-reader table */}
      <table className="sr-only">
        <caption>{t('reports.techChart.tableCaption', 'Teknoloji Kullanım Oranları')}</caption>
        <thead>
          <tr>
            <th scope="col">{t('projects.filters.technology', 'Teknoloji')}</th>
            <th scope="col">{t('projects.filters.category', 'Kategori')}</th>
            <th scope="col">{t('reports.table.projectCount', 'Proje Sayısı')}</th>
          </tr>
        </thead>
        <tbody>
          {technologies.map((tech) => (
            <tr key={`sr-tech-${tech.id}`}>
              <td>{tech.name}</td>
              <td>{tech.category}</td>
              <td>{tech.projectCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default TechnologyLandscapeChart;
