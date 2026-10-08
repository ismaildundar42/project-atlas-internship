import { CheckCircle2, FolderKanban, PlusCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import type { ConfirmImportResponseDto } from '../../types/importExport';

interface Step4SuccessProps {
  confirmResult: ConfirmImportResponseDto;
  onReset: () => void;
}

export function Step4Success({ confirmResult, onReset }: Step4SuccessProps) {
  const { t, i18n } = useTranslation(['excel', 'common', 'projects']);
  const navigate = useNavigate();

  return (
    <div className="import-step-container">
      <Card padding="lg">
        <div className="import-success-box">
          <div className="import-success-icon" aria-hidden="true">
            <CheckCircle2 size={36} strokeWidth={2.5} />
          </div>

          <h3 className="import-success-title">{t('import.step4.title', { ns: 'excel' })}</h3>

          <p className="import-success-desc">
            {t('import.step4.subtitle', { count: confirmResult.importedCount, ns: 'excel' })}
          </p>

          <div
            style={{
              padding: '14px 18px',
              backgroundColor: 'var(--color-surface-subtle)',
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--radius-md)',
              maxWidth: '560px',
              textAlign: 'left',
              fontSize: 'var(--font-size-xs)',
              color: 'var(--color-text-secondary)',
              lineHeight: '1.5',
            }}
          >
            {i18n.language === 'en'
              ? 'Imported projects are saved as unpublished drafts. You can review them from the Project Management page, edit details, and submit them for administrator approval.'
              : 'İçe aktarılan projeler Proje Kütüphanesinde henüz yayınlanmamıştır. Projelerinizi Proje Yönetimi ekranından inceleyebilir, gerekiyorsa düzenleyip tek tek yönetici onayına (İncelemeye) gönderebilirsiniz.'}
          </div>

          <div className="import-success-actions">
            <Button
              variant="primary"
              size="lg"
              onClick={() => navigate('/admin/projects')}
            >
              <FolderKanban size={18} aria-hidden="true" /> {t('import.step4.goToProjectsBtn', { ns: 'excel' })}
            </Button>

            <Button variant="secondary" size="lg" onClick={onReset}>
              <PlusCircle size={18} aria-hidden="true" /> {t('import.step4.importAnotherBtn', { ns: 'excel' })}
            </Button>
          </div>
        </div>
      </Card>
    </div>
  );
}

export default Step4Success;
