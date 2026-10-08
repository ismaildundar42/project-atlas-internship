import { useTranslation } from 'react-i18next';
import demirExportLogo from '../../assets/brand/demir-export-logo.png';

interface BrandMarkProps {
  /** Opsiyonel CSS sınıfı */
  className?: string;
}

/**
 * BrandMark — Demir Export kurumsal marka bileşeni.
 * Demir Export ana kurumsal logosunu ve ürün kimliğini sunar.
 */
function BrandMark({ className }: BrandMarkProps) {
  const { t } = useTranslation(['navigation', 'common']);

  return (
    <div
      className={`brand-mark ${className ?? ''}`}
      aria-label={`${t('navigation.brandTitle', 'Demir Export')} ${t('navigation.brandSubtitle', 'Proje Kütüphanesi')}`}
    >
      <img
        src={demirExportLogo}
        alt="Demir Export"
        className="brand-mark__logo"
      />
      <span className="brand-mark__subtitle">
        {t('navigation.brandSubtitle', 'Proje Kütüphanesi')}
      </span>
    </div>
  );
}

export default BrandMark;
