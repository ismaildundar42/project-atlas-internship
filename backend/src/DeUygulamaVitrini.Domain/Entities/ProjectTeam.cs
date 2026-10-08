namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Project ↔ Team many-to-many ilişkisi için explicit join entity.
///
/// Explicit join entity tercih edilmiştir çünkü:
/// 1. IsPrimary gibi ek alan taşımaktadır.
/// 2. Gelecekte "atama tarihi" veya "rol" eklenebilir.
/// 3. Cascade delete: Proje veya ekip silindiğinde bu kayıt otomatik silinir (bkz. configuration).
/// </summary>
public class ProjectTeam
{
    public int ProjectId { get; set; }
    public int TeamId { get; set; }

    /// <summary>
    /// Bu ekip projenin birincil/sorumlu ekibi mi?
    /// Her proje için en fazla bir ekip primary olarak işaretlenebilir.
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    // Navigation
    public Project Project { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
