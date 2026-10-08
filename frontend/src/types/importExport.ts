export interface ColumnMappingDto {
  excelColumn: string;
  systemField: string;
}

export interface AppliedColumnMappingDto {
  excelColumn: string;
  excelHeader: string;
  systemField: string;
  systemFieldDisplayName: string;
  isRequired: boolean;
}

export interface WorkbookHeaderDto {
  index: number;
  columnLetter: string;
  name: string;
  rawName: string;
  isEmpty: boolean;
  isDuplicate: boolean;
  suggestedSystemField?: string;
  suggestionConfidence?: 'exact' | 'alias' | 'none';
}

export interface WorkbookSheetDto {
  name: string;
  isHidden: boolean;
  headerRowNumber: number;
  usedRowCount: number;
  dataRowCount: number;
  usedColumnCount: number;
  headers: WorkbookHeaderDto[];
  hasFormulaCells: boolean;
  formulaCellCount: number;
  formulaCells: string[];
}

export interface WorkbookInspectionIssueDto {
  code: string;
  message: string;
  severity: 'Error' | 'Warning' | 'Info';
  sheetName?: string;
  cellReference?: string;
}

export interface InspectWorkbookResponseDto {
  fileToken?: string;
  fileName: string;
  expiresAtUtc?: string;
  sheetNames: string[];
  defaultSheetName?: string;
  sheets: WorkbookSheetDto[];
  sampleRows: Record<string, string>[];
  suggestedMappings: ColumnMappingDto[];
  canProceedToMapping: boolean;
  issues: WorkbookInspectionIssueDto[];
}

export interface ValidateImportRequestDto {
  fileToken: string;
  sheetName?: string;
  columnMappings: ColumnMappingDto[];
}

export interface ProjectImportIssueDto {
  rowNumber?: number;
  excelColumn?: string;
  excelHeader?: string;
  systemField?: string;
  rawValue?: string;
  errorCode: string;
  message: string;
  severity: 'error' | 'warning';
}

export interface ProjectImportRowPreviewDto {
  rowNumber: number;
  isValid: boolean;
  rawValues: Record<string, string | null>;
  displayValues: Record<string, string | null>;
  issues: ProjectImportIssueDto[];
}

export interface ProposedNewReferenceDto {
  type: string;
  value: string;
}

export interface ValidateImportResponseDto {
  fileToken: string;
  sheetName: string;
  totalRows: number;
  validRowCount: number;
  invalidRowCount: number;
  errorCount: number;
  warningCount: number;
  canImport: boolean;
  mapping: AppliedColumnMappingDto[];
  previewRows: ProjectImportRowPreviewDto[];
  errors: ProjectImportIssueDto[];
  warnings: ProjectImportIssueDto[];
  proposedNewReferences?: ProposedNewReferenceDto[];
  totalErrorCount: number;
  errorsTruncated: boolean;
}

export interface ConfirmImportRequestDto {
  fileToken: string;
  sheetName?: string;
  columnMappings: ColumnMappingDto[];
}

export interface ConfirmImportResponseDto {
  success: boolean;
  importedCount: number;
  createdProjectIds: number[];
  approvalStatus: string;
  isPublished: boolean;
  message: string;
  validationResult?: ValidateImportResponseDto;
}
