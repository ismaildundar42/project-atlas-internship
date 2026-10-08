namespace DeUygulamaVitrini.Domain.Constants;

/// <summary>
/// Sistemdeki kullanıcı rollerini tanımlayan sabitler.
/// Basitleştirilmiş modelde tek bir yönetimsel rol (Admin) bulunur.
/// Proje girişi yapabilen kullanıcılar ApplicationUser.CanCreateProjects bayrağı ile yönetilir.
/// </summary>
public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";

    public static readonly string[] AllRoles = { SuperAdmin, Admin };

    public static bool IsValidRole(string roleName)
    {
        return string.Equals(SuperAdmin, roleName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Admin, roleName, StringComparison.OrdinalIgnoreCase);
    }
}
