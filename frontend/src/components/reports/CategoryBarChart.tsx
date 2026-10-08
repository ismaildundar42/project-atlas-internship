import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ReportCategoryDistribution } from '../../types/report';
import { getCategoryColor } from './reportColors';
import { useTheme } from '../../context/ThemeContext';

interface CategoryBarChartProps {
  categories: ReportCategoryDistribution[];
}

export function CategoryBarChart({ categories }: CategoryBarChartProps) {
  const { t } = useTranslation(['reports', 'common', 'projects']);
  const { resolvedTheme } = useTheme();
  const isDark = resolvedTheme === 'dark';
  const [hoveredId, setHoveredId] = useState<number | null>(null);

  const maxCount = Math.max(...categories.map((c) => c.count), 1);

  return (
    <div className="category-chart-wrapper" role="region" aria-label={t('reports.categoryChart.chartAria', 'Kategori Karşılaştırma Grafiği')}>
      {/* Grid Lines Header / Axis scale */}
      <div className="category-chart-axis-header" aria-hidden="true">
        <span>0</span>
        <span>{Math.round(maxCount * 0.25)}</span>
        <span>{Math.round(maxCount * 0.5)}</span>
        <span>{Math.round(maxCount * 0.75)}</span>
        <span>{maxCount}</span>
      </div>

      <div className="category-chart-plot" role="list">
        {/* Background Vertical Grid Lines */}
        <div className="category-chart-grid-lines" aria-hidden="true">
          <div className="category-chart-grid-line" style={{ left: '0%' }} />
          <div className="category-chart-grid-line" style={{ left: '25%' }} />
          <div className="category-chart-grid-line" style={{ left: '50%' }} />
          <div className="category-chart-grid-line" style={{ left: '75%' }} />
          <div className="category-chart-grid-line" style={{ left: '100%' }} />
        </div>

        {/* Category Rows */}
        {categories.map((cat, idx) => {
          const widthPercent = Math.max((cat.count / maxCount) * 100, 3);
          const color = getCategoryColor(idx, isDark);
          const isHovered = hoveredId === cat.categoryId;

          return (
            <div
              key={cat.categoryId}
              role="listitem"
              className={`category-chart-row ${isHovered ? 'category-chart-row--active' : ''}`}
              onMouseEnter={() => setHoveredId(cat.categoryId)}
              onMouseLeave={() => setHoveredId(null)}
              tabIndex={0}
              onFocus={() => setHoveredId(cat.categoryId)}
              onBlur={() => setHoveredId(null)}
              aria-label={`${cat.name}: ${cat.count} ${t('projects.projectCountSuffix', 'proje')} (%${cat.percentage})`}
            >
              <div className="category-chart-row__label" title={cat.name}>
                <span className="category-chart-row__dot" style={{ backgroundColor: color }} aria-hidden="true" />
                <span className="category-chart-row__text">{cat.name}</span>
              </div>

              <div className="category-chart-row__track">
                <div
                  className="category-chart-row__bar"
                  style={{
                    width: `${widthPercent}%`,
                    backgroundColor: color,
                  }}
                >
                  <span className="category-chart-row__bar-value">
                    {cat.count}
                  </span>
                </div>
                <span className="category-chart-row__pct-badge">
                  %{cat.percentage}
                </span>
              </div>
            </div>
          );
        })}
      </div>

      {/* Screen-reader table */}
      <table className="sr-only">
        <caption>{t('reports.sections.categoryDistributionTitle', 'Kategori Dağılımı')}</caption>
        <thead>
          <tr>
            <th scope="col">{t('projects.filters.category', 'Kategori')}</th>
            <th scope="col">{t('reports.table.projectCount', 'Proje Sayısı')}</th>
            <th scope="col">{t('reports.table.percentage', 'Yüzde')}</th>
          </tr>
        </thead>
        <tbody>
          {categories.map((cat) => (
            <tr key={`sr-cat-${cat.categoryId}`}>
              <td>{cat.name}</td>
              <td>{cat.count}</td>
              <td>%{cat.percentage}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default CategoryBarChart;
