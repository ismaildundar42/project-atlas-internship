using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DeUygulamaVitrini.Application.Common.Helpers;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models.ImportExport;
using DeUygulamaVitrini.Application.DTOs.ImportExport;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using DeUygulamaVitrini.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeUygulamaVitrini.Infrastructure.Services;

public class ProjectExcelService : IProjectExcelService
{
    private readonly ApplicationDbContext _context;
    private readonly IProjectImportFileStore _fileStore;
    private readonly ILogger<ProjectExcelService> _logger;
    private readonly IAuditLogService _auditLogService;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private const int MaxWorksheets = 10;
    private const int MaxRows = 500;
    private const int MaxColumns = 40;
    private const int MaxSampleRows = 5;
    private const int MaxPreviewRows = 20;
    private const int MaxReportedErrors = 200;
    private const int MaxReportedFormulaCells = 20;
    private const int MaxErrorStringLength = 200;

    public ProjectExcelService(
        ApplicationDbContext context,
        IProjectImportFileStore fileStore,
        ILogger<ProjectExcelService> logger,
        IAuditLogService auditLogService)
    {
        _context = context;
        _fileStore = fileStore;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<InspectWorkbookResponseDto> InspectWorkbookAsync(
        Stream stream,
        string fileName,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var response = new InspectWorkbookResponseDto
        {
            FileName = Path.GetFileName(fileName),
            CanProceedToMapping = false
        };

        // ── 1. Basic File Validations ─────────────────────────────────────────
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            response.Issues.Add(new WorkbookInspectionIssueDto
            {
                Code = "UNSUPPORTED_FILE_TYPE",
                Message = "Yalnızca .xlsx formatındaki Excel dosyaları desteklenmektedir.",
                Severity = "Error"
            });
            _logger.LogWarning("Workbook inspection rejected: unsupported extension {Extension}", ext);
            return response;
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var fileBytes = memoryStream.ToArray();

        if (fileBytes.Length == 0)
        {
            response.Issues.Add(new WorkbookInspectionIssueDto
            {
                Code = "FILE_EMPTY",
                Message = "Yüklenen dosya boş olamaz.",
                Severity = "Error"
            });
            return response;
        }

        if (fileBytes.Length > MaxFileSize)
        {
            response.Issues.Add(new WorkbookInspectionIssueDto
            {
                Code = "FILE_TOO_LARGE",
                Message = "Excel dosyası en fazla 10 MB olabilir.",
                Severity = "Error"
            });
            return response;
        }

        // ── 2. ZIP & OpenXML Defensive Checks ──────────────────────────────────
        if (!IsValidZipPackage(fileBytes, out var isMacroEnabled, out var zipIssue))
        {
            response.Issues.Add(new WorkbookInspectionIssueDto
            {
                Code = isMacroEnabled ? "UNSUPPORTED_FILE_TYPE" : "INVALID_XLSX",
                Message = zipIssue ?? "Dosya geçerli bir Excel (.xlsx) çalışma kitabı yapısına sahip değil.",
                Severity = "Error"
            });
            return response;
        }

        // ── 3. Parse with ClosedXML ───────────────────────────────────────────
        try
        {
            using var workbookStream = new MemoryStream(fileBytes);
            using var workbook = new XLWorkbook(workbookStream);

            if (workbook.Worksheets.Count > MaxWorksheets)
            {
                response.Issues.Add(new WorkbookInspectionIssueDto
                {
                    Code = "TOO_MANY_SHEETS",
                    Message = $"Çalışma kitabı en fazla {MaxWorksheets} sayfa içerebilir. Bulunan: {workbook.Worksheets.Count} sayfa.",
                    Severity = "Error"
                });
            }

            IXLWorksheet? selectedSheet = null;

            foreach (var ws in workbook.Worksheets)
            {
                response.SheetNames.Add(ws.Name);
                var isHidden = ws.Visibility != XLWorksheetVisibility.Visible;

                var rangeUsed = ws.RangeUsed();
                var hasContent = rangeUsed != null && !rangeUsed.IsEmpty();

                var sheetDto = new WorkbookSheetDto
                {
                    Name = ws.Name,
                    IsHidden = isHidden,
                    HeaderRowNumber = hasContent ? rangeUsed!.FirstRow().RowNumber() : 1,
                    UsedRowCount = hasContent ? rangeUsed!.RowCount() : 0,
                    DataRowCount = hasContent ? Math.Max(0, rangeUsed!.RowCount() - 1) : 0,
                    UsedColumnCount = hasContent ? rangeUsed!.ColumnCount() : 0,
                    HasFormulaCells = false,
                    FormulaCellCount = 0
                };

                if (hasContent && rangeUsed != null)
                {
                    var headerRow = rangeUsed.FirstRow();
                    var headerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    for (int c = 1; c <= rangeUsed.ColumnCount(); c++)
                    {
                        var cell = headerRow.Cell(c);
                        var rawName = cell.GetString();
                        var trimmedName = rawName.Trim();
                        var isEmpty = string.IsNullOrWhiteSpace(trimmedName);
                        var isDuplicate = !isEmpty && headerNames.Contains(trimmedName);

                        if (!isEmpty)
                        {
                            headerNames.Add(trimmedName);
                        }

                        // Auto-mapping suggestion
                        var (suggestedField, confidence) = SuggestFieldForHeader(trimmedName);

                        sheetDto.Headers.Add(new WorkbookHeaderDto
                        {
                            Index = cell.Address.ColumnNumber,
                            ColumnLetter = cell.Address.ColumnLetter,
                            Name = trimmedName,
                            RawName = rawName,
                            IsEmpty = isEmpty,
                            IsDuplicate = isDuplicate,
                            SuggestedSystemField = suggestedField,
                            SuggestionConfidence = confidence
                        });
                    }

                    // Check formula cells in used range
                    var formulaLocations = new List<string>();
                    foreach (var cell in rangeUsed.CellsUsed())
                    {
                        if (cell.HasFormula)
                        {
                            if (formulaLocations.Count < MaxReportedFormulaCells)
                            {
                                var addr = cell.Address?.ToString();
                                if (!string.IsNullOrEmpty(addr))
                                {
                                    formulaLocations.Add(addr);
                                }
                            }
                        }
                    }

                    sheetDto.FormulaCellCount = formulaLocations.Count;
                    sheetDto.FormulaCells = formulaLocations;
                    sheetDto.HasFormulaCells = formulaLocations.Count > 0;
                }

                response.Sheets.Add(sheetDto);

                // Default candidate: first visible non-empty worksheet
                if (selectedSheet == null && !isHidden && hasContent)
                {
                    selectedSheet = ws;
                    response.DefaultSheetName = ws.Name;
                }
            }

            if (selectedSheet == null)
            {
                response.Issues.Add(new WorkbookInspectionIssueDto
                {
                    Code = "NO_VISIBLE_DATA_SHEET",
                    Message = "Excel dosyasında veri içeren görünür bir çalışma sayfası bulunamadı.",
                    Severity = "Error"
                });
                return response;
            }

            var defaultSheetDto = response.Sheets.FirstOrDefault(s => s.Name == response.DefaultSheetName);
            if (defaultSheetDto != null)
            {
                // Check row and column limits
                if (defaultSheetDto.DataRowCount > MaxRows)
                {
                    response.Issues.Add(new WorkbookInspectionIssueDto
                    {
                        Code = "TOO_MANY_ROWS",
                        Message = $"Çalışma sayfasında en fazla {MaxRows} veri satırı desteklenmektedir. Bulunan: {defaultSheetDto.DataRowCount} satır.",
                        Severity = "Error",
                        SheetName = defaultSheetDto.Name
                    });
                }

                if (defaultSheetDto.UsedColumnCount > MaxColumns)
                {
                    response.Issues.Add(new WorkbookInspectionIssueDto
                    {
                        Code = "TOO_MANY_COLUMNS",
                        Message = $"Çalışma sayfasında en fazla {MaxColumns} sütun desteklenmektedir. Bulunan: {defaultSheetDto.UsedColumnCount} sütun.",
                        Severity = "Error",
                        SheetName = defaultSheetDto.Name
                    });
                }

                // Check header issues
                foreach (var header in defaultSheetDto.Headers)
                {
                    if (header.IsEmpty)
                    {
                        response.Issues.Add(new WorkbookInspectionIssueDto
                        {
                            Code = "EMPTY_HEADER",
                            Message = $"Sütun {header.ColumnLetter} için başlık boş bırakılamaz.",
                            Severity = "Error",
                            SheetName = defaultSheetDto.Name,
                            CellReference = $"{header.ColumnLetter}{defaultSheetDto.HeaderRowNumber}"
                        });
                    }
                    else if (header.IsDuplicate)
                    {
                        response.Issues.Add(new WorkbookInspectionIssueDto
                        {
                            Code = "DUPLICATE_HEADER",
                            Message = $"'{header.Name}' sütun başlığı {header.ColumnLetter} sütununda mükerrer olarak kullanılmış.",
                            Severity = "Error",
                            SheetName = defaultSheetDto.Name,
                            CellReference = $"{header.ColumnLetter}{defaultSheetDto.HeaderRowNumber}"
                        });
                    }
                }

                // Check formula cells
                if (defaultSheetDto.HasFormulaCells)
                {
                    var formulaSummary = string.Join(", ", defaultSheetDto.FormulaCells.Take(5));
                    if (defaultSheetDto.FormulaCellCount > 5) formulaSummary += "...";

                    response.Issues.Add(new WorkbookInspectionIssueDto
                    {
                        Code = "FORMULA_CELL_NOT_ALLOWED",
                        Message = $"Çalışma sayfasında formüllü hücre tespit edildi ({formulaSummary}). İçe aktarım için tüm hücreler statik değer içermelidir.",
                        Severity = "Error",
                        SheetName = defaultSheetDto.Name
                    });
                }

                // Extract sample rows (up to 5 data rows)
                var rangeUsed = selectedSheet.RangeUsed();
                if (rangeUsed != null && defaultSheetDto.DataRowCount > 0)
                {
                    var rows = rangeUsed.Rows().Skip(1).Take(MaxSampleRows);
                    foreach (var row in rows)
                    {
                        var sampleRowDict = new Dictionary<string, string>();
                        foreach (var header in defaultSheetDto.Headers)
                        {
                            var cell = row.Cell(header.Index);
                            var val = cell.IsEmpty() ? string.Empty : cell.GetFormattedString().Trim();
                            sampleRowDict[header.Name.Length > 0 ? header.Name : $"Col_{header.ColumnLetter}"] = val;
                        }
                        response.SampleRows.Add(sampleRowDict);
                    }
                }

                // Populate SuggestedMappings list
                foreach (var header in defaultSheetDto.Headers)
                {
                    if (!string.IsNullOrEmpty(header.SuggestedSystemField))
                    {
                        response.SuggestedMappings.Add(new ColumnMappingDto
                        {
                            ExcelColumn = header.ColumnLetter,
                            SystemField = header.SuggestedSystemField
                        });
                    }
                }
            }

            // Determine if eligible to proceed
            var hasBlockingErrors = response.Issues.Any(i => i.Severity == "Error");
            response.CanProceedToMapping = !hasBlockingErrors;

            if (response.CanProceedToMapping)
            {
                var ttl = TimeSpan.FromMinutes(30);
                var token = await _fileStore.StoreAsync(
                    fileBytes,
                    response.FileName,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    currentUserId,
                    ttl,
                    cancellationToken);

                response.FileToken = token;
                response.ExpiresAtUtc = DateTime.UtcNow.Add(ttl);
                _logger.LogInformation("Workbook inspected successfully. TokenPrefix={TokenPrefix}, User={User}, Rows={Rows}",
                    token[..8], currentUserId, defaultSheetDto?.DataRowCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Excel workbook {FileName}", fileName);
            response.Issues.Add(new WorkbookInspectionIssueDto
            {
                Code = "INVALID_XLSX",
                Message = "Excel dosyası okunamadı veya bozuk: " + ex.Message,
                Severity = "Error"
            });
            return response;
        }
    }

    public async Task<ValidateImportResponseDto> ValidateImportAsync(
        ValidateImportRequestDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Retrieve session from store ────────────────────────────────────
        var session = await _fileStore.GetAsync(request.FileToken, currentUserId, cancellationToken);
        if (session == null)
        {
            return new ValidateImportResponseDto
            {
                FileToken = request.FileToken,
                CanImport = false,
                Errors = new List<ProjectImportIssueDto>
                {
                    new()
                    {
                        ErrorCode = "INVALID_FILE_TOKEN",
                        Message = "Geçersiz veya süresi dolmuş içe aktarma oturumu. Lütfen dosyayı tekrar yükleyiniz.",
                        Severity = "error"
                    }
                },
                TotalErrorCount = 1
            };
        }

        var result = await ValidateInternalAsync(
            session.FileBytes,
            session.FileName,
            request.SheetName,
            request.ColumnMappings,
            currentUserId,
            request.FileToken,
            cancellationToken);

        return result.Response;
    }

    public async Task<ConfirmImportResponseDto> ConfirmImportAsync(
        ConfirmImportRequestDto request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Atomic Lock Acquisition (Double-Click & Concurrency Protection) ─
        var session = await _fileStore.TryAcquireForConfirmAsync(request.FileToken, currentUserId, cancellationToken);
        if (session == null)
        {
            return new ConfirmImportResponseDto
            {
                Success = false,
                Message = "Geçersiz veya süresi dolmuş içe aktarma oturumu. Lütfen dosyayı tekrar yükleyiniz."
            };
        }

        try
        {
            // ── 2. Authoritative Revalidation against CURRENT Database State ───────
            var validationResult = await ValidateInternalAsync(
                session.FileBytes,
                session.FileName,
                request.SheetName,
                request.ColumnMappings,
                currentUserId,
                request.FileToken,
                cancellationToken);

            if (!validationResult.CanImport || validationResult.Projects.Count == 0)
            {
                // Revalidation failed — release lock so user can correct/revalidate
                await _fileStore.ReleaseConfirmLockAsync(request.FileToken, currentUserId, cancellationToken);
                return new ConfirmImportResponseDto
                {
                    Success = false,
                    ImportedCount = 0,
                    ValidationResult = validationResult.Response,
                    Message = "Dosya yeniden doğrulandı ve bazı sorunlar bulundu. Lütfen hataları düzelttikten sonra tekrar deneyiniz."
                };
            }

            // ── 3. Persistence inside ONE Database Transaction ────────────────────
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // A. Resolve / Create missing Technologies
                var allTechNames = validationResult.CandidateItems
                    .SelectMany(ci => ci.PendingTechnologies)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var techMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (allTechNames.Count > 0)
                {
                    var existingTechs = await _context.Technologies.ToListAsync(cancellationToken);
                    foreach (var t in existingTechs)
                    {
                        techMap[t.Name] = t.Id;
                        techMap[NormalizeText(t.Name)] = t.Id;
                    }

                    var newlyCreatedTechs = new List<Technology>();
                    foreach (var techName in allTechNames)
                    {
                        var norm = NormalizeText(techName);
                        if (!techMap.ContainsKey(techName) && !techMap.ContainsKey(norm))
                        {
                            var newTech = new Technology
                            {
                                Name = techName,
                                Category = TechnologyCategory.Other,
                                CreatedAt = DateTime.UtcNow
                            };
                            newlyCreatedTechs.Add(newTech);
                            _context.Technologies.Add(newTech);
                            techMap[techName] = -1;
                            techMap[norm] = -1;
                        }
                    }

                    if (newlyCreatedTechs.Count > 0)
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        foreach (var nt in newlyCreatedTechs)
                        {
                            techMap[nt.Name] = nt.Id;
                            techMap[NormalizeText(nt.Name)] = nt.Id;
                        }
                    }
                }

                // B. Resolve / Create missing Locations
                var allLocNames = validationResult.CandidateItems
                    .SelectMany(ci => ci.PendingLocations)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var locMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (allLocNames.Count > 0)
                {
                    var existingLocs = await _context.Locations.ToListAsync(cancellationToken);
                    foreach (var l in existingLocs)
                    {
                        locMap[l.Name] = l.Id;
                        locMap[NormalizeText(l.Name)] = l.Id;
                    }

                    var newlyCreatedLocs = new List<Location>();
                    foreach (var locName in allLocNames)
                    {
                        var norm = NormalizeText(locName);
                        if (!locMap.ContainsKey(locName) && !locMap.ContainsKey(norm))
                        {
                            var newLoc = new Location
                            {
                                Name = locName,
                                LocationType = LocationType.Other,
                                CreatedAt = DateTime.UtcNow
                            };
                            newlyCreatedLocs.Add(newLoc);
                            _context.Locations.Add(newLoc);
                            locMap[locName] = -1;
                            locMap[norm] = -1;
                        }
                    }

                    if (newlyCreatedLocs.Count > 0)
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        foreach (var nl in newlyCreatedLocs)
                        {
                            locMap[nl.Name] = nl.Id;
                            locMap[NormalizeText(nl.Name)] = nl.Id;
                        }
                    }
                }

                // C. Resolve / Create missing Tags
                var allTagNames = validationResult.CandidateItems
                    .SelectMany(ci => ci.PendingTags)
                    .Select(tg => tg.Trim())
                    .Where(tg => !string.IsNullOrEmpty(tg))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var tagMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (allTagNames.Count > 0)
                {
                    var existingTags = await _context.Tags.ToListAsync(cancellationToken);
                    foreach (var t in existingTags)
                    {
                        tagMap[t.Name] = t.Id;
                        tagMap[NormalizeText(t.Name)] = t.Id;
                    }

                    var newlyCreatedTags = new List<Tag>();
                    foreach (var tagName in allTagNames)
                    {
                        var norm = NormalizeText(tagName);
                        if (!tagMap.ContainsKey(tagName) && !tagMap.ContainsKey(norm))
                        {
                            var baseSlug = SlugHelper.GenerateSlug(tagName);
                            if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "etiket";

                            var slugExists = await _context.Tags.AnyAsync(t => t.Slug == baseSlug, cancellationToken) ||
                                             newlyCreatedTags.Any(t => t.Slug == baseSlug);
                            var finalSlug = slugExists ? $"{baseSlug}-{DateTime.UtcNow.Ticks % 100000}" : baseSlug;

                            var newTag = new Tag
                            {
                                Name = tagName,
                                Slug = finalSlug,
                                CreatedAt = DateTime.UtcNow
                            };
                            newlyCreatedTags.Add(newTag);
                            _context.Tags.Add(newTag);
                            tagMap[tagName] = -1;
                            tagMap[norm] = -1;
                        }
                    }

                    if (newlyCreatedTags.Count > 0)
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        foreach (var nt in newlyCreatedTags)
                        {
                            tagMap[nt.Name] = nt.Id;
                            tagMap[NormalizeText(nt.Name)] = nt.Id;
                        }
                    }
                }

                // D. Attach newly resolved tech/loc/tag IDs to each project
                foreach (var item in validationResult.CandidateItems)
                {
                    var p = item.Project;

                    foreach (var pt in item.PendingTechnologies)
                    {
                        var norm = NormalizeText(pt);
                        if ((techMap.TryGetValue(pt, out var techId) || techMap.TryGetValue(norm, out techId)) && techId > 0)
                        {
                            if (!p.ProjectTechnologies.Any(x => x.TechnologyId == techId))
                            {
                                p.ProjectTechnologies.Add(new ProjectTechnology { TechnologyId = techId });
                            }
                        }
                    }

                    foreach (var pl in item.PendingLocations)
                    {
                        var norm = NormalizeText(pl);
                        if ((locMap.TryGetValue(pl, out var locId) || locMap.TryGetValue(norm, out locId)) && locId > 0)
                        {
                            if (!p.ProjectLocations.Any(x => x.LocationId == locId))
                            {
                                p.ProjectLocations.Add(new ProjectLocation { LocationId = locId });
                            }
                        }
                    }

                    foreach (var ptag in item.PendingTags)
                    {
                        var norm = NormalizeText(ptag);
                        if ((tagMap.TryGetValue(ptag, out var tagId) || tagMap.TryGetValue(norm, out tagId)) && tagId > 0)
                        {
                            if (!p.ProjectTags.Any(x => x.TagId == tagId))
                            {
                                p.ProjectTags.Add(new ProjectTag { TagId = tagId });
                            }
                        }
                    }
                }

                // Insert all Projects and their join relationships
                await _context.Projects.AddRangeAsync(validationResult.Projects, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var createdProjectIds = validationResult.Projects.Select(p => p.Id).ToList();

                // Batch-level audit log via central IAuditLogService.
                // Actor identity is resolved from ApplicationUser — Member.UserId link is NOT used.
                var batchMetadata = new
                {
                    source = "ExcelImport",
                    fileName = session.FileName,
                    importedCount = createdProjectIds.Count,
                    createdProjectIds = createdProjectIds
                };

                await _auditLogService.LogWithActorAsync(
                    currentUserId,
                    "ProjectBatchImported",
                    "ProjectBatch",
                    null,
                    $"{createdProjectIds.Count} Proje",
                    $"Excel içe aktarımı ile {createdProjectIds.Count} proje oluşturuldu.",
                    batchMetadata,
                    cancellationToken);

                // Commit transaction atomically
                await transaction.CommitAsync(cancellationToken);

                // Consume/remove FileToken ONLY AFTER successful commit
                await _fileStore.RemoveAsync(request.FileToken, currentUserId, cancellationToken);

                return new ConfirmImportResponseDto
                {
                    Success = true,
                    ImportedCount = createdProjectIds.Count,
                    CreatedProjectIds = createdProjectIds,
                    ApprovalStatus = "Draft",
                    IsPublished = false,
                    Message = $"{createdProjectIds.Count} proje taslak olarak başarıyla içe aktarıldı."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Transaction failed during Excel import confirm. Token: {FileToken}", request.FileToken);
                await _fileStore.ReleaseConfirmLockAsync(request.FileToken, currentUserId, cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during import confirmation for token {FileToken}", request.FileToken);
            await _fileStore.ReleaseConfirmLockAsync(request.FileToken, currentUserId, cancellationToken);
            throw;
        }
    }

    private class CandidateProjectImportItem
    {
        public Project Project { get; set; } = null!;
        public List<string> PendingTechnologies { get; set; } = new();
        public List<string> PendingLocations { get; set; } = new();
        public List<string> PendingTags { get; set; } = new();
    }

    private class ValidationInternalResult
    {
        public ValidateImportResponseDto Response { get; set; } = new();
        public List<CandidateProjectImportItem> CandidateItems { get; set; } = new();
        public List<Project> Projects => CandidateItems.Select(c => c.Project).ToList();
        public bool CanImport => Response.CanImport && Response.InvalidRowCount == 0 && Response.TotalErrorCount == 0 && CandidateItems.Count > 0;
    }

    private async Task<ValidationInternalResult> ValidateInternalAsync(
        byte[] fileBytes,
        string fileName,
        string? requestedSheetName,
        List<ColumnMappingDto> columnMappings,
        int currentUserId,
        string fileToken,
        CancellationToken cancellationToken)
    {
        var result = new ValidationInternalResult();
        var response = result.Response;
        response.FileToken = fileToken;
        response.CanImport = false;

        // ── 1. Mapping Validation ─────────────────────────────────────────────
        var mappingErrors = ValidateMappingContract(columnMappings, out var appliedMappings, out var columnToFieldMap);
        if (mappingErrors.Count > 0)
        {
            response.Errors.AddRange(mappingErrors);
            response.TotalErrorCount = mappingErrors.Count;
            response.Mapping = appliedMappings;
            return result;
        }
        response.Mapping = appliedMappings;

        // ── 2. Parse Workbook Safely ──────────────────────────────────────────
        using var workbookStream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(workbookStream);

        var sheetName = string.IsNullOrWhiteSpace(requestedSheetName) ? null : requestedSheetName.Trim();
        IXLWorksheet? worksheet = null;

        if (!string.IsNullOrEmpty(sheetName))
        {
            worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
            if (worksheet == null)
            {
                response.Errors.Add(new ProjectImportIssueDto
                {
                    ErrorCode = "SHEET_NOT_FOUND",
                    Message = $"'{sheetName}' isimli çalışma sayfası dosyada bulunamadı.",
                    Severity = "error"
                });
                response.TotalErrorCount = 1;
                return result;
            }
        }
        else
        {
            // Pick first visible non-empty worksheet
            worksheet = workbook.Worksheets.FirstOrDefault(w => w.Visibility == XLWorksheetVisibility.Visible && w.RangeUsed() != null && !w.RangeUsed()!.IsEmpty());
            if (worksheet == null)
            {
                response.Errors.Add(new ProjectImportIssueDto
                {
                    ErrorCode = "NO_VISIBLE_DATA_SHEET",
                    Message = "Veri içeren görünür bir çalışma sayfası bulunamadı.",
                    Severity = "error"
                });
                response.TotalErrorCount = 1;
                return result;
            }
        }

        response.SheetName = worksheet.Name;

        if (worksheet.Visibility != XLWorksheetVisibility.Visible)
        {
            response.Errors.Add(new ProjectImportIssueDto
            {
                ErrorCode = "HIDDEN_SHEET_NOT_ALLOWED",
                Message = $"'{worksheet.Name}' sayfası gizli olduğundan içe aktarma yapılamaz.",
                Severity = "error"
            });
            response.TotalErrorCount = 1;
            return result;
        }

        var rangeUsed = worksheet.RangeUsed();
        if (rangeUsed == null || rangeUsed.IsEmpty())
        {
            response.Errors.Add(new ProjectImportIssueDto
            {
                ErrorCode = "EMPTY_WORKBOOK",
                Message = "Seçilen çalışma sayfasında veri bulunamadı.",
                Severity = "error"
            });
            response.TotalErrorCount = 1;
            return result;
        }

        // Structural limits revalidation
        if (rangeUsed.RowCount() - 1 > MaxRows)
        {
            response.Errors.Add(new ProjectImportIssueDto
            {
                ErrorCode = "TOO_MANY_ROWS",
                Message = $"Çalışma sayfasında en fazla {MaxRows} satır bulunabilir.",
                Severity = "error"
            });
            response.TotalErrorCount = 1;
            return result;
        }

        // Formula cells check
        foreach (var cell in rangeUsed.CellsUsed())
        {
            if (cell.HasFormula)
            {
                response.Errors.Add(new ProjectImportIssueDto
                {
                    RowNumber = cell.Address.RowNumber,
                    ExcelColumn = cell.Address.ColumnLetter,
                    ErrorCode = "FORMULA_CELL_NOT_ALLOWED",
                    Message = $"Formül içeren hücre tespit edildi ({cell.Address}). Tüm hücreler statik değer içermelidir.",
                    Severity = "error"
                });
            }
        }

        if (response.Errors.Count > 0)
        {
            response.TotalErrorCount = response.Errors.Count;
            return result;
        }

        // Map column letter/index to header info
        var headerRow = rangeUsed.FirstRow();
        var headerRowNumber = headerRow.RowNumber();
        var headersByColLetter = new Dictionary<string, (int ColIndex, string HeaderName)>(StringComparer.OrdinalIgnoreCase);

        for (int c = 1; c <= rangeUsed.ColumnCount(); c++)
        {
            var cell = headerRow.Cell(c);
            var colLetter = cell.Address.ColumnLetter;
            var hName = cell.GetString().Trim();
            headersByColLetter[colLetter] = (cell.Address.ColumnNumber, hName);
        }

        // Validate mapped columns exist in sheet
        foreach (var mapping in columnMappings)
        {
            var colKey = mapping.ExcelColumn.Trim();
            if (!headersByColLetter.ContainsKey(colKey))
            {
                response.Errors.Add(new ProjectImportIssueDto
                {
                    ExcelColumn = colKey,
                    SystemField = mapping.SystemField,
                    ErrorCode = "COLUMN_NOT_FOUND",
                    Message = $"'{colKey}' sütunu çalışma sayfasında bulunamadı.",
                    Severity = "error"
                });
            }
        }

        if (response.Errors.Count > 0)
        {
            response.TotalErrorCount = response.Errors.Count;
            return result;
        }

        // Update applied mappings with real header names
        foreach (var applied in appliedMappings)
        {
            if (headersByColLetter.TryGetValue(applied.ExcelColumn, out var hInfo))
            {
                applied.ExcelHeader = hInfo.HeaderName;
            }
        }

        // ── 3. Batched Database Lookups (Read-Only) ───────────────────────────
        var dbCategories = await _context.ProjectCategories.AsNoTracking().Select(c => new { c.Id, c.Name, c.Code }).ToListAsync(cancellationToken);
        var dbStatuses = await _context.ProjectStatuses.AsNoTracking().Select(s => new { s.Id, s.Name, s.Code }).ToListAsync(cancellationToken);
        var dbTeams = await _context.Teams.AsNoTracking().Select(t => new { t.Id, t.Name }).ToListAsync(cancellationToken);
        var dbMembers = await _context.Members.AsNoTracking().Select(m => new { m.Id, m.FirstName, m.LastName, m.Email }).ToListAsync(cancellationToken);
        var dbTechnologies = await _context.Technologies.AsNoTracking().Select(t => new { t.Id, t.Name }).ToListAsync(cancellationToken);
        var dbLocations = await _context.Locations.AsNoTracking().Select(l => new { l.Id, l.Name }).ToListAsync(cancellationToken);
        var dbTags = await _context.Tags.AsNoTracking().Select(t => new { t.Id, t.Name, t.Slug }).ToListAsync(cancellationToken);

        // Soft-deleted + active projects for uniqueness check
        var dbProjects = await _context.Projects.IgnoreQueryFilters().AsNoTracking()
            .Select(p => new { p.Id, p.Name, p.Slug, p.IsDeleted })
            .ToListAsync(cancellationToken);

        // ── 4. Build Lookup Indexes with Normalization & Ambiguity Detection ───
        var categoryLookup = BuildLookupMap(dbCategories.Select(c => (c.Id, c.Name, (string?)c.Code)));
        var statusLookup = BuildLookupMap(dbStatuses.Select(s => (s.Id, s.Name, (string?)s.Code)));
        var teamLookup = BuildLookupMap(dbTeams.Select(t => (t.Id, t.Name, (string?)null)));
        var technologyLookup = BuildLookupMap(dbTechnologies.Select(t => (t.Id, t.Name, (string?)null)));
        var locationLookup = BuildLookupMap(dbLocations.Select(l => (l.Id, l.Name, (string?)null)));
        var tagLookup = BuildTagLookupMap(dbTags.Select(t => (t.Id, t.Name, t.Slug)));
        var memberLookup = BuildMemberLookupMap(dbMembers);

        var existingProjectNames = dbProjects.Select(p => NormalizeText(p.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingProjectSlugs = dbProjects.Select(p => p.Slug.Trim().ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ── 5. Row-by-Row Iteration & Validation ──────────────────────────────
        var rows = rangeUsed.Rows().Skip(1); // skip header row
        var seenFileProjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenFileProjectSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var proposedTechnologies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var proposedLocations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var proposedTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        int totalDataRows = 0;
        int validRows = 0;
        int invalidRows = 0;
        int totalErrors = 0;
        int totalWarnings = 0;

        var previewRows = new List<ProjectImportRowPreviewDto>();
        var detailedErrors = new List<ProjectImportIssueDto>();
        var detailedWarnings = new List<ProjectImportIssueDto>();
        var candidateItems = new List<CandidateProjectImportItem>();

        foreach (var row in rows)
        {
            var rowNum = row.RowNumber();

            // Check if entire row is empty
            bool isEntirelyBlank = true;
            for (int c = 1; c <= rangeUsed.ColumnCount(); c++)
            {
                if (!string.IsNullOrWhiteSpace(row.Cell(c).GetString()))
                {
                    isEntirelyBlank = false;
                    break;
                }
            }

            if (isEntirelyBlank)
            {
                continue; // Ignore fully empty row
            }

            totalDataRows++;
            var rowIssues = new List<ProjectImportIssueDto>();
            var rawValuesDict = new Dictionary<string, string?>();
            var displayValuesDict = new Dictionary<string, string?>();

            // Extract cell raw values for mapped columns
            var fieldValues = new Dictionary<string, (string Raw, string ColLetter, string Header)>(StringComparer.OrdinalIgnoreCase);
            foreach (var (colLetter, sysField) in columnToFieldMap)
            {
                if (headersByColLetter.TryGetValue(colLetter, out var hInfo))
                {
                    var cell = row.Cell(hInfo.ColIndex);
                    string rawVal;

                    if (cell.DataType == XLDataType.DateTime)
                    {
                        rawVal = cell.GetDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        rawVal = cell.IsEmpty() ? string.Empty : cell.GetFormattedString().Trim();
                    }

                    fieldValues[sysField] = (rawVal, colLetter, hInfo.HeaderName);
                    rawValuesDict[sysField] = rawVal;
                }
            }

            // ── A. Validate Name (Required, Max 200, DB & File Uniqueness) ────
            string resolvedName = string.Empty;
            string resolvedSlug = string.Empty;

            if (fieldValues.TryGetValue("name", out var nameVal))
            {
                var name = nameVal.Raw;
                var col = nameVal.ColLetter;
                var hdr = nameVal.Header;

                if (string.IsNullOrWhiteSpace(name))
                {
                    AddRowIssue(rowIssues, rowNum, col, hdr, "name", name, "REQUIRED_FIELD", "Proje adı zorunludur.");
                }
                else
                {
                    if (name.Length > 200)
                    {
                        AddRowIssue(rowIssues, rowNum, col, hdr, "name", name, "MAX_LENGTH_EXCEEDED", $"Proje adı en fazla 200 karakter olabilir. (Mevcut: {name.Length} karakter)");
                    }

                    var normName = NormalizeText(name);
                    var slug = SlugHelper.GenerateSlug(name);

                    // Duplicate check in existing DB
                    if (existingProjectNames.Contains(normName))
                    {
                        AddRowIssue(rowIssues, rowNum, col, hdr, "name", name, "DUPLICATE_PROJECT", $"'{name}' isimli proje veritabanında zaten mevcut.");
                    }
                    else if (existingProjectSlugs.Contains(slug))
                    {
                        AddRowIssue(rowIssues, rowNum, col, hdr, "name", name, "DUPLICATE_PROJECT", $"'{slug}' URL adresi (slug) veritabanındaki başka bir proje tarafından kullanılıyor.");
                    }

                    // Duplicate check within Excel file
                    if (seenFileProjectNames.Contains(normName))
                    {
                        AddRowIssue(rowIssues, rowNum, col, hdr, "name", name, "DUPLICATE_PROJECT_IN_FILE", $"'{name}' proje adı dosya içerisinde birden fazla satırda yer alıyor.");
                    }
                    else
                    {
                        seenFileProjectNames.Add(normName);
                    }

                    if (!string.IsNullOrEmpty(slug))
                    {
                        if (seenFileProjectSlugs.Contains(slug))
                        {
                            AddRowIssue(rowIssues, rowNum, col, hdr, "name", name, "DUPLICATE_PROJECT_IN_FILE", $"'{name}' projesinden üretilen '{slug}' URL adresi dosya içerisinde mükerrer.");
                        }
                        else
                        {
                            seenFileProjectSlugs.Add(slug);
                        }
                    }

                    resolvedName = name.Trim();
                    resolvedSlug = slug;
                    displayValuesDict["name"] = name;
                }
            }

            // ── B. Validate ShortDescription (Required, Max 500) ──────────────
            string resolvedShortDescription = string.Empty;
            if (fieldValues.TryGetValue("shortDescription", out var shortDescVal))
            {
                var val = shortDescVal.Raw;
                if (string.IsNullOrWhiteSpace(val))
                {
                    AddRowIssue(rowIssues, rowNum, shortDescVal.ColLetter, shortDescVal.Header, "shortDescription", val, "REQUIRED_FIELD", "Kısa açıklama zorunludur.");
                }
                else
                {
                    if (val.Length > 500)
                    {
                        AddRowIssue(rowIssues, rowNum, shortDescVal.ColLetter, shortDescVal.Header, "shortDescription", val, "MAX_LENGTH_EXCEEDED", $"Kısa açıklama en fazla 500 karakter olabilir. (Mevcut: {val.Length} karakter)");
                    }
                    resolvedShortDescription = val.Trim();
                    displayValuesDict["shortDescription"] = val;
                }
            }

            // ── C. Validate Category (Required Lookup) ────────────────────────
            int resolvedCategoryId = 0;
            if (fieldValues.TryGetValue("category", out var catVal))
            {
                var val = catVal.Raw;
                if (string.IsNullOrWhiteSpace(val))
                {
                    AddRowIssue(rowIssues, rowNum, catVal.ColLetter, catVal.Header, "category", val, "REQUIRED_FIELD", "Kategori zorunludur.");
                }
                else
                {
                    var res = ResolveLookup(categoryLookup, val);
                    if (res.Status == LookupMatchStatus.NotFound)
                    {
                        AddRowIssue(rowIssues, rowNum, catVal.ColLetter, catVal.Header, "category", val, "LOOKUP_NOT_FOUND", $"'{val}' kategorisi sistemde tanımlı değil.");
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, catVal.ColLetter, catVal.Header, "category", val, "LOOKUP_AMBIGUOUS", $"'{val}' kategorisi sistemde birden fazla kayıtla eşleşiyor.");
                    }
                    else
                    {
                        resolvedCategoryId = res.ResolvedId;
                        displayValuesDict["category"] = res.ResolvedName;
                    }
                }
            }

            // ── D. Validate Status (Required Lookup) ──────────────────────────
            int resolvedStatusId = 0;
            if (fieldValues.TryGetValue("status", out var statVal))
            {
                var val = statVal.Raw;
                if (string.IsNullOrWhiteSpace(val))
                {
                    AddRowIssue(rowIssues, rowNum, statVal.ColLetter, statVal.Header, "status", val, "REQUIRED_FIELD", "Durum zorunludur.");
                }
                else
                {
                    var res = ResolveLookup(statusLookup, val);
                    if (res.Status == LookupMatchStatus.NotFound)
                    {
                        AddRowIssue(rowIssues, rowNum, statVal.ColLetter, statVal.Header, "status", val, "LOOKUP_NOT_FOUND", $"'{val}' proje durumu sistemde tanımlı değil.");
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, statVal.ColLetter, statVal.Header, "status", val, "LOOKUP_AMBIGUOUS", $"'{val}' proje durumu birden fazla kayıtla eşleşiyor.");
                    }
                    else
                    {
                        resolvedStatusId = res.ResolvedId;
                        displayValuesDict["status"] = res.ResolvedName;
                    }
                }
            }

            // ── E. Validate Primary Team (Optional Lookup) ────────────────────
            int? primaryTeamId = null;
            if (fieldValues.TryGetValue("primaryTeam", out var teamVal) && !string.IsNullOrWhiteSpace(teamVal.Raw))
            {
                var res = ResolveLookup(teamLookup, teamVal.Raw);
                if (res.Status == LookupMatchStatus.NotFound)
                {
                    AddRowIssue(rowIssues, rowNum, teamVal.ColLetter, teamVal.Header, "primaryTeam", teamVal.Raw, "LOOKUP_NOT_FOUND", $"'{teamVal.Raw}' ekibi sistemde tanımlı değil.");
                }
                else if (res.Status == LookupMatchStatus.Ambiguous)
                {
                    AddRowIssue(rowIssues, rowNum, teamVal.ColLetter, teamVal.Header, "primaryTeam", teamVal.Raw, "LOOKUP_AMBIGUOUS", $"'{teamVal.Raw}' ekibi birden fazla kayıtla eşleşiyor.");
                }
                else
                {
                    primaryTeamId = res.ResolvedId;
                    displayValuesDict["primaryTeam"] = res.ResolvedName;
                }
            }

            // ── F. Validate Supporting Teams (Optional Multi-Lookup) ──────────
            var resolvedSupportingTeamIds = new List<int>();
            if (fieldValues.TryGetValue("supportingTeams", out var supTeamVal) && !string.IsNullOrWhiteSpace(supTeamVal.Raw))
            {
                var parts = SplitMultiValues(supTeamVal.Raw);
                var resolvedTeamNames = new List<string>();

                foreach (var part in parts)
                {
                    var res = ResolveLookup(teamLookup, part);
                    if (res.Status == LookupMatchStatus.NotFound)
                    {
                        AddRowIssue(rowIssues, rowNum, supTeamVal.ColLetter, supTeamVal.Header, "supportingTeams", part, "LOOKUP_NOT_FOUND", $"'{part}' destekleyen ekibi sistemde tanımlı değil.");
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, supTeamVal.ColLetter, supTeamVal.Header, "supportingTeams", part, "LOOKUP_AMBIGUOUS", $"'{part}' destekleyen ekibi birden fazla kayıtla eşleşiyor.");
                    }
                    else
                    {
                        if (primaryTeamId.HasValue && res.ResolvedId == primaryTeamId.Value)
                        {
                            // Warning: Primary team in supporting teams
                            rowIssues.Add(new ProjectImportIssueDto
                            {
                                RowNumber = rowNum,
                                ExcelColumn = supTeamVal.ColLetter,
                                ExcelHeader = supTeamVal.Header,
                                SystemField = "supportingTeams",
                                RawValue = BoundRawValue(part),
                                ErrorCode = "PRIMARY_TEAM_IN_SUPPORTING_TEAMS",
                                Message = $"'{part}' ekibi zaten Sorumlu Ekip olarak seçildiği için Destekleyen Ekipler listesinden kaldırıldı.",
                                Severity = "warning"
                            });
                        }
                        else
                        {
                            resolvedSupportingTeamIds.Add(res.ResolvedId);
                            resolvedTeamNames.Add(res.ResolvedName);
                        }
                    }
                }
                displayValuesDict["supportingTeams"] = string.Join("; ", resolvedTeamNames.Distinct());
            }

            // ── G. Validate Members (Optional Multi-Lookup by Email / Name) ───
            var resolvedMemberIds = new List<int>();
            if (fieldValues.TryGetValue("members", out var memVal) && !string.IsNullOrWhiteSpace(memVal.Raw))
            {
                var parts = SplitMultiValues(memVal.Raw);
                var resolvedMembers = new List<string>();

                foreach (var part in parts)
                {
                    var res = ResolveMember(memberLookup, part);
                    if (res.Status == LookupMatchStatus.NotFound)
                    {
                        AddRowIssue(rowIssues, rowNum, memVal.ColLetter, memVal.Header, "members", part, "LOOKUP_NOT_FOUND", $"'{part}' e-posta/isme sahip proje üyesi sistemde bulunamadı.");
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, memVal.ColLetter, memVal.Header, "members", part, "LOOKUP_AMBIGUOUS", $"'{part}' ile eşleşen birden fazla üye kaydı var.");
                    }
                    else
                    {
                        resolvedMemberIds.Add(res.ResolvedId);
                        resolvedMembers.Add(res.ResolvedName);
                    }
                }
                displayValuesDict["members"] = string.Join("; ", resolvedMembers.Distinct());
            }

            // ── H. Validate DevelopmentType (Optional Enum) ───────────────────
            var parsedDevType = DevelopmentType.Internal;
            if (fieldValues.TryGetValue("developmentType", out var devVal) && !string.IsNullOrWhiteSpace(devVal.Raw))
            {
                var val = devVal.Raw;
                if (Enum.TryParse<DevelopmentType>(val, true, out var parsed))
                {
                    parsedDevType = parsed;
                    displayValuesDict["developmentType"] = parsed.ToString();
                }
                else
                {
                    AddRowIssue(rowIssues, rowNum, devVal.ColLetter, devVal.Header, "developmentType", val, "INVALID_DEVELOPMENT_TYPE", $"'{val}' geçerli bir geliştirme tipi değil. (Beklenen: Internal, External, Hybrid)");
                }
            }
            else
            {
                displayValuesDict["developmentType"] = DevelopmentType.Internal.ToString();
            }

            // ── I. Validate Technologies (Auto-creatable) ─────────────────────
            var resolvedTechIds = new List<int>();
            var pendingTechNames = new List<string>();
            if (fieldValues.TryGetValue("technologies", out var techVal) && !string.IsNullOrWhiteSpace(techVal.Raw))
            {
                var parts = SplitMultiValues(techVal.Raw);
                var resolvedTechs = new List<string>();

                foreach (var part in parts)
                {
                    var cleanPart = part.Trim();
                    if (string.IsNullOrEmpty(cleanPart)) continue;

                    if (cleanPart.Length > 100)
                    {
                        AddRowIssue(rowIssues, rowNum, techVal.ColLetter, techVal.Header, "technologies", cleanPart, "MAX_LENGTH_EXCEEDED", $"Teknoloji adı en fazla 100 karakter olabilir. (Mevcut: {cleanPart.Length} karakter)");
                        continue;
                    }

                    var res = ResolveLookup(technologyLookup, cleanPart);
                    if (res.Status == LookupMatchStatus.ExactMatch)
                    {
                        resolvedTechIds.Add(res.ResolvedId);
                        resolvedTechs.Add(res.ResolvedName);
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, techVal.ColLetter, techVal.Header, "technologies", cleanPart, "LOOKUP_AMBIGUOUS", $"'{cleanPart}' teknolojisi birden fazla kayıtla eşleşiyor.");
                    }
                    else
                    {
                        // Auto-creatable: Proposed new Technology
                        var normKey = NormalizeText(cleanPart);
                        if (!proposedTechnologies.ContainsKey(normKey))
                        {
                            proposedTechnologies[normKey] = cleanPart;
                        }

                        pendingTechNames.Add(cleanPart);
                        resolvedTechs.Add(cleanPart);

                        rowIssues.Add(new ProjectImportIssueDto
                        {
                            RowNumber = rowNum,
                            ExcelColumn = techVal.ColLetter,
                            ExcelHeader = techVal.Header,
                            SystemField = "technologies",
                            RawValue = BoundRawValue(cleanPart),
                            ErrorCode = "NEW_TECHNOLOGY_PROPOSED",
                            Message = $"'{cleanPart}' sistemde kayıtlı değil. İçe aktarma onaylandığında yeni teknoloji olarak oluşturulacaktır.",
                            Severity = "warning"
                        });
                    }
                }
                displayValuesDict["technologies"] = string.Join("; ", resolvedTechs.Distinct());
            }

            // ── J. Validate Locations (Auto-creatable) ────────────────────────
            var resolvedLocIds = new List<int>();
            var pendingLocNames = new List<string>();
            if (fieldValues.TryGetValue("locations", out var locVal) && !string.IsNullOrWhiteSpace(locVal.Raw))
            {
                var parts = SplitMultiValues(locVal.Raw);
                var resolvedLocs = new List<string>();

                foreach (var part in parts)
                {
                    var cleanPart = part.Trim();
                    if (string.IsNullOrEmpty(cleanPart)) continue;

                    if (cleanPart.Length > 200)
                    {
                        AddRowIssue(rowIssues, rowNum, locVal.ColLetter, locVal.Header, "locations", cleanPart, "MAX_LENGTH_EXCEEDED", $"Lokasyon adı en fazla 200 karakter olabilir. (Mevcut: {cleanPart.Length} karakter)");
                        continue;
                    }

                    var res = ResolveLookup(locationLookup, cleanPart);
                    if (res.Status == LookupMatchStatus.ExactMatch)
                    {
                        resolvedLocIds.Add(res.ResolvedId);
                        resolvedLocs.Add(res.ResolvedName);
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, locVal.ColLetter, locVal.Header, "locations", cleanPart, "LOOKUP_AMBIGUOUS", $"'{cleanPart}' lokasyonu birden fazla kayıtla eşleşiyor.");
                    }
                    else
                    {
                        // Auto-creatable: Proposed new Location
                        var normKey = NormalizeText(cleanPart);
                        if (!proposedLocations.ContainsKey(normKey))
                        {
                            proposedLocations[normKey] = cleanPart;
                        }

                        pendingLocNames.Add(cleanPart);
                        resolvedLocs.Add(cleanPart);

                        rowIssues.Add(new ProjectImportIssueDto
                        {
                            RowNumber = rowNum,
                            ExcelColumn = locVal.ColLetter,
                            ExcelHeader = locVal.Header,
                            SystemField = "locations",
                            RawValue = BoundRawValue(cleanPart),
                            ErrorCode = "NEW_LOCATION_PROPOSED",
                            Message = $"'{cleanPart}' sistemde kayıtlı değil. İçe aktarma onaylandığında yeni lokasyon olarak oluşturulacaktır.",
                            Severity = "warning"
                        });
                    }
                }
                displayValuesDict["locations"] = string.Join("; ", resolvedLocs.Distinct());
            }

            // ── K. Validate Tags (Auto-creatable) ─────────────────────────────
            var resolvedTagIds = new List<int>();
            var pendingTagNames = new List<string>();
            if (fieldValues.TryGetValue("tags", out var tagVal) && !string.IsNullOrWhiteSpace(tagVal.Raw))
            {
                var parts = SplitMultiValues(tagVal.Raw);
                var resolvedTags = new List<string>();

                foreach (var part in parts)
                {
                    var cleanPart = part.Trim();
                    if (string.IsNullOrEmpty(cleanPart)) continue;

                    if (cleanPart.Length > 100)
                    {
                        AddRowIssue(rowIssues, rowNum, tagVal.ColLetter, tagVal.Header, "tags", cleanPart, "MAX_LENGTH_EXCEEDED", $"Etiket adı en fazla 100 karakter olabilir. (Mevcut: {cleanPart.Length} karakter)");
                        continue;
                    }

                    var res = ResolveTag(tagLookup, cleanPart);
                    if (res.Status == LookupMatchStatus.ExactMatch)
                    {
                        resolvedTagIds.Add(res.ResolvedId);
                        resolvedTags.Add(res.ResolvedName);
                    }
                    else if (res.Status == LookupMatchStatus.Ambiguous)
                    {
                        AddRowIssue(rowIssues, rowNum, tagVal.ColLetter, tagVal.Header, "tags", cleanPart, "LOOKUP_AMBIGUOUS", $"'{cleanPart}' etiketi birden fazla kayıtla eşleşiyor.");
                    }
                    else
                    {
                        // Auto-creatable: Proposed new Tag
                        var normKey = NormalizeText(cleanPart);
                        if (!proposedTags.ContainsKey(normKey))
                        {
                            proposedTags[normKey] = cleanPart;
                        }

                        pendingTagNames.Add(cleanPart);
                        resolvedTags.Add(cleanPart);

                        rowIssues.Add(new ProjectImportIssueDto
                        {
                            RowNumber = rowNum,
                            ExcelColumn = tagVal.ColLetter,
                            ExcelHeader = tagVal.Header,
                            SystemField = "tags",
                            RawValue = BoundRawValue(cleanPart),
                            ErrorCode = "NEW_TAG_PROPOSED",
                            Message = $"'{cleanPart}' sistemde kayıtlı değil. İçe aktarma onaylandığında yeni etiket olarak oluşturulacaktır.",
                            Severity = "warning"
                        });
                    }
                }
                displayValuesDict["tags"] = string.Join("; ", resolvedTags.Distinct());
            }

            // ── L. Validate Text Fields (Optional) ────────────────────────────
            ValidateOptionalTextField(fieldValues, "description", displayValuesDict, rowIssues, rowNum, null);
            ValidateOptionalTextField(fieldValues, "purpose", displayValuesDict, rowIssues, rowNum, null);
            ValidateOptionalTextField(fieldValues, "problemSolved", displayValuesDict, rowIssues, rowNum, null);
            ValidateOptionalTextField(fieldValues, "nonTechnicalDescription", displayValuesDict, rowIssues, rowNum, null);
            ValidateOptionalTextField(fieldValues, "technicalDescription", displayValuesDict, rowIssues, rowNum, null);
            ValidateOptionalTextField(fieldValues, "businessImpact", displayValuesDict, rowIssues, rowNum, null);
            ValidateOptionalTextField(fieldValues, "targetAudience", displayValuesDict, rowIssues, rowNum, 1000);
            ValidateOptionalTextField(fieldValues, "accessInstructions", displayValuesDict, rowIssues, rowNum, null);

            // ── M. Validate Dates (StartDate & EndDate) ───────────────────────
            DateOnly? parsedStartDate = null;
            DateOnly? parsedEndDate = null;

            if (fieldValues.TryGetValue("startDate", out var sDateVal) && !string.IsNullOrWhiteSpace(sDateVal.Raw))
            {
                if (TryParseDate(sDateVal.Raw, out var sDate))
                {
                    parsedStartDate = sDate;
                    displayValuesDict["startDate"] = sDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    AddRowIssue(rowIssues, rowNum, sDateVal.ColLetter, sDateVal.Header, "startDate", sDateVal.Raw, "INVALID_DATE", $"'{sDateVal.Raw}' geçerli bir tarih formatı değil. (Beklenen: YYYY-MM-DD)");
                }
            }

            if (fieldValues.TryGetValue("endDate", out var eDateVal) && !string.IsNullOrWhiteSpace(eDateVal.Raw))
            {
                if (TryParseDate(eDateVal.Raw, out var eDate))
                {
                    parsedEndDate = eDate;
                    displayValuesDict["endDate"] = eDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    AddRowIssue(rowIssues, rowNum, eDateVal.ColLetter, eDateVal.Header, "endDate", eDateVal.Raw, "INVALID_DATE", $"'{eDateVal.Raw}' geçerli bir tarih formatı değil. (Beklenen: YYYY-MM-DD)");
                }
            }

            if (parsedStartDate.HasValue && parsedEndDate.HasValue && parsedEndDate.Value < parsedStartDate.Value)
            {
                var col = fieldValues.TryGetValue("endDate", out var ev) ? ev.ColLetter : "U";
                var hdr = fieldValues.TryGetValue("endDate", out var eh) ? eh.Header : "Bitiş Tarihi";
                AddRowIssue(rowIssues, rowNum, col, hdr, "endDate", parsedEndDate.Value.ToString("yyyy-MM-dd"), "INVALID_DATE_RANGE", $"Bitiş tarihi ({parsedEndDate.Value:yyyy-MM-dd}), başlangıç tarihinden ({parsedStartDate.Value:yyyy-MM-dd}) önce olamaz.");
            }

            // ── N. Validate URLs (ApplicationUrl, RepositoryUrl, CoverImageUrl) ─
            ValidateUrlField(fieldValues, "applicationUrl", displayValuesDict, rowIssues, rowNum);
            ValidateUrlField(fieldValues, "repositoryUrl", displayValuesDict, rowIssues, rowNum);
            ValidateUrlField(fieldValues, "coverImageUrl", displayValuesDict, rowIssues, rowNum);

            // ── O. Validate IsFeatured (Optional Boolean) ─────────────────────
            bool parsedIsFeatured = false;
            if (fieldValues.TryGetValue("isFeatured", out var featVal) && !string.IsNullOrWhiteSpace(featVal.Raw))
            {
                if (TryParseBoolean(featVal.Raw, out var isFeat))
                {
                    parsedIsFeatured = isFeat;
                    displayValuesDict["isFeatured"] = isFeat ? "true" : "false";
                }
                else
                {
                    AddRowIssue(rowIssues, rowNum, featVal.ColLetter, featVal.Header, "isFeatured", featVal.Raw, "INVALID_BOOLEAN", $"'{featVal.Raw}' geçerli bir mantıksal değer değil. (Beklenen: EVET, HAYIR, true veya false)");
                }
            }
            else
            {
                displayValuesDict["isFeatured"] = "false";
            }

            // ── P. Row Issue Accounting ───────────────────────────────────────
            var rowErrors = rowIssues.Where(i => i.Severity == "error").ToList();
            var rowWarnings = rowIssues.Where(i => i.Severity == "warning").ToList();

            if (rowErrors.Count > 0)
            {
                invalidRows++;
            }
            else
            {
                validRows++;

                // Construct candidate Project entity
                fieldValues.TryGetValue("description", out var descVal);
                fieldValues.TryGetValue("purpose", out var purpVal);
                fieldValues.TryGetValue("problemSolved", out var probVal);
                fieldValues.TryGetValue("nonTechnicalDescription", out var nonTechVal);
                fieldValues.TryGetValue("technicalDescription", out var techTextVal);
                fieldValues.TryGetValue("businessImpact", out var impactVal);
                fieldValues.TryGetValue("targetAudience", out var audVal);
                fieldValues.TryGetValue("accessInstructions", out var accessVal);
                fieldValues.TryGetValue("applicationUrl", out var appUrlVal);
                fieldValues.TryGetValue("repositoryUrl", out var repoUrlVal);
                fieldValues.TryGetValue("coverImageUrl", out var coverUrlVal);

                var project = new Project
                {
                    Name = resolvedName,
                    Slug = resolvedSlug,
                    ShortDescription = resolvedShortDescription,
                    Description = string.IsNullOrWhiteSpace(descVal.Raw) ? null : descVal.Raw.Trim(),
                    Purpose = string.IsNullOrWhiteSpace(purpVal.Raw) ? null : purpVal.Raw.Trim(),
                    ProblemSolved = string.IsNullOrWhiteSpace(probVal.Raw) ? null : probVal.Raw.Trim(),
                    NonTechnicalDescription = string.IsNullOrWhiteSpace(nonTechVal.Raw) ? null : nonTechVal.Raw.Trim(),
                    TechnicalDescription = string.IsNullOrWhiteSpace(techTextVal.Raw) ? null : techTextVal.Raw.Trim(),
                    BusinessImpact = string.IsNullOrWhiteSpace(impactVal.Raw) ? null : impactVal.Raw.Trim(),
                    TargetAudience = string.IsNullOrWhiteSpace(audVal.Raw) ? null : audVal.Raw.Trim(),
                    AccessInstructions = string.IsNullOrWhiteSpace(accessVal.Raw) ? null : accessVal.Raw.Trim(),
                    ApplicationUrl = string.IsNullOrWhiteSpace(appUrlVal.Raw) ? null : appUrlVal.Raw.Trim(),
                    RepositoryUrl = string.IsNullOrWhiteSpace(repoUrlVal.Raw) ? null : repoUrlVal.Raw.Trim(),
                    CoverImageUrl = string.IsNullOrWhiteSpace(coverUrlVal.Raw) ? null : coverUrlVal.Raw.Trim(),
                    CategoryId = resolvedCategoryId,
                    StatusId = resolvedStatusId,
                    DevelopmentType = parsedDevType,
                    StartDate = parsedStartDate,
                    EndDate = parsedEndDate,
                    IsFeatured = parsedIsFeatured,
                    IsPublished = false,
                    ApprovalStatus = ProjectApprovalStatus.Draft,
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                // Add relations
                if (primaryTeamId.HasValue)
                {
                    project.ProjectTeams.Add(new ProjectTeam
                    {
                        TeamId = primaryTeamId.Value,
                        IsPrimary = true
                    });
                }

                foreach (var supTeamId in resolvedSupportingTeamIds.Distinct())
                {
                    if (!primaryTeamId.HasValue || supTeamId != primaryTeamId.Value)
                    {
                        project.ProjectTeams.Add(new ProjectTeam
                        {
                            TeamId = supTeamId,
                            IsPrimary = false
                        });
                    }
                }

                foreach (var memId in resolvedMemberIds.Distinct())
                {
                    project.ProjectMembers.Add(new ProjectMember
                    {
                        MemberId = memId,
                        ProjectRole = "Geliştirici"
                    });
                }

                foreach (var techId in resolvedTechIds.Distinct())
                {
                    project.ProjectTechnologies.Add(new ProjectTechnology
                    {
                        TechnologyId = techId
                    });
                }

                foreach (var locId in resolvedLocIds.Distinct())
                {
                    project.ProjectLocations.Add(new ProjectLocation
                    {
                        LocationId = locId
                    });
                }

                foreach (var tagId in resolvedTagIds.Distinct())
                {
                    project.ProjectTags.Add(new ProjectTag
                    {
                        TagId = tagId
                    });
                }

                candidateItems.Add(new CandidateProjectImportItem
                {
                    Project = project,
                    PendingTechnologies = pendingTechNames,
                    PendingLocations = pendingLocNames,
                    PendingTags = pendingTagNames
                });
            }

            totalErrors += rowErrors.Count;
            totalWarnings += rowWarnings.Count;

            // Collect errors up to limit
            foreach (var err in rowErrors)
            {
                if (detailedErrors.Count < MaxReportedErrors)
                {
                    detailedErrors.Add(err);
                }
            }

            // Collect warnings
            foreach (var warn in rowWarnings)
            {
                detailedWarnings.Add(warn);
            }

            // Preview rows (up to first 20)
            if (previewRows.Count < MaxPreviewRows)
            {
                previewRows.Add(new ProjectImportRowPreviewDto
                {
                    RowNumber = rowNum,
                    IsValid = rowErrors.Count == 0,
                    RawValues = rawValuesDict,
                    DisplayValues = displayValuesDict,
                    Issues = rowIssues
                });
            }
        }

        // ── 6. Summary & Response ─────────────────────────────────────────────
        var proposedRefList = new List<ProposedNewReferenceDto>();
        foreach (var tech in proposedTechnologies.Values.OrderBy(v => v))
        {
            proposedRefList.Add(new ProposedNewReferenceDto { Type = "Technology", Value = tech });
        }
        foreach (var loc in proposedLocations.Values.OrderBy(v => v))
        {
            proposedRefList.Add(new ProposedNewReferenceDto { Type = "Location", Value = loc });
        }
        foreach (var tag in proposedTags.Values.OrderBy(v => v))
        {
            proposedRefList.Add(new ProposedNewReferenceDto { Type = "Tag", Value = tag });
        }
        response.ProposedNewReferences = proposedRefList;

        response.TotalRows = totalDataRows;
        response.ValidRowCount = validRows;
        response.InvalidRowCount = invalidRows;
        response.ErrorCount = totalErrors;
        response.WarningCount = totalWarnings;
        response.TotalErrorCount = totalErrors;
        response.ErrorsTruncated = totalErrors > MaxReportedErrors;
        response.CanImport = (invalidRows == 0 && totalErrors == 0 && totalDataRows > 0);
        response.PreviewRows = previewRows;
        response.Errors = detailedErrors;
        response.Warnings = detailedWarnings;

        if (response.CanImport)
        {
            result.CandidateItems = candidateItems;
        }

        return result;
    }

    public async Task<byte[]> GenerateImportTemplateAsync(CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        // ── 1. Projeler Sayfası ────────────────────────────────────────────────
        var wsProjects = workbook.Worksheets.Add("Projeler");

        var headers = new[]
        {
            "Proje Adı *",
            "Kısa Açıklama *",
            "Kategori *",
            "Durum *",
            "Sorumlu Ekip",
            "Destekleyen Ekipler",
            "Proje Üyeleri",
            "Geliştirme Tipi",
            "Teknolojiler",
            "Lokasyonlar",
            "Etiketler",
            "Genel Açıklama",
            "Amaç",
            "Çözülen Problem",
            "Teknik Olmayan Açıklama",
            "Teknik Açıklama",
            "İş Etkisi",
            "Hedef Kitle",
            "Erişim Talimatları",
            "Başlangıç Tarihi",
            "Bitiş Tarihi",
            "Canlı Uygulama URL",
            "Repository URL",
            "Kapak Görseli URL",
            "Öne Çıkan"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = wsProjects.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = headers[i].EndsWith('*') ? XLColor.FromHtml("#1E3A8A") : XLColor.FromHtml("#334155");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        wsProjects.Row(1).Height = 28;
        wsProjects.SheetView.FreezeRows(1);

        // Date formatting on date columns (Column 20 and 21: Başlangıç Tarihi, Bitiş Tarihi)
        wsProjects.Column(20).Style.DateFormat.Format = "yyyy-MM-dd";
        wsProjects.Column(21).Style.DateFormat.Format = "yyyy-MM-dd";

        // Wrap text on description columns
        wsProjects.Column(2).Style.Alignment.WrapText = true; // Kısa Açıklama
        wsProjects.Column(12).Style.Alignment.WrapText = true; // Genel Açıklama
        wsProjects.Column(13).Style.Alignment.WrapText = true; // Amaç
        wsProjects.Column(14).Style.Alignment.WrapText = true; // Çözülen Problem
        wsProjects.Column(15).Style.Alignment.WrapText = true; // Teknik Olmayan Açıklama
        wsProjects.Column(16).Style.Alignment.WrapText = true; // Teknik Açıklama

        // ── 2. Yardım ve Değer Listeleri Sayfası ─────────────────────────────
        var wsHelp = workbook.Worksheets.Add("Yardım ve Değer Listeleri");

        // Fetch DB lookups
        var categories = await _context.ProjectCategories.AsNoTracking().OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).Select(c => c.Name).ToListAsync(cancellationToken);
        var statuses = await _context.ProjectStatuses.AsNoTracking().OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name).Select(s => s.Name).ToListAsync(cancellationToken);
        var teams = await _context.Teams.AsNoTracking().OrderBy(t => t.Name).Select(t => t.Name).ToListAsync(cancellationToken);
        var devTypes = Enum.GetNames<DevelopmentType>().ToList();
        var technologies = await _context.Technologies.AsNoTracking().OrderBy(t => t.Name).Select(t => t.Name).ToListAsync(cancellationToken);
        var locations = await _context.Locations.AsNoTracking().OrderBy(l => l.Name).Select(l => l.Name).ToListAsync(cancellationToken);
        var tags = await _context.Tags.AsNoTracking().OrderBy(t => t.Name).Select(t => t.Name).ToListAsync(cancellationToken);

        // Populate Lookup Reference Columns
        PopulateReferenceList(wsHelp, 1, "Kategoriler", categories);
        PopulateReferenceList(wsHelp, 2, "Durumlar", statuses);
        PopulateReferenceList(wsHelp, 3, "Ekipler", teams);
        PopulateReferenceList(wsHelp, 4, "Geliştirme Tipleri", devTypes);
        PopulateReferenceList(wsHelp, 5, "Teknolojiler", technologies);
        PopulateReferenceList(wsHelp, 6, "Lokasyonlar", locations);
        PopulateReferenceList(wsHelp, 7, "Etiketler", tags);

        // Field Description Documentation (Columns 9-13: I to M)
        var docHeaders = new[] { "Alan Adı", "Zorunlu mu?", "Veri Tipi", "Örnek Değer", "Açıklama" };
        for (int i = 0; i < docHeaders.Length; i++)
        {
            var cell = wsHelp.Cell(1, 9 + i);
            cell.Value = docHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
        }

        var docRows = new (string Name, string Req, string Type, string Example, string Desc)[]
        {
            ("Proje Adı *", "Evet", "Metin (Max 200)", "Saha Takip Sistemi", "Projenin resmi adı."),
            ("Kısa Açıklama *", "Evet", "Metin (Max 500)", "Yapay zeka destekli saha takip uygulaması.", "Proje kartlarında ve özetlerde görünen tek cümlelik açıklama."),
            ("Kategori *", "Evet", "Seçim Listesi", "Yapay Zeka", "Sistemde tanımlı kategorilerden biri."),
            ("Durum *", "Evet", "Seçim Listesi", "Aktif Geliştirme", "Projenin geliştirme durumu."),
            ("Sorumlu Ekip", "Hayır", "Seçim Listesi", "Yazılım Geliştirme Ekibi", "Projeden birinci derecede sorumlu ekip."),
            ("Destekleyen Ekipler", "Hayır", "Metin (Çoklu ;)", "Veri Analitiği Ekibi; Otomasyon Ekibi", "Projeye destek veren diğer ekipler. Birden fazla değer için ; kullanınız."),
            ("Proje Üyeleri", "Hayır", "Metin (Çoklu ;)", "ali.yilmaz@demirexport.com; ayse.kaya@demirexport.com", "Projede görev alan kişilerin kurumsal e-posta adresleri."),
            ("Geliştirme Tipi", "Hayır", "Seçim Listesi", "Internal", "Internal (Şirket İçi), External (Dış Kaynak), Hybrid (Hibrit)."),
            ("Teknolojiler", "Hayır", "Metin (Çoklu ;)", "React; .NET 9; SQL Server", "Projede kullanılan teknolojiler. Mevcut olanlar eşleştirilir, yeni teknolojiler içe aktarımda otomatik oluşturulur."),
            ("Lokasyonlar", "Hayır", "Metin (Çoklu ;)", "Gediktepe Maden Sahası; Genel Müdürlük", "Projenin uygulandığı lokasyonlar. Mevcut olanlar eşleştirilir, yeni lokasyonlar içe aktarımda otomatik oluşturulur."),
            ("Etiketler", "Hayır", "Metin (Çoklu ;)", "Saha Yönetimi; IoT; Yapay Zeka", "Arama ve keşif etiketleri. Mevcut olanlar eşleştirilir, yeni etiketler içe aktarımda otomatik oluşturulur."),
            ("Genel Açıklama", "Hayır", "Metin (Uzun)", "Proje detaylı kapsam metni...", "Proje hakkında detaylı genel açıklama."),
            ("Amaç", "Hayır", "Metin", "Saha veri giriş sürelerini azaltmak.", "Projenin geliştirilme amacı."),
            ("Çözülen Problem", "Hayır", "Metin", "Manuel raporlamadaki gecikmeler.", "Projenin çözdüğü iş veya operasyonel problem."),
            ("Teknik Olmayan Açıklama", "Hayır", "Metin", "Saha çalışanlarının operasyonları dijital kaydetmesini sağlar.", "Teknik bilgisi olmayan çalışanlar için sade açıklama."),
            ("Teknik Açıklama", "Hayır", "Metin", "Mikroservis mimarisinde .NET 9 Web API ve React SPA.", "Yazılım ve mimari detaylar."),
            ("İş Etkisi", "Hayır", "Metin", "Operasyon süresinde %30 verimlilik artışı.", "İş kazancı ve verimlilik etkisi."),
            ("Hedef Kitle", "Hayır", "Metin", "Saha Operatörleri, Vardiya Amirleri", "Projenin son kullanıcı kitlesi."),
            ("Erişim Talimatları", "Hayır", "Metin", "VPN bağlantısı ile saha portalından erişilebilir.", "Uygulamaya erişim ve kullanım yönergeleri."),
            ("Başlangıç Tarihi", "Hayır", "Tarih (YYYY-MM-DD)", "2026-01-15", "Projenin başladığı tarih."),
            ("Bitiş Tarihi", "Hayır", "Tarih (YYYY-MM-DD)", "2026-12-31", "Projenin tamamlandığı/hedeflenen bitiş tarihi."),
            ("Canlı Uygulama URL", "Hayır", "URL", "https://saha.demirexport.com", "Canlı uygulamanın web adresi."),
            ("Repository URL", "Hayır", "URL", "https://github.com/demirexport/saha-takip", "Proje kaynak kod deposu adresi."),
            ("Kapak Görseli URL", "Hayır", "URL", "https://example.com/cover.jpg", "Kapak görseli web bağlantısı."),
            ("Öne Çıkan", "Hayır", "Boolean (true/false)", "false", "Ana sayfada öne çıkarılsın mı?")
        };

        for (int rowIdx = 0; rowIdx < docRows.Length; rowIdx++)
        {
            var item = docRows[rowIdx];
            wsHelp.Cell(rowIdx + 2, 9).Value = item.Name;
            wsHelp.Cell(rowIdx + 2, 10).Value = item.Req;
            wsHelp.Cell(rowIdx + 2, 11).Value = item.Type;
            wsHelp.Cell(rowIdx + 2, 12).Value = item.Example;
            wsHelp.Cell(rowIdx + 2, 13).Value = item.Desc;
        }

        // ── 3. Dropdown Data Validation Linkages ──────────────────────────────
        if (categories.Count > 0)
        {
            var categoryValidation = wsProjects.Range("C2:C500").CreateDataValidation();
            categoryValidation.List($"='Yardım ve Değer Listeleri'!$A$2:$A${categories.Count + 1}", true);
            categoryValidation.InputTitle = "Kategori";
            categoryValidation.InputMessage = "Listeden geçerli bir kategori seçiniz.";
        }

        if (statuses.Count > 0)
        {
            var statusValidation = wsProjects.Range("D2:D500").CreateDataValidation();
            statusValidation.List($"='Yardım ve Değer Listeleri'!$B$2:$B${statuses.Count + 1}", true);
            statusValidation.InputTitle = "Durum";
            statusValidation.InputMessage = "Listeden geçerli bir proje durumu seçiniz.";
        }

        if (teams.Count > 0)
        {
            var teamValidation = wsProjects.Range("E2:E500").CreateDataValidation();
            teamValidation.List($"='Yardım ve Değer Listeleri'!$C$2:$C${teams.Count + 1}", true);
            teamValidation.InputTitle = "Sorumlu Ekip";
            teamValidation.InputMessage = "Listeden sorumlu ekibi seçiniz.";
        }

        if (devTypes.Count > 0)
        {
            var devTypeValidation = wsProjects.Range("H2:H500").CreateDataValidation();
            devTypeValidation.List($"='Yardım ve Değer Listeleri'!$D$2:$D${devTypes.Count + 1}", true);
            devTypeValidation.InputTitle = "Geliştirme Tipi";
            devTypeValidation.InputMessage = "Internal, External veya Hybrid seçiniz.";
        }

        wsProjects.Columns().AdjustToContents(1, 30);
        wsHelp.Columns().AdjustToContents(1, 40);

        using var resultStream = new MemoryStream();
        workbook.SaveAs(resultStream);
        return resultStream.ToArray();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static (string? FieldKey, string? Confidence) SuggestFieldForHeader(string rawHeader)
    {
        if (string.IsNullOrWhiteSpace(rawHeader)) return (null, null);

        var cleaned = rawHeader.Trim().TrimEnd('*').Trim();
        var normalized = NormalizeText(cleaned);

        foreach (var def in ProjectImportFieldCatalog.Fields)
        {
            // Exact display name or key match
            if (string.Equals(def.Key, cleaned, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(def.DisplayName, cleaned, StringComparison.OrdinalIgnoreCase))
            {
                return (def.Key, "exact");
            }

            // Normalized display name match
            if (string.Equals(NormalizeText(def.DisplayName), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return (def.Key, "exact");
            }

            // Alias matches
            foreach (var alias in def.Aliases)
            {
                if (string.Equals(NormalizeText(alias), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return (def.Key, "alias");
                }
            }
        }

        return (null, "none");
    }

    private static List<ProjectImportIssueDto> ValidateMappingContract(
        List<ColumnMappingDto>? mappings,
        out List<AppliedColumnMappingDto> appliedMappings,
        out Dictionary<string, string> columnToFieldMap)
    {
        appliedMappings = new List<AppliedColumnMappingDto>();
        columnToFieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<ProjectImportIssueDto>();

        if (mappings == null || mappings.Count == 0)
        {
            errors.Add(new ProjectImportIssueDto
            {
                ErrorCode = "MAPPING_EMPTY",
                Message = "Sütun eşleştirmesi belirtilmedi.",
                Severity = "error"
            });
            return errors;
        }

        var mappedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var mappedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var m in mappings)
        {
            var col = m.ExcelColumn?.Trim();
            var field = m.SystemField?.Trim();

            if (string.IsNullOrWhiteSpace(col))
            {
                errors.Add(new ProjectImportIssueDto
                {
                    ErrorCode = "COLUMN_EMPTY",
                    Message = "Eşleştirme listesinde sütun harfi boş olamaz.",
                    Severity = "error"
                });
                continue;
            }

            if (string.IsNullOrWhiteSpace(field))
            {
                continue; // Unmapped column
            }

            // Check if column mapped multiple times
            if (mappedColumns.Contains(col))
            {
                errors.Add(new ProjectImportIssueDto
                {
                    ExcelColumn = col,
                    SystemField = field,
                    ErrorCode = "DUPLICATE_MAPPING",
                    Message = $"'{col}' Excel sütunu birden fazla sistem alanına eşlenemez.",
                    Severity = "error"
                });
            }
            else
            {
                mappedColumns.Add(col);
            }

            // Check if field is protected
            if (ProjectImportFieldCatalog.IsProtectedField(field))
            {
                errors.Add(new ProjectImportIssueDto
                {
                    ExcelColumn = col,
                    SystemField = field,
                    ErrorCode = "PROTECTED_FIELD",
                    Message = $"'{field}' alanı sistem tarafından otomatik yönetildiği için eşleştirilemez.",
                    Severity = "error"
                });
                continue;
            }

            // Check if field is valid
            var fieldDef = ProjectImportFieldCatalog.GetByKey(field);
            if (fieldDef == null)
            {
                errors.Add(new ProjectImportIssueDto
                {
                    ExcelColumn = col,
                    SystemField = field,
                    ErrorCode = "UNKNOWN_SYSTEM_FIELD",
                    Message = $"'{field}' adında geçerli bir sistem alanı bulunmamaktadır.",
                    Severity = "error"
                });
                continue;
            }

            // Check if single-value field mapped multiple times
            if (mappedFields.Contains(field))
            {
                errors.Add(new ProjectImportIssueDto
                {
                    ExcelColumn = col,
                    SystemField = field,
                    ErrorCode = "DUPLICATE_MAPPING",
                    Message = $"'{fieldDef.DisplayName}' alanı birden fazla sütuna eşlenemez.",
                    Severity = "error"
                });
            }
            else
            {
                mappedFields.Add(field);
            }

            columnToFieldMap[col] = field;
            appliedMappings.Add(new AppliedColumnMappingDto
            {
                ExcelColumn = col,
                SystemField = fieldDef.Key,
                SystemFieldDisplayName = fieldDef.DisplayName,
                IsRequired = fieldDef.IsRequired
            });
        }

        // Check required fields
        var requiredFields = ProjectImportFieldCatalog.Fields.Where(f => f.IsRequired).ToList();
        foreach (var req in requiredFields)
        {
            if (!mappedFields.Contains(req.Key))
            {
                errors.Add(new ProjectImportIssueDto
                {
                    SystemField = req.Key,
                    ErrorCode = "REQUIRED_MAPPING_MISSING",
                    Message = $"Zorunlu alan olan '{req.DisplayName}' için bir Excel sütunu eşleştirilmelidir.",
                    Severity = "error"
                });
            }
        }

        return errors;
    }

    private static void PopulateReferenceList(IXLWorksheet ws, int colIndex, string header, List<string> items)
    {
        var headerCell = ws.Cell(1, colIndex);
        headerCell.Value = header;
        headerCell.Style.Font.Bold = true;
        headerCell.Style.Font.FontColor = XLColor.White;
        headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#475569");
        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        for (int i = 0; i < items.Count; i++)
        {
            ws.Cell(i + 2, colIndex).Value = items[i];
        }
    }

    private static bool IsValidZipPackage(byte[] bytes, out bool isMacroEnabled, out string? issue)
    {
        isMacroEnabled = false;
        issue = null;

        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B || bytes[2] != 0x03 || bytes[3] != 0x04)
        {
            issue = "Dosya geçerli bir ZIP arşivi (Excel formatı) başlığı taşımıyor.";
            return false;
        }

        try
        {
            using var stream = new MemoryStream(bytes);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var entryCount = archive.Entries.Count;
            if (entryCount == 0)
            {
                issue = "ZIP arşivi boş.";
                return false;
            }

            if (entryCount > 1000)
            {
                issue = "ZIP arşivi aşırı sayıda dosya içeriyor (olası zip bombası).";
                return false;
            }

            bool hasWorkbook = false;
            bool hasContentTypes = false;

            foreach (var entry in archive.Entries)
            {
                var entryName = entry.FullName;
                if (entryName.Contains("..") || entryName.StartsWith('/') || entryName.StartsWith('\\'))
                {
                    issue = "Güvenlik uyarısı: ZIP arşivinde geçersiz yol karakteri tespit edildi.";
                    return false;
                }

                if (entryName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) && entryName.Contains("vbaProject"))
                {
                    isMacroEnabled = true;
                    issue = "Makro içeren (.xlsm veya VBA kodlu) Excel dosyaları güvenlik nedeniyle desteklenmemektedir.";
                    return false;
                }

                if (entryName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase))
                {
                    hasContentTypes = true;
                }

                if (entryName.Equals("xl/workbook.xml", StringComparison.OrdinalIgnoreCase))
                {
                    hasWorkbook = true;
                }
            }

            if (!hasContentTypes && !hasWorkbook)
            {
                issue = "ZIP arşivi Excel çalışma kitabı dosyalarını (xl/workbook.xml) içermiyor.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            issue = "ZIP yapısı ayrıştırılamadı: " + ex.Message;
            return false;
        }
    }

    private enum LookupMatchStatus { ExactMatch, Ambiguous, NotFound }

    private class LookupResolutionResult
    {
        public LookupMatchStatus Status { get; set; }
        public int ResolvedId { get; set; }
        public string ResolvedName { get; set; } = string.Empty;
    }

    private class LookupStore
    {
        public Dictionary<string, List<(int Id, string Name)>> ByName { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<(int Id, string Name)>> ByCode { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private static LookupStore BuildLookupMap(IEnumerable<(int Id, string Name, string? Code)> items)
    {
        var store = new LookupStore();
        foreach (var item in items)
        {
            var normName = NormalizeText(item.Name);
            if (!store.ByName.TryGetValue(normName, out var nameList))
            {
                nameList = new List<(int, string)>();
                store.ByName[normName] = nameList;
            }
            nameList.Add((item.Id, item.Name));

            if (!string.IsNullOrWhiteSpace(item.Code))
            {
                var normCode = NormalizeText(item.Code);
                if (!store.ByCode.TryGetValue(normCode, out var codeList))
                {
                    codeList = new List<(int, string)>();
                    store.ByCode[normCode] = codeList;
                }
                codeList.Add((item.Id, item.Name));
            }
        }
        return store;
    }

    private static LookupResolutionResult ResolveLookup(LookupStore store, string input)
    {
        var norm = NormalizeText(input);
        if (store.ByName.TryGetValue(norm, out var nameMatches))
        {
            if (nameMatches.Count == 1)
                return new LookupResolutionResult { Status = LookupMatchStatus.ExactMatch, ResolvedId = nameMatches[0].Id, ResolvedName = nameMatches[0].Name };
            return new LookupResolutionResult { Status = LookupMatchStatus.Ambiguous };
        }

        if (store.ByCode.TryGetValue(norm, out var codeMatches))
        {
            if (codeMatches.Count == 1)
                return new LookupResolutionResult { Status = LookupMatchStatus.ExactMatch, ResolvedId = codeMatches[0].Id, ResolvedName = codeMatches[0].Name };
            return new LookupResolutionResult { Status = LookupMatchStatus.Ambiguous };
        }

        return new LookupResolutionResult { Status = LookupMatchStatus.NotFound };
    }

    private class TagLookupStore
    {
        public Dictionary<string, List<(int Id, string Name)>> ByName { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<(int Id, string Name)>> BySlug { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private static TagLookupStore BuildTagLookupMap(IEnumerable<(int Id, string Name, string Slug)> items)
    {
        var store = new TagLookupStore();
        foreach (var item in items)
        {
            var normName = NormalizeText(item.Name);
            if (!store.ByName.TryGetValue(normName, out var nameList))
            {
                nameList = new List<(int, string)>();
                store.ByName[normName] = nameList;
            }
            nameList.Add((item.Id, item.Name));

            var normSlug = NormalizeText(item.Slug);
            if (!store.BySlug.TryGetValue(normSlug, out var slugList))
            {
                slugList = new List<(int, string)>();
                store.BySlug[normSlug] = slugList;
            }
            slugList.Add((item.Id, item.Name));
        }
        return store;
    }

    private static LookupResolutionResult ResolveTag(TagLookupStore store, string input)
    {
        var norm = NormalizeText(input);
        if (store.ByName.TryGetValue(norm, out var nameMatches))
        {
            if (nameMatches.Count == 1)
                return new LookupResolutionResult { Status = LookupMatchStatus.ExactMatch, ResolvedId = nameMatches[0].Id, ResolvedName = nameMatches[0].Name };
            return new LookupResolutionResult { Status = LookupMatchStatus.Ambiguous };
        }

        if (store.BySlug.TryGetValue(norm, out var slugMatches))
        {
            if (slugMatches.Count == 1)
                return new LookupResolutionResult { Status = LookupMatchStatus.ExactMatch, ResolvedId = slugMatches[0].Id, ResolvedName = slugMatches[0].Name };
            return new LookupResolutionResult { Status = LookupMatchStatus.Ambiguous };
        }

        return new LookupResolutionResult { Status = LookupMatchStatus.NotFound };
    }

    private class MemberLookupStore
    {
        public Dictionary<string, (int Id, string DisplayName)> ByEmail { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<(int Id, string DisplayName)>> ByFullName { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private static MemberLookupStore BuildMemberLookupMap(IEnumerable<dynamic> members)
    {
        var store = new MemberLookupStore();
        foreach (var m in members)
        {
            int id = m.Id;
            string fn = m.FirstName ?? "";
            string ln = m.LastName ?? "";
            string email = m.Email ?? "";
            string fullName = $"{fn} {ln}".Trim();
            string display = !string.IsNullOrEmpty(email) ? email : fullName;

            if (!string.IsNullOrWhiteSpace(email))
            {
                store.ByEmail[NormalizeText(email)] = (id, display);
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var normFull = NormalizeText(fullName);
                if (!store.ByFullName.TryGetValue(normFull, out var list))
                {
                    list = new List<(int, string)>();
                    store.ByFullName[normFull] = list;
                }
                list.Add((id, display));
            }
        }
        return store;
    }

    private static LookupResolutionResult ResolveMember(MemberLookupStore store, string input)
    {
        var norm = NormalizeText(input);
        if (store.ByEmail.TryGetValue(norm, out var emailMatch))
        {
            return new LookupResolutionResult { Status = LookupMatchStatus.ExactMatch, ResolvedId = emailMatch.Id, ResolvedName = emailMatch.DisplayName };
        }

        if (store.ByFullName.TryGetValue(norm, out var nameMatches))
        {
            if (nameMatches.Count == 1)
                return new LookupResolutionResult { Status = LookupMatchStatus.ExactMatch, ResolvedId = nameMatches[0].Id, ResolvedName = nameMatches[0].DisplayName };
            return new LookupResolutionResult { Status = LookupMatchStatus.Ambiguous };
        }

        return new LookupResolutionResult { Status = LookupMatchStatus.NotFound };
    }

    private static List<string> SplitMultiValues(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return new List<string>();
        return input.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var trimmed = text.Trim();
        var collapsed = Regex.Replace(trimmed, @"\s+", " ");
        return collapsed.ToLower(new CultureInfo("tr-TR"));
    }

    private static bool TryParseDate(string input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var formats = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "dd.MM.yyyy", "dd-MM-yyyy", "yyyy.MM.dd" };
        if (DateTime.TryParseExact(input.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            date = DateOnly.FromDateTime(dt);
            return true;
        }

        if (DateTime.TryParse(input.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        {
            date = DateOnly.FromDateTime(dt);
            return true;
        }

        return false;
    }

    private static bool TryParseBoolean(string input, out bool result)
    {
        result = false;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var norm = NormalizeText(input);
        if (norm is "true" or "evet" or "1" or "yes" or "t" or "e")
        {
            result = true;
            return true;
        }

        if (norm is "false" or "hayır" or "hayir" or "0" or "no" or "f" or "h")
        {
            result = false;
            return true;
        }

        return false;
    }

    private static void ValidateUrlField(
        Dictionary<string, (string Raw, string ColLetter, string Header)> fieldValues,
        string fieldKey,
        Dictionary<string, string?> displayDict,
        List<ProjectImportIssueDto> issues,
        int rowNum)
    {
        if (fieldValues.TryGetValue(fieldKey, out var item) && !string.IsNullOrWhiteSpace(item.Raw))
        {
            var raw = item.Raw;
            if (raw.Length > 500)
            {
                AddRowIssue(issues, rowNum, item.ColLetter, item.Header, fieldKey, raw, "MAX_LENGTH_EXCEEDED", $"URL adresi en fazla 500 karakter olabilir. (Mevcut: {raw.Length} karakter)");
                return;
            }

            if (Uri.TryCreate(raw, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                displayDict[fieldKey] = raw;
            }
            else
            {
                AddRowIssue(issues, rowNum, item.ColLetter, item.Header, fieldKey, raw, "INVALID_URL", $"'{raw}' geçerli bir web URL adresi değil. (Yalnızca http:// veya https:// adresleri kabul edilir)");
            }
        }
    }

    private static void ValidateOptionalTextField(
        Dictionary<string, (string Raw, string ColLetter, string Header)> fieldValues,
        string fieldKey,
        Dictionary<string, string?> displayDict,
        List<ProjectImportIssueDto> issues,
        int rowNum,
        int? maxLength)
    {
        if (fieldValues.TryGetValue(fieldKey, out var item) && !string.IsNullOrWhiteSpace(item.Raw))
        {
            var raw = item.Raw;
            if (maxLength.HasValue && raw.Length > maxLength.Value)
            {
                var def = ProjectImportFieldCatalog.GetByKey(fieldKey);
                var name = def?.DisplayName ?? fieldKey;
                AddRowIssue(issues, rowNum, item.ColLetter, item.Header, fieldKey, raw, "MAX_LENGTH_EXCEEDED", $"{name} en fazla {maxLength.Value} karakter olabilir. (Mevcut: {raw.Length} karakter)");
            }
            else
            {
                displayDict[fieldKey] = raw;
            }
        }
    }

    private static void AddRowIssue(
        List<ProjectImportIssueDto> issues,
        int rowNum,
        string excelColumn,
        string excelHeader,
        string systemField,
        string? rawValue,
        string errorCode,
        string message,
        string severity = "error")
    {
        issues.Add(new ProjectImportIssueDto
        {
            RowNumber = rowNum,
            ExcelColumn = excelColumn,
            ExcelHeader = excelHeader,
            SystemField = systemField,
            RawValue = BoundRawValue(rawValue),
            ErrorCode = errorCode,
            Message = message,
            Severity = severity
        });
    }

    private static string? BoundRawValue(string? rawValue)
    {
        if (string.IsNullOrEmpty(rawValue)) return rawValue;
        if (rawValue.Length <= MaxErrorStringLength) return rawValue;
        return rawValue[..MaxErrorStringLength] + "...";
    }

    public async Task<byte[]> ExportProjectsAsync(
        DeUygulamaVitrini.Application.DTOs.Admin.AdminProjectQueryParams parameters,
        int? createdByUserIdFilter = null,
        CancellationToken cancellationToken = default)
    {
        var isArchivedQuery = string.Equals(parameters.LifecycleState, "archived", StringComparison.OrdinalIgnoreCase);
        var isAllLifecycleQuery = string.Equals(parameters.LifecycleState, "all", StringComparison.OrdinalIgnoreCase);

        var query = isArchivedQuery || isAllLifecycleQuery
            ? _context.Projects.IgnoreQueryFilters().AsNoTracking()
            : _context.Projects.AsNoTracking();

        if (isArchivedQuery)
        {
            query = query.Where(p => p.IsDeleted);
        }
        else if (!isAllLifecycleQuery)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        // Ownership filter
        if (createdByUserIdFilter.HasValue && createdByUserIdFilter.Value > 0)
        {
            query = query.Where(p => p.CreatedByUserId == createdByUserIdFilter.Value);
        }

        // Publication state filter
        if (string.Equals(parameters.PublicationState, "published", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.IsPublished);
        }
        else if (string.Equals(parameters.PublicationState, "draft", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => !p.IsPublished);
        }

        // Approval state filter
        var normalizedApprovalState = parameters.ApprovalState?.Replace("_", "").Replace("-", "");
        if (string.Equals(normalizedApprovalState, "draft", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.Draft);
        }
        else if (string.Equals(normalizedApprovalState, "pendingreview", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.PendingReview);
        }
        else if (string.Equals(normalizedApprovalState, "approved", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.Approved);
        }
        else if (string.Equals(normalizedApprovalState, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.Rejected);
        }

        // StatusId filter
        if (parameters.StatusId.HasValue && parameters.StatusId > 0)
        {
            query = query.Where(p => p.StatusId == parameters.StatusId.Value);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.ShortDescription.ToLower().Contains(searchTerm));
        }

        // Sorting
        var isAscending = string.Equals(parameters.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var sortBy = parameters.SortBy?.ToLowerInvariant() ?? "updatedat";

        query = sortBy switch
        {
            "name" => isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "createdat" => isAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            "startdate" => isAscending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
            _ => isAscending ? query.OrderBy(p => p.UpdatedAt ?? p.CreatedAt) : query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
        };

        // Eager load all relations (prevent N+1 queries)
        var projects = await query
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team)
            .Include(p => p.ProjectMembers).ThenInclude(pm => pm.Member)
            .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
            .Include(p => p.ProjectLocations).ThenInclude(pl => pl.Location)
            .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Projeler");

        // Headers
        var headers = new[]
        {
            "Proje Adı",
            "Kısa Açıklama",
            "Kategori",
            "Durum",
            "Onay Durumu",
            "Yayın Durumu",
            "Geliştirme Tipi",
            "Sorumlu Ekip",
            "Destekleyen Ekipler",
            "Proje Üyeleri",
            "Teknolojiler",
            "Lokasyonlar",
            "Etiketler",
            "Başlangıç Tarihi",
            "Bitiş Tarihi",
            "Öne Çıkan",
            "Canlı Uygulama URL",
            "Repository URL",
            "Oluşturulma Tarihi",
            "Güncellenme Tarihi"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(15, 39, 68); // Kurumsal lacivert (#0F2744)
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        ws.Row(1).Height = 26;

        // Data Rows
        int rowIdx = 2;
        foreach (var p in projects)
        {
            // 1. Proje Adı (Literal Text - Formula Injection Safe)
            SetTextCell(ws.Cell(rowIdx, 1), p.Name);

            // 2. Kısa Açıklama
            SetTextCell(ws.Cell(rowIdx, 2), p.ShortDescription);
            ws.Cell(rowIdx, 2).Style.Alignment.WrapText = true;

            // 3. Kategori
            SetTextCell(ws.Cell(rowIdx, 3), p.Category?.Name ?? "");

            // 4. Durum
            SetTextCell(ws.Cell(rowIdx, 4), p.Status?.Name ?? "");

            // 5. Onay Durumu (Taslak, İnceleme Bekliyor, Onaylandı, Düzeltme İstendi)
            string approvalLabel = p.ApprovalStatus switch
            {
                ProjectApprovalStatus.Draft => "Taslak",
                ProjectApprovalStatus.PendingReview => "İnceleme Bekliyor",
                ProjectApprovalStatus.Approved => "Onaylandı",
                ProjectApprovalStatus.Rejected => "Düzeltme İstendi",
                _ => p.ApprovalStatus.ToString()
            };
            SetTextCell(ws.Cell(rowIdx, 5), approvalLabel);

            // 6. Yayın Durumu
            SetTextCell(ws.Cell(rowIdx, 6), p.IsPublished ? "Yayında" : "Yayında Değil");

            // 7. Geliştirme Tipi
            string devTypeLabel = p.DevelopmentType switch
            {
                DevelopmentType.Internal => "İç Kaynak",
                DevelopmentType.External => "Dış Kaynak",
                DevelopmentType.Hybrid => "Hibrit",
                _ => p.DevelopmentType.ToString()
            };
            SetTextCell(ws.Cell(rowIdx, 7), devTypeLabel);

            // 8. Sorumlu Ekip (IsPrimary == true)
            var primaryTeam = p.ProjectTeams.FirstOrDefault(t => t.IsPrimary)?.Team?.Name ?? "";
            SetTextCell(ws.Cell(rowIdx, 8), primaryTeam);

            // 9. Destekleyen Ekipler (IsPrimary == false)
            var supTeams = string.Join("; ", p.ProjectTeams
                .Where(t => !t.IsPrimary && t.Team != null)
                .Select(t => t.Team!.Name)
                .OrderBy(n => n));
            SetTextCell(ws.Cell(rowIdx, 9), supTeams);

            // 10. Proje Üyeleri
            var members = string.Join("; ", p.ProjectMembers
                .Where(m => m.Member != null)
                .Select(m => $"{m.Member!.FirstName} {m.Member.LastName}".Trim())
                .OrderBy(n => n));
            SetTextCell(ws.Cell(rowIdx, 10), members);

            // 11. Teknolojiler
            var techs = string.Join("; ", p.ProjectTechnologies
                .Where(t => t.Technology != null)
                .Select(t => t.Technology!.Name)
                .OrderBy(n => n));
            SetTextCell(ws.Cell(rowIdx, 11), techs);

            // 12. Lokasyonlar
            var locs = string.Join("; ", p.ProjectLocations
                .Where(l => l.Location != null)
                .Select(l => l.Location!.Name)
                .OrderBy(n => n));
            SetTextCell(ws.Cell(rowIdx, 12), locs);

            // 13. Etiketler
            var tags = string.Join("; ", p.ProjectTags
                .Where(t => t.Tag != null)
                .Select(t => t.Tag!.Name)
                .OrderBy(n => n));
            SetTextCell(ws.Cell(rowIdx, 13), tags);

            // 14. Başlangıç Tarihi
            if (p.StartDate.HasValue)
            {
                var dCell = ws.Cell(rowIdx, 14);
                dCell.Value = p.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                dCell.Style.DateFormat.Format = "dd.MM.yyyy";
            }

            // 15. Bitiş Tarihi
            if (p.EndDate.HasValue)
            {
                var dCell = ws.Cell(rowIdx, 15);
                dCell.Value = p.EndDate.Value.ToDateTime(TimeOnly.MinValue);
                dCell.Style.DateFormat.Format = "dd.MM.yyyy";
            }

            // 16. Öne Çıkan
            SetTextCell(ws.Cell(rowIdx, 16), p.IsFeatured ? "Evet" : "Hayır");

            // 17. Canlı Uygulama URL
            SetTextCell(ws.Cell(rowIdx, 17), p.ApplicationUrl ?? "");

            // 18. Repository URL
            SetTextCell(ws.Cell(rowIdx, 18), p.RepositoryUrl ?? "");

            // 19. Oluşturulma Tarihi
            var crCell = ws.Cell(rowIdx, 19);
            crCell.Value = p.CreatedAt;
            crCell.Style.DateFormat.Format = "dd.MM.yyyy HH:mm";

            // 20. Güncellenme Tarihi
            if (p.UpdatedAt.HasValue)
            {
                var upCell = ws.Cell(rowIdx, 20);
                upCell.Value = p.UpdatedAt.Value;
                upCell.Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            }

            rowIdx++;
        }

        // Freeze Top Row
        ws.SheetView.FreezeRows(1);

        // AutoFilter on whole range
        int maxRow = Math.Max(2, rowIdx - 1);
        ws.Range(1, 1, maxRow, headers.Length).SetAutoFilter();

        // Adjust column widths defensively
        for (int col = 1; col <= headers.Length; col++)
        {
            ws.Column(col).AdjustToContents(1, Math.Min(maxRow, 100));
            var currentWidth = ws.Column(col).Width;
            if (currentWidth < 12) ws.Column(col).Width = 12;
            if (col == 2) // Short description
            {
                ws.Column(col).Width = Math.Min(50, Math.Max(currentWidth, 30));
            }
            else if (currentWidth > 35)
            {
                ws.Column(col).Width = 35;
            }
        }

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private static void SetTextCell(IXLCell cell, string? text)
    {
        var safeText = text ?? string.Empty;
        cell.Value = safeText;
    }
}

