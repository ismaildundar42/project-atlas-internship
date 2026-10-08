using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Organizasyon içindeki bir departmanı (bölümü) temsil eder.
/// Örnek: Ar-Ge, Operasyon, Bilgi Teknolojileri
/// </summary>
public class Department : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
