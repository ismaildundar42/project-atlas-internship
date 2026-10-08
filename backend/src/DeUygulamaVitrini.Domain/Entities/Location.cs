using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projenin kullanıldığı coğrafi veya fiziksel konumu temsil eder.
/// Birden fazla proje aynı lokasyonu paylaşabilir.
///
/// Örnekler: Gediktepe Maden Sahası, Genel Müdürlük Ofisi
/// </summary>
public class Location : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>Lokasyon türü (MineSite, Office, Facility, Other).</summary>
    public LocationType LocationType { get; set; }

    // Navigation
    public ICollection<ProjectLocation> ProjectLocations { get; set; } = new List<ProjectLocation>();
}
