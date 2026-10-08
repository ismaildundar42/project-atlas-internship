import { AlertCircle } from 'lucide-react';
import Button from './Button';

// ─── Types ────────────────────────────────────────────────────────────────────

interface ErrorStateProps {
  /** Hata başlığı */
  title?: string;
  /** Hata açıklaması */
  description?: string;
  /** Yeniden dene butonu etiketi */
  retryLabel?: string;
  /** Yeniden deneme geri çağrısı */
  onRetry?: () => void;
  className?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

function ErrorState({
  title = 'Bir hata oluştu',
  description = 'İşlem gerçekleştirilirken beklenmeyen bir sorunla karşılaşıldı.',
  retryLabel = 'Yeniden Dene',
  onRetry,
  className = '',
}: ErrorStateProps) {
  return (
    <div
      className={`error-state ${className}`}
      role="alert"
      aria-live="assertive"
    >
      <div className="error-state__icon" aria-hidden="true">
        <AlertCircle size={48} strokeWidth={1.25} />
      </div>

      <h2 className="error-state__title">{title}</h2>

      <p className="error-state__description">{description}</p>

      {onRetry && (
        <Button variant="secondary" onClick={onRetry} className="error-state__action">
          {retryLabel}
        </Button>
      )}
    </div>
  );
}

export default ErrorState;
export type { ErrorStateProps };
