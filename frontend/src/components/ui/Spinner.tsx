// ─── Types ────────────────────────────────────────────────────────────────────

type SpinnerSize = 'sm' | 'md' | 'lg';

interface SpinnerProps {
  size?: SpinnerSize;
  /** Ekran okuyucular için açıklama (varsayılan: "Yükleniyor") */
  'aria-label'?: string;
  className?: string;
}

const sizeMap: Record<SpinnerSize, number> = {
  sm: 16,
  md: 24,
  lg: 36,
};

// ─── Component ────────────────────────────────────────────────────────────────

function Spinner({
  size = 'md',
  'aria-label': ariaLabel = 'Yükleniyor',
  className = '',
  ...props
}: SpinnerProps) {
  const px = sizeMap[size];

  return (
    <span
      role="status"
      aria-label={ariaLabel}
      className={`spinner spinner--${size} ${className}`}
      {...props}
    >
      <svg
        width={px}
        height={px}
        viewBox="0 0 24 24"
        fill="none"
        aria-hidden="true"
        focusable="false"
      >
        <circle
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeDasharray="31.4"
          strokeDashoffset="10"
        />
      </svg>
    </span>
  );
}

export default Spinner;
export type { SpinnerSize, SpinnerProps };
