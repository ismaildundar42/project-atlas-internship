import { useEffect, useRef, useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  LayoutDashboard,
  FolderKanban,
  BarChart2,
  Users,
  Building2,
  Settings2,
  History,
  KeyRound,
  Lock,
  Clock,
  X,
} from 'lucide-react';

import BrandMark from '../brand/BrandMark';
import IconButton from '../ui/IconButton';
import LanguageSwitcher from './LanguageSwitcher';
import kocLogo from '../../assets/brand/koc-logo.png';
import { useAuth } from '../../hooks/useAuth';
import { useModuleAccess } from '../../hooks/useModuleAccess';
import { getAdminPendingCount, MODULE_ACCESS_CHANGED_EVENT } from '../../services/moduleAccessService';
import AccessRequestModal from '../access/AccessRequestModal';

interface MobileDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

interface NavItem {
  path: string;
  label: string;
  icon: React.ReactNode;
  moduleKey?: 'reports' | 'teams';
  badgeCount?: number;
}

function MobileDrawer({ isOpen, onClose }: MobileDrawerProps) {
  const { t } = useTranslation(['navigation', 'common', 'access']);
  const drawerRef = useRef<HTMLDivElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  const location = useLocation();
  const { isAdmin, canCreateProjects, canAccessProjectManagement } = useAuth();
  const { hasModuleAccess, isModulePending, requestAccess } = useModuleAccess();

  const [pendingRequestsCount, setPendingRequestsCount] = useState<number>(0);
  const [modalState, setModalState] = useState<{
    isOpen: boolean;
    moduleKey: 'reports' | 'teams';
    moduleName: string;
    isPending: boolean;
  }>({
    isOpen: false,
    moduleKey: 'reports',
    moduleName: '',
    isPending: false,
  });

  useEffect(() => {
    const refreshCount = () => {
      if (isAdmin) {
        getAdminPendingCount()
          .then((count) => setPendingRequestsCount(count))
          .catch(() => {});
      }
    };

    refreshCount();

    window.addEventListener(MODULE_ACCESS_CHANGED_EVENT, refreshCount);
    return () => {
      window.removeEventListener(MODULE_ACCESS_CHANGED_EVENT, refreshCount);
    };
  }, [isAdmin]);

  const mainNavItems: NavItem[] = [
    { path: '/',         label: t('navigation.dashboard', 'Ana Sayfa'),         icon: <LayoutDashboard size={20} /> },
    { path: '/projects', label: t('navigation.projectLibrary', 'Proje Kütüphanesi'), icon: <FolderKanban size={20} /> },
    { path: '/reports',  label: t('navigation.reports', 'Raporlama'),          icon: <BarChart2 size={20} />, moduleKey: 'reports' },
    { path: '/teams',    label: t('navigation.teams', 'Ekipler'),            icon: <Users size={20} />, moduleKey: 'teams' },
  ];

  const dynamicAdminNavItems: NavItem[] = [];
  if (isAdmin) {
    dynamicAdminNavItems.push(
      { path: '/admin',              label: t('navigation.adminCenter', 'Yönetim Merkezi'), icon: <Settings2 size={20} /> },
      { path: '/admin/projects',     label: t('navigation.projects', 'Projeler'),        icon: <FolderKanban size={20} /> },
      { path: '/admin/organization', label: t('navigation.organization', 'Organizasyon'),    icon: <Building2 size={20} /> },
      {
        path: '/admin/access-requests',
        label: t('navigation.accessRequests', 'Erişim Talepleri'),
        icon: <KeyRound size={20} />,
        badgeCount: pendingRequestsCount,
      },
      { path: '/admin/audit-logs',   label: t('navigation.auditHistory', 'İşlem Geçmişi'),   icon: <History size={20} /> }
    );
  } else if (canCreateProjects) {
    dynamicAdminNavItems.push(
      { path: '/admin/projects',     label: t('navigation.projects', 'Projeler'),        icon: <FolderKanban size={20} /> }
    );
  }

  // Sayfa değişince drawer'ı kapat
  useEffect(() => {
    onClose();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname]);

  // Escape ile kapat
  useEffect(() => {
    if (!isOpen) return;

    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handleTabOrEscape);

    function handleTabOrEscape(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose();
    }
  }, [isOpen, onClose]);

  // Açılınca focus'u kapat butonuna taşı
  useEffect(() => {
    if (isOpen) {
      closeButtonRef.current?.focus();
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }

    return () => {
      document.body.style.overflow = '';
    };
  }, [isOpen]);

  const handleItemClick = (e: React.MouseEvent, item: NavItem, hasAccess: boolean, isPending: boolean) => {
    if (!hasAccess && item.moduleKey) {
      e.preventDefault();
      setModalState({
        isOpen: true,
        moduleKey: item.moduleKey,
        moduleName: item.label,
        isPending,
      });
    }
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className={`backdrop ${isOpen ? 'visible' : ''}`}
        onClick={onClose}
        aria-hidden="true"
        style={{ zIndex: 'var(--z-drawer)' }}
      />

      {/* Drawer */}
      <div
        ref={drawerRef}
        className={`mobile-drawer ${isOpen ? 'mobile-drawer--open' : ''}`}
        role="dialog"
        aria-modal="true"
        aria-label={t('navigation.mainNavigation', 'Navigasyon menüsü')}
        style={{ zIndex: 'calc(var(--z-drawer) + 1)' }}
      >
        {/* Drawer Header */}
        <div className="mobile-drawer__header">
          <NavLink to="/" className="sidebar-brand" aria-label={t('navigation.goToDashboard', 'Ana sayfaya git')}>
            <BrandMark />
          </NavLink>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <LanguageSwitcher variant="drawer" />
            <IconButton
              ref={closeButtonRef}
              icon={<X size={18} />}
              aria-label={t('navigation.closeMenu', 'Menüyü kapat')}
              variant="ghost"
              onClick={onClose}
            />
          </div>
        </div>

        {/* Nav */}
        <nav className="mobile-drawer__nav" aria-label={t('navigation.mainMenu', 'Ana menü')}>
          <ul role="list" className="sidebar-nav-list">
            {mainNavItems.map((item) => {
              const hasAccess = item.moduleKey ? hasModuleAccess(item.moduleKey) : true;
              const isPending = item.moduleKey ? isModulePending(item.moduleKey) : false;
              const isLocked = !hasAccess;

              return (
                <li key={item.path}>
                  <NavLink
                    to={item.path}
                    end={item.path === '/'}
                    onClick={(e) => handleItemClick(e, item, hasAccess, isPending)}
                    className={({ isActive }: { isActive: boolean }) =>
                      `sidebar-nav-item ${isActive && !isLocked ? 'active' : ''} ${isLocked ? 'sidebar-nav-item--locked' : ''}`
                    }
                    style={isLocked ? { opacity: 0.75, cursor: 'pointer' } : undefined}
                  >
                    <span className="sidebar-nav-item__icon" aria-hidden="true" style={{ position: 'relative' }}>
                      {item.icon}
                      {isLocked && (
                        <span
                          style={{
                            position: 'absolute',
                            bottom: '-2px',
                            right: '-4px',
                            backgroundColor: isPending ? 'var(--color-info, #0284c7)' : 'var(--color-warning, #d97706)',
                            color: '#ffffff',
                            borderRadius: '50%',
                            width: '13px',
                            height: '13px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                          }}
                        >
                          {isPending ? <Clock size={9} /> : <Lock size={8} />}
                        </span>
                      )}
                    </span>
                    <span className="sidebar-nav-item__label" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
                      <span>{item.label}</span>
                      {isLocked && (
                        <span
                          style={{
                            fontSize: '10px',
                            fontWeight: 600,
                            padding: '2px 6px',
                            borderRadius: '4px',
                            backgroundColor: isPending ? 'var(--color-info-bg, #e0f2fe)' : 'var(--color-bg-secondary)',
                            color: isPending ? 'var(--color-info-text, #0284c7)' : 'var(--color-text-muted)',
                            marginLeft: '8px',
                          }}
                        >
                          {isPending ? 'Talep Bekliyor' : <Lock size={12} />}
                        </span>
                      )}
                    </span>
                  </NavLink>
                </li>
              );
            })}
          </ul>
        </nav>

        {/* Footer / Admin (Yalnızca yetkili kullanıcılara gösterilir) */}
        {canAccessProjectManagement && (
          <div className="mobile-drawer__footer">
            <p className="sidebar-section-title">{t('navigation.management', 'Yönetim')}</p>
            <nav aria-label={t('navigation.managementMenu', 'Yönetim menüsü')}>
              <ul role="list" className="sidebar-nav-list">
                {dynamicAdminNavItems.map((item) => (
                  <li key={item.path}>
                    <NavLink
                      to={item.path}
                      end={item.path === '/admin'}
                      className={({ isActive }: { isActive: boolean }) => `sidebar-nav-item ${isActive ? 'active' : ''}`}
                    >
                      <span className="sidebar-nav-item__icon" aria-hidden="true">{item.icon}</span>
                      <span className="sidebar-nav-item__label" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
                        <span>{item.label}</span>
                        {typeof item.badgeCount === 'number' && item.badgeCount > 0 && (
                          <span
                            style={{
                              fontSize: '11px',
                              fontWeight: 700,
                              padding: '1px 6px',
                              borderRadius: '999px',
                              backgroundColor: 'var(--color-primary)',
                              color: '#ffffff',
                              marginLeft: '8px',
                            }}
                          >
                            {item.badgeCount}
                          </span>
                        )}
                      </span>
                    </NavLink>
                  </li>
                ))}
              </ul>
            </nav>

            <div className="sidebar-affiliation" style={{ marginTop: 'var(--space-4)', paddingTop: 'var(--space-4)' }}>
              <img src={kocLogo} alt="Koç" className="sidebar-affiliation__logo" />
            </div>
          </div>
        )}
      </div>

      {/* Erişim Talep Modalı */}
      <AccessRequestModal
        isOpen={modalState.isOpen}
        moduleKey={modalState.moduleKey}
        moduleName={modalState.moduleName}
        isPending={modalState.isPending}
        onClose={() => setModalState((prev) => ({ ...prev, isOpen: false }))}
        onSubmit={async (mod, reason) => {
          await requestAccess(mod, reason);
        }}
      />
    </>
  );
}

export default MobileDrawer;
