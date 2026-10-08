import { useId, type InputHTMLAttributes, type ReactNode } from 'react';

// ─── Types ────────────────────────────────────────────────────────────────────

interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id'> {
  /** Görüntülenen etiket metni */
  label: string;
  /** Hata mesajı */
  error?: string;
  /** Yardımcı ipucu metni */
  hint?: string;
  /** Sol ikon */
  leftIcon?: ReactNode;
  /** Sağ ikon */
  rightIcon?: ReactNode;
  /** Etiketi gizle (yalnızca screen reader için) */
  hideLabel?: boolean;
}

// ─── Component ────────────────────────────────────────────────────────────────

function Input({
  label,
  error,
  hint,
  leftIcon,
  rightIcon,
  hideLabel = false,
  disabled,
  className = '',
  ...props
}: InputProps) {
  const inputId = useId();
  const errorId = error ? `${inputId}-error` : undefined;
  const hintId = hint ? `${inputId}-hint` : undefined;

  const describedBy = [errorId, hintId].filter(Boolean).join(' ') || undefined;

  return (
    <div className={`input-field ${error ? 'input-field--error' : ''} ${disabled ? 'input-field--disabled' : ''} ${className}`}>
      <label
        htmlFor={inputId}
        className={`input-label ${hideLabel ? 'sr-only' : ''}`}
      >
        {label}
      </label>

      <div className="input-wrapper">
        {leftIcon && (
          <span className="input-icon input-icon--left" aria-hidden="true">
            {leftIcon}
          </span>
        )}

        <input
          id={inputId}
          disabled={disabled}
          aria-invalid={error ? 'true' : undefined}
          aria-describedby={describedBy}
          className={`input-control ${leftIcon ? 'input-control--left-icon' : ''} ${rightIcon ? 'input-control--right-icon' : ''}`}
          {...props}
        />

        {rightIcon && (
          <span className="input-icon input-icon--right" aria-hidden="true">
            {rightIcon}
          </span>
        )}
      </div>

      {hint && !error && (
        <p id={hintId} className="input-hint">
          {hint}
        </p>
      )}

      {error && (
        <p id={errorId} className="input-error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}

export default Input;
export type { InputProps };
