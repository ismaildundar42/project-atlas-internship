using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projenin ait olduğu teknik/iş kategorisini tanımlayan referans veri tablosu.
/// Seed data ile önceden doldurulur.
///
/// Örnekler: Software, AI, IoT, Automation, Mining Technology
/// </summary>
public class ProjectCategory : BaseEntity
{
    /// <summary>Kategori adı. (ör. "Yapay Zeka")</summary>
    public required string Name { get; set; }

    /// <summary>Programatik kod. (ör. "ARTIFICIAL_INTELLIGENCE")</summary>
    public required string Code { get; set; }

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    // Navigation
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
