import {
  ArrowLeft,
  ArrowRight,
  CheckCircle2,
  AlertTriangle,
  Sparkles,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import Button from '../../components/ui/Button';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import {
  SYSTEM_IMPORT_FIELDS,
  REQUIRED_SYSTEM_FIELDS,
} from '../../utils/importFieldCatalog';
import type {
  WorkbookHeaderDto,
  ColumnMappingDto,
} from '../../types/importExport';

interface Step2ColumnMappingProps {
  headers: WorkbookHeaderDto[];
  mappings: ColumnMappingDto[];
  onChangeMapping: (columnLetter: string, systemField: string) => void;
  onAutoMapAll: () => void;
  onClearAllMappings: () => void;
  onBack: () => void;
  onValidate: () => Promise<void>;
  isValidating: boolean;
}

export function Step2ColumnMapping({
  headers,
  mappings,
  onChangeMapping,
  onAutoMapAll,
  onClearAllMappings,
  onBack,
  onValidate,
  isValidating,
}: Step2ColumnMappingProps) {
  const { t, i18n } = useTranslation(['excel', 'common']);
  // Compute mapped field map
  const mappingMap = new Map<string, string>();
  mappings.forEach((m) => {
    if (m.systemField) {
      mappingMap.set(m.excelColumn, m.systemField);
    }
  });

  // Calculate mapped required and optional counts
  const mappedSystemFields = Array.from(mappingMap.values());
  const mappedRequiredCount = REQUIRED_SYSTEM_FIELDS.filter((rf) =>
    mappedSystemFields.includes(rf.key)
  ).length;
  const mappedOptionalCount = mappedSystemFields.filter(
    (f) => !REQUIRED_SYSTEM_FIELDS.some((rf) => rf.key === f)
  ).length;

  // Identify duplicate system field mappings
  const fieldCounts = new Map<string, number>();
  mappedSystemFields.forEach((field) => {
    fieldCounts.set(field, (fieldCounts.get(field) ?? 0) + 1);
  });
  const duplicateFields = Array.from(fieldCounts.entries())
    .filter(([_, count]) => count > 1)
    .map(([field]) => field);

  const isAllRequiredMapped = mappedRequiredCount === REQUIRED_SYSTEM_FIELDS.length;
  const hasDuplicateMappings = duplicateFields.length > 0;

  return (
    <div className="import-step-container">
      <Card padding="lg">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '16px', marginBottom: '16px' }}>
          <div>
            <h3 style={{ fontSize: 'var(--font-size-lg)', fontWeight: 700, color: 'var(--color-text-primary)', margin: 0 }}>
              {t('import.step2.title', { ns: 'excel' })}
            </h3>
            <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', marginTop: '4px', margin: 0 }}>
              {t('import.step2.subtitle', { ns: 'excel' })}
            </p>
          </div>

          <div style={{ display: 'flex', gap: '8px' }}>
            <Button type="button" variant="ghost" size="sm" onClick={onAutoMapAll}>
              <Sparkles size={14} aria-hidden="true" /> {i18n.language === 'en' ? 'Auto Map' : 'Otomatik Eşle'}
            </Button>
            <Button type="button" variant="ghost" size="sm" onClick={onClearAllMappings}>
              {i18n.language === 'en' ? 'Clear All' : 'Tümünü Temizle'}
            </Button>
          </div>
        </div>

        {/* ─── Mapping Summary Counters ────────────────────────────────────── */}
        <div className="import-mapping-summary-bar">
          <div className="import-mapping-counters">
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              {isAllRequiredMapped ? (
                <CheckCircle2 size={16} color="var(--color-success)" aria-hidden="true" />
              ) : (
                <AlertTriangle size={16} color="var(--color-warning)" aria-hidden="true" />
              )}
              <span>
                {i18n.language === 'en' ? (
                  <><strong>{mappedRequiredCount}</strong> of <strong>{REQUIRED_SYSTEM_FIELDS.length}</strong> required fields mapped</>
                ) : (
                  <><strong>{REQUIRED_SYSTEM_FIELDS.length}</strong> zorunlu alanın <strong style={{ color: isAllRequiredMapped ? 'var(--color-success)' : 'var(--color-warning)' }}>{mappedRequiredCount}</strong>'si eşleştirildi</>
                )}
              </span>
            </div>

            <div style={{ color: 'var(--color-text-muted)' }}>|</div>

            <div>
              {i18n.language === 'en' ? (
                <><strong>{mappedOptionalCount}</strong> optional fields mapped</>
              ) : (
                <><strong>{mappedOptionalCount}</strong> isteğe bağlı alan eşleştirildi</>
              )}
            </div>
          </div>

          {!isAllRequiredMapped && (
            <Badge variant="warning" size="sm">
              {i18n.language === 'en' ? 'Missing required fields' : 'Eksik zorunlu alanlar var'}
            </Badge>
          )}

          {hasDuplicateMappings && (
            <Badge variant="danger" size="sm">
              {i18n.language === 'en' ? 'Duplicate field mapping detected' : 'Aynı alana birden fazla sütun eşlenmiş'}
            </Badge>
          )}
        </div>

        {/* ─── Column Mapping Table ────────────────────────────────────────── */}
        <div className="import-mapping-table-wrapper">
          <table className="import-mapping-table">
            <thead>
              <tr>
                <th style={{ width: '80px' }}>{t('import.step2.columnHeader', { ns: 'excel' })}</th>
                <th>{t('import.step2.excelHeaderHeader', { ns: 'excel' })}</th>
                <th>{t('import.step2.systemFieldHeader', { ns: 'excel' })}</th>
                <th style={{ width: '140px' }}>{t('import.step3.tableStatus', { ns: 'excel' })}</th>
              </tr>
            </thead>
            <tbody>
              {headers.map((hdr) => {
                const currentField = mappingMap.get(hdr.columnLetter) ?? '';
                const isMapped = Boolean(currentField);
                const isDuplicate = isMapped && (fieldCounts.get(currentField) ?? 0) > 1;
                const fieldDef = SYSTEM_IMPORT_FIELDS.find((f) => f.key === currentField);
                const isSuggested =
                  !isMapped &&
                  Boolean(hdr.suggestedSystemField) &&
                  hdr.suggestionConfidence !== 'none';

                return (
                  <tr key={hdr.columnLetter}>
                    <td>
                      <strong style={{ fontFamily: 'var(--font-family-mono)', color: 'var(--color-brand-navy)' }}>
                        {hdr.columnLetter}
                      </strong>
                    </td>
                    <td>
                      <span style={{ fontWeight: 600, color: 'var(--color-text-primary)' }}>
                        {hdr.name || hdr.rawName || (i18n.language === 'en' ? '(Empty Header)' : '(Boş Başlık)')}
                      </span>
                    </td>
                    <td>
                      <select
                        className="import-field-select"
                        value={currentField}
                        onChange={(e) => onChangeMapping(hdr.columnLetter, e.target.value)}
                        aria-label={`${hdr.columnLetter} field mapping`}
                      >
                        <option value="">{t('import.step2.unmappedOption', { ns: 'excel' })}</option>

                        <optgroup label={i18n.language === 'en' ? '── Required Fields ──' : '── Zorunlu Alanlar ──'}>
                          {REQUIRED_SYSTEM_FIELDS.map((f) => {
                            const isAlreadyUsed =
                              mappedSystemFields.includes(f.key) && currentField !== f.key;
                            return (
                              <option key={f.key} value={f.key}>
                                {f.label} {isAlreadyUsed ? (i18n.language === 'en' ? '(Already Used)' : '(Zaten Seçildi)') : ''}
                              </option>
                            );
                          })}
                        </optgroup>

                        <optgroup label={i18n.language === 'en' ? '── Optional Fields ──' : '── İsteğe Bağlı Alanlar ──'}>
                          {SYSTEM_IMPORT_FIELDS.filter((f) => !f.required).map((f) => {
                            const isAlreadyUsed =
                              mappedSystemFields.includes(f.key) && currentField !== f.key;
                            return (
                              <option key={f.key} value={f.key}>
                                {f.label} {isAlreadyUsed ? (i18n.language === 'en' ? '(Already Used)' : '(Zaten Seçildi)') : ''}
                              </option>
                            );
                          })}
                        </optgroup>
                      </select>
                      {fieldDef && (
                        <div style={{ fontSize: '11px', color: 'var(--color-text-muted)', marginTop: '2px' }}>
                          {fieldDef.description}
                        </div>
                      )}
                    </td>
                    <td>
                      {isDuplicate ? (
                        <Badge variant="danger" size="sm">
                          {i18n.language === 'en' ? 'Duplicate' : 'Mükerrer'}
                        </Badge>
                      ) : isMapped ? (
                        <Badge variant="success" size="sm">
                          {fieldDef?.required ? (i18n.language === 'en' ? 'Required Mapped' : 'Zorunlu Eşleşti') : (i18n.language === 'en' ? 'Mapped' : 'Eşleşti')}
                        </Badge>
                      ) : isSuggested ? (
                        <Badge variant="info" size="sm">
                          {i18n.language === 'en' ? 'Suggested' : 'Öneri'}
                        </Badge>
                      ) : (
                        <Badge variant="default" size="sm">
                          {i18n.language === 'en' ? 'Unmapped' : 'Eşleme Yok'}
                        </Badge>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </Card>

      {/* ─── Footer Navigation ────────────────────────────────────────────── */}
      <div className="import-wizard-footer">
        <Button variant="secondary" size="md" onClick={onBack} disabled={isValidating}>
          <ArrowLeft size={16} aria-hidden="true" /> {i18n.language === 'en' ? 'Back to File' : 'Dosya Adımına Dön'}
        </Button>

        <Button
          variant="primary"
          size="md"
          onClick={onValidate}
          isLoading={isValidating}
          disabled={!isAllRequiredMapped || hasDuplicateMappings}
        >
          {t('import.step2.validateBtn', { ns: 'excel' })} <ArrowRight size={16} aria-hidden="true" />
        </Button>
      </div>
    </div>
  );
}

export default Step2ColumnMapping;
