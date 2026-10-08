import { useEffect, useRef } from 'react';
import { X, RotateCcw } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAccessibility } from '../../context/AccessibilityContext';
import IconButton from '../ui/IconButton';

// ─── Types ────────────────────────────────────────────────────────────────────

interface AccessibilityPanelProps {
  isOpen: boolean;
  onClose: () => void;
}

// ─── Component ────────────────────────────────────────────────────────────────

function AccessibilityPanel({ isOpen, onClose }: AccessibilityPanelProps) {
  const { t } = useTranslation('common');
  const {
    highContrast,
    setHighContrast,
    fontSize,
    setFontSize,
    reducedMotion,
    setReducedMotion,
    enhancedFocus,
    setEnhancedFocus,
    resetPreferences,
  } = useAccessibility();

  const panelRef = useRef<HTMLDivElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  // Açılınca close butonuna focus
  useEffect(() => {
    if (isOpen) {
      closeButtonRef.current?.focus();
    }
  }, [isOpen]);

  // Escape ile kapat
  useEffect(() => {
    if (!isOpen) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [isOpen, onClose]);

  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div
          className="backdrop visible"
          onClick={onClose}
          aria-hidden="true"
          style={{ zIndex: 'var(--z-panel)' }}
        />
      )}

      {/* Panel */}
      <div
        ref={panelRef}
        className={`a11y-panel ${isOpen ? 'open' : ''}`}
        role="dialog"
        aria-modal="true"
        aria-label={t('accessibility.panelAria', 'Erişilebilirlik ayarları')}
        id="a11y-panel"
        style={{ zIndex: 'calc(var(--z-panel) + 1)' }}
      >
        {/* Panel Header */}
        <div className="a11y-panel__header">
          <h2 className="a11y-panel__title" id="a11y-panel-title">
            {t('accessibility.title', 'Erişilebilirlik')}
          </h2>
          <IconButton
            ref={closeButtonRef}
            icon={<X size={18} />}
            aria-label={t('accessibility.closePanel', 'Paneli kapat')}
            variant="default"
            onClick={onClose}
          />
        </div>

        {/* Panel Body */}
        <div className="a11y-panel__body" role="group" aria-labelledby="a11y-panel-title">

          {/* Yüksek Kontrast */}
          <div className="a11y-setting">
            <div className="a11y-setting__info">
              <label htmlFor="a11y-high-contrast" className="a11y-setting__label">
                {t('accessibility.highContrast', 'Yüksek Kontrast')}
              </label>
              <p className="a11y-setting__desc">
                {t('accessibility.highContrastDesc', 'Renk kontrastını artırarak metni daha okunaklı hale getirir.')}
              </p>
            </div>
            <button
              id="a11y-high-contrast"
              role="switch"
              aria-checked={highContrast}
              className={`a11y-toggle ${highContrast ? 'a11y-toggle--on' : ''}`}
              onClick={() => setHighContrast(!highContrast)}
              type="button"
            >
              <span className="a11y-toggle__thumb" />
              <span className="sr-only">{highContrast ? t('common.on', 'Açık') : t('common.off', 'Kapalı')}</span>
            </button>
          </div>

          {/* Metin Boyutu */}
          <div className="a11y-setting a11y-setting--col">
            <div className="a11y-setting__info">
              <p className="a11y-setting__label">{t('accessibility.fontSize', 'Metin Boyutu')}</p>
              <p className="a11y-setting__desc">
                {t('accessibility.fontSizeDesc', 'Uygulama genelindeki font boyutunu ayarlayın.')}
              </p>
            </div>
            <div
              className="a11y-font-options"
              role="radiogroup"
              aria-label={t('accessibility.fontSizeSelectionAria', 'Metin boyutu seçimi')}
            >
              {([
                { value: 'normal',  label: t('accessibility.fontSizeNormal', 'Normal'),    sample: 'A' },
                { value: 'large',   label: t('accessibility.fontSizeLarge', 'Büyük'),     sample: 'A' },
                { value: 'xlarge',  label: t('accessibility.fontSizeXLarge', 'Çok Büyük'), sample: 'A' },
              ] as const).map((opt) => (
                <button
                  key={opt.value}
                  role="radio"
                  aria-checked={fontSize === opt.value}
                  className={`a11y-font-btn ${fontSize === opt.value ? 'a11y-font-btn--selected' : ''} a11y-font-btn--${opt.value}`}
                  onClick={() => setFontSize(opt.value)}
                  type="button"
                >
                  <span className="a11y-font-btn__sample" aria-hidden="true">
                    {opt.sample}
                  </span>
                  <span className="a11y-font-btn__label">{opt.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Hareketleri Azalt */}
          <div className="a11y-setting">
            <div className="a11y-setting__info">
              <label htmlFor="a11y-reduced-motion" className="a11y-setting__label">
                {t('accessibility.reducedMotion', 'Hareketleri Azalt')}
              </label>
              <p className="a11y-setting__desc">
                {t('accessibility.reducedMotionDesc', 'Animasyon ve geçiş efektlerini azaltır.')}
              </p>
            </div>
            <button
              id="a11y-reduced-motion"
              role="switch"
              aria-checked={reducedMotion}
              className={`a11y-toggle ${reducedMotion ? 'a11y-toggle--on' : ''}`}
              onClick={() => setReducedMotion(!reducedMotion)}
              type="button"
            >
              <span className="a11y-toggle__thumb" />
              <span className="sr-only">{reducedMotion ? t('common.on', 'Açık') : t('common.off', 'Kapalı')}</span>
            </button>
          </div>

          {/* Odak Göstergeleri */}
          <div className="a11y-setting">
            <div className="a11y-setting__info">
              <label htmlFor="a11y-enhanced-focus" className="a11y-setting__label">
                {t('accessibility.enhancedFocus', 'Odak Göstergelerini Güçlendir')}
              </label>
              <p className="a11y-setting__desc">
                {t('accessibility.enhancedFocusDesc', 'Klavye ile gezinirken odak çerçevesini belirginleştirir.')}
              </p>
            </div>
            <button
              id="a11y-enhanced-focus"
              role="switch"
              aria-checked={enhancedFocus}
              className={`a11y-toggle ${enhancedFocus ? 'a11y-toggle--on' : ''}`}
              onClick={() => setEnhancedFocus(!enhancedFocus)}
              type="button"
            >
              <span className="a11y-toggle__thumb" />
              <span className="sr-only">{enhancedFocus ? t('common.on', 'Açık') : t('common.off', 'Kapalı')}</span>
            </button>
          </div>
        </div>

        {/* Panel Footer */}
        <div className="a11y-panel__footer">
          <button
            className="a11y-reset-btn"
            onClick={resetPreferences}
            type="button"
            aria-label={t('accessibility.resetPreferencesAria', 'Tüm erişilebilirlik tercihlerini varsayılana sıfırla')}
          >
            <RotateCcw size={14} aria-hidden="true" />
            <span>{t('accessibility.resetToDefault', 'Varsayılana Sıfırla')}</span>
          </button>
          <p className="a11y-panel__note">
            {t('accessibility.preferencesSavedLocally', 'Tercihler tarayıcınızda kaydedilir.')}
          </p>
        </div>
      </div>
    </>
  );
}

export default AccessibilityPanel;

