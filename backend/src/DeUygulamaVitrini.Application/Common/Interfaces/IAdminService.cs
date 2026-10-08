using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Admin;
using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<AdminProjectListItemDto>> GetAdminProjectsAsync(
        AdminProjectQueryParams parameters,
        int? createdByUserIdFilter = null,
        CancellationToken cancellationToken = default);

    Task<AdminProjectEditDto?> GetProjectForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateProjectAsync(CreateProjectRequestDto request, int? createdByUserId = null, CancellationToken cancellationToken = default);

    Task UpdateProjectAsync(int id, UpdateProjectRequestDto request, int? actorUserId = null, CancellationToken cancellationToken = default);

    Task DeleteProjectAsync(int id, int? actorUserId = null, CancellationToken cancellationToken = default);

    Task RestoreProjectAsync(int id, int? actorUserId = null, CancellationToken cancellationToken = default);

    Task SetProjectPublishedAsync(int id, bool isPublished, int? actorUserId = null, CancellationToken cancellationToken = default);

    Task SubmitProjectForReviewAsync(int id, int userId, bool isAdmin, CancellationToken cancellationToken = default);

    Task ApproveProjectAsync(int id, int adminUserId, CancellationToken cancellationToken = default);

    Task RejectProjectAsync(int id, string reason, int adminUserId, CancellationToken cancellationToken = default);

    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryParams queryParams, CancellationToken cancellationToken = default);

    Task<List<AuditLogDto>> GetProjectAuditLogsAsync(int projectId, CancellationToken cancellationToken = default);

    Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyRequestDto request, CancellationToken cancellationToken = default);

    Task<LocationDto> CreateLocationAsync(CreateLocationRequestDto request, CancellationToken cancellationToken = default);
}
