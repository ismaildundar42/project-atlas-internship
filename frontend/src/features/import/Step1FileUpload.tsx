import React, { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import {
  FileSpreadsheet,
  Upload,
  Download,
  CheckCircle2,
  AlertCircle,
  FileCheck,
  RefreshCw,
  ArrowRight,
  Layers,
} from 'lucide-react';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import type { InspectWorkbookResponseDto } from '../../types/importExport';

interface Step1FileUploadProps {
  selectedFile: File | null;
  onSelectFile: (file: File | null) => void;
  onInspect: (file: File) => Promise<void>;
  isInspecting: boolean;
  inspectResult: InspectWorkbookResponseDto | null;
  selectedSheet: string;
  onSelectSheet: (sheetName: string) => void;
  onDownloadTemplate: () => Promise<void>;
  isDownloadingTemplate: boolean;
  onProceedToMapping: () => void;
}

export function Step1FileUpload({
  selectedFile,
  onSelectFile,
  onInspect,
  isInspecting,
  inspectResult,
  selectedSheet,
  onSelectSheet,
  onDownloadTemplate,
  isDownloadingTemplate,
  onProceedToMapping,
}: Step1FileUploadProps) {
  const { t, i18n } = useTranslation(['excel', 'common']);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    validateAndSetFile(file);
  };

  const validateAndSetFile = (file: File) => {
    setLocalError(null);

    // Extension check
    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      setLocalError(i18n.language === 'en' ? 'Only Excel files in .xlsx format are supported.' : 'Yalnızca .xlsx formatındaki Excel dosyaları desteklenmektedir.');
      return;
    }

    // Size check (10 MB)
    if (file.size > 10 * 1024 * 1024) {
      setLocalError(i18n.language === 'en' ? 'Excel file cannot exceed 10 MB.' : 'Excel dosyası en fazla 10 MB boyutunda olabilir.');
      return;
    }

    if (file.size === 0) {
      setLocalError(i18n.language === 'en' ? 'Selected file cannot be empty.' : 'Seçilen dosya boş olamaz.');
      return;
    }

    onSelectFile(file);
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(false);

    const file = e.dataTransfer.files?.[0];
    if (file) {
      validateAndSetFile(file);
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = () => {
    setIsDragOver(false);
  };

  const handleInspectClick = () => {
    if (selectedFile) {
      onInspect(selectedFile);
    }
  };

  const currentSheet = inspectResult?.sheets.find((s) => s.name === selectedSheet);

  return (
    <div className="import-step-container">
      {/* ─── Template Download Card ────────────────────────────────────────── */}
      <div className="import-template-card">
        <div className="import-template-info">
          <Download size={22} className="import-template-icon" aria-hidden="true" />
          <div>
            <div className="import-template-title">{i18n.language === 'en' ? 'Official Excel Template' : 'Resmi Excel Şablonu'}</div>
            <p className="import-template-text">
              {t('import.step1.downloadTemplateDesc', { ns: 'excel' })}
            </p>
          </div>
        </div>
        <Button
          variant="secondary"
          size="sm"
          onClick={onDownloadTemplate}
          isLoading={isDownloadingTemplate}
        >
          <FileSpreadsheet size={16} aria-hidden="true" /> {t('import.step1.downloadTemplateBtn', { ns: 'excel' })}
        </Button>
      </div>

      {/* ─── Upload Card ──────────────────────────────────────────────────── */}
      <Card padding="lg">
        <h3 style={{ fontSize: 'var(--font-size-lg)', fontWeight: 700, color: 'var(--color-text-primary)', marginBottom: '16px' }}>
          {i18n.language === 'en' ? 'Excel File Selection' : 'Excel Dosyası Seçimi'}
        </h3>

        <input
          ref={fileInputRef}
          type="file"
          accept=".xlsx"
          style={{ display: 'none' }}
          onChange={handleFileChange}
          aria-label="Upload Excel file"
        />

        {!selectedFile ? (
          <div
            className={`import-dropzone ${isDragOver ? 'import-dropzone--drag-over' : ''}`}
            onClick={() => fileInputRef.current?.click()}
            onDrop={handleDrop}
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                fileInputRef.current?.click();
              }
            }}
          >
            <div className="import-dropzone-icon" aria-hidden="true">
              <Upload size={24} />
            </div>
            <div className="import-dropzone-title">{t('import.step1.dropzoneTitle', { ns: 'excel' })}</div>
            <div className="import-dropzone-desc">{t('import.step1.dropzoneSubtitle', { ns: 'excel' })}</div>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                fileInputRef.current?.click();
              }}
            >
              {i18n.language === 'en' ? 'Browse File' : 'Dosya Seç'}
            </Button>
          </div>
        ) : (
          <div className="import-selected-file-wrapper">
            <div className="import-selected-file-card">
              <div className="import-selected-file-info">
                <FileSpreadsheet size={32} color="var(--color-brand-navy)" aria-hidden="true" />
                <div>
                  <div className="import-selected-file-name">{selectedFile.name}</div>
                  <div className="import-selected-file-size">{formatFileSize(selectedFile.size)}</div>
                </div>
              </div>

              <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    onSelectFile(null);
                    if (fileInputRef.current) fileInputRef.current.value = '';
                  }}
                  disabled={isInspecting}
                >
                  <RefreshCw size={14} aria-hidden="true" /> {i18n.language === 'en' ? 'Change File' : 'Dosyayı Değiştir'}
                </Button>

                {!inspectResult?.canProceedToMapping && (
                  <Button
                    type="button"
                    variant="primary"
                    size="md"
                    onClick={handleInspectClick}
                    isLoading={isInspecting}
                  >
                    <FileCheck size={16} aria-hidden="true" /> {i18n.language === 'en' ? 'Inspect File' : 'Dosyayı İncele'}
                  </Button>
                )}
              </div>
            </div>

            {/* Inspection Results / Sheet Selector */}
            {inspectResult && (
              <div style={{ marginTop: '20px' }}>
                {inspectResult.canProceedToMapping ? (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                    <div className="import-status-banner import-status-banner--success">
                      <CheckCircle2 size={20} style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
                      <div>
                        <div style={{ fontWeight: 700, fontSize: 'var(--font-size-sm)' }}>
                          {i18n.language === 'en' ? 'File Structure Validated' : 'Dosya Yapısı Doğrulandı'}
                        </div>
                        <div style={{ fontSize: 'var(--font-size-xs)', marginTop: '2px' }}>
                          {i18n.language === 'en' ? 'Workbook inspected. Worksheets and headers read successfully.' : 'Excel dosyası incelendi. Sayfalar ve sütun başlıkları başarıyla okundu.'}
                        </div>
                      </div>
                    </div>

                    {/* Sheet Selection if multiple sheets exist */}
                    {inspectResult.sheets.length > 1 ? (
                      <div className="import-sheet-selector-box">
                        <Layers size={18} color="var(--color-brand-navy)" aria-hidden="true" />
                        <label htmlFor="sheet-select" className="import-sheet-label">
                          {t('import.step2.sheetSelectLabel', { ns: 'excel' })}:
                        </label>
                        <select
                          id="sheet-select"
                          className="import-sheet-select"
                          value={selectedSheet}
                          onChange={(e) => onSelectSheet(e.target.value)}
                        >
                          {inspectResult.sheets.map((sheet) => (
                            <option key={sheet.name} value={sheet.name}>
                              {sheet.name} ({sheet.dataRowCount} {i18n.language === 'en' ? 'rows' : 'veri satırı'}, {sheet.usedColumnCount} {i18n.language === 'en' ? 'cols' : 'sütun'})
                            </option>
                          ))}
                        </select>
                      </div>
                    ) : (
                      <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', padding: '4px 8px' }}>
                        {t('import.step2.sheetSelectLabel', { ns: 'excel' })}: <strong>{selectedSheet || inspectResult.sheets[0]?.name}</strong> ({currentSheet?.dataRowCount ?? 0} {i18n.language === 'en' ? 'rows detected' : 'veri satırı tespit edildi'})
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="import-status-banner import-status-banner--danger">
                    <AlertCircle size={20} style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
                    <div>
                      <div style={{ fontWeight: 700, fontSize: 'var(--font-size-sm)' }}>
                        {i18n.language === 'en' ? 'File Structure Issue Detected' : 'Dosya Yapısında Sorun Tespit Edildi'}
                      </div>
                      <div style={{ fontSize: 'var(--font-size-xs)', marginTop: '4px' }}>
                        {inspectResult.issues.map((issue, idx) => (
                          <div key={idx} style={{ marginTop: '2px' }}>
                            • {issue.message}
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        )}

        {localError && (
          <div className="import-status-banner import-status-banner--danger" style={{ marginTop: '16px' }}>
            <AlertCircle size={18} style={{ flexShrink: 0 }} aria-hidden="true" />
            <span style={{ fontSize: 'var(--font-size-xs)' }}>{localError}</span>
          </div>
        )}
      </Card>

      {/* Footer Navigation */}
      {inspectResult?.canProceedToMapping && (
        <div className="import-wizard-footer">
          <div />
          <Button variant="primary" size="md" onClick={onProceedToMapping}>
            {t('import.step1.continueBtn', { ns: 'excel' })} <ArrowRight size={16} aria-hidden="true" />
          </Button>
        </div>
      )}
    </div>
  );
}

export default Step1FileUpload;
