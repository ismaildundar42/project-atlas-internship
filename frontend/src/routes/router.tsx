import { createBrowserRouter, Navigate } from 'react-router-dom';
import RootLayout from '../layouts/RootLayout';
import DashboardPage from '../pages/DashboardPage';
import ProjectsPage from '../pages/ProjectsPage';
import ReportsPage from '../pages/ReportsPage';
import TeamsPage from '../pages/TeamsPage';
import TeamDetailPage from '../pages/TeamDetailPage';
import AdminOverviewPage from '../pages/AdminOverviewPage';
import AdminProjectsPage from '../pages/AdminProjectsPage';
import AdminOrganizationPage from '../pages/AdminOrganizationPage';
import AdminAuditLogsPage from '../pages/AdminAuditLogsPage';
import AdminModuleAccessRequestsPage from '../pages/AdminModuleAccessRequestsPage';
import ProjectEditorPage from '../pages/ProjectEditorPage';
import LoginPage from '../pages/LoginPage';
import NotFoundPage from '../pages/NotFoundPage';
import ProjectDetailPage from '../pages/ProjectDetailPage';
import ProjectImportWizardPage from '../pages/ProjectImportWizardPage';
import ProfilePage from '../pages/ProfilePage';
import ProtectedRoute from '../components/auth/ProtectedRoute';

/**
 * Uygulama route yapısı.
 *
 * Tek bir createBrowserRouter çağrısıyla tüm rotalar merkezi olarak tanımlanır.
 * RootLayout tüm iç route'ları saran application shell'i barındırır.
 *
 * Route hiyerarşisi:
 * / (RootLayout — sidebar + header + outlet)
 * ├── /                     → DashboardPage (Default access)
 * ├── /projects             → ProjectsPage (Default access)
 * ├── /projects/:slug        → ProjectDetailPage (Default access)
 * ├── /assistant            → Redirect(/)
 * ├── /reports              → ProtectedRoute(ReportsPage, requireModule="reports")
 * ├── /teams                → ProtectedRoute(TeamsPage, requireModule="teams")
 * ├── /teams/:id            → ProtectedRoute(TeamDetailPage, requireModule="teams")
 * ├── /profile              → ProtectedRoute(ProfilePage)
 * ├── /admin                → ProtectedRoute(AdminOverviewPage, requireAdmin)
 * ├── /admin/organization   → ProtectedRoute(AdminOrganizationPage, requireAdmin)
 * ├── /admin/access-requests → ProtectedRoute(AdminModuleAccessRequestsPage, requireAdmin)
 * ├── /admin/audit-logs     → ProtectedRoute(AdminAuditLogsPage, requireAdmin)
 * ├── /admin/access         → Redirect(/admin/access-requests)
 * ├── /admin/projects       → ProtectedRoute(AdminProjectsPage)
 * ├── /admin/projects/new   → ProtectedRoute(ProjectEditorPage mode="create")
 * └── /admin/projects/:id/edit → ProtectedRoute(ProjectEditorPage mode="edit")
 *
 * /login                    → LoginPage (standalone shell dışında)
 * *                         → NotFoundPage (shell dışında)
 */
const router = createBrowserRouter([
  {
    path: '/',
    element: <RootLayout />,
    children: [
      {
        index: true,
        element: <DashboardPage />,
      },
      {
        path: 'projects',
        element: <ProjectsPage />,
      },
      {
        path: 'projects/:slug',
        element: <ProjectDetailPage />,
      },
      {
        path: 'assistant',
        element: <Navigate to="/" replace />,
      },
      {
        path: 'reports',
        element: (
          <ProtectedRoute requireModule="reports">
            <ReportsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'teams',
        element: (
          <ProtectedRoute requireModule="teams">
            <TeamsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'teams/:id',
        element: (
          <ProtectedRoute requireModule="teams">
            <TeamDetailPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'profile',
        element: (
          <ProtectedRoute>
            <ProfilePage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin',
        element: (
          <ProtectedRoute requireAdmin={true}>
            <AdminOverviewPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/organization',
        element: (
          <ProtectedRoute requireAdmin={true}>
            <AdminOrganizationPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/access-requests',
        element: (
          <ProtectedRoute requireAdmin={true}>
            <AdminModuleAccessRequestsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/audit-logs',
        element: (
          <ProtectedRoute requireAdmin={true}>
            <AdminAuditLogsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/access',
        element: <Navigate to="/admin/access-requests" replace />,
      },

      {
        path: 'admin/projects',
        element: (
          <ProtectedRoute requireProjectManagement={true}>
            <AdminProjectsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/projects/import',
        element: (
          <ProtectedRoute requireProjectManagement={true}>
            <ProjectImportWizardPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/projects/new',
        element: (
          <ProtectedRoute requireProjectManagement={true}>
            <ProjectEditorPage mode="create" />
          </ProtectedRoute>
        ),
      },
      {
        path: 'admin/projects/:id/edit',
        element: (
          <ProtectedRoute requireProjectManagement={true}>
            <ProjectEditorPage mode="edit" />
          </ProtectedRoute>
        ),
      },
    ],
  },
  {
    path: 'login',
    element: <LoginPage />,
  },
  {
    // Eşleşmeyen tüm route'lar için 404 sayfası (shell dışında)
    path: '*',
    element: <NotFoundPage />,
  },
]);

export default router;
