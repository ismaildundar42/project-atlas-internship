using System.ComponentModel.DataAnnotations;

namespace DeUygulamaVitrini.Application.DTOs.Profile;

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; set; }
    public bool CanCreateProjects { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsAdmin { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class MemberSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Title { get; set; }
    public string? Email { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsLinked { get; set; }
}

public class ProfileProjectItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? CoverImageUrl { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string DevelopmentType { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserProfileResponseDto
{
    public UserSummaryDto User { get; set; } = null!;
    public MemberSummaryDto? Member { get; set; }
    public IReadOnlyList<ProfileProjectItemDto> OwnedProjects { get; set; } = new List<ProfileProjectItemDto>();
    public IReadOnlyList<ProfileProjectItemDto> ContributedProjects { get; set; } = new List<ProfileProjectItemDto>();
}

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifreler eşleşmiyor.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
