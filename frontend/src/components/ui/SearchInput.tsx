import { useId, useRef, type KeyboardEvent, type InputHTMLAttributes } from 'react';
import { Search, X } from 'lucide-react';

// ─── Types ────────────────────────────────────────────────────────────────────

interface SearchInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id' | 'type' | 'onKeyDown'> {
  /** Arama tetiklendiğinde çağrılır (Enter veya buton tıklaması) */
  onSearch?: (value: string) => void;
  /** Değer silindiğinde çağrılır */
  onClear?: () => void;
}

// ─── Component ────────────────────────────────────────────────────────────────

function SearchInput({
  onSearch,
  onClear,
  value,
  className = '',
  placeholder = 'Proje, teknoloji veya ekip ara…',
  ...props
}: SearchInputProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && onSearch) {
      onSearch((e.currentTarget as HTMLInputElement).value);
    }
    if (e.key === 'Escape') {
      onClear?.();
      inputRef.current?.blur();
    }
  };

  const handleClear = () => {
    onClear?.();
    inputRef.current?.focus();
  };

  const hasValue = value !== undefined && value !== '';

  return (
    <div className={`search-input ${className}`} role="search">
      <label htmlFor={inputId} className="sr-only">
        Arama
      </label>

      <span className="search-input__icon search-input__icon--left" aria-hidden="true">
        <Search size={16} />
      </span>

      <input
        ref={inputRef}
        id={inputId}
        type="search"
        role="searchbox"
        aria-label="Proje, teknoloji veya ekip ara"
        placeholder={placeholder}
        value={value}
        onKeyDown={handleKeyDown}
        className="search-input__control"
        autoComplete="off"
        {...props}
      />

      {hasValue && onClear && (
        <button
          type="button"
          className="search-input__clear"
          aria-label="Aramayı temizle"
          onClick={handleClear}
        >
          <X size={14} aria-hidden="true" />
        </button>
      )}
    </div>
  );
}

export default SearchInput;
export type { SearchInputProps };
