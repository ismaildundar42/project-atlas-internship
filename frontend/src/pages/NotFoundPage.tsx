import { Link } from 'react-router-dom';
import { LayoutDashboard } from 'lucide-react';
import { useTranslation } from 'react-i18next';

/**
 * 404 Sayfa Bulunamadı.
 * Tanımsız route'lara düşen tüm ziyaretçiler buraya yönlendirilir.
 * Shell dışında çalışır — kendi tam sayfa düzenine sahiptir.
 */
function NotFoundPage() {
  const { t } = useTranslation(['common', 'navigation']);

  return (
    <div
      style={{
        minHeight: '100dvh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'var(--color-bg)',
        padding: 'var(--space-6)',
      }}
      role="main"
      aria-label={t('common.pageNotFound', 'Sayfa bulunamadı')}
    >
      <div
        style={{
          textAlign: 'center',
          maxWidth: '480px',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 'var(--space-4)',
        }}
      >
        {/* 404 Numara */}
        <div
          style={{
            fontSize: '6rem',
            fontWeight: 'var(--font-weight-bold)',
            lineHeight: 1,
            color: 'var(--color-brand-navy)',
            letterSpacing: '-0.04em',
            opacity: 0.12,
          }}
          aria-hidden="true"
        >
          404
        </div>

        {/* İkon */}
        <div style={{ color: 'var(--color-border-strong)', marginTop: '-3.5rem' }}>
          <LayoutDashboard size={48} strokeWidth={1} aria-hidden="true" />
        </div>

        {/* Başlık */}
        <h1
          style={{
            fontSize: 'var(--font-size-2xl)',
            fontWeight: 'var(--font-weight-bold)',
            color: 'var(--color-text-primary)',
          }}
        >
          {t('common.pageNotFound', 'Sayfa Bulunamadı')}
        </h1>

        <p
          style={{
            fontSize: 'var(--font-size-md)',
            color: 'var(--color-text-muted)',
            lineHeight: 'var(--line-height-relaxed)',
          }}
        >
          {t('common.pageNotFoundDesc', 'Aradığınız sayfa mevcut değil veya taşınmış olabilir.')}
        </p>

        <Link
          to="/"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 'var(--space-2)',
            marginTop: 'var(--space-2)',
            padding: 'var(--space-2) var(--space-5)',
            backgroundColor: 'var(--color-brand-navy)',
            color: 'white',
            borderRadius: 'var(--radius-md)',
            fontSize: 'var(--font-size-sm)',
            fontWeight: 'var(--font-weight-medium)',
            textDecoration: 'none',
            transition: 'background-color var(--transition-fast)',
          }}
          aria-label={t('navigation.goToDashboard', 'Ana sayfaya dön')}
        >
          ← {t('navigation.goToDashboard', 'Ana Sayfaya Dön')}
        </Link>
      </div>
    </div>
  );
}

export default NotFoundPage;
