import { type ButtonHTMLAttributes, type ReactNode } from 'react';
import Spinner from './Spinner';

// ─── Types ────────────────────────────────────────────────────────────────────

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** Görsel stil varyantı */
  variant?: ButtonVariant;
  /** Boyut */
  size?: ButtonSize;
  /** Yükleniyor durumu — içeriği gizler, spinner gösterir */
  isLoading?: boolean;
  /** Sol ikon */
  leftIcon?: ReactNode;
  /** Sağ ikon */
  rightIcon?: ReactNode;
  /** İçerik */
  children: ReactNode;
}

// ─── Component ────────────────────────────────────────────────────────────────

function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  leftIcon,
  rightIcon,
  children,
  disabled,
  className = '',
  ...props
}: ButtonProps) {
  const isDisabled = disabled || isLoading;

  const classes = [
    'btn',
    `btn--${variant}`,
    `btn--${size}`,
    isLoading ? 'btn--loading' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <button
      className={classes}
      disabled={isDisabled}
      aria-disabled={isDisabled}
      aria-busy={isLoading}
      {...props}
    >
      {isLoading ? (
        <>
          <Spinner size="sm" aria-hidden="true" />
          <span className="sr-only">Yükleniyor…</span>
          <span aria-hidden="true">{children}</span>
        </>
      ) : (
        <>
          {leftIcon && <span className="btn__icon btn__icon--left" aria-hidden="true">{leftIcon}</span>}
          {children}
          {rightIcon && <span className="btn__icon btn__icon--right" aria-hidden="true">{rightIcon}</span>}
        </>
      )}
    </button>
  );
}

export default Button;
export type { ButtonVariant, ButtonSize, ButtonProps };
