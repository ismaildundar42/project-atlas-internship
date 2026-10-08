import { ShieldCheck, LogOut, Lock, UserCheck } from 'lucide-react';
import PageLayout from '../layouts/PageLayout';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Badge from '../components/ui/Badge';

function AdminPage() {
  const { user, logout } = useAuth();

  return (
    <PageLayout
      title="Yönetim Paneli"
      description="Demir Export Proje Kütüphanesi sistem ve içerik yönetimi."
      breadcrumbs={[{ label: 'Ana Sayfa', href: '/' }, { label: 'Yönetim Paneli' }]}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
        {/* Welcome Banner Card */}
        <Card padding="lg">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '16px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
              <div style={{
                width: '56px',
                height: '56px',
                borderRadius: '50%',
                backgroundColor: 'rgba(197, 22, 5, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--color-brand-primary)'
              }}>
                <ShieldCheck size={32} aria-hidden="true" />
              </div>
              <div>
                <h2 style={{ fontSize: 'var(--font-size-xl)', fontWeight: 700, color: 'var(--color-text-primary)', marginBottom: '4px' }}>
                  Hoş geldiniz, {user?.firstName} {user?.lastName}
                </h2>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <span style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)' }}>E-posta: {user?.email}</span>
                  {user?.roles?.map((role) => (
                    <Badge key={role} variant="navy" size="sm">
                      {role}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>

            <Button variant="secondary" size="md" onClick={() => logout()}>
              <LogOut size={16} aria-hidden="true" /> Oturumu Kapat
            </Button>
          </div>
        </Card>

        {/* Info Box */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 'var(--space-4)' }}>
          <Card padding="md">
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '8px' }}>
              <UserCheck size={20} color="var(--color-brand-primary)" />
              <h3 style={{ fontSize: 'var(--font-size-md)', fontWeight: 600 }}>Yetkili Oturum Açıldı</h3>
            </div>
            <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', lineHeight: 1.5 }}>
              Kimliğiniz ASP.NET Core Identity ile başarıyla doğrulandı ve korumalı oturum aktif.
            </p>
          </Card>

          <Card padding="md">
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '8px' }}>
              <Lock size={20} color="var(--color-brand-primary)" />
              <h3 style={{ fontSize: 'var(--font-size-md)', fontWeight: 600 }}>Yönetim Özellikleri (Phase 9)</h3>
            </div>
            <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', lineHeight: 1.5 }}>
              Proje ekleme, düzenleme, silme ve medya yükleme formları bir sonraki fazda aktif edilecektir.
            </p>
          </Card>
        </div>
      </div>
    </PageLayout>
  );
}

export default AdminPage;
