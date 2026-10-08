import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ReportTimeline } from '../../types/report';

interface TimelineBarChartProps {
  timeline: ReportTimeline[];
}

export function TimelineBarChart({ timeline }: TimelineBarChartProps) {
  const { t } = useTranslation(['reports', 'common', 'projects']);
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);

  if (timeline.length === 0) {
    return (
      <div className="timeline-empty-state">
        <p>{t('reports.timeline.empty', 'Başlangıç tarihi belirtilmiş proje kaydı bulunmamaktadır.')}</p>
      </div>
    );
  }

  const maxCount = Math.max(...timeline.map((tItem) => tItem.projectCount), 1);
  // Calculate dynamic minimum width so each month gets at least 56px of horizontal space
  const slotWidth = 56;
  const minWidthPx = Math.max(520, timeline.length * slotWidth);

  // Generate distinct Y-axis ticks from max down to 0
  const yTicks = [
    maxCount,
    Math.round(maxCount * 0.75),
    Math.round(maxCount * 0.5),
    Math.round(maxCount * 0.25),
    0,
  ];
  const uniqueYTicks = Array.from(new Set(yTicks)).sort((a, b) => b - a);

  return (
    <div className="timeline-analytics-chart" role="region" aria-label={t('reports.timeline.chartAria', 'Aylık Proje Başlangıç Zaman Çizgisi Grafiği')}>
      <div className="timeline-chart-layout">
        {/* Y-Axis Scale */}
        <div className="timeline-y-axis" aria-hidden="true">
          {uniqueYTicks.map((tick) => (
            <span key={tick} className="timeline-y-axis__tick">
              {tick}
            </span>
          ))}
        </div>

        {/* Chart Main Section (Plot area + dedicated X-axis labels below baseline) */}
        <div className="timeline-main-area">
          {/* Plot Area with baseline */}
          <div className="timeline-plot-area" style={{ minWidth: `${minWidthPx}px` }}>
            {/* Horizontal Grid lines */}
            <div className="timeline-grid-lines" aria-hidden="true">
              {uniqueYTicks.map((tick) => (
                <div key={tick} className="timeline-grid-line" />
              ))}
            </div>

            {/* Bars */}
            <div className="timeline-bars-row">
              {timeline.map((item, idx) => {
                const heightPercent = Math.max((item.projectCount / maxCount) * 100, 10);
                const isHovered = hoveredIndex === idx;

                return (
                  <div
                    key={`${item.year}-${item.month}`}
                    className={`timeline-bar-slot ${isHovered ? 'timeline-bar-slot--active' : ''}`}
                    tabIndex={0}
                    role="button"
                    aria-label={`${item.periodLabel}: ${item.projectCount} ${t('reports.timeline.projectStartsSuffix', 'proje başlangıcı')}`}
                    onMouseEnter={() => setHoveredIndex(idx)}
                    onMouseLeave={() => setHoveredIndex(null)}
                    onFocus={() => setHoveredIndex(idx)}
                    onBlur={() => setHoveredIndex(null)}
                  >
                    {/* Floating Tooltip */}
                    {isHovered && (
                      <div className="timeline-tooltip" role="tooltip">
                        <span className="timeline-tooltip__title">{item.periodLabel}</span>
                        <span className="timeline-tooltip__desc">{item.projectCount} {t('reports.timeline.projectStartsTooltip', 'Proje Başlangıcı')}</span>
                      </div>
                    )}

                    {/* Numeric Value above the bar */}
                    <span className="timeline-bar-val">{item.projectCount}</span>

                    {/* Bar Pillar */}
                    <div
                      className="timeline-bar-pillar"
                      style={{ height: `${heightPercent}%` }}
                    />
                  </div>
                );
              })}
            </div>
          </div>

          {/* Dedicated X-Axis Month Labels Row BELOW the Baseline */}
          <div className="timeline-x-axis-row" style={{ minWidth: `${minWidthPx}px` }} aria-hidden="true">
            {timeline.map((item, idx) => {
              const isHovered = hoveredIndex === idx;
              return (
                <div
                  key={`label-${item.year}-${item.month}`}
                  className={`timeline-x-axis-label ${isHovered ? 'timeline-x-axis-label--active' : ''}`}
                >
                  {item.periodLabel}
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Screen-reader accessible table */}
      <table className="sr-only">
        <caption>{t('reports.timeline.tableCaption', 'Aylık Proje Başlangıç Dağılımı')}</caption>
        <thead>
          <tr>
            <th scope="col">{t('reports.timeline.tablePeriod', 'Dönem')}</th>
            <th scope="col">{t('reports.timeline.tableCount', 'Başlayan Proje Sayısı')}</th>
          </tr>
        </thead>
        <tbody>
          {timeline.map((item) => (
            <tr key={`sr-time-${item.year}-${item.month}`}>
              <td>{item.periodLabel}</td>
              <td>{item.projectCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default TimelineBarChart;

