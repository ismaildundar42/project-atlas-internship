import { useState, useCallback, useEffect } from 'react';
import { Outlet, useLocation, ScrollRestoration } from 'react-router-dom';
import Sidebar, { useSidebarCollapse } from '../components/navigation/Sidebar';
import MobileDrawer from '../components/navigation/MobileDrawer';
import TopHeader from '../components/navigation/TopHeader';
import { AccessibilityProvider } from '../context/AccessibilityContext';
import { ThemeProvider } from '../context/ThemeContext';
import { AssistantProvider } from '../context/AssistantContext';
import ProjectAssistantWidget from '../components/ai/ProjectAssistantWidget';

import { useTranslation } from 'react-i18next';

function usePageTitle(): string {
  const { t } = useTranslation(['navigation', 'common']);
  const { pathname } = useLocation();

  if (pathname === '/') return t('navigation.dashboard', 'Ana Sayfa');
  if (pathname.startsWith('/projects')) return t('navigation.projectLibrary', 'Proje Kütüphanesi');
  if (pathname.startsWith('/reports')) return t('navigation.reports', 'Raporlama');
  if (pathname.startsWith('/teams')) return t('navigation.teams', 'Ekipler');
  if (pathname.startsWith('/admin')) return t('navigation.adminCenter', 'Yönetim Merkezi');

  return 'Demir Export';
}

// ─── Root Layout ──────────────────────────────────────────────────────────────

function RootLayoutInner() {
  const { t } = useTranslation(['accessibility', 'common']);
  const { collapsed, toggle: toggleSidebar } = useSidebarCollapse();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const pageTitle = usePageTitle();
  const { pathname } = useLocation();

  // Route değiştiğinde sayfanın en üstten açılmasını sağla
  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: 'instant' });
  }, [pathname]);

  const openDrawer = useCallback(() => setDrawerOpen(true), []);
  const closeDrawer = useCallback(() => setDrawerOpen(false), []);

  return (
    <div className={`app-shell ${collapsed ? 'app-shell--collapsed' : ''}`}>
      <ScrollRestoration />
      {/* Skip Navigation — Klavye erişilebilirliği */}
      <a href="#main-content" className="skip-link">
        {t('accessibility.skipToContent', 'Ana içeriğe geç')}
      </a>

      {/* Desktop Sidebar */}
      <Sidebar collapsed={collapsed} onToggleCollapse={toggleSidebar} />

      {/* Mobile Drawer */}
      <MobileDrawer isOpen={drawerOpen} onClose={closeDrawer} />

      {/* Top Header */}
      <TopHeader onMenuOpen={openDrawer} pageTitle={pageTitle} />

      {/* Main Content Area */}
      <main
        id="main-content"
        className="main-area"
        role="main"
        aria-label={pageTitle}
      >
        <Outlet />
      </main>

      {/* Global Floating Project Assistant Widget */}
      <ProjectAssistantWidget />
    </div>
  );
}

/**
 * Uygulama kök layout bileşeni.
 * AccessibilityProvider ve AssistantProvider burada wrap edildiği için tüm alt bileşenler
 * tercihlere ve asistan durumuna erişebilir.
 */
function RootLayout() {
  return (
    <ThemeProvider>
      <AccessibilityProvider>
        <AssistantProvider>
          <RootLayoutInner />
        </AssistantProvider>
      </AccessibilityProvider>
    </ThemeProvider>
  );
}

export default RootLayout;

