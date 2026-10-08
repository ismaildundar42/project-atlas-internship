// ─── Types ────────────────────────────────────────────────────────────────────

type SkeletonVariant = 'text' | 'rect' | 'circle';

interface SkeletonProps {
  variant?: SkeletonVariant;
  width?: string | number;
  height?: string | number;
  className?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

function Skeleton({
  variant = 'rect',
  width,
  height,
  className = '',
}: SkeletonProps) {
  const style: React.CSSProperties = {};
  if (width !== undefined) style.width = typeof width === 'number' ? `${width}px` : width;
  if (height !== undefined) style.height = typeof height === 'number' ? `${height}px` : height;

  return (
    <span
      className={`skeleton skeleton--${variant} ${className}`}
      style={style}
      aria-hidden="true"
      role="presentation"
    />
  );
}

// ─── Kart iskelet yardımcıları ────────────────────────────────────────────────

function SkeletonCard() {
  return (
    <div className="skeleton-card" aria-hidden="true" role="presentation">
      <Skeleton variant="rect" height={160} />
      <div className="skeleton-card__body">
        <Skeleton variant="text" width="60%" height={18} />
        <Skeleton variant="text" width="90%" height={14} />
        <Skeleton variant="text" width="75%" height={14} />
      </div>
    </div>
  );
}

Skeleton.Card = SkeletonCard;

export default Skeleton;
export type { SkeletonVariant, SkeletonProps };
