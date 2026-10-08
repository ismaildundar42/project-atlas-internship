using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Bir proje ekibini temsil eder.
/// Her ekip bir departmana bağlıdır.
///
/// Örnek:
/// Ar-Ge Departmanı
///  ├── Yazılım Geliştirme Ekibi
///  ├── Veri Analitiği Ekibi
///  └── Otomasyon Ekibi
/// </summary>
public class Team : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Foreign key
    public int DepartmentId { get; set; }

    // Navigation
    public Department Department { get; set; } = null!;
    public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();
}
