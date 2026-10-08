import {
  FolderKanban,
  CheckCircle2,
  Clock,
  CheckCheck,
  Star,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { ReportSummary } from '../../types/report';

interface ReportSummaryStripProps {
  summary: ReportSummary;
}

/**
 * Portföy KPI Özet Şeridi.
 * Dashboard'daki devasa KPI kartları yerine, raporlama sayfasının en üstünde
 * yer kaplamayan, kompakt ve kurumsal bir özet şerit sunar.
 */
export function ReportSummaryStrip({ summary }: ReportSummaryStripProps) {
  const { t } = useTranslation(['reports', 'common', 'projects']);

  return (
    <div className="report-summary-strip" role="region" aria-label={t('reports.portfolioSummaryAria', 'Portföy Özet İstatistikleri')}>
      {/* Toplam Proje */}
      <div className="report-summary-strip__item">
        <div className="report-summary-strip__icon report-summary-strip__icon--navy" aria-hidden="true">
          <FolderKanban size={18} />
        </div>
        <div className="report-summary-strip__text">
          <span className="report-summary-strip__value">{summary.totalProjects}</span>
          <span className="report-summary-strip__label">{t('reports.kpi.totalProjects', 'Toplam Proje')}</span>
        </div>
      </div>

      <div className="report-summary-strip__divider" aria-hidden="true" />

      {/* Aktif Proje */}
      <div className="report-summary-strip__item">
        <div className="report-summary-strip__icon report-summary-strip__icon--success" aria-hidden="true">
          <CheckCircle2 size={18} />
        </div>
        <div className="report-summary-strip__text">
          <span className="report-summary-strip__value">{summary.activeProjects}</span>
          <span className="report-summary-strip__label">{t('reports.kpi.active', 'Aktif')}</span>
        </div>
      </div>

      <div className="report-summary-strip__divider" aria-hidden="true" />

      {/* Devam Eden */}
      <div className="report-summary-strip__item">
        <div className="report-summary-strip__icon report-summary-strip__icon--warning" aria-hidden="true">
          <Clock size={18} />
        </div>
        <div className="report-summary-strip__text">
          <span className="report-summary-strip__value">{summary.inProgressProjects}</span>
          <span className="report-summary-strip__label">{t('reports.kpi.inProgress', 'Devam Eden')}</span>
        </div>
      </div>

      <div className="report-summary-strip__divider" aria-hidden="true" />

      {/* Tamamlanan */}
      <div className="report-summary-strip__item">
        <div className="report-summary-strip__icon report-summary-strip__icon--blue" aria-hidden="true">
          <CheckCheck size={18} />
        </div>
        <div className="report-summary-strip__text">
          <span className="report-summary-strip__value">{summary.completedProjects}</span>
          <span className="report-summary-strip__label">{t('reports.kpi.completed', 'Tamamlanan')}</span>
        </div>
      </div>

      <div className="report-summary-strip__divider" aria-hidden="true" />

      {/* Öne Çıkan */}
      <div className="report-summary-strip__item">
        <div className="report-summary-strip__icon report-summary-strip__icon--red" aria-hidden="true">
          <Star size={18} />
        </div>
        <div className="report-summary-strip__text">
          <span className="report-summary-strip__value">{summary.featuredProjects}</span>
          <span className="report-summary-strip__label">{t('reports.kpi.featured', 'Öne Çıkan')}</span>
        </div>
      </div>
    </div>
  );
}

