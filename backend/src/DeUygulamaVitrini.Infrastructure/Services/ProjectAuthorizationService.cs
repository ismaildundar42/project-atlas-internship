using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Infrastructure.Services;

/// <summary>
/// Basitleştirilmiş Proje Yetkilendirme Servisi implementasyonu.
/// </summary>
public class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectAuthorizationService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<bool> UserHasCreatePermissionAsync(int userId)
    {
        if (userId <= 0) return false;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null && user.IsActive && user.CanCreateProjects;
    }

    public async Task<bool> CanAccessManagementAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (isAdmin) return true;
        return await UserHasCreatePermissionAsync(userId);
    }

    public async Task<bool> CanCreateProjectAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (isAdmin) return true;
        return await UserHasCreatePermissionAsync(userId);
    }

    public async Task<bool> CanEditProjectAsync(int userId, bool isAdmin, int projectId, CancellationToken cancellationToken = default)
    {
        if (isAdmin) return true;
        if (userId <= 0 || projectId <= 0) return false;

        var hasPermission = await UserHasCreatePermissionAsync(userId);
        if (!hasPermission) return false;

        // Yalnızca kendi oluşturduğu projeleri düzenleyebilir
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null || project.CreatedByUserId != userId) return false;

        // İnceleme bekleyen projeler yönetici incelemesi tamamlanana kadar düzenlenemez
        if (project.ApprovalStatus == Domain.Enums.ProjectApprovalStatus.PendingReview) return false;

        return true;
    }

    public Task<bool> CanPublishProjectAsync(int userId, bool isAdmin, int projectId, CancellationToken cancellationToken = default)
    {
        // Yalnızca Admin yayınlayabilir veya yayından kaldırabilir
        return Task.FromResult(isAdmin);
    }

    public Task<bool> CanArchiveProjectAsync(int userId, bool isAdmin, int projectId, CancellationToken cancellationToken = default)
    {
        // Yalnızca Admin arşivleyebilir veya geri yükleyebilir
        return Task.FromResult(isAdmin);
    }
}
