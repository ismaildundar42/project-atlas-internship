using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.ModuleAccess;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeUygulamaVitrini.Infrastructure.Services;

/// <summary>
/// Modül erişim kontrolü ve erişim talepleri iş akışı servisi implementasyonu.
/// </summary>
public class ModuleAccessService : IModuleAccessService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ModuleAccessService> _logger;

    public ModuleAccessService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        ILogger<ModuleAccessService> logger)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _logger = logger;
    }

    private static string GetModuleDisplayName(ApplicationModule module) => module switch
    {
        ApplicationModule.Reports => "Raporlama",
        ApplicationModule.Teams => "Ekipler",
        _ => module.ToString()
    };

    private static string GetModuleKey(ApplicationModule module) => module switch
    {
        ApplicationModule.Reports => "reports",
        ApplicationModule.Teams => "teams",
        _ => module.ToString().ToLowerInvariant()
    };

    private static string GetStatusDisplayName(AccessRequestStatus status) => status switch
    {
        AccessRequestStatus.Pending => "İnceleme Bekliyor",
        AccessRequestStatus.Approved => "Onaylandı",
        AccessRequestStatus.Rejected => "Reddedildi",
        _ => status.ToString()
    };

    private async Task<string> ResolveUserDisplayNameAsync(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0) return "Kullanıcı";

        var member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        if (member != null && (!string.IsNullOrWhiteSpace(member.FirstName) || !string.IsNullOrWhiteSpace(member.LastName)))
        {
            return $"{member.FirstName} {member.LastName}".Trim();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
            return user.UserName ?? user.Email ?? "Kullanıcı";
        }

        return "Kullanıcı";
    }

    public async Task<bool> CanAccessModuleAsync(int userId, bool isAdmin, ApplicationModule module, CancellationToken cancellationToken = default)
    {
        // 1. Admin veya SuperAdmin tüm modüllere örtük olarak tam erişebilir
        if (isAdmin) return true;

        if (userId <= 0) return false;

        // 2. Kullanıcının aktifliği doğrulanır
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive) return false;

        // 3. Modül izin kaydı kontrol edilir
        return await _context.UserModulePermissions
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.Module == module, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, ModuleAccessStatusDto>> GetUserModuleAccessSummaryAsync(
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, ModuleAccessStatusDto>(StringComparer.OrdinalIgnoreCase);

        var modules = new[] { ApplicationModule.Reports, ApplicationModule.Teams };

        List<UserModulePermission> permissions = new();
        List<ModuleAccessRequest> userRequests = new();

        if (userId > 0)
        {
            permissions = await _context.UserModulePermissions
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .ToListAsync(cancellationToken);

            userRequests = await _context.ModuleAccessRequests
                .AsNoTracking()
                .Where(r => r.RequestedByUserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        foreach (var mod in modules)
        {
            var key = GetModuleKey(mod);
            var hasAccess = isAdmin || permissions.Any(p => p.Module == mod);

            var latestRequest = userRequests.FirstOrDefault(r => r.Module == mod);

            result[key] = new ModuleAccessStatusDto
            {
                Module = mod,
                ModuleKey = key,
                ModuleName = GetModuleDisplayName(mod),
                HasAccess = hasAccess,
                IsDefaultAccess = false,
                RequestStatus = latestRequest?.Status,
                PendingRequestId = latestRequest?.Status == AccessRequestStatus.Pending ? latestRequest.Id : null,
                RequestedAt = latestRequest?.RequestedAt
            };
        }

        return result;
    }

    public async Task<ModuleAccessRequestDto> CreateAccessRequestAsync(
        int userId,
        ApplicationModule module,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new UnauthorizedAccessException("Geçerli bir oturum gereklidir.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Hesabınız aktif değil.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(Domain.Constants.AppRoles.Admin) || roles.Contains(Domain.Constants.AppRoles.SuperAdmin);

        var hasAccess = await CanAccessModuleAsync(userId, isAdmin, module, cancellationToken);
        if (hasAccess)
        {
            throw new InvalidOperationException("Bu modüle zaten erişim yetkiniz bulunmaktadır.");
        }

        // Bekleyen mükerrer talep kontrolü
        var existingPending = await _context.ModuleAccessRequests
            .FirstOrDefaultAsync(r => r.RequestedByUserId == userId && r.Module == module && r.Status == AccessRequestStatus.Pending, cancellationToken);

        if (existingPending != null)
        {
            throw new InvalidOperationException("Bu modül için zaten inceleme bekleyen bir erişim talebiniz bulunmaktadır.");
        }

        var request = new ModuleAccessRequest
        {
            RequestedByUserId = userId,
            Module = module,
            Reason = reason?.Trim(),
            Status = AccessRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _context.ModuleAccessRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        var requesterName = await ResolveUserDisplayNameAsync(userId, cancellationToken);
        var moduleNameTr = GetModuleDisplayName(module);

        // Audit Log
        await _auditLogService.LogWithActorAsync(
            userId,
            "ModuleAccessRequested",
            "ModuleAccessRequest",
            request.Id.ToString(),
            moduleNameTr,
            $"'{requesterName}' kullanıcısı '{moduleNameTr}' modülü için erişim talebinde bulundu.",
            cancellationToken: cancellationToken);

        // Notify Admins
        var notifMessage = string.IsNullOrWhiteSpace(reason)
            ? $"{requesterName}, '{moduleNameTr}' alanına erişim talebinde bulundu."
            : $"{requesterName}, '{moduleNameTr}' alanına erişim talebinde bulundu. Gerekçe: \"{reason.Trim()}\"";

        await _notificationService.CreateNotificationsForAdminsAsync(
            "ModuleAccessRequested",
            "Yeni Modül Erişim Talebi",
            notifMessage,
            null,
            "/admin/access-requests",
            cancellationToken);

        return new ModuleAccessRequestDto
        {
            Id = request.Id,
            RequestedByUserId = userId,
            RequesterName = requesterName,
            RequesterEmail = user.Email ?? string.Empty,
            Module = module,
            ModuleKey = GetModuleKey(module),
            ModuleName = moduleNameTr,
            Reason = request.Reason,
            Status = request.Status,
            StatusName = GetStatusDisplayName(request.Status),
            RequestedAt = request.RequestedAt
        };
    }

    public async Task<PagedResult<ModuleAccessRequestDto>> GetAccessRequestsAsync(
        AccessRequestQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ModuleAccessRequests
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .AsQueryable();

        // Status filtresi
        if (!string.IsNullOrWhiteSpace(queryParams.Status) && !string.Equals(queryParams.Status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<AccessRequestStatus>(queryParams.Status, true, out var status))
            {
                query = query.Where(r => r.Status == status);
            }
        }

        // Module filtresi
        if (!string.IsNullOrWhiteSpace(queryParams.Module) && !string.Equals(queryParams.Module, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<ApplicationModule>(queryParams.Module, true, out var mod))
            {
                query = query.Where(r => r.Module == mod);
            }
        }

        // Metin arama (kullanıcı adı, e-posta veya gerekçe)
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var s = queryParams.Search.Trim().ToLower();
            query = query.Where(r =>
                (r.RequestedByUser.FirstName + " " + r.RequestedByUser.LastName).ToLower().Contains(s) ||
                (r.RequestedByUser.Email != null && r.RequestedByUser.Email.ToLower().Contains(s)) ||
                (r.Reason != null && r.Reason.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(r => new ModuleAccessRequestDto
            {
                Id = r.Id,
                RequestedByUserId = r.RequestedByUserId,
                RequesterName = r.RequestedByUser != null ? (r.RequestedByUser.FirstName + " " + r.RequestedByUser.LastName).Trim() : "Kullanıcı",
                RequesterEmail = r.RequestedByUser != null ? (r.RequestedByUser.Email ?? string.Empty) : string.Empty,
                Module = r.Module,
                ModuleKey = r.Module == ApplicationModule.Reports ? "reports" : "teams",
                ModuleName = r.Module == ApplicationModule.Reports ? "Raporlama" : "Ekipler",
                Reason = r.Reason,
                Status = r.Status,
                StatusName = r.Status == AccessRequestStatus.Pending ? "İnceleme Bekliyor" :
                             r.Status == AccessRequestStatus.Approved ? "Onaylandı" : "Reddedildi",
                RequestedAt = r.RequestedAt,
                ReviewedByUserId = r.ReviewedByUserId,
                ReviewerName = r.ReviewedByUser != null ? (r.ReviewedByUser.FirstName + " " + r.ReviewedByUser.LastName).Trim() : null,
                ReviewedAt = r.ReviewedAt,
                ReviewNote = r.ReviewNote
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ModuleAccessRequestDto>(items, queryParams.PageNumber, queryParams.PageSize, totalCount);
    }

    public async Task<int> GetPendingRequestCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ModuleAccessRequests
            .AsNoTracking()
            .CountAsync(r => r.Status == AccessRequestStatus.Pending, cancellationToken);
    }

    public async Task<ModuleAccessRequestDto> ApproveAccessRequestAsync(
        int requestId,
        int reviewerAdminUserId,
        string? reviewNote,
        CancellationToken cancellationToken = default)
    {
        var request = await _context.ModuleAccessRequests
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null)
        {
            throw new KeyNotFoundException($"Id={requestId} olan erişim talebi bulunamadı.");
        }

        if (request.Status != AccessRequestStatus.Pending)
        {
            throw new InvalidOperationException("Bu talep zaten sonuçlandırılmıştır.");
        }

        request.Status = AccessRequestStatus.Approved;
        request.ReviewedByUserId = reviewerAdminUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNote = reviewNote?.Trim();

        // UserModulePermission var mı kontrol et, yoksa oluştur
        var existingPerm = await _context.UserModulePermissions
            .FirstOrDefaultAsync(p => p.UserId == request.RequestedByUserId && p.Module == request.Module, cancellationToken);

        if (existingPerm == null)
        {
            _context.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = request.RequestedByUserId,
                Module = request.Module,
                GrantedByUserId = reviewerAdminUserId,
                GrantedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var requesterName = request.RequestedByUser != null
            ? $"{request.RequestedByUser.FirstName} {request.RequestedByUser.LastName}".Trim()
            : "Kullanıcı";
        var reviewerName = await ResolveUserDisplayNameAsync(reviewerAdminUserId, cancellationToken);
        var moduleNameTr = GetModuleDisplayName(request.Module);

        // Audit Log
        await _auditLogService.LogWithActorAsync(
            reviewerAdminUserId,
            "ModuleAccessApproved",
            "ModuleAccessRequest",
            request.Id.ToString(),
            moduleNameTr,
            $"'{requesterName}' kullanıcısının '{moduleNameTr}' erişim talebi '{reviewerName}' tarafından onaylandı.",
            cancellationToken: cancellationToken);

        // Bildirim
        var targetUrl = request.Module == ApplicationModule.Reports ? "/reports" : "/teams";
        await _notificationService.CreateNotificationAsync(
            request.RequestedByUserId,
            "ModuleAccessApproved",
            "Erişim Talebiniz Onaylandı",
            $"'{moduleNameTr}' modülüne erişim talebiniz onaylandı ve kullanıma açıldı.",
            null,
            targetUrl,
            cancellationToken);

        return new ModuleAccessRequestDto
        {
            Id = request.Id,
            RequestedByUserId = request.RequestedByUserId,
            RequesterName = requesterName,
            RequesterEmail = request.RequestedByUser?.Email ?? string.Empty,
            Module = request.Module,
            ModuleKey = GetModuleKey(request.Module),
            ModuleName = moduleNameTr,
            Reason = request.Reason,
            Status = request.Status,
            StatusName = GetStatusDisplayName(request.Status),
            RequestedAt = request.RequestedAt,
            ReviewedByUserId = reviewerAdminUserId,
            ReviewerName = reviewerName,
            ReviewedAt = request.ReviewedAt,
            ReviewNote = request.ReviewNote
        };
    }

    public async Task<ModuleAccessRequestDto> RejectAccessRequestAsync(
        int requestId,
        int reviewerAdminUserId,
        string? reviewNote,
        CancellationToken cancellationToken = default)
    {
        var request = await _context.ModuleAccessRequests
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null)
        {
            throw new KeyNotFoundException($"Id={requestId} olan erişim talebi bulunamadı.");
        }

        if (request.Status != AccessRequestStatus.Pending)
        {
            throw new InvalidOperationException("Bu talep zaten sonuçlandırılmıştır.");
        }

        request.Status = AccessRequestStatus.Rejected;
        request.ReviewedByUserId = reviewerAdminUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNote = reviewNote?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        var requesterName = request.RequestedByUser != null
            ? $"{request.RequestedByUser.FirstName} {request.RequestedByUser.LastName}".Trim()
            : "Kullanıcı";
        var reviewerName = await ResolveUserDisplayNameAsync(reviewerAdminUserId, cancellationToken);
        var moduleNameTr = GetModuleDisplayName(request.Module);

        // Audit Log
        await _auditLogService.LogWithActorAsync(
            reviewerAdminUserId,
            "ModuleAccessRejected",
            "ModuleAccessRequest",
            request.Id.ToString(),
            moduleNameTr,
            $"'{requesterName}' kullanıcısının '{moduleNameTr}' erişim talebi '{reviewerName}' tarafından reddedildi. Not: {reviewNote?.Trim()}",
            cancellationToken: cancellationToken);

        // Bildirim
        var rejectMsg = string.IsNullOrWhiteSpace(reviewNote)
            ? $"'{moduleNameTr}' modülüne erişim talebiniz yönetici tarafından reddedildi."
            : $"'{moduleNameTr}' modülüne erişim talebiniz reddedildi. Yönetici notu: \"{reviewNote.Trim()}\"";

        await _notificationService.CreateNotificationAsync(
            request.RequestedByUserId,
            "ModuleAccessRejected",
            "Erişim Talebiniz Reddedildi",
            rejectMsg,
            null,
            null,
            cancellationToken);

        return new ModuleAccessRequestDto
        {
            Id = request.Id,
            RequestedByUserId = request.RequestedByUserId,
            RequesterName = requesterName,
            RequesterEmail = request.RequestedByUser?.Email ?? string.Empty,
            Module = request.Module,
            ModuleKey = GetModuleKey(request.Module),
            ModuleName = moduleNameTr,
            Reason = request.Reason,
            Status = request.Status,
            StatusName = GetStatusDisplayName(request.Status),
            RequestedAt = request.RequestedAt,
            ReviewedByUserId = reviewerAdminUserId,
            ReviewerName = reviewerName,
            ReviewedAt = request.ReviewedAt,
            ReviewNote = request.ReviewNote
        };
    }

    public async Task RevokeModuleAccessAsync(
        int targetUserId,
        ApplicationModule module,
        int actorAdminUserId,
        CancellationToken cancellationToken = default)
    {
        var permission = await _context.UserModulePermissions
            .FirstOrDefaultAsync(p => p.UserId == targetUserId && p.Module == module, cancellationToken);

        if (permission != null)
        {
            _context.UserModulePermissions.Remove(permission);
        }

        var actorName = await ResolveUserDisplayNameAsync(actorAdminUserId, cancellationToken);

        // İlgili modül için önceden onaylanmış talepleri de 'Reddedildi / Kaldırıldı' durumuna çek
        var approvedRequests = await _context.ModuleAccessRequests
            .Where(r => r.RequestedByUserId == targetUserId && r.Module == module && r.Status == AccessRequestStatus.Approved)
            .ToListAsync(cancellationToken);

        foreach (var req in approvedRequests)
        {
            req.Status = AccessRequestStatus.Rejected;
            req.ReviewedByUserId = actorAdminUserId;
            req.ReviewedAt = DateTime.UtcNow;
            req.ReviewNote = $"Erişim yetkisi {actorName} tarafından kaldırıldı.";
        }

        await _context.SaveChangesAsync(cancellationToken);

        var targetUserName = await ResolveUserDisplayNameAsync(targetUserId, cancellationToken);
        var moduleNameTr = GetModuleDisplayName(module);

        // Audit Log
        await _auditLogService.LogWithActorAsync(
            actorAdminUserId,
            "ModuleAccessRevoked",
            "UserModulePermission",
            targetUserId.ToString(),
            moduleNameTr,
            $"'{targetUserName}' kullanıcısının '{moduleNameTr}' erişim yetkisi '{actorName}' tarafından kaldırıldı.",
            cancellationToken: cancellationToken);

        // Bildirim
        await _notificationService.CreateNotificationAsync(
            targetUserId,
            "ModuleAccessRevoked",
            "Modül Erişim Yetkisi Kaldırıldı",
            $"'{moduleNameTr}' modülü erişim yetkiniz yönetici tarafından sonlandırıldı.",
            null,
            null,
            cancellationToken);
    }

    public async Task GrantDirectModuleAccessAsync(
        int targetUserId,
        ApplicationModule module,
        int actorAdminUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException("Kullanıcı bulunamadı veya hesabı aktif değil.");
        }

        var existing = await _context.UserModulePermissions
            .FirstOrDefaultAsync(p => p.UserId == targetUserId && p.Module == module, cancellationToken);

        if (existing == null)
        {
            _context.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = targetUserId,
                Module = module,
                GrantedByUserId = actorAdminUserId,
                GrantedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            var targetUserName = await ResolveUserDisplayNameAsync(targetUserId, cancellationToken);
            var moduleNameTr = GetModuleDisplayName(module);
            var actorName = await ResolveUserDisplayNameAsync(actorAdminUserId, cancellationToken);

            // Audit Log
            await _auditLogService.LogWithActorAsync(
                actorAdminUserId,
                "ModuleAccessGranted",
                "UserModulePermission",
                targetUserId.ToString(),
                moduleNameTr,
                $"'{targetUserName}' kullanıcısına '{moduleNameTr}' erişim yetkisi doğrudan tanımlandı (İşlemi yapan: {actorName}).",
                cancellationToken: cancellationToken);

            // Bildirim
            var targetUrl = module == ApplicationModule.Reports ? "/reports" : "/teams";
            await _notificationService.CreateNotificationAsync(
                targetUserId,
                "ModuleAccessGranted",
                "Yeni Modül Yetkisi Tanımlandı",
                $"'{moduleNameTr}' modülü erişim yetkisi hesabınıza tanımlandı.",
                null,
                targetUrl,
                cancellationToken);
        }
    }
}
