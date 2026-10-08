import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import trResources from './resources/tr';
import enResources from './resources/en';

export const LANGUAGE_STORAGE_KEY = 'demir-export-language';
export const SUPPORTED_LANGUAGES = ['tr', 'en'] as const;
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

export function normalizeLanguage(lang?: string | null): SupportedLanguage {
  if (!lang) return 'tr';
  const clean = lang.trim().toLowerCase();
  if (clean.startsWith('en')) return 'en';
  if (clean.startsWith('tr')) return 'tr';
  return 'tr';
}

function getInitialLanguage(): SupportedLanguage {
  try {
    const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY);
    if (stored) {
      return normalizeLanguage(stored);
    }
  } catch {
    // localStorage might be restricted in some environments
  }
  return 'tr';
}

const initialLang = getInitialLanguage();

// Ensure document lang is synchronized immediately on script load
if (typeof document !== 'undefined') {
  document.documentElement.lang = initialLang;
}

// Combined translation dictionary for root/dot-path access
const trBundle = {
  ...trResources,
  ...trResources.common,
};

const enBundle = {
  ...enResources,
  ...enResources.common,
};

i18n
  .use(initReactI18next)
  .init({
    resources: {
      tr: {
        translation: trBundle,
        ...trResources,
      },
      en: {
        translation: enBundle,
        ...enResources,
      },
    },
    lng: initialLang,
    fallbackLng: 'tr',
    defaultNS: 'translation',
    fallbackNS: [
      'translation',
      'common',
      'navigation',
      'projects',
      'workflow',
      'notifications',
      'organization',
      'reports',
      'excel',
      'validation',
      'profile',
    ],
    interpolation: {
      escapeValue: false, // React already escapes values
    },
    react: {
      useSuspense: false,
      bindI18n: 'languageChanged loaded',
      bindI18nStore: 'added removed',
    },
  });

i18n.on('languageChanged', (lng: string) => {
  const safeLang = normalizeLanguage(lng);

  try {
    localStorage.setItem(LANGUAGE_STORAGE_KEY, safeLang);
  } catch {
    // Ignore storage failure
  }

  if (typeof document !== 'undefined') {
    document.documentElement.lang = safeLang;
  }
});

export async function changeAppLanguage(lang: string): Promise<void> {
  const safeLang = normalizeLanguage(lang);
  await i18n.changeLanguage(safeLang);
  try {
    localStorage.setItem(LANGUAGE_STORAGE_KEY, safeLang);
  } catch {
    // Ignore storage failure
  }
  if (typeof document !== 'undefined') {
    document.documentElement.lang = safeLang;
  }
}

export default i18n;
