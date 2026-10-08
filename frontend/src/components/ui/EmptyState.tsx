import { type ReactNode } from 'react';
import { FolderOpen } from 'lucide-react';
import Button from './Button';

// ─── Types ────────────────────────────────────────────────────────────────────

interface EmptyStateProps {
  /** Opsiyonel özel ikon */
  icon?: ReactNode;
  /** Başlık */
  title: string;
  /** Açıklama metni */
  description?: string;
  /** İsteğe bağlı aksiyon butonu etiketi */
  actionLabel?: string;
  /** Aksiyon geri çağrısı */
  onAction?: () => void;
  className?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

function EmptyState({
  icon,
  title,
  description,
  actionLabel,
  onAction,
  className = '',
}: EmptyStateProps) {
  return (
    <div
      className={`empty-state ${className}`}
      role="status"
      aria-live="polite"
    >
      <div className="empty-state__icon" aria-hidden="true">
        {icon ?? <FolderOpen size={48} strokeWidth={1.25} />}
      </div>

      <h2 className="empty-state__title">{title}</h2>

      {description && (
        <p className="empty-state__description">{description}</p>
      )}

      {actionLabel && onAction && (
        <Button variant="secondary" onClick={onAction} className="empty-state__action">
          {actionLabel}
        </Button>
      )}
    </div>
  );
}

export default EmptyState;
export type { EmptyStateProps };
