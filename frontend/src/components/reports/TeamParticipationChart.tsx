import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ReportTeam } from '../../types/report';

interface TeamParticipationChartProps {
  teams: ReportTeam[];
  totalProjects: number;
}

export function TeamParticipationChart({ teams, totalProjects }: TeamParticipationChartProps) {
  const { t } = useTranslation(['reports', 'common', 'projects']);
  const [hoveredId, setHoveredId] = useState<number | null>(null);

  if (teams.length === 0) {
    return (
      <div className="report-empty-inline">
        <p>{t('reports.teamChart.empty', 'Ekip ataması yapılmış proje bulunmamaktadır.')}</p>
      </div>
    );
  }

  const maxCount = Math.max(...teams.map((tItem) => tItem.projectCount), 1);

  return (
    <div className="team-part-wrapper" role="region" aria-label={t('reports.teamChart.chartAria', 'Ekiplerin Proje Katılım Grafiği')}>
      <div className="team-part-list" role="list">
        {teams.map((team) => {
          const widthPercent = Math.max((team.projectCount / maxCount) * 100, 4);
          const portfolioPercent = totalProjects > 0 ? Math.round((team.projectCount / totalProjects) * 100) : 0;
          const isHovered = hoveredId === team.id;

          return (
            <div
              key={team.id}
              role="listitem"
              className={`team-part-row ${isHovered ? 'team-part-row--active' : ''}`}
              onMouseEnter={() => setHoveredId(team.id)}
              onMouseLeave={() => setHoveredId(null)}
              tabIndex={0}
              onFocus={() => setHoveredId(team.id)}
              onBlur={() => setHoveredId(null)}
              aria-label={`${team.name} (${team.departmentName}): ${team.projectCount} ${t('reports.teamChart.tasksInProjects', 'projede görev alıyor')} (${team.primaryProjectCount} ${t('reports.teamChart.primaryDuty', 'ana sorumluluk')})`}
            >
              {/* Left Identity */}
              <div className="team-part-row__left">
                <span className="team-part-row__name" title={team.name}>
                  {team.name}
                </span>
                <span className="team-part-row__dept">{team.departmentName}</span>
              </div>

              {/* Bar track and metrics */}
              <div className="team-part-row__track-wrapper">
                <div className="team-part-row__track">
                  <div
                    className="team-part-row__bar"
                    style={{ width: `${widthPercent}%` }}
                  />
                </div>
                <div className="team-part-row__metrics">
                  <span className="team-part-row__count">{team.projectCount} {t('projects.projectCountSuffix', 'proje')}</span>
                  {team.primaryProjectCount > 0 && (
                    <span className="team-part-row__primary-badge">
                      {team.primaryProjectCount} {t('reports.teamChart.primaryTeam', 'ana ekip')}
                    </span>
                  )}
                  <span className="team-part-row__pct">%{portfolioPercent}</span>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Screen-reader table */}
      <table className="sr-only">
        <caption>{t('reports.sections.teamParticipationTitle', 'Ekip Katılım Dağılımı')}</caption>
        <thead>
          <tr>
            <th scope="col">{t('projects.filters.team', 'Ekip')}</th>
            <th scope="col">{t('reports.table.department', 'Departman')}</th>
            <th scope="col">{t('reports.table.totalProjects', 'Toplam Proje')}</th>
            <th scope="col">{t('reports.table.primaryLeadProjects', 'Ana Sorumlu Proje')}</th>
          </tr>
        </thead>
        <tbody>
          {teams.map((team) => (
            <tr key={`sr-team-${team.id}`}>
              <td>{team.name}</td>
              <td>{team.departmentName}</td>
              <td>{team.projectCount}</td>
              <td>{team.primaryProjectCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default TeamParticipationChart;
