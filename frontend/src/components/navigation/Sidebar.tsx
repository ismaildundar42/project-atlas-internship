import { NavLink } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import {
  LayoutDashboard,
  FolderKanban,
  BarChart2,
  Users,
  Building2,
  Settings2,
  ChevronLeft,
  ChevronRight,
  History,
  KeyRound,
  Lock,
  Clock,
} from 'lucide-react';

import BrandMark from '../brand/BrandMark';
import IconButton from '../ui/IconButton';
import kocLogo from '../../assets/brand/koc-logo.png';
import { useAuth } from '../../hooks/useAuth';
import { useModuleAccess } from '../../hooks/useModuleAccess';
import { getAdminPendingCount, MODULE_ACCESS_CHANGED_EVENT } from '../../services/moduleAccessService';
import AccessRequestModal from '../access/AccessRequestModal';

// ─── Types ────────────────────────────────────────────────────────────────────

interface NavItem {
  path: string;
  label: string;
  icon: React.ReactNode;
  moduleKey?: 'reports' | 'teams';
  badgeCount?: number;
}

interface SidebarProps {
  /** Desktop'ta collapse/expand durumu */
  collapsed: boolean;
  onToggleCollapse: () => void;
}

// ─── Nav Item Component ───────────────────────────────────────────────────────

interface SidebarNavItemProps {
  item: NavItem;
  collapsed: boolean;
  onLockedClick?: (moduleKey: 'reports' | 'teams', moduleName: string, isPending: boolean) => void;
  hasAccess?: boolean;
  isPending?: boolean;
}

function SidebarNavItem({ item, collapsed, onLockedClick, hasAccess = true, isPending = false }: SidebarNavItemProps) {
  const isExact = item.path === '/' || item.path === '/admin' || item.path === '/assistant';
  const isLocked = !hasAccess;

  const handleClick = (e: React.MouseEvent) => {
    if (isLocked && onLockedClick && item.moduleKey) {
      e.preventDefault();
      onLockedClick(item.moduleKey, item.label, isPending);
    }
  };

  return (
    <NavLink
      to={item.path}
      end={isExact}
      onClick={handleClick}
      className={({ isActive }: { isActive: boolean }) =>
        `sidebar-nav-item ${isActive && !isLocked ? 'active' : ''} ${collapsed ? 'sidebar-nav-item--collapsed' : ''} ${isLocked ? 'sidebar-nav-item--locked' : ''}`
      }
      title={collapsed ? `${item.label}${isLocked ? (isPending ? ' (Talep Bekliyor)' : ' (Kilitli)') : ''}` : undefined}
      style={
        isLocked
          ? {
              opacity: 0.75,
              cursor: 'pointer',
            }
          : undefined
      }
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

      {!collapsed && (
        <span
          className="sidebar-nav-item__label"
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            width: '100%',
          }}
        >
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
                whiteSpace: 'nowrap',
              }}
            >
              {isPending ? 'Talep Bekliyor' : <Lock size={12} />}
            </span>
          )}
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
      )}

      {collapsed && (
        <span className="sr-only">
          {item.label} {isLocked ? (isPending ? 'Talep Bekliyor' : 'Kilitli') : ''}
        </span>
      )}
    </NavLink>
  );
}

// ─── Sidebar Component ────────────────────────────────────────────────────────

