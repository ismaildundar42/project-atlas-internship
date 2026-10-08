import { useState } from 'react';
import { MapPin } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { ReportLocation } from '../../types/report';

interface LocationDistributionChartProps {
  locations: ReportLocation[];
  totalProjects: number;
}

export function LocationDistributionChart({ locations, totalProjects }: LocationDistributionChartProps) {
  const { t } = useTranslation(['reports', 'common', 'projects', 'organization']);
  const [hoveredId, setHoveredId] = useState<number | null>(null);

  const formatLocationType = (type: string): string => {
    switch (type) {
      case 'MineSite':
        return t('organization.locations.types.mineSite', 'Maden Sahası');
      case 'Office':
        return t('organization.locations.types.office', 'Ofis');
      case 'Facility':
        return t('organization.locations.types.facility', 'Tesis / İşletme');
      case 'Other':
        return t('organization.locations.types.other', 'Diğer');
      default:
        return type;
    }
  };

  if (locations.length === 0) {
    return (
      <div className="report-empty-inline">
        <p>{t('reports.locationChart.empty', 'Lokasyon bilgisi atanmış proje bulunmamaktadır.')}</p>
      </div>
    );
  }

  const maxCount = Math.max(...locations.map((l) => l.projectCount), 1);

  return (
    <div className="location-chart-wrapper" role="region" aria-label={t('reports.locationChart.chartAria', 'Proje Lokasyonları Dağılım Grafiği')}>
      <div className="location-chart-list" role="list">
        {locations.map((loc) => {
          const widthPercent = Math.max((loc.projectCount / maxCount) * 100, 4);
          const portfolioPercent = totalProjects > 0 ? Math.round((loc.projectCount / totalProjects) * 100) : 0;
          const isHovered = hoveredId === loc.id;

          return (
            <div
              key={loc.id}
              role="listitem"
              className={`location-chart-row ${isHovered ? 'location-chart-row--active' : ''}`}
              onMouseEnter={() => setHoveredId(loc.id)}
              onMouseLeave={() => setHoveredId(null)}
              tabIndex={0}
              onFocus={() => setHoveredId(loc.id)}
              onBlur={() => setHoveredId(null)}
              aria-label={`${loc.name} (${formatLocationType(loc.type)}): ${loc.projectCount} ${t('projects.projectCountSuffix', 'proje')} (%${portfolioPercent})`}
            >
              {/* Left location identity */}
              <div className="location-chart-row__left">
                <div className="location-chart-row__icon" aria-hidden="true">
                  <MapPin size={15} />
                </div>
                <div className="location-chart-row__info">
                  <span className="location-chart-row__name" title={loc.name}>
                    {loc.name}
                  </span>
                  <span className="location-chart-row__type">{formatLocationType(loc.type)}</span>
                </div>
              </div>

              {/* Bar track and count */}
              <div className="location-chart-row__track-wrapper">
                <div className="location-chart-row__track">
                  <div
                    className="location-chart-row__bar"
                    style={{ width: `${widthPercent}%` }}
                  />
                </div>
                <div className="location-chart-row__metrics">
                  <span className="location-chart-row__count">{loc.projectCount} {t('projects.projectCountSuffix', 'proje')}</span>
                  <span className="location-chart-row__pct">%{portfolioPercent}</span>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Screen-reader table */}
      <table className="sr-only">
        <caption>{t('reports.sections.locationDistributionTitle', 'Lokasyon Dağılımı')}</caption>
        <thead>
          <tr>
            <th scope="col">{t('projects.filters.location', 'Lokasyon')}</th>
            <th scope="col">{t('reports.table.type', 'Tür')}</th>
            <th scope="col">{t('reports.table.projectCount', 'Proje Sayısı')}</th>
          </tr>
        </thead>
        <tbody>
          {locations.map((loc) => (
            <tr key={`sr-loc-${loc.id}`}>
              <td>{loc.name}</td>
              <td>{formatLocationType(loc.type)}</td>
              <td>{loc.projectCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default LocationDistributionChart;
