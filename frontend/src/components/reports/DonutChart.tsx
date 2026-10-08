import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { PieChart } from 'lucide-react';
import type { ChartSegment } from './reportColors';

interface DonutChartProps {
  segments: ChartSegment[];
  totalCount: number;
  centerLabel?: string;
  size?: number;
  thickness?: number;
  title: string;
}

export function DonutChart({
  segments,
  totalCount,
  centerLabel,
  size = 180,
  thickness = 26,
  title,
}: DonutChartProps) {
  const { t } = useTranslation(['reports', 'common', 'projects']);
  const defaultCenterLabel = centerLabel || t('projects.projectCountSuffix', 'Proje');
  const [activeId, setActiveId] = useState<string | number | null>(null);

  const isEmpty = !totalCount || totalCount === 0 || segments.length === 0 || segments.every((s) => s.value === 0);

  const radius = (size - thickness) / 2;
  const circumference = 2 * Math.PI * radius;
  const center = size / 2;

  // Calculate accumulated offsets for each segment immutably
  const calculatedSegments = isEmpty
    ? []
    : segments.reduce<
        Array<(typeof segments)[number] & { strokeDash: number; strokeOffset: number }>
      >((acc, seg) => {
        const strokeDash = (seg.value / (totalCount || 1)) * circumference;
        const accumulated = acc.reduce((sum, item) => sum + item.strokeDash, 0);
        acc.push({
          ...seg,
          strokeDash,
          strokeOffset: -accumulated,
        });
        return acc;
      }, []);

  const activeSegment = calculatedSegments.find((s) => s.id === activeId);

  return (
    <div className="donut-chart-container" role="region" aria-label={title}>
      {/* SVG Ring */}
      <div className="donut-chart__visual">
        <svg
          width={size}
          height={size}
          viewBox={`0 0 ${size} ${size}`}
          className="donut-chart__svg"
          aria-hidden="true"
        >
          {/* Base background circle */}
          <circle
            cx={center}
            cy={center}
            r={radius}
            fill="none"
            stroke="var(--color-surface-subtle)"
            strokeWidth={thickness}
            strokeDasharray={isEmpty ? '4 4' : undefined}
          />

          {/* Slices (only rendered when there is data) */}
          {!isEmpty &&
            calculatedSegments.map((seg) => {
              const isHovered = activeId === seg.id;
              return (
                <circle
                  key={seg.id}
                  cx={center}
                  cy={center}
                  r={radius}
                  fill="none"
                  stroke={seg.color}
                  strokeWidth={isHovered ? thickness + 4 : thickness}
                  strokeDasharray={`${seg.strokeDash} ${circumference}`}
                  strokeDashoffset={seg.strokeOffset}
                  className="donut-chart__slice"
                  style={{
                    transformOrigin: 'center',
                    transform: 'rotate(-90deg)',
                    transition: 'stroke-width var(--transition-fast), opacity var(--transition-fast)',
                    opacity: activeId && !isHovered ? 0.45 : 1,
                    cursor: 'pointer',
                  }}
                  onMouseEnter={() => setActiveId(seg.id)}
                  onMouseLeave={() => setActiveId(null)}
                  tabIndex={0}
                  role="button"
                  aria-label={`${seg.label}: ${seg.value} ${defaultCenterLabel} (%${seg.percentage})`}
                  onFocus={() => setActiveId(seg.id)}
                  onBlur={() => setActiveId(null)}
                />
              );
            })}
        </svg>

        {/* Center content */}
        <div className="donut-chart__center">
          <span className="donut-chart__center-value">{activeSegment ? activeSegment.value : totalCount}</span>
          <span className="donut-chart__center-label">
            {activeSegment ? activeSegment.label : defaultCenterLabel}
          </span>
        </div>
      </div>

      {/* Legend List or Empty State */}
      {isEmpty ? (
        <div className="donut-chart__empty" role="status">
          <PieChart size={28} className="donut-chart__empty-icon" aria-hidden="true" />
          <span className="donut-chart__empty-text">{t('charts.noData', { defaultValue: 'Veri bulunamadı' })}</span>
          <span className="donut-chart__empty-subtext">{t('charts.emptyFilter', { defaultValue: 'Seçili filtrelere uygun proje verisi bulunamadı.' })}</span>
        </div>
      ) : (
        <div className="donut-chart__legend" role="list">
          {calculatedSegments.map((seg) => {
            const isHovered = activeId === seg.id;
            return (
              <div
                key={seg.id}
                role="listitem"
                className={`donut-chart__legend-item ${isHovered ? 'donut-chart__legend-item--active' : ''}`}
                onMouseEnter={() => setActiveId(seg.id)}
                onMouseLeave={() => setActiveId(null)}
                tabIndex={0}
                onFocus={() => setActiveId(seg.id)}
                onBlur={() => setActiveId(null)}
              >
                <div className="donut-chart__legend-left">
                  <span
                    className="donut-chart__legend-dot"
                    style={{ backgroundColor: seg.color }}
                    aria-hidden="true"
                  />
                  <span className="donut-chart__legend-label">{seg.label}</span>
                </div>
                <div className="donut-chart__legend-right">
                  <span className="donut-chart__legend-count">{seg.value}</span>
                  <span className="donut-chart__legend-percent">%{seg.percentage}</span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Screen-reader hidden summary table */}
      <table className="sr-only">
        <caption>{title}</caption>
        <thead>
          <tr>
            <th scope="col">{t('reports.table.group', 'Grup')}</th>
            <th scope="col">{t('reports.table.count', 'Adet')}</th>
            <th scope="col">{t('reports.table.ratio', 'Oran')}</th>
          </tr>
        </thead>
        <tbody>
          {isEmpty ? (
            <tr>
              <td colSpan={3}>{t('charts.noData', { defaultValue: 'Veri bulunamadı' })}</td>
            </tr>
          ) : (
            segments.map((seg) => (
              <tr key={`sr-${seg.id}`}>
                <td>{seg.label}</td>
                <td>{seg.value}</td>
                <td>%{seg.percentage}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

