using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projede kullanılan bir teknolojiyi temsil eder.
/// Aynı teknoloji birden fazla projede kullanılabilir;
/// bu sayede string tekrarı önlenir ve filtreleme/analiz kolaylaşır.
///
/// Örnekler: React (Frontend), Python (Backend/AI), Azure (Cloud)
/// </summary>
public class Technology : BaseEntity
{
    public required string Name { get; set; }

    /// <summary>Teknoloji kategorisi. (ör. Frontend, Backend, Cloud)</summary>
    public TechnologyCategory Category { get; set; }

    // Navigation
    public ICollection<ProjectTechnology> ProjectTechnologies { get; set; } = new List<ProjectTechnology>();
}
