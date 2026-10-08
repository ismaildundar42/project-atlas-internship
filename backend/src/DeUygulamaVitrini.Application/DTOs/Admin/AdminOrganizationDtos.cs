using System.ComponentModel.DataAnnotations;

namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class DepartmentAdminDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int TeamCount { get; set; }
}

public class CreateDepartmentRequestDto
{
    [Required(ErrorMessage = "Departman adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateDepartmentRequestDto : CreateDepartmentRequestDto
{
}

public class TeamAdminDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int ProjectCount { get; set; }
}

public class CreateTeamRequestDto
{
    [Required(ErrorMessage = "Ekip adı zorunludur.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Required(ErrorMessage = "Departman seçimi zorunludur.")]
    public int DepartmentId { get; set; }
}

public class UpdateTeamRequestDto : CreateTeamRequestDto
{
}

public class MemberAdminDto
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public int ProjectCount { get; set; }

    // ─── Organization Placement (Phase 19.7) ─────────────────────────────────
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    // ─── Identity Link & Access Info (Phase 13.2 & 19.7) ─────────────────────
    public bool HasApplicationAccount { get; set; }
    public int? ApplicationUserId { get; set; }
    public string? ApplicationUserEmail { get; set; }
    public bool? ApplicationUserIsActive { get; set; }
    public bool CanCreateProjects { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool HasMatchingUnlinkedAccount { get; set; }
    public int? MatchingUnlinkedUserId { get; set; }
}

public class CreateMemberRequestDto
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string LastName { get; set; } = string.Empty;

    public string? Title { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string? Email { get; set; }

    public int? TeamId { get; set; }
    public int? DepartmentId { get; set; }
}

public class UpdateMemberRequestDto : CreateMemberRequestDto
{
}

public class UpdateMemberProjectAccessRequestDto
{
    public bool CanCreateProjects { get; set; }
}

public class UpdateMemberAdminRoleRequestDto
{
    public bool IsAdmin { get; set; }
}

public class CreateMemberAccountRequestDto
{
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LinkMemberAccountRequestDto
{
    public int? UserId { get; set; }
}

