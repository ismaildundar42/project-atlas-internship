import { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Lock, X, Send, AlertCircle, CheckCircle2 } from 'lucide-react';
import Button from '../ui/Button';
import IconButton from '../ui/IconButton';
import type { ApplicationModule } from '../../types/moduleAccess';

interface AccessRequestModalProps {
  isOpen: boolean;
  moduleKey: 'reports' | 'teams' | string;
  moduleName?: string;
  onClose: () => void;
  onSubmit: (module: ApplicationModule, reason?: string) => Promise<void>;
  isPending?: boolean;
}

export function AccessRequestModal({
  isOpen,
  moduleKey,
  moduleName,
  onClose,
  onSubmit,
  isPending = false,
}: AccessRequestModalProps) {
  const { t } = useTranslation(['access', 'common']);
  const [reason, setReason] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const dialogRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  const normalizedModule: ApplicationModule =
    moduleKey.toLowerCase() === 'reports' ? 'Reports' : 'Teams';

  const defaultModuleName =
    normalizedModule === 'Reports'
      ? t('access.modules.reports', 'Raporlama')
      : t('access.modules.teams', 'Ekipler');

  const resolvedModuleName = moduleName || defaultModuleName;

  useEffect(() => {
    if (isOpen) {
      setReason('');
      setError(null);
      setSuccess(false);
      setIsSubmitting(false);

      const timer = setTimeout(() => {
        if (!isPending) {
          textareaRef.current?.focus();
        } else {
          closeButtonRef.current?.focus();
        }
      }, 50);

      document.body.style.overflow = 'hidden';

      return () => {
        clearTimeout(timer);
        document.body.style.overflow = '';
      };
    }
  }, [isOpen, isPending]);

  // Escape key listener
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSubmitting || isPending) return;

    setIsSubmitting(true);
    setError(null);

    try {
      await onSubmit(normalizedModule, reason.trim() || undefined);
      setSuccess(true);
      setTimeout(() => {
        onClose();
      }, 1500);
    } catch (err: any) {
      const serverDetail = err?.response?.data?.detail || err?.response?.data?.message;
      if (serverDetail && typeof serverDetail === 'string' && !serverDetail.startsWith('Request failed')) {
        setError(serverDetail);
      } else {
        setError(t('access.requestFailed', 'Erişim talebi iletilemedi. Lütfen tekrar deneyin.'));
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="editor-modal-overlay"
      role="dialog"
      aria-modal="true"
      aria-labelledby="access-request-title"
      onClick={(e) => {
        if (e.target === e.currentTarget && !isSubmitting) onClose();
      }}
    >
      <div className="editor-modal" style={{ maxWidth: '480px' }} ref={dialogRef}>
        {/* Header */}
        <div className="editor-modal__header" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
            <div
              style={{
                width: '36px',
                height: '36px',
                borderRadius: '50%',
                backgroundColor: 'var(--color-warning-bg)',
                color: 'var(--color-warning)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Lock size={18} aria-hidden="true" />
            </div>
            <h3
              id="access-request-title"
              className="editor-modal__title"
            >
              {resolvedModuleName} {t('access.requiredTitle', 'Erişimi Gerekli')}
            </h3>
          </div>
          <IconButton
            ref={closeButtonRef}
            icon={<X size={18} />}
            aria-label={t('common.close', 'Kapat')}
            variant="ghost"
            size="sm"
            onClick={onClose}
          />
        </div>

        {/* Content */}
        <div className="editor-modal__body">
          {success ? (
            <div style={{ textAlign: 'center', padding: 'var(--space-4) 0' }}>
              <CheckCircle2 size={48} color="var(--color-success)" style={{ margin: '0 auto 16px' }} />
              <h3 style={{ fontSize: 'var(--font-size-base)', fontWeight: 600, color: 'var(--color-text-primary)', marginBottom: '8px' }}>
                {t('access.requestSentTitle', 'Talebiniz Alındı')}
              </h3>
              <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', margin: 0 }}>
                {t('access.requestSentDesc', 'Erişim talebiniz yöneticilere iletildi. Onaylandığında bildirim alacaksınız.')}
              </p>
            </div>
          ) : isPending ? (
            <div style={{ textAlign: 'center', padding: 'var(--space-4) 0' }}>
              <div
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '8px',
                  padding: '6px 14px',
                  borderRadius: '999px',
                  backgroundColor: 'var(--color-info-bg)',
                  color: 'var(--color-info-text)',
                  border: '1px solid var(--color-info-border)',
                  fontSize: 'var(--font-size-sm)',
                  fontWeight: 600,
                  marginBottom: '16px',
                }}
              >
                {t('access.pendingBadge', 'İnceleme Bekliyor')}
              </div>
              <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', lineHeight: 1.5, margin: 0 }}>
                {t('access.pendingDesc', 'Bu modül için daha önce ilettiğiniz bir erişim talebi bulunmaktadır. Yöneticilerin incelemesi tamamlandığında alan kullanıma açılacaktır.')}
              </p>
            </div>
          ) : (
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', lineHeight: 1.5, margin: 0 }}>
                {t('access.modalIntro', 'Bu alanı görüntüleme yetkiniz bulunmuyor. Modülü kullanabilmek için yöneticilerden erişim talep edebilirsiniz.')}
              </p>

              {error && (
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px',
                    padding: '10px 14px',
                    borderRadius: 'var(--radius-md)',
                    backgroundColor: 'var(--color-danger-bg)',
                    border: '1px solid var(--color-danger-border)',
                    color: 'var(--color-danger-text)',
                    fontSize: 'var(--font-size-xs)',
                  }}
                >
                  <AlertCircle size={16} aria-hidden="true" />
                  <span>{error}</span>
                </div>
              )}

              <div className="form-group">
                <label
                  htmlFor="access-request-reason"
                  className="form-label"
                >
                  {t('access.reasonLabel', 'Gerekçe / Açıklama (İsteğe bağlı)')}
                </label>
                <textarea
                  id="access-request-reason"
                  ref={textareaRef}
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  placeholder={t('access.reasonPlaceholder', 'Örn: Çevre projelerinin aylık metrik raporlarını incelemem gerekiyor.')}
                  rows={3}
                  maxLength={500}
                  disabled={isSubmitting}
                  className="form-textarea"
                />
              </div>

              {/* Actions */}
              <div className="editor-modal__footer" style={{ margin: '0 -20px -20px -20px', padding: '14px 20px' }}>
                <Button
                  type="button"
                  variant="ghost"
                  size="md"
                  onClick={onClose}
                  disabled={isSubmitting}
                >
                  {t('common.cancel', 'Vazgeç')}
                </Button>
                <Button
                  type="submit"
                  variant="primary"
                  size="md"
                  isLoading={isSubmitting}
                >
                  <Send size={16} aria-hidden="true" />
                  {t('access.submitRequest', 'Erişim Talebi Gönder')}
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}

export default AccessRequestModal;
