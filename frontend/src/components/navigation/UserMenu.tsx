import { useState, useRef, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { User, KeyRound, Shield, PlusCircle, LogOut, ChevronDown } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../hooks/useAuth';

export function UserMenu() {
  const { t } = useTranslation(['navigation', 'profile', 'common']);
  const { user, isAdmin, isSuperAdmin, canCreateProjects, logout } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  const userInitials = user
    ? `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase()
    : 'U';

  const roleLabel = isSuperAdmin
    ? t('profile.roles.superAdmin', 'Süper Yönetici')
    : isAdmin
    ? t('profile.roles.admin', 'Yönetici')
    : canCreateProjects
    ? t('profile.roles.creator', 'Proje Geliştirici')
    : t('profile.roles.user', 'Kurumsal Kullanıcı');

  // Close dropdown on outside click or Escape
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    }

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      document.addEventListener('keydown', handleKeyDown);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  const handleLogout = async () => {
    setIsOpen(false);
    await logout();
    navigate('/login');
  };

  return (
    <div className="user-menu-container" ref={menuRef} style={{ position: 'relative' }}>
      <button
        type="button"
        className="top-header__user-avatar"
        onClick={() => setIsOpen((prev) => !prev)}
        aria-expanded={isOpen}
        aria-haspopup="true"
        aria-label={`${t('navigation.userProfile', 'Kullanıcı Profili')}: ${user?.firstName} ${user?.lastName}`}
        id="user-menu-btn"
        title={`${user?.firstName} ${user?.lastName} (${roleLabel})`}
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          padding: '3px 10px 3px 4px',
          borderRadius: '24px',
          border: '1px solid rgba(255, 255, 255, 0.18)',
          background: 'rgba(255, 255, 255, 0.08)',
          cursor: 'pointer',
          transition: 'all var(--transition-fast)',
        }}
      >
        <span
          style={{
            width: '28px',
            height: '28px',
            borderRadius: '50%',
            backgroundColor: 'rgba(255, 255, 255, 0.2)',
            color: '#ffffff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '12px',
            fontWeight: 700,
            letterSpacing: '0.5px',
            flexShrink: 0,
          }}
        >
          {userInitials}
        </span>
        <span
          className="user-menu-name-label"
          style={{
            fontSize: '13px',
            fontWeight: 600,
            color: 'rgba(255, 255, 255, 0.92)',
            maxWidth: '120px',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {user?.firstName}
        </span>
        <ChevronDown
          size={14}
          style={{
            color: 'rgba(255, 255, 255, 0.7)',
            transition: 'transform var(--transition-fast)',
            transform: isOpen ? 'rotate(180deg)' : 'none',
          }}
        />
      </button>

      {isOpen && (
        <div
          className="user-dropdown-menu"
          role="menu"
          aria-orientation="vertical"
          aria-labelledby="user-menu-btn"
          style={{
            position: 'absolute',
            top: 'calc(100% + 8px)',
            right: 0,
            width: '260px',
            backgroundColor: 'var(--color-surface-raised, #ffffff)',
            borderRadius: 'var(--radius-lg, 12px)',
            boxShadow: 'var(--shadow-xl)',
            border: '1px solid var(--color-border)',
            padding: '8px 0',
            zIndex: 'var(--z-dropdown, 1000)',
          }}
        >
          {/* Header Info */}
          <div
            style={{
              padding: '10px 16px',
              borderBottom: '1px solid var(--color-border-subtle)',
            }}
          >
            <div style={{ fontWeight: 600, fontSize: '14px', color: 'var(--color-text-primary)' }}>
              {user?.firstName} {user?.lastName}
            </div>
            <div style={{ fontSize: '12px', color: 'var(--color-text-muted)', marginTop: '2px', wordBreak: 'break-all' }}>
              {user?.email}
            </div>
            <div
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '4px',
                marginTop: '6px',
                padding: '2px 8px',
                borderRadius: 'var(--radius-xs, 4px)',
                fontSize: '11px',
                fontWeight: 600,
                backgroundColor: 'var(--color-surface-subtle)',
                color: 'var(--color-text-secondary)',
                border: '1px solid var(--color-border-subtle)',
              }}
            >
              {roleLabel}
            </div>
          </div>

          {/* Menu Items */}
          <div style={{ padding: '6px 0' }}>
            <Link
              to="/profile"
              role="menuitem"
              onClick={() => setIsOpen(false)}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                padding: '9px 16px',
                fontSize: '13px',
                color: 'var(--color-text-primary)',
                textDecoration: 'none',
                transition: 'background var(--transition-fast)',
              }}
              onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-subtle)')}
              onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
            >
              <User size={16} style={{ color: 'var(--color-brand-navy-light, #1e4a7c)' }} />
              <span>{t('navigation.myProfile', 'Profilim')}</span>
            </Link>

            <Link
              to="/profile?tab=security"
              role="menuitem"
              onClick={() => setIsOpen(false)}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                padding: '9px 16px',
                fontSize: '13px',
                color: 'var(--color-text-primary)',
                textDecoration: 'none',
                transition: 'background var(--transition-fast)',
              }}
              onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-subtle)')}
              onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
            >
              <KeyRound size={16} style={{ color: 'var(--color-brand-navy-light, #1e4a7c)' }} />
              <span>{t('profile:tabs.security', 'Şifre Değiştir')}</span>
            </Link>

            {(isAdmin || isSuperAdmin) && (
              <Link
                to="/admin"
                role="menuitem"
                onClick={() => setIsOpen(false)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '10px',
                  padding: '9px 16px',
                  fontSize: '13px',
                  color: 'var(--color-text-primary)',
                  textDecoration: 'none',
                  transition: 'background var(--transition-fast)',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-subtle)')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
              >
                <Shield size={16} style={{ color: 'var(--color-brand-navy-light, #1e4a7c)' }} />
                <span>{t('navigation.adminDashboard', 'Yönetim Merkezi')}</span>
              </Link>
            )}

            {(canCreateProjects || isAdmin || isSuperAdmin) && (
              <Link
                to="/admin/projects/new"
                role="menuitem"
                onClick={() => setIsOpen(false)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '10px',
                  padding: '9px 16px',
                  fontSize: '13px',
                  color: 'var(--color-text-primary)',
                  textDecoration: 'none',
                  transition: 'background var(--transition-fast)',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-subtle)')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
              >
                <PlusCircle size={16} style={{ color: 'var(--color-brand-navy-light, #1e4a7c)' }} />
                <span>{t('navigation.newProject', 'Yeni Proje')}</span>
              </Link>
            )}
          </div>

          <div style={{ borderTop: '1px solid var(--color-border-subtle)', paddingTop: '4px' }}>
            <button
              type="button"
              role="menuitem"
              id="logout-menu-item"
              onClick={handleLogout}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                width: '100%',
                padding: '9px 16px',
                fontSize: '13px',
                color: 'var(--color-danger)',
                background: 'none',
                border: 'none',
                cursor: 'pointer',
                textAlign: 'left',
                transition: 'background var(--transition-fast)',
              }}
              onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'var(--color-danger-bg)')}
              onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
            >
              <LogOut size={16} />
              <span>{t('navigation.logout', 'Oturumu Kapat')}</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default UserMenu;
