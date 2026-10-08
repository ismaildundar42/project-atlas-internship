import { useEffect } from 'react';
import { AlertTriangle, CheckCircle, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Button from '../../components/ui/Button';

interface ConfirmImportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isConfirming: boolean;
  projectCount: number;
}

export function ConfirmImportModal({
  isOpen,
  onClose,
  onConfirm,
  isConfirming,
  projectCount,
}: ConfirmImportModalProps) {
  const { t, i18n } = useTranslation(['excel', 'common']);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !isConfirming) {
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isConfirming, onClose]);

  if (!isOpen) return null;

  return (
    <div className="editor-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="confirm-import-title">
      <div className="editor-modal" style={{ maxWidth: '540px' }}>
        <div className="editor-modal__header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <CheckCircle size={20} color="var(--color-brand-navy)" aria-hidden="true" />
            <h3 id="confirm-import-title" className="editor-modal__title">
              {t('import.modal.title', { ns: 'excel' })}
            </h3>
          </div>
          <button
            type="button"
            className="btn btn--ghost btn--sm"
            onClick={onClose}
            disabled={isConfirming}
            aria-label={t('actions.close', { ns: 'common' })}
            style={{ padding: '4px' }}
          >
            <X size={18} />
          </button>
        </div>

        <div className="editor-modal__body">
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-primary)', margin: 0, lineHeight: varLineHeight() }}>
            {t('import.modal.message', { count: projectCount, ns: 'excel' })}
          </p>

          <div
            style={{
              display: 'flex',
              alignItems: 'flex-start',
              gap: '12px',
              padding: '12px 14px',
              backgroundColor: 'var(--color-surface-subtle)',
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--radius-md)',
            }}
          >
            <AlertTriangle size={18} color="var(--color-warning)" style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
            <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', lineHeight: '1.4' }}>
              {i18n.language === 'en'
                ? 'Projects will not be published or submitted for review automatically. Once imported, you can review each record and submit for review individually.'
                : 'Projeler otomatik olarak yayınlanmayacak veya incelemeye gönderilmeyecektir. İçe aktarım tamamlandıktan sonra projelerinizi kontrol edip tek tek incelemeye gönderebilirsiniz.'}
            </div>
          </div>
        </div>

        <div className="editor-modal__footer">
          <Button variant="ghost" size="md" onClick={onClose} disabled={isConfirming}>
            {t('import.modal.cancelBtn', { ns: 'excel' })}
          </Button>
          <Button
            variant="primary"
            size="md"
            onClick={onConfirm}
            isLoading={isConfirming}
          >
            {t('import.modal.confirmBtn', { ns: 'excel' })} ({projectCount} {i18n.language === 'en' ? 'Projects' : 'Proje'})
          </Button>
        </div>
      </div>
    </div>
  );
}

function varLineHeight() {
  return 'var(--line-height-normal)';
}

export default ConfirmImportModal;
