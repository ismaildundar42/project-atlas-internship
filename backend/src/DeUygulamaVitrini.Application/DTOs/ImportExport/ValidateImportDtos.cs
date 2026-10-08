namespace DeUygulamaVitrini.Application.DTOs.ImportExport;

public class ColumnMappingDto
{
    /// <summary>
    /// Excel sütun harfi veya 1-tabanlı endeksi (ör. "A", "B" veya "1", "2").
    /// </summary>
    public string ExcelColumn { get; set; } = string.Empty;

    /// <summary>
    /// Eşlenen sistem alanı anahtarı (ör. "name", "category", "shortDescription").
    /// </summary>
    public string SystemField { get; set; } = string.Empty;
}

public class AppliedColumnMappingDto
{
    public string ExcelColumn { get; set; } = string.Empty;
    public string ExcelHeader { get; set; } = string.Empty;
    public string SystemField { get; set; } = string.Empty;
    public string SystemFieldDisplayName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}

public class ValidateImportRequestDto
{
    public string FileToken { get; set; } = string.Empty;
    public string? SheetName { get; set; }
    public List<ColumnMappingDto> ColumnMappings { get; set; } = new();
}

public class ProjectImportIssueDto
{
    public int? RowNumber { get; set; }
    public string? ExcelColumn { get; set; }
    public string? ExcelHeader { get; set; }
    public string? SystemField { get; set; }
    public string? RawValue { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "error"; // "error" | "warning"
}

public class ProjectImportRowPreviewDto
{
    public int RowNumber { get; set; }
    public bool IsValid { get; set; }
    public Dictionary<string, string?> RawValues { get; set; } = new();
    public Dictionary<string, string?> DisplayValues { get; set; } = new();
    public List<ProjectImportIssueDto> Issues { get; set; } = new();
}

public class ProposedNewReferenceDto
{
    public string Type { get; set; } = string.Empty; // "Technology", "Location", "Tag"
    public string Value { get; set; } = string.Empty;
}

public class ValidateImportResponseDto
{
    public string FileToken { get; set; } = string.Empty;
    public string SheetName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRowCount { get; set; }
    public int InvalidRowCount { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public bool CanImport { get; set; }
    public List<AppliedColumnMappingDto> Mapping { get; set; } = new();
    public List<ProjectImportRowPreviewDto> PreviewRows { get; set; } = new();
    public List<ProjectImportIssueDto> Errors { get; set; } = new();
    public List<ProjectImportIssueDto> Warnings { get; set; } = new();
    public List<ProposedNewReferenceDto> ProposedNewReferences { get; set; } = new();
    public int TotalErrorCount { get; set; }
    public bool ErrorsTruncated { get; set; }
}

public class ConfirmImportRequestDto
{
    public string FileToken { get; set; } = string.Empty;
    public string? SheetName { get; set; }
    public List<ColumnMappingDto> ColumnMappings { get; set; } = new();
}

public class ConfirmImportResponseDto
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public List<int> CreatedProjectIds { get; set; } = new();
    public string ApprovalStatus { get; set; } = "Draft";
    public bool IsPublished { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public ValidateImportResponseDto? ValidationResult { get; set; }
}
