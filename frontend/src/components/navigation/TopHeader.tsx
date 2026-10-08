import { Menu, Shield } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { GlobalSearch } from '../search/GlobalSearch';
import IconButton from '../ui/IconButton';
import AccessibilityToggle from '../accessibility/AccessibilityToggle';
import ThemeToggle from './ThemeToggle';
import LanguageSwitcher from './LanguageSwitcher';
import NotificationBell from './NotificationBell';
import { UserMenu } from './UserMenu';
import { useAuth } from '../../hooks/useAuth';

// ─── Types ────────────────────────────────────────────────────────────────────

interface TopHeaderProps {
  /** Mobil hamburger menü tetikleyicisi */
  onMenuOpen: () => void;
  /** Sayfa başlığı */
  pageTitle?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

function TopHeader({ onMenuOpen, pageTitle }: TopHeaderProps) {
  const { t } = useTranslation(['navigation', 'common']);
  const { isAuthenticated } = useAuth();

  return (
    <header className="top-header" role="banner">
      {/* Sol Bölüm: Hamburger (mobile) + Sayfa başlığı */}
      <div className="top-header__left">
        {/* Hamburger — yalnızca mobilde görünür */}
        <IconButton
          icon={<Menu size={20} />}
          aria-label={t('navigation.openMenu', 'Navigasyon menüsünü aç')}
          variant="ghost"
          className="top-header__hamburger"
          onClick={onMenuOpen}
          id="hamburger-btn"
        />

        {/* Sayfa başlığı (mobil) */}
        {pageTitle && (
          <span className="top-header__page-title" aria-hidden="true">
            {pageTitle}
          </span>
        )}
      </div>

      {/* Orta Bölüm: Genel Arama ve Proje Keşif Motoru */}
      <div className="top-header__center">
        <GlobalSearch />
      </div>

      {/* Sağ Bölüm: Araçlar */}
      <div className="top-header__right">
        {/* Dil Seçici (TR / EN) */}
        <LanguageSwitcher variant="header" />

        {/* Görünüm / Tema seçimi tetikleyicisi */}
        <ThemeToggle />

        {/* Erişilebilirlik panel tetikleyicisi */}
        <AccessibilityToggle />

        {/* Bildirimler */}
        {isAuthenticated ? <NotificationBell /> : null}

        {/* Auth State Area */}
        {isAuthenticated ? (
          <UserMenu />
        ) : (
          <Link to="/login" style={{ textDecoration: 'none' }}>
            <button
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '6px',
                padding: '6px 12px',
                fontSize: 'var(--font-size-xs)',
                fontWeight: 600,
                color: 'var(--color-text-secondary)',
                backgroundColor: 'transparent',
                border: '1px solid var(--color-border-subtle)',
                borderRadius: 'var(--radius-md)',
                cursor: 'pointer',
              }}
              aria-label={t('navigation.login', 'Giriş Yap')}
              id="login-btn"
            >
              <Shield size={14} aria-hidden="true" />
              <span>{t('navigation.login', 'Giriş Yap')}</span>
            </button>
          </Link>
        )}
      </div>
    </header>
  );
}

export default TopHeader;
