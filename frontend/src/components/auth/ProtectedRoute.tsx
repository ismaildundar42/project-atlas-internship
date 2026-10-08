import { type ReactNode, useState } from 'react';
import { Navigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useModuleAccess } from '../../hooks/useModuleAccess';
import Skeleton from '../ui/Skeleton';
import Card from '../ui/Card';
import Button from '../ui/Button';
import { ShieldAlert, ArrowLeft, Lock, Send } from 'lucide-react';
import AccessRequestModal from '../access/AccessRequestModal';

interface ProtectedRouteProps {
  children: ReactNode;
  requireAdmin?: boolean;
  requireProjectManagement?: boolean;
  requireModule?: 'reports' | 'teams';
}

export function ProtectedRoute({
  children,
  requireAdmin = false,
  requireProjectManagement = false,
  requireModule,
}: ProtectedRouteProps) {
  const { isAuthenticated, isAdmin, canAccessProjectManagement, isLoading: isAuthLoading } = useAuth();
  const { hasModuleAccess, isModulePending, requestAccess, isLoading: isModuleLoading } = useModuleAccess();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const location = useLocation();

  const isLoading = isAuthLoading || (Boolean(requireModule) && isModuleLoading);

  if (isLoading) {
    return (
      <div style={{ padding: 'var(--space-6)', maxWidth: '800px', margin: '0 auto' }}>
        <Card padding="lg">
          <div style={{ marginBottom: '16px' }}>
            <Skeleton variant="text" width="40%" height={28} />
          </div>
          <Skeleton variant="rect" height={140} />
        </Card>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (requireAdmin && !isAdmin) {
    return (
      <div style={{ padding: 'var(--space-6)', maxWidth: '600px', margin: '40px auto' }}>
        <Card padding="lg">
          <div style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
            <ShieldAlert size={48} color="var(--color-danger)" style={{ marginBottom: '16px' }} />
            <h2 style={{ fontSize: 'var(--font-size-xl)', fontWeight: 700, marginBottom: '8px', color: 'var(--color-text-primary)' }}>
              Yetkisiz Erişim (403)
            </h2>
            <p style={{ color: 'var(--color-text-secondary)', fontSize: 'var(--font-size-sm)', marginBottom: '24px' }}>
              Bu alana erişebilmek için Yönetici (Admin) yetkisine sahip olmanız gerekmektedir.
            </p>
            <Link to="/">
              <Button variant="secondary" size="md">
                <ArrowLeft size={16} aria-hidden="true" /> Ana Sayfaya Dön
              </Button>
            </Link>
          </div>
        </Card>
      </div>
    );
  }

  if (requireProjectManagement && !canAccessProjectManagement) {
    return (
      <div style={{ padding: 'var(--space-6)', maxWidth: '600px', margin: '40px auto' }}>
        <Card padding="lg">
          <div style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
            <ShieldAlert size={48} color="var(--color-danger)" style={{ marginBottom: '16px' }} />
            <h2 style={{ fontSize: 'var(--font-size-xl)', fontWeight: 700, marginBottom: '8px', color: 'var(--color-text-primary)' }}>
              Yetkisiz Erişim (403)
            </h2>
            <p style={{ color: 'var(--color-text-secondary)', fontSize: 'var(--font-size-sm)', marginBottom: '24px' }}>
              Proje yönetim paneline erişebilmek için proje girişi yetkinizin bulunması gerekmektedir.
            </p>
            <Link to="/">
              <Button variant="secondary" size="md">
                <ArrowLeft size={16} aria-hidden="true" /> Ana Sayfaya Dön
              </Button>
            </Link>
          </div>
        </Card>
      </div>
    );
  }

  if (requireModule && !hasModuleAccess(requireModule)) {
    const moduleName = requireModule === 'reports' ? 'Raporlama' : 'Ekipler';
    const isPending = isModulePending(requireModule);

    return (
      <div style={{ padding: 'var(--space-6)', maxWidth: '600px', margin: '40px auto' }}>
        <Card padding="lg">
          <div style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
            <div
              style={{
                width: '64px',
                height: '64px',
                borderRadius: '50%',
                backgroundColor: 'var(--color-warning-bg)',
                color: 'var(--color-warning)',
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                marginBottom: '16px',
              }}
            >
              <Lock size={32} aria-hidden="true" />
            </div>
            <h2 style={{ fontSize: 'var(--font-size-xl)', fontWeight: 700, marginBottom: '8px', color: 'var(--color-text-primary)' }}>
              {moduleName} Modülü Kilitli
            </h2>
            <p style={{ color: 'var(--color-text-secondary)', fontSize: 'var(--font-size-sm)', marginBottom: '24px', lineHeight: 1.5 }}>
              {isPending
                ? 'Bu modül için daha önce ilettiğiniz erişim talebi bulunmaktadır. Yöneticilerin incelemesi bekleniyor.'
                : 'Bu modülü görüntüleme yetkiniz bulunmuyor. Modülü kullanabilmek için yöneticilerden erişim talep edebilirsiniz.'}
            </p>
            <div style={{ display: 'flex', justifyContent: 'center', gap: 'var(--space-3)' }}>
              <Link to="/">
                <Button variant="secondary" size="md">
                  <ArrowLeft size={16} aria-hidden="true" /> Ana Sayfaya Dön
                </Button>
              </Link>
              <Button
                variant="primary"
                size="md"
                onClick={() => setIsModalOpen(true)}
              >
                <Send size={16} aria-hidden="true" /> {isPending ? 'Talebi İncele' : 'Erişim Talep Et'}
              </Button>
            </div>
          </div>
        </Card>

        <AccessRequestModal
          isOpen={isModalOpen}
          moduleKey={requireModule}
          moduleName={moduleName}
          isPending={isPending}
          onClose={() => setIsModalOpen(false)}
          onSubmit={async (mod, reason) => {
            await requestAccess(mod, reason);
          }}
        />
      </div>
    );
  }

  return <>{children}</>;
}

export default ProtectedRoute;
