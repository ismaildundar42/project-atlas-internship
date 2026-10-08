using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Profile;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public ProfileService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı profili bulunamadı.");

        var roles = await _userManager.GetRolesAsync(user);
        var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
        var isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);

        var userSummary = new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CanCreateProjects = user.CanCreateProjects,
            IsSuperAdmin = isSuperAdmin,
            IsAdmin = isAdmin,
            Roles = roles.ToList()
        };

        // 1. Linked Member Bilgisi (Organizasyonel Profil)
        var member = await _context.Members
            .AsNoTracking()
            .Include(m => m.Team)
                .ThenInclude(t => t!.Department)
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        MemberSummaryDto? memberSummary = null;
        if (member != null)
        {
            memberSummary = new MemberSummaryDto
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Title = member.Title,
                Email = member.Email,
                TeamId = member.TeamId,
                TeamName = member.Team?.Name,
                DepartmentId = member.Team?.DepartmentId,
                DepartmentName = member.Team?.Department?.Name,
                IsLinked = true
            };
        }

        // 2. Sahip Olunan / Oluşturulan Projeler (Server CreatedByUserId)
        var ownedProjects = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Status)
            .Include(p => p.Category)
            .Where(p => p.CreatedByUserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProfileProjectItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                CoverImageUrl = p.CoverImageUrl,
                StatusCode = p.Status.Code,
                StatusName = p.Status.Name,
                CategoryCode = p.Category.Code,
                CategoryName = p.Category.Name,
                DevelopmentType = p.DevelopmentType.ToString(),
                IsPublished = p.IsPublished,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // 3. Katkı Sağlanan Projeler (ProjectMember Join İlişkisi)
        var contributedProjects = new List<ProfileProjectItemDto>();
        if (member != null)
        {
            contributedProjects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Status)
                .Include(p => p.Category)
                .Where(p => p.ProjectMembers.Any(pm => pm.MemberId == member.Id) && !p.IsDeleted)
                // Yetkilendirme: Yayınlanmamış olanları sadece Admin, SuperAdmin veya projenin sahibi görebilir
                .Where(p => p.IsPublished || isAdmin || p.CreatedByUserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProfileProjectItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    ShortDescription = p.ShortDescription,
                    CoverImageUrl = p.CoverImageUrl,
                    StatusCode = p.Status.Code,
                    StatusName = p.Status.Name,
                    CategoryCode = p.Category.Code,
                    CategoryName = p.Category.Name,
                    DevelopmentType = p.DevelopmentType.ToString(),
                    IsPublished = p.IsPublished,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        return new UserProfileResponseDto
        {
            User = userSummary,
            Member = memberSummary,
            OwnedProjects = ownedProjects,
            ContributedProjects = contributedProjects
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            throw new ArgumentException("Mevcut şifre zorunludur.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ArgumentException("Yeni şifre zorunludur.");

        if (request.NewPassword != request.ConfirmNewPassword)
            throw new ArgumentException("Yeni şifre ile şifre tekrarı eşleşmiyor.");

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            throw new ArgumentException($"Şifre değiştirilemedi: {errors}");
        }

        await _userManager.UpdateSecurityStampAsync(user);

        // Güvenlik gereği parolaları asla loglama!
        await _auditLogService.LogAsync(
            "UserPasswordChanged",
            "UserSecurity",
            user.Id.ToString(),
            user.Email,
            "Kullanıcı kendi parolasını başarıyla güncelledi.",
            cancellationToken: cancellationToken);
    }
}
