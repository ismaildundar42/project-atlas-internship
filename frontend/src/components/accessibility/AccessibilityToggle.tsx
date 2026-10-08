import { useState, forwardRef } from 'react';
import { Accessibility } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import AccessibilityPanel from './AccessibilityPanel';
import IconButton from '../ui/IconButton';

/**
 * Header'da gösterilen erişilebilirlik paneli tetikleyici butonu.
 * Panel açıkken aria-expanded ve aria-controls ile bağlantısı kurulur.
 */
const AccessibilityToggle = forwardRef<HTMLButtonElement>((_, ref) => {
  const { t } = useTranslation('common');
  const [panelOpen, setPanelOpen] = useState(false);

  return (
    <>
      <IconButton
        ref={ref}
        icon={<Accessibility size={18} />}
        aria-label={t('accessibility.openSettings', 'Erişilebilirlik ayarlarını aç')}
        aria-controls="a11y-panel"
        aria-expanded={panelOpen}
        isActive={panelOpen}
        variant="ghost"
        size="md"
        id="a11y-toggle-btn"
        onClick={() => setPanelOpen((prev) => !prev)}
      />

      <AccessibilityPanel
        isOpen={panelOpen}
        onClose={() => setPanelOpen(false)}
      />
    </>
  );
});

AccessibilityToggle.displayName = 'AccessibilityToggle';

export default AccessibilityToggle;

