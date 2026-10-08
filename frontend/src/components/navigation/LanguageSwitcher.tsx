import React from 'react';
import { useTranslation } from 'react-i18next';
import { changeAppLanguage, normalizeLanguage, type SupportedLanguage } from '../../i18n';
import './LanguageSwitcher.css';

interface LanguageSwitcherProps {
  className?: string;
  variant?: 'header' | 'compact' | 'drawer' | 'segmented';
}

export const LanguageSwitcher: React.FC<LanguageSwitcherProps> = ({
  className = '',
  variant = 'header',
}) => {
  const { i18n, t } = useTranslation('common');
  const activeRaw = i18n.resolvedLanguage || i18n.language || 'tr';
  const currentLang = normalizeLanguage(activeRaw);

  const handleToggle = async () => {
    const nextLang: SupportedLanguage = currentLang === 'tr' ? 'en' : 'tr';
    await changeAppLanguage(nextLang);
  };

  const handleSetLanguage = async (e: React.MouseEvent, lang: SupportedLanguage) => {
    e.stopPropagation();
    if (lang !== currentLang) {
      await changeAppLanguage(lang);
    } else {
      // If clicking already active language, also toggle like a physical switch
      await handleToggle();
    }
  };

  return (
    <div
      className={`lang-switcher lang-switcher--${variant} lang-switcher--${currentLang} ${className}`}
      role="group"
      aria-label={t('language.selectLanguage', 'Dil seçimi')}
      onClick={handleToggle}
      title={currentLang === 'tr' ? 'Switch to English' : "Türkçe'ye geç"}
    >
      {/* Animated Sliding Thumb / Indicator */}
      <div className="lang-switcher__thumb" aria-hidden="true" />

      <button
        type="button"
        id="lang-switcher-tr"
        className={`lang-switcher__btn ${currentLang === 'tr' ? 'lang-switcher__btn--active' : ''}`}
        onClick={(e) => { void handleSetLanguage(e, 'tr'); }}
        aria-pressed={currentLang === 'tr'}
        aria-label={t('language.turkish', 'Türkçe')}
      >
        TR
      </button>

      <button
        type="button"
        id="lang-switcher-en"
        className={`lang-switcher__btn ${currentLang === 'en' ? 'lang-switcher__btn--active' : ''}`}
        onClick={(e) => { void handleSetLanguage(e, 'en'); }}
        aria-pressed={currentLang === 'en'}
        aria-label={t('language.english', 'English')}
      >
        EN
      </button>
    </div>
  );
};

export default LanguageSwitcher;