function Sidebar({ collapsed, onToggleCollapse }: SidebarProps) {
  const { t } = useTranslation(['navigation', 'common', 'access']);
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
      { path: '/admin', label: t('navigation.adminCenter', 'Yönetim Merkezi'), icon: <Settings2 size={20} /> },
      { path: '/admin/projects', label: t('navigation.projects', 'Projeler'), icon: <FolderKanban size={20} /> },
      { path: '/admin/organization', label: t('navigation.organization', 'Organizasyon'), icon: <Building2 size={20} /> },
      {
        path: '/admin/access-requests',
        label: t('navigation.accessRequests', 'Erişim Talepleri'),
        icon: <KeyRound size={20} />,
        badgeCount: pendingRequestsCount,
      },
      { path: '/admin/audit-logs', label: t('navigation.auditHistory', 'İşlem Geçmişi'), icon: <History size={20} /> }
    );
  } else if (canCreateProjects) {
    dynamicAdminNavItems.push(
      { path: '/admin/projects', label: t('navigation.projects', 'Projeler'), icon: <FolderKanban size={20} /> }
    );
  }

  const handleLockedItemClick = (moduleKey: 'reports' | 'teams', moduleName: string, isPending: boolean) => {
    setModalState({
      isOpen: true,
      moduleKey,
      moduleName,
      isPending,
    });
  };

  return (
    <>
      <aside
        className={`sidebar ${collapsed ? 'sidebar--collapsed' : ''}`}
        aria-label={t('navigation.mainNavigation', 'Ana navigasyon')}
      >
        {/* Üst: Brand + Collapse toggle */}
        <div className="sidebar-header">
          {!collapsed && (
            <NavLink to="/" className="sidebar-brand" aria-label={t('navigation.goToDashboard', 'Ana sayfaya git')}>
              <BrandMark />
            </NavLink>
          )}

          <IconButton
            icon={collapsed ? <ChevronRight size={18} /> : <ChevronLeft size={16} />}
            aria-label={collapsed ? t('navigation.expandMenu', 'Menüyü genişlet') : t('navigation.collapseMenu', 'Menüyü daralt')}
            aria-expanded={!collapsed}
            variant="ghost"
            size="sm"
            className="sidebar-collapse-btn"
            onClick={onToggleCollapse}
          />
        </div>

        {/* Ana navigasyon */}
        <nav className="sidebar-nav" aria-label={t('navigation.mainMenu', 'Ana menü')}>
          <ul role="list" className="sidebar-nav-list">
            {mainNavItems.map((item) => {
              const hasAccess = item.moduleKey ? hasModuleAccess(item.moduleKey) : true;
              const isPending = item.moduleKey ? isModulePending(item.moduleKey) : false;

              return (
                <li key={item.path}>
                  <SidebarNavItem
                    item={item}
                    collapsed={collapsed}
                    hasAccess={hasAccess}
                    isPending={isPending}
                    onLockedClick={handleLockedItemClick}
                  />
                </li>
              );
            })}
          </ul>
        </nav>

        {/* Alt: Yönetim (Yalnızca Admin veya Proje Girişi Yetkisi olanlar görür) */}
        {canAccessProjectManagement && (
          <div className="sidebar-footer">
            <div className="sidebar-section-label">
              {!collapsed && (
                <span className="sidebar-section-title" aria-hidden="true">{t('navigation.management', 'Yönetim')}</span>
              )}
            </div>
            <nav aria-label={t('navigation.managementMenu', 'Yönetim menüsü')}>
              <ul role="list" className="sidebar-nav-list">
                {dynamicAdminNavItems.map((item) => (
                  <li key={item.path}>
                    <SidebarNavItem item={item} collapsed={collapsed} />
                  </li>
                ))}
              </ul>
            </nav>

            {/* Ikincil Kurumsal Marka — Koç */}
            {!collapsed && (
              <div className="sidebar-affiliation">
                <img src={kocLogo} alt="Koç" className="sidebar-affiliation__logo" />
              </div>
            )}
          </div>
        )}
      </aside>

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

// ─── Collapse Preference Hook ─────────────────────────────────────────────────

const COLLAPSE_KEY = 'de-sidebar-collapsed';

export function useSidebarCollapse() {
  const [collapsed, setCollapsed] = useState<boolean>(() => {
    try {
      return localStorage.getItem(COLLAPSE_KEY) === 'true';
    } catch {
      return false;
    }
  });

  const toggle = () => {
    setCollapsed((prev) => {
      const next = !prev;
      try {
        localStorage.setItem(COLLAPSE_KEY, String(next));
      } catch { /* ignore */ }
      return next;
    });
  };

  // Ekran küçük olduğunda otomatik collapse etme (tablet ve mobil)
  useEffect(() => {
    const mq = window.matchMedia('(max-width: 1023px)');
    if (mq.matches) {
      setCollapsed(false);
    }
  }, []);

  return { collapsed, toggle };
}

export default Sidebar;
