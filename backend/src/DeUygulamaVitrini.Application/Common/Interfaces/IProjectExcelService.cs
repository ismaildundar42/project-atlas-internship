using DeUygulamaVitrini.Application.DTOs.ImportExport;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

public interface IProjectExcelService
{
    Task<InspectWorkbookResponseDto> InspectWorkbookAsync(
        Stream stream,
        string fileName,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateImportTemplateAsync(
        CancellationToken cancellationToken = default);

    Task<ValidateImportResponseDto> ValidateImportAsync(
        ValidateImportRequestDto request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ConfirmImportResponseDto> ConfirmImportAsync(
        ConfirmImportRequestDto request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportProjectsAsync(
        DeUygulamaVitrini.Application.DTOs.Admin.AdminProjectQueryParams parameters,
        int? createdByUserIdFilter = null,
        CancellationToken cancellationToken = default);
}
