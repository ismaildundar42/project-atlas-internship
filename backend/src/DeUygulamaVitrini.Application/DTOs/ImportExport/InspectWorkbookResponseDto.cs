namespace DeUygulamaVitrini.Application.DTOs.ImportExport;

public class WorkbookInspectionIssueDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error"; // "Error" | "Warning" | "Info"
    public string? SheetName { get; set; }
    public string? CellReference { get; set; }
}

public class WorkbookHeaderDto
{
    public int Index { get; set; }
    public string ColumnLetter { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RawName { get; set; } = string.Empty;
    public bool IsEmpty { get; set; }
    public bool IsDuplicate { get; set; }
    public string? SuggestedSystemField { get; set; }
    public string? SuggestionConfidence { get; set; } // "exact" | "alias" | "none"
}

public class WorkbookSheetDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public int HeaderRowNumber { get; set; }
    public int UsedRowCount { get; set; }
    public int DataRowCount { get; set; }
    public int UsedColumnCount { get; set; }
    public List<WorkbookHeaderDto> Headers { get; set; } = new();
    public bool HasFormulaCells { get; set; }
    public int FormulaCellCount { get; set; }
    public List<string> FormulaCells { get; set; } = new();
}

public class InspectWorkbookResponseDto
{
    public string? FileToken { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
    public List<string> SheetNames { get; set; } = new();
    public string? DefaultSheetName { get; set; }
    public List<WorkbookSheetDto> Sheets { get; set; } = new();
    public List<Dictionary<string, string>> SampleRows { get; set; } = new();
    public List<ColumnMappingDto> SuggestedMappings { get; set; } = new();
    public bool CanProceedToMapping { get; set; }
    public List<WorkbookInspectionIssueDto> Issues { get; set; } = new();
}
