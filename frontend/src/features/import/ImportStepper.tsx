import { Check } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface ImportStepperProps {
  currentStep: number;
}

export function ImportStepper({ currentStep }: ImportStepperProps) {
  const { t, i18n } = useTranslation(['excel']);

  const steps = [
    { step: 1, label: i18n.language === 'en' ? 'Step 1' : 'Adım 1', title: t('import.stepper.step1', { ns: 'excel' }) },
    { step: 2, label: i18n.language === 'en' ? 'Step 2' : 'Adım 2', title: t('import.stepper.step2', { ns: 'excel' }) },
    { step: 3, label: i18n.language === 'en' ? 'Step 3' : 'Adım 3', title: t('import.stepper.step3', { ns: 'excel' }) },
    { step: 4, label: i18n.language === 'en' ? 'Step 4' : 'Adım 4', title: t('import.stepper.step4', { ns: 'excel' }) },
  ];

  return (
    <nav className="import-stepper" aria-label={t('import.wizardTitle', { ns: 'excel' })}>
      {steps.map((s) => {
        const isCompleted = currentStep > s.step;
        const isActive = currentStep === s.step;

        return (
          <div
            key={s.step}
            className={`import-step-item ${
              isCompleted ? 'import-step-item--completed' : ''
            } ${isActive ? 'import-step-item--active' : ''}`}
            aria-current={isActive ? 'step' : undefined}
          >
            <div className="import-step-bubble" aria-hidden="true">
              {isCompleted ? <Check size={18} strokeWidth={3} /> : s.step}
            </div>
            <div className="import-step-meta">
              <span className="import-step-label">{s.label}</span>
              <span className="import-step-title">{s.title}</span>
            </div>
          </div>
        );
      })}
    </nav>
  );
}

export default ImportStepper;
