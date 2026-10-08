using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Application.DTOs.ModuleAccess;

/// <summary>
/// Kullanıcının belirli bir modüle erişim durumunu ve bekleyen talep bilgisini özetler.
/// </summary>
public class ModuleAccessStatusDto
{
    public ApplicationModule Module { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public bool HasAccess { get; set; }
    public bool IsDefaultAccess { get; set; }
    public AccessRequestStatus? RequestStatus { get; set; }
    public int? PendingRequestId { get; set; }
    public DateTime? RequestedAt { get; set; }
}

/// <summary>
/// Yeni bir modül erişim talebi oluşturma DTO'su.
/// </summary>
public class CreateModuleAccessRequestDto
{
    public ApplicationModule Module { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Erişim talebi detay DTO'su.
/// </summary>
public class ModuleAccessRequestDto
{
    public int Id { get; set; }
    public int RequestedByUserId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string? RequesterTitle { get; set; }
    public string? RequesterTeamName { get; set; }
    public ApplicationModule Module { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public AccessRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}

/// <summary>
/// Yönetici erişim talepleri listeleme parametreleri.
/// </summary>
public class AccessRequestQueryParams
{
    public string? Status { get; set; }
    public string? Module { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Erişim talebi inceleme DTO'su (Onay veya Ret notu).
/// </summary>
public class ReviewModuleAccessRequestDto
{
    public string? ReviewNote { get; set; }
}

/// <summary>
/// Doğrudan modül yetkisi atama DTO'su.
/// </summary>
public class GrantModuleAccessRequestDto
{
    public ApplicationModule Module { get; set; }
}
