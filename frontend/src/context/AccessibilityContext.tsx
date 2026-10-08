import {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  type ReactNode,
} from 'react';

// ─── Types ────────────────────────────────────────────────────────────────────

type FontSize = 'normal' | 'large' | 'xlarge';

interface AccessibilityPreferences {
  /** Yüksek kontrast modu */
  highContrast: boolean;
  /** Metin boyutu tercihi */
  fontSize: FontSize;
  /**
   * Animasyon ve geçiş efektlerini azalt.
   * İlk değer sistem prefers-reduced-motion'dan okunur.
   */
  reducedMotion: boolean;
  /** Odak göstergelerini belirgin hale getir */
  enhancedFocus: boolean;
}

interface AccessibilityContextValue extends AccessibilityPreferences {
  setHighContrast: (value: boolean) => void;
  setFontSize: (value: FontSize) => void;
  setReducedMotion: (value: boolean) => void;
  setEnhancedFocus: (value: boolean) => void;
  resetPreferences: () => void;
}

// ─── Storage Key ──────────────────────────────────────────────────────────────

const STORAGE_KEY = 'de-a11y-preferences';

// ─── Default Values ───────────────────────────────────────────────────────────

function getSystemReducedMotion(): boolean {
  if (typeof window === 'undefined') return false;
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

const defaultPreferences: AccessibilityPreferences = {
  highContrast: false,
  fontSize: 'normal',
  reducedMotion: getSystemReducedMotion(),
  enhancedFocus: false,
};

// ─── Load from localStorage ───────────────────────────────────────────────────

function loadPreferences(): AccessibilityPreferences {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (!stored) return { ...defaultPreferences, reducedMotion: getSystemReducedMotion() };

    const parsed: Partial<AccessibilityPreferences> = JSON.parse(stored);
    return {
      highContrast: typeof parsed.highContrast === 'boolean' ? parsed.highContrast : defaultPreferences.highContrast,
      fontSize:
        parsed.fontSize === 'normal' || parsed.fontSize === 'large' || parsed.fontSize === 'xlarge'
          ? parsed.fontSize
          : defaultPreferences.fontSize,
      // localStorage değeri yoksa sistem tercihini kullan
      reducedMotion: typeof parsed.reducedMotion === 'boolean' ? parsed.reducedMotion : getSystemReducedMotion(),
      enhancedFocus: typeof parsed.enhancedFocus === 'boolean' ? parsed.enhancedFocus : defaultPreferences.enhancedFocus,
    };
  } catch {
    return { ...defaultPreferences, reducedMotion: getSystemReducedMotion() };
  }
}

// ─── Apply to DOM ─────────────────────────────────────────────────────────────

function applyPreferencesToDom(prefs: AccessibilityPreferences): void {
  const root = document.documentElement;

  root.setAttribute('data-contrast', prefs.highContrast ? 'high' : 'normal');
  root.setAttribute('data-font-size', prefs.fontSize);
  root.setAttribute('data-reduced-motion', prefs.reducedMotion ? 'true' : 'false');
  root.setAttribute('data-enhanced-focus', prefs.enhancedFocus ? 'true' : 'false');
}

// ─── Context ──────────────────────────────────────────────────────────────────

const AccessibilityContext = createContext<AccessibilityContextValue | null>(null);

// ─── Provider ─────────────────────────────────────────────────────────────────

interface AccessibilityProviderProps {
  children: ReactNode;
}

export function AccessibilityProvider({ children }: AccessibilityProviderProps) {
  const [prefs, setPrefs] = useState<AccessibilityPreferences>(loadPreferences);

  // DOM attribute'larını ve localStorage'ı güncelle
  useEffect(() => {
    applyPreferencesToDom(prefs);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
    } catch {
      // localStorage kullanılamıyorsa sessizce devam et
    }
  }, [prefs]);

  // Sistem prefers-reduced-motion değişimlerini dinle
  useEffect(() => {
    const mq = window.matchMedia('(prefers-reduced-motion: reduce)');
    const handler = (e: MediaQueryListEvent) => {
      // Yalnızca kullanıcı override yapmamışsa sistem değerini uygula
      const stored = localStorage.getItem(STORAGE_KEY);
      if (!stored) {
        setPrefs((prev) => ({ ...prev, reducedMotion: e.matches }));
      }
    };
    mq.addEventListener('change', handler);
    return () => mq.removeEventListener('change', handler);
  }, []);

  const setHighContrast = useCallback((value: boolean) => {
    setPrefs((prev) => ({ ...prev, highContrast: value }));
  }, []);

  const setFontSize = useCallback((value: FontSize) => {
    setPrefs((prev) => ({ ...prev, fontSize: value }));
  }, []);

  const setReducedMotion = useCallback((value: boolean) => {
    setPrefs((prev) => ({ ...prev, reducedMotion: value }));
  }, []);

  const setEnhancedFocus = useCallback((value: boolean) => {
    setPrefs((prev) => ({ ...prev, enhancedFocus: value }));
  }, []);

  const resetPreferences = useCallback(() => {
    const fresh = { ...defaultPreferences, reducedMotion: getSystemReducedMotion() };
    setPrefs(fresh);
  }, []);

  const value: AccessibilityContextValue = {
    ...prefs,
    setHighContrast,
    setFontSize,
    setReducedMotion,
    setEnhancedFocus,
    resetPreferences,
  };

  return (
    <AccessibilityContext.Provider value={value}>
      {children}
    </AccessibilityContext.Provider>
  );
}

// ─── Hook ─────────────────────────────────────────────────────────────────────

export function useAccessibility(): AccessibilityContextValue {
  const ctx = useContext(AccessibilityContext);
  if (!ctx) {
    throw new Error('useAccessibility, AccessibilityProvider içinde kullanılmalıdır.');
  }
  return ctx;
}
