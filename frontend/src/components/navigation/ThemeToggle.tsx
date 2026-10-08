import { useState, useRef, useEffect, forwardRef } from 'react';
import { Sun, Moon, Monitor, Check } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useTheme, type ThemeMode } from '../../context/ThemeContext';
import IconButton from '../ui/IconButton';

export const ThemeToggle = forwardRef<HTMLButtonElement>((_, ref) => {
  const { t } = useTranslation('common');
  const { theme, resolvedTheme, setTheme } = useTheme();
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Popover dışına tıklandığında veya Escape'e basıldığında kapat
  useEffect(() => {
    if (!isOpen) return;

    const handlePointerDown = (e: PointerEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setIsOpen(false);
      }
    };

    document.addEventListener('pointerdown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('pointerdown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  const handleSelect = (mode: ThemeMode) => {
    setTheme(mode);
    setIsOpen(false);
  };

  // Tetikleyici ikon
  const getTriggerIcon = () => {
    if (theme === 'system') {
      return <Monitor size={18} />;
    }
    return resolvedTheme === 'dark' ? <Moon size={18} /> : <Sun size={18} />;
  };

  const options: Array<{ mode: ThemeMode; label: string; icon: React.ReactNode }> = [
    { mode: 'light',  label: t('theme.light', 'Açık'),   icon: <Sun size={16} /> },
    { mode: 'dark',   label: t('theme.dark', 'Koyu'),   icon: <Moon size={16} /> },
    { mode: 'system', label: t('theme.system', 'Sistem'), icon: <Monitor size={16} /> },
  ];

  return (
    <div className="theme-toggle-container" ref={containerRef} style={{ position: 'relative' }}>
      <IconButton
        ref={ref}
        icon={getTriggerIcon()}
        aria-label={t('theme.selectTheme', 'Görünüm ve Tema Seçimi')}
        aria-haspopup="true"
        aria-expanded={isOpen}
        aria-controls="theme-popover-menu"
        isActive={isOpen}
        variant="ghost"
        size="md"
        id="theme-toggle-btn"
        onClick={() => setIsOpen((prev) => !prev)}
      />

      {isOpen && (
        <div
          id="theme-popover-menu"
          className="theme-popover"
          role="menu"
          aria-label={t('theme.themeOptionsAria', 'Görünüm Teması Seçenekleri')}
        >
          <div className="theme-popover__header">
            <span>{t('theme.theme', 'Görünüm Teması')}</span>
          </div>

          <div className="theme-popover__list">
            {options.map((opt) => {
              const isSelected = theme === opt.mode;
              return (
                <button
                  key={opt.mode}
                  type="button"
                  role="menuitemradio"
                  aria-checked={isSelected}
                  className={`theme-popover__item ${isSelected ? 'theme-popover__item--selected' : ''}`}
                  onClick={() => handleSelect(opt.mode)}
                >
                  <span className="theme-popover__item-icon" aria-hidden="true">
                    {opt.icon}
                  </span>
                  <span className="theme-popover__item-label">{opt.label}</span>
                  {isSelected && (
                    <span className="theme-popover__item-check" aria-hidden="true">
                      <Check size={14} />
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
});

ThemeToggle.displayName = 'ThemeToggle';

export default ThemeToggle;

