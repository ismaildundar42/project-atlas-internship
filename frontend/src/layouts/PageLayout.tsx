import { type ReactNode } from 'react';
import { Link } from 'react-router-dom';

// ─── Types ────────────────────────────────────────────────────────────────────

interface Breadcrumb {
  label: string;
  href?: string;
}

interface PageLayoutProps {
  /** Sayfa başlığı */
  title: string;
  /** Opsiyonel açıklama */
  description?: string;
  /** Breadcrumb navigasyonu */
  breadcrumbs?: Breadcrumb[];
  /** Sağ tarafa yerleştirilen aksiyonlar (butonlar vb.) */
  actions?: ReactNode;
  /** Sayfa içeriği */
  children: ReactNode;
}

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * Reusable sayfa container bileşeni.
 * Tüm sayfalar bu layout'u kullanarak tutarlı başlık ve içerik yapısı sağlar.
 */
function PageLayout({
  title,
  description,
  breadcrumbs,
  actions,
  children,
}: PageLayoutProps) {
  return (
    <div className="page-layout">
      {/* Sayfa Başlığı */}
      <header className="page-header">
        {/* Breadcrumbs */}
        {breadcrumbs && breadcrumbs.length > 0 && (
          <nav
            aria-label="Konum izleği"
            className="page-header__breadcrumbs"
          >
            {breadcrumbs.map((crumb, i) => (
              <span key={i} className="page-header__breadcrumb-item">
                {i > 0 && (
                  <span className="page-header__breadcrumb-sep" aria-hidden="true">
                    /
                  </span>
                )}
                {crumb.href ? (
                  <Link to={crumb.href}>{crumb.label}</Link>
                ) : (
                  <span aria-current={i === breadcrumbs.length - 1 ? 'page' : undefined}>
                    {crumb.label}
                  </span>
                )}
              </span>
            ))}
          </nav>
        )}

        <div className="page-header__top">
          <div>
            <h1 className="page-header__title">{title}</h1>
            {description && (
              <p className="page-header__description">{description}</p>
            )}
          </div>

          {actions && (
            <div className="page-header__actions" aria-label="Sayfa aksiyonları">
              {actions}
            </div>
          )}
        </div>
      </header>

      {/* İçerik */}
      <div className="page-content">
        {children}
      </div>
    </div>
  );
}

export default PageLayout;
export type { PageLayoutProps, Breadcrumb };
