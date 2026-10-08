import { type ReactNode } from 'react';

// ─── Types ────────────────────────────────────────────────────────────────────

type BadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info' | 'navy';
type BadgeSize = 'sm' | 'md';

interface BadgeProps {
  children: ReactNode;
  variant?: BadgeVariant;
  size?: BadgeSize;
  /** İkon (sol) */
  icon?: ReactNode;
  className?: string;
  title?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

function Badge({
  children,
  variant = 'default',
  size = 'md',
  icon,
  className = '',
  title,
}: BadgeProps) {
  const classes = [
    'badge',
    `badge--${variant}`,
    `badge--${size}`,
    className,
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <span className={classes} title={title}>
      {icon && <span className="badge__icon" aria-hidden="true">{icon}</span>}
      {children}
    </span>
  );
}

export default Badge;
export type { BadgeVariant, BadgeSize, BadgeProps };
