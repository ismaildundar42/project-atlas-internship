import { useState } from 'react';
import {
  CheckCircle2,
  AlertCircle,
  AlertTriangle,
  ArrowLeft,
  RotateCcw,
  Check,
  X,
  Sparkles,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import ConfirmImportModal from './ConfirmImportModal';
import { getFormattedImportError } from '../../utils/importErrorHelper';
import type {
  ValidateImportResponseDto,
} from '../../types/importExport';

interface Step3ValidatePreviewProps {
  validationResult: ValidateImportResponseDto;
  onBackToMapping: () => void;
  onResetToFile: () => void;
  onConfirmImport: () => Promise<void>;
  isConfirming: boolean;
}

export function Step3ValidatePreview({
  validationResult,
  onBackToMapping,
  onResetToFile,
  onConfirmImport,
  isConfirming,
}: Step3ValidatePreviewProps) {
  const { t, i18n } = useTranslation(['excel', 'common']);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const {
    totalRows,
    validRowCount,
    invalidRowCount,
    warningCount,
    canImport,
    previewRows,
    errors,
    warnings,
    totalErrorCount,
    errorsTruncated,
    proposedNewReferences = [],
  } = validationResult;

  const proposedRefs = proposedNewReferences;
  const proposedTechs = proposedRefs.filter((r) => r.type === 'Technology' || r.type === 'Teknoloji').map((r) => r.value);
  const proposedLocs = proposedRefs.filter((r) => r.type === 'Location' || r.type === 'Lokasyon').map((r) => r.value);
  const proposedTags = proposedRefs.filter((r) => r.type === 'Tag' || r.type === 'Etiket').map((r) => r.value);

  const handleOpenConfirm = () => {
    if (canImport) {
      setIsModalOpen(true);
    }
  };

  const handleExecuteConfirm = async () => {
    setIsModalOpen(false);
    await onConfirmImport();
  };

  return (
    <div className="import-step-container">
      {/* ─── Summary Metric Cards ──────────────────────────────────────────── */}
      <div className="import-metrics-grid">
        <div className="import-metric-card">
          <span className="import-metric-title">{t('import.step3.totalRows', { ns: 'excel' })}</span>
          <span className="import-metric-value">{totalRows}</span>
        </div>

        <div className={`import-metric-card ${validRowCount > 0 ? 'import-metric-card--success' : ''}`}>
          <span className="import-metric-title">{t('import.step3.validRows', { ns: 'excel' })}</span>
          <span className="import-metric-value" style={{ color: 'var(--color-success)' }}>
            {validRowCount}
          </span>
        </div>

        <div className={`import-metric-card ${invalidRowCount > 0 ? 'import-metric-card--danger' : ''}`}>
          <span className="import-metric-title">{t('import.step3.invalidRows', { ns: 'excel' })}</span>
          <span className="import-metric-value" style={{ color: invalidRowCount > 0 ? 'var(--color-danger)' : 'var(--color-text-primary)' }}>
            {invalidRowCount}
          </span>
        </div>

        <div className={`import-metric-card ${warningCount > 0 ? 'import-metric-card--warning' : ''}`}>
          <span className="import-metric-title">{t('import.step3.warnings', { ns: 'excel' })}</span>
          <span className="import-metric-value" style={{ color: warningCount > 0 ? 'var(--color-warning)' : 'var(--color-text-primary)' }}>
            {warningCount}
          </span>
        </div>
      </div>

      {/* ─── Status Banner ─────────────────────────────────────────────────── */}
      {canImport ? (
        <div className="import-status-banner import-status-banner--success">
          <CheckCircle2 size={24} style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
          <div>
            <div style={{ fontWeight: 700, fontSize: 'var(--font-size-md)' }}>
              {i18n.language === 'en' ? `File Ready for Import (${validRowCount} Projects)` : `Dosya İçe Aktarmaya Hazır (${validRowCount} Proje)`}
            </div>
            <div style={{ fontSize: 'var(--font-size-xs)', marginTop: '4px' }}>
              {t('import.step3.canImportNotice', { ns: 'excel' })}
            </div>
          </div>
        </div>
      ) : (
        <div className="import-status-banner import-status-banner--danger">
          <AlertCircle size={24} style={{ flexShrink: 0, marginTop: '2px' }} aria-hidden="true" />
          <div>
            <div style={{ fontWeight: 700, fontSize: 'var(--font-size-md)' }}>
              {i18n.language === 'en' ? `Validation Issues Found (${invalidRowCount} Invalid Rows)` : `Dosyada Düzeltilmesi Gereken Hatalar Var (${invalidRowCount} Satır Hatalı)`}
            </div>
            <div style={{ fontSize: 'var(--font-size-xs)', marginTop: '4px' }}>
              {t('import.step3.cannotImportNotice', { ns: 'excel' })}
            </div>
          </div>
        </div>
      )}

      {/* ─── Proposed New Master-Data References (If any) ──────────────── */}
      {proposedRefs.length > 0 && (
        <Card padding="md" style={{ marginBottom: '20px', borderLeft: '4px solid var(--color-primary-500, #2563eb)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
            <Sparkles size={18} color="var(--color-primary-500, #2563eb)" aria-hidden="true" />
            <h4 style={{ fontSize: 'var(--font-size-sm)', fontWeight: 700, margin: 0, color: 'var(--color-text-primary)' }}>
              {t('import.step3.proposedReferences', { ns: 'excel' })} ({proposedRefs.length})
            </h4>
          </div>
          <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', margin: '0 0 12px 0' }}>
            {t('import.step3.proposedNotice', { ns: 'excel' })}
          </p>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px' }}>
            {proposedTechs.length > 0 && (
              <div style={{ backgroundColor: 'var(--color-surface-subtle)', padding: '10px 12px', borderRadius: '6px' }}>
                <span style={{ fontSize: '11px', fontWeight: 700, textTransform: 'uppercase', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '6px' }}>
                  {i18n.language === 'en' ? `Technologies (${proposedTechs.length})` : `Teknolojiler (${proposedTechs.length})`}
                </span>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                  {proposedTechs.map((tech, idx) => (
                    <Badge key={idx} variant="navy" size="sm">
                      {tech}
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            {proposedLocs.length > 0 && (
              <div style={{ backgroundColor: 'var(--color-surface-subtle)', padding: '10px 12px', borderRadius: '6px' }}>
                <span style={{ fontSize: '11px', fontWeight: 700, textTransform: 'uppercase', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '6px' }}>
                  {i18n.language === 'en' ? `Locations (${proposedLocs.length})` : `Lokasyonlar (${proposedLocs.length})`}
                </span>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                  {proposedLocs.map((loc, idx) => (
                    <Badge key={idx} variant="info" size="sm">
                      {loc}
                    </Badge>
                  ))}
                </div>
              </div>
            )}

            {proposedTags.length > 0 && (
              <div style={{ backgroundColor: 'var(--color-surface-subtle)', padding: '10px 12px', borderRadius: '6px' }}>
                <span style={{ fontSize: '11px', fontWeight: 700, textTransform: 'uppercase', color: 'var(--color-text-secondary)', display: 'block', marginBottom: '6px' }}>
                  {i18n.language === 'en' ? `Tags (${proposedTags.length})` : `Etiketler (${proposedTags.length})`}
                </span>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                  {proposedTags.map((tag, idx) => (
                    <Badge key={idx} variant="default" size="sm">
                      {tag}
                    </Badge>
                  ))}
                </div>
              </div>
            )}
          </div>
        </Card>
      )}

      {/* ─── Warnings List (If any) ───────────────────────────────────────── */}
      {warnings.length > 0 && (
        <Card padding="md" style={{ marginBottom: '20px', borderLeft: '4px solid var(--color-warning)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
            <AlertTriangle size={18} color="var(--color-warning)" aria-hidden="true" />
            <h4 style={{ fontSize: 'var(--font-size-sm)', fontWeight: 700, margin: 0, color: 'var(--color-text-primary)' }}>
              {i18n.language === 'en' ? `Warnings (${warnings.length})` : `Bilgilendirme ve Uyarılar (${warnings.length})`}
            </h4>
          </div>
          <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', lineHeight: '1.5' }}>
            {warnings.map((w, idx) => (
              <div key={idx} style={{ marginTop: '4px' }}>
                • {i18n.language === 'en' ? 'Row' : 'Satır'} {w.rowNumber ?? '-'}: {w.message}
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* ─── Error Table (If any) ─────────────────────────────────────────── */}
      {errors.length > 0 && (
        <Card padding="lg" style={{ marginBottom: '24px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
            <h4 style={{ fontSize: 'var(--font-size-md)', fontWeight: 700, margin: 0, color: 'var(--color-danger)' }}>
              {i18n.language === 'en' ? `Error Details (${errors.length}${errorsTruncated ? ` / Total ${totalErrorCount}` : ''})` : `Hata Detayları (${errors.length}${errorsTruncated ? ` / Toplam ${totalErrorCount}` : ''})`}
            </h4>
            {errorsTruncated && (
              <Badge variant="warning" size="sm">
                {i18n.language === 'en' ? 'Showing first 200 errors' : 'İlk 200 hata gösteriliyor'}
              </Badge>
            )}
          </div>

          <div className="import-issues-table-wrapper">
            <table className="import-mapping-table">
              <thead>
                <tr>
                  <th style={{ width: '60px' }}>{t('import.step3.tableRow', { ns: 'excel' })}</th>
                  <th style={{ width: '60px' }}>{t('import.step2.columnHeader', { ns: 'excel' })}</th>
                  <th>{t('import.step2.systemFieldHeader', { ns: 'excel' })}</th>
                  <th>{t('import.step2.sampleHeader', { ns: 'excel' })}</th>
                  <th>{t('import.step3.tableIssues', { ns: 'excel' })}</th>
                </tr>
              </thead>
              <tbody>
                {errors.map((err, idx) => {
                  const errorInfo = getFormattedImportError(err.errorCode, err.message);

                  return (
                    <tr key={idx}>
                      <td>
                        <strong style={{ fontFamily: 'var(--font-family-mono)', color: 'var(--color-danger)' }}>
                          {err.rowNumber ?? '-'}
                        </strong>
                      </td>
                      <td>
                        <strong style={{ fontFamily: 'var(--font-family-mono)' }}>
                          {err.excelColumn ?? '-'}
                        </strong>
                      </td>
                      <td>
                        <span style={{ fontWeight: 600 }}>{err.excelHeader || err.systemField || '-'}</span>
                      </td>
                      <td>
                        <code
                          style={{
                            fontSize: '12px',
                            padding: '2px 6px',
                            backgroundColor: 'var(--color-surface-subtle)',
                            border: '1px solid var(--color-border)',
                            borderRadius: '4px',
                            maxWidth: '180px',
                            display: 'inline-block',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap',
                          }}
                          title={err.rawValue ?? ''}
                        >
                          {err.rawValue ? String(err.rawValue) : (i18n.language === 'en' ? '(Empty)' : '(Boş)')}
                        </code>
                      </td>
                      <td>
                        <div style={{ fontWeight: 600, color: 'var(--color-danger)', fontSize: '13px' }}>
                          {err.message || errorInfo.title}
                        </div>
                        <div style={{ fontSize: '11px', color: 'var(--color-text-secondary)', marginTop: '2px' }}>
                          {errorInfo.remedy}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {/* ─── Preview Table (First 20 Rows) ─────────────────────────────────── */}
      <Card padding="lg">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
          <div>
            <h4 style={{ fontSize: 'var(--font-size-md)', fontWeight: 700, margin: 0, color: 'var(--color-text-primary)' }}>
              {i18n.language === 'en' ? `Data Preview (First ${previewRows.length} Rows)` : `Veri Önizleme (İlk ${previewRows.length} Satır)`}
            </h4>
            <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-muted)', margin: 0, marginTop: '2px' }}>
              {i18n.language === 'en' ? 'Preview of validated rows transformed to system fields.' : 'Doğrulanan ve sistem alanlarına dönüştürülen satırların önizlemesi.'}
            </p>
          </div>
          <Badge variant="navy" size="sm">
            {previewRows.length} / {totalRows} {i18n.language === 'en' ? 'Rows' : 'Satır'}
          </Badge>
        </div>

        <div className="import-preview-table-wrapper">
          <table className="import-mapping-table">
            <thead>
              <tr>
                <th style={{ width: '50px' }}>No</th>
                <th style={{ width: '80px' }}>{t('import.step3.tableStatus', { ns: 'excel' })}</th>
                <th>{t('admin.columns.name', { ns: 'projects' })}</th>
                <th>{t('admin.columns.category', { ns: 'projects' })}</th>
                <th>{t('admin.columns.status', { ns: 'projects' })}</th>
                <th>{t('admin.columns.primaryTeam', { ns: 'projects' })}</th>
                <th>{t('editor.fields.developmentType', { ns: 'projects' })}</th>
                <th>{t('editor.fields.technologies', { ns: 'projects' })}</th>
                <th>{t('editor.fields.locations', { ns: 'projects' })}</th>
              </tr>
            </thead>
            <tbody>
              {previewRows.map((row) => {
                const disp = row.displayValues || {};

                return (
                  <tr
                    key={row.rowNumber}
                    className={!row.isValid ? 'import-preview-row--invalid' : ''}
                  >
                    <td>
                      <strong style={{ fontFamily: 'var(--font-family-mono)', color: 'var(--color-text-muted)' }}>
                        {row.rowNumber}
                      </strong>
                    </td>
                    <td>
                      {row.isValid ? (
                        <Badge variant="success" size="sm">
                          <Check size={12} aria-hidden="true" /> {i18n.language === 'en' ? 'Valid' : 'Geçerli'}
                        </Badge>
                      ) : (
                        <Badge variant="danger" size="sm">
                          <X size={12} aria-hidden="true" /> {i18n.language === 'en' ? 'Invalid' : 'Hatalı'}
                        </Badge>
                      )}
                    </td>
                    <td>
                      <strong style={{ color: 'var(--color-text-primary)' }}>
                        {disp.name ? String(disp.name) : '-'}
                      </strong>
                    </td>
                    <td>{disp.category ? String(disp.category) : '-'}</td>
                    <td>{disp.status ? String(disp.status) : '-'}</td>
                    <td>{disp.primaryTeam ? String(disp.primaryTeam) : '-'}</td>
                    <td>{disp.developmentType ? String(disp.developmentType) : 'Internal'}</td>
                    <td>{disp.technologies ? String(disp.technologies) : '-'}</td>
                    <td>{disp.locations ? String(disp.locations) : '-'}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </Card>

      {/* ─── Footer Actions ────────────────────────────────────────────────── */}
      <div className="import-wizard-footer">
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="secondary" size="md" onClick={onBackToMapping} disabled={isConfirming}>
            <ArrowLeft size={16} aria-hidden="true" /> {i18n.language === 'en' ? 'Back to Mapping' : 'Sütun Eşlemeye Dön'}
          </Button>

          <Button variant="ghost" size="md" onClick={onResetToFile} disabled={isConfirming}>
            <RotateCcw size={16} aria-hidden="true" /> {i18n.language === 'en' ? 'Change File' : 'Dosyayı Değiştir'}
          </Button>
        </div>

        {canImport ? (
          <Button
            variant="primary"
            size="md"
            onClick={handleOpenConfirm}
            isLoading={isConfirming}
          >
            <Sparkles size={16} aria-hidden="true" /> {t('import.step3.confirmBtn', { count: validRowCount, ns: 'excel' })}
          </Button>
        ) : (
          <Button
            variant="primary"
            size="md"
            disabled={true}
            title={i18n.language === 'en' ? 'Blocking errors must be resolved before import.' : 'Hatalı satırlar düzeltilmeden içe aktarma yapılamaz.'}
          >
            {i18n.language === 'en' ? 'Errors Must Be Fixed' : 'Hatalar Düzeltilmeli'}
          </Button>
        )}
      </div>

      {/* ─── Confirmation Modal ────────────────────────────────────────────── */}
      <ConfirmImportModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onConfirm={handleExecuteConfirm}
        isConfirming={isConfirming}
        projectCount={validRowCount}
      />
    </div>
  );
}

export default Step3ValidatePreview;
