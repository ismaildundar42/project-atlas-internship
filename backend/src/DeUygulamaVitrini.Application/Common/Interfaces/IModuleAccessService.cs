using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.ModuleAccess;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Modül erişim kontrolü ve erişim talepleri iş akışı servisi arayüzü.
/// Kurallar:
/// 1. SuperAdmin ve Admin tüm modüllere örtük (implicit) olarak tam erişebilir.
/// 2. Dashboard ve Projects varsayılan erişim modülleridir; izin kaydı aranmaz.
/// 3. Normal kullanıcıların Reports ve Teams gibi kısıtlı modüllere erişimi için UserModulePermission kaydı zorunludur.
/// 4. Erişim talepleri Admin veya SuperAdmin tarafından incelenir, onay/ret akışı atomik ve denetlenebilir (audit) şekilde işletilir.
/// </summary>
public interface IModuleAccessService
{
    Task<bool> CanAccessModuleAsync(int userId, bool isAdmin, ApplicationModule module, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, ModuleAccessStatusDto>> GetUserModuleAccessSummaryAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default);

    Task<ModuleAccessRequestDto> CreateAccessRequestAsync(int userId, ApplicationModule module, string? reason, CancellationToken cancellationToken = default);

    Task<PagedResult<ModuleAccessRequestDto>> GetAccessRequestsAsync(AccessRequestQueryParams queryParams, CancellationToken cancellationToken = default);

    Task<int> GetPendingRequestCountAsync(CancellationToken cancellationToken = default);

    Task<ModuleAccessRequestDto> ApproveAccessRequestAsync(int requestId, int reviewerAdminUserId, string? reviewNote, CancellationToken cancellationToken = default);

    Task<ModuleAccessRequestDto> RejectAccessRequestAsync(int requestId, int reviewerAdminUserId, string? reviewNote, CancellationToken cancellationToken = default);

    Task RevokeModuleAccessAsync(int targetUserId, ApplicationModule module, int actorAdminUserId, CancellationToken cancellationToken = default);

    Task GrantDirectModuleAccessAsync(int targetUserId, ApplicationModule module, int actorAdminUserId, CancellationToken cancellationToken = default);
}
