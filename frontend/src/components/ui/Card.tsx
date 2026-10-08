import { type ElementType, type HTMLAttributes, type ReactNode } from 'react';

// ─── Types ────────────────────────────────────────────────────────────────────

interface CardProps extends HTMLAttributes<HTMLElement> {
  children: ReactNode;
  /** Anlamsal HTML elementi */
  as?: ElementType;
  /** Özel dolgu stili */
  padding?: 'none' | 'sm' | 'md' | 'lg';
  /** Gölge/yükselme efekti */
  elevated?: boolean;
  /** Tıklanabilir kart görünümü */
  clickable?: boolean;
}

// ─── Component ────────────────────────────────────────────────────────────────

function Card({
  children,
  as: Tag = 'div',
  padding = 'md',
  elevated = false,
  clickable = false,
  className = '',
  ...props
}: CardProps) {
  const classes = [
    'card',
    `card--padding-${padding}`,
    elevated ? 'card--elevated' : '',
    clickable ? 'card--clickable' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <Tag className={classes} {...props}>
      {children}
    </Tag>
  );
}

export default Card;
export type { CardProps };
