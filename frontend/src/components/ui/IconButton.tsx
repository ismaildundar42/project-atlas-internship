import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react';

// ─── Types ────────────────────────────────────────────────────────────────────

type IconButtonVariant = 'default' | 'ghost' | 'danger';
type IconButtonSize = 'sm' | 'md' | 'lg';

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** Erişilebilir ad — ZORUNLU: ekran okuyucular bu metni okur */
  'aria-label': string;
  /** Görüntülenecek ikon */
  icon: ReactNode;
  /** Görsel stil varyantı */
  variant?: IconButtonVariant;
  /** Boyut */
  size?: IconButtonSize;
  /** Aktif/seçili durum */
  isActive?: boolean;
}

// ─── Component ────────────────────────────────────────────────────────────────

const IconButton = forwardRef<HTMLButtonElement, IconButtonProps>(function IconButton(
  {
    icon,
    variant = 'default',
    size = 'md',
    isActive = false,
    disabled,
    className = '',
    ...props
  },
  ref
) {
  const classes = [
    'icon-btn',
    `icon-btn--${variant}`,
    `icon-btn--${size}`,
    isActive ? 'icon-btn--active' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <button
      ref={ref}
      className={classes}
      disabled={disabled}
      aria-pressed={isActive}
      {...props}
    >
      <span aria-hidden="true">{icon}</span>
    </button>
  );
});

IconButton.displayName = 'IconButton';

export default IconButton;
export type { IconButtonVariant, IconButtonSize, IconButtonProps };
