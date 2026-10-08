import { useState, useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { AlertCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import PageLayout from '../layouts/PageLayout';
import ImportStepper from '../features/import/ImportStepper';
import Step1FileUpload from '../features/import/Step1FileUpload';
import Step2ColumnMapping from '../features/import/Step2ColumnMapping';
import Step3ValidatePreview from '../features/import/Step3ValidatePreview';
import Step4Success from '../features/import/Step4Success';
import { projectImportService } from '../services/projectImportService';
import { extractErrorMessage } from '../utils/urlUtils';
import type {
  InspectWorkbookResponseDto,
  ColumnMappingDto,
  ValidateImportResponseDto,
  ConfirmImportResponseDto,
} from '../types/importExport';

export function ProjectImportWizardPage() {
  const { t, i18n } = useTranslation(['excel', 'navigation', 'common', 'projects']);
  const queryClient = useQueryClient();

  // Wizard state
  const [currentStep, setCurrentStep] = useState<number>(1);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [inspectResult, setInspectResult] = useState<InspectWorkbookResponseDto | null>(null);
  const [selectedSheet, setSelectedSheet] = useState<string>('');
  const [columnMappings, setColumnMappings] = useState<ColumnMappingDto[]>([]);
  const [validationResult, setValidationResult] = useState<ValidateImportResponseDto | null>(null);
  const [confirmResult, setConfirmResult] = useState<ConfirmImportResponseDto | null>(null);

  // Loading states
  const [isDownloadingTemplate, setIsDownloadingTemplate] = useState<boolean>(false);
  const [isInspecting, setIsInspecting] = useState<boolean>(false);
  const [isValidating, setIsValidating] = useState<boolean>(false);
  const [isConfirming, setIsConfirming] = useState<boolean>(false);
  const [globalError, setGlobalError] = useState<string | null>(null);

  // ─── 1. Template Download ──────────────────────────────────────────────────
  const handleDownloadTemplate = async () => {
    try {
      setIsDownloadingTemplate(true);
      setGlobalError(null);
      await projectImportService.downloadTemplate();
    } catch (err: unknown) {
      setGlobalError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to download template file.' : 'Şablon dosyası indirilirken bir hata oluştu.'));
    } finally {
      setIsDownloadingTemplate(false);
    }
  };

  // ─── 2. File Select & Reset ────────────────────────────────────────────────
  const handleSelectFile = (file: File | null) => {
    setSelectedFile(file);
    setInspectResult(null);
    setSelectedSheet('');
    setColumnMappings([]);
    setValidationResult(null);
    setConfirmResult(null);
    setGlobalError(null);
  };

  // ─── 3. Inspect Workbook ───────────────────────────────────────────────────
  const handleInspect = async (file: File) => {
    try {
      setIsInspecting(true);
      setGlobalError(null);
      const res = await projectImportService.inspectWorkbook(file);
      setInspectResult(res);

      if (res.canProceedToMapping) {
        const initialSheet = res.defaultSheetName || res.sheetNames[0] || '';
        setSelectedSheet(initialSheet);
        setColumnMappings(res.suggestedMappings || []);
      }
    } catch (err: unknown) {
      setGlobalError(extractErrorMessage(err, i18n.language === 'en' ? 'Failed to inspect Excel workbook.' : 'Excel dosyası incelenirken bir hata oluştu.'));
    } finally {
      setIsInspecting(false);
    }
  };

  // ─── 4. Sheet Change ───────────────────────────────────────────────────────
  const handleSelectSheet = (sheetName: string) => {
    setSelectedSheet(sheetName);

    if (inspectResult) {
      const targetSheet = inspectResult.sheets.find((s) => s.name === sheetName);
      if (targetSheet) {
        // Compute initial mappings for the chosen sheet
        const sheetMappings: ColumnMappingDto[] = [];
        targetSheet.headers.forEach((h) => {
          if (h.suggestedSystemField && h.suggestionConfidence !== 'none') {
            sheetMappings.push({
              excelColumn: h.columnLetter,
              systemField: h.suggestedSystemField,
            });
          }
        });
        setColumnMappings(sheetMappings);
      }
    }
  };

  // ─── 5. Step 2: Mapping Handlers ───────────────────────────────────────────
  const handleChangeMapping = useCallback((columnLetter: string, systemField: string) => {
    setColumnMappings((prev) => {
      const filtered = prev.filter((m) => m.excelColumn !== columnLetter);
      if (systemField) {
        return [...filtered, { excelColumn: columnLetter, systemField }];
      }
      return filtered;
    });
  }, []);

  const handleAutoMapAll = useCallback(() => {
    if (!inspectResult) return;
    const currentSheetObj = inspectResult.sheets.find((s) => s.name === selectedSheet);
    if (!currentSheetObj) return;

    const autoMappings: ColumnMappingDto[] = [];
    currentSheetObj.headers.forEach((h) => {
      if (h.suggestedSystemField && h.suggestionConfidence !== 'none') {
        autoMappings.push({
          excelColumn: h.columnLetter,
          systemField: h.suggestedSystemField,
        });
      }
    });
    setColumnMappings(autoMappings);
  }, [inspectResult, selectedSheet]);

  const handleClearAllMappings = useCallback(() => {
    setColumnMappings([]);
  }, []);

  // ─── 6. Validate Import ────────────────────────────────────────────────────
  const handleValidate = async () => {
    if (!inspectResult?.fileToken) {
      setGlobalError(i18n.language === 'en' ? 'Valid file session not found. Please upload again.' : 'Geçerli bir dosya oturumu bulunamadı. Lütfen dosyayı tekrar yükleyiniz.');
      setCurrentStep(1);
      return;
    }

    try {
      setIsValidating(true);
      setGlobalError(null);

      const res = await projectImportService.validateImport({
        fileToken: inspectResult.fileToken,
        sheetName: selectedSheet,
        columnMappings,
      });

      setValidationResult(res);
      setCurrentStep(3);
    } catch (err: unknown) {
      const errorMsg = extractErrorMessage(err, i18n.language === 'en' ? 'An error occurred during validation.' : 'Doğrulama işlemi sırasında bir hata oluştu.');
      if (errorMsg.includes('INVALID_FILE_TOKEN') || errorMsg.includes('oturum') || errorMsg.includes('session')) {
        setGlobalError(i18n.language === 'en' ? 'Import session expired. Please re-upload your Excel workbook.' : 'İçe aktarma oturumunun süresi doldu. Lütfen Excel dosyanızı yeniden yükleyin.');
        setCurrentStep(1);
      } else {
        setGlobalError(errorMsg);
      }
    } finally {
      setIsValidating(false);
    }
  };

  // ─── 7. Confirm Import ─────────────────────────────────────────────────────
  const handleConfirmImport = async () => {
    if (!inspectResult?.fileToken) {
      setGlobalError(i18n.language === 'en' ? 'Import session not found.' : 'İçe aktarma oturumu bulunamadı.');
      setCurrentStep(1);
      return;
    }

    try {
      setIsConfirming(true);
      setGlobalError(null);

      const res = await projectImportService.confirmImport({
        fileToken: inspectResult.fileToken,
        sheetName: selectedSheet,
        columnMappings,
      });

      if (res.success) {
        setConfirmResult(res);
        setCurrentStep(4);

        // Invalidate relevant TanStack Query caches
        await queryClient.invalidateQueries({ queryKey: ['admin', 'projects'] });
        await queryClient.invalidateQueries({ queryKey: ['admin'] });
        await queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      } else {
        // Confirm-time revalidation failure
        if (res.validationResult) {
          setValidationResult(res.validationResult);
          setGlobalError(res.message || (i18n.language === 'en' ? 'File was re-validated and issues were found.' : 'Dosya yeniden doğrulandı ve bazı sorunlar bulundu.'));
          setCurrentStep(3);
        } else {
          setGlobalError(res.message || (i18n.language === 'en' ? 'Import could not be confirmed.' : 'İçe aktarma onaylanamadı.'));
        }
      }
    } catch (err: unknown) {
      const errorMsg = extractErrorMessage(err, i18n.language === 'en' ? 'An error occurred while confirming import.' : 'İçe aktarma onaylanırken bir hata oluştu.');
      if (errorMsg.includes('INVALID_FILE_TOKEN') || errorMsg.includes('oturum') || errorMsg.includes('session')) {
        setGlobalError(i18n.language === 'en' ? 'Import session expired. Please re-upload your Excel workbook.' : 'İçe aktarma oturumunun süresi doldu. Lütfen Excel dosyanızı yeniden yükleyin.');
        setCurrentStep(1);
      } else {
        setGlobalError(errorMsg);
      }
    } finally {
      setIsConfirming(false);
    }
  };

  // ─── 8. Reset to Step 1 ────────────────────────────────────────────────────
  const handleReset = () => {
    setCurrentStep(1);
    setSelectedFile(null);
    setInspectResult(null);
    setSelectedSheet('');
    setColumnMappings([]);
    setValidationResult(null);
    setConfirmResult(null);
    setGlobalError(null);
  };

  const currentHeaders =
    inspectResult?.sheets.find((s) => s.name === selectedSheet)?.headers || [];

  return (
    <PageLayout
      title={t('import.wizardTitle', { ns: 'excel' })}
      description={t('import.wizardSubtitle', { ns: 'excel' })}
      breadcrumbs={[
        { label: t('breadcrumb.home', { ns: 'navigation' }), href: '/' },
        { label: t('admin.overview', { ns: 'navigation' }), href: '/admin' },
        { label: t('admin.title', { ns: 'projects' }), href: '/admin/projects' },
        { label: t('admin.importExcelBtn', { ns: 'projects' }) },
      ]}
    >
      <div className="import-wizard">
        {/* Global Error Banner if any */}
        {globalError && (
          <div className="import-status-banner import-status-banner--danger" role="alert">
            <AlertCircle size={20} style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
            <div>
              <div style={{ fontWeight: 700, fontSize: 'var(--font-size-sm)' }}>{t('errors.genericTitle', { ns: 'common' })}</div>
              <div style={{ fontSize: 'var(--font-size-xs)', marginTop: '2px' }}>{globalError}</div>
            </div>
          </div>
        )}

        {/* Stepper Indicator */}
        <ImportStepper currentStep={currentStep} />

        {/* Step 1: File Upload */}
        {currentStep === 1 && (
          <Step1FileUpload
            selectedFile={selectedFile}
            onSelectFile={handleSelectFile}
            onInspect={handleInspect}
            isInspecting={isInspecting}
            inspectResult={inspectResult}
            selectedSheet={selectedSheet}
            onSelectSheet={handleSelectSheet}
            onDownloadTemplate={handleDownloadTemplate}
            isDownloadingTemplate={isDownloadingTemplate}
            onProceedToMapping={() => setCurrentStep(2)}
          />
        )}

        {/* Step 2: Column Mapping */}
        {currentStep === 2 && (
          <Step2ColumnMapping
            headers={currentHeaders}
            mappings={columnMappings}
            onChangeMapping={handleChangeMapping}
            onAutoMapAll={handleAutoMapAll}
            onClearAllMappings={handleClearAllMappings}
            onBack={() => setCurrentStep(1)}
            onValidate={handleValidate}
            isValidating={isValidating}
          />
        )}

        {/* Step 3: Validate & Preview */}
        {currentStep === 3 && validationResult && (
          <Step3ValidatePreview
            validationResult={validationResult}
            onBackToMapping={() => setCurrentStep(2)}
            onResetToFile={handleReset}
            onConfirmImport={handleConfirmImport}
            isConfirming={isConfirming}
          />
        )}

        {/* Step 4: Success */}
        {currentStep === 4 && confirmResult && (
          <Step4Success confirmResult={confirmResult} onReset={handleReset} />
        )}
      </div>
    </PageLayout>
  );
}

export default ProjectImportWizardPage;
