using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projenin yaşam döngüsündeki aşamasını tanımlayan referans veri tablosu.
/// Seed data ile önceden doldurulur; uygulama içinden yönetilebilir.
///
/// Örnekler: Planning, Active Development, Active, Completed, Archived
/// </summary>
public class ProjectStatus : BaseEntity
{
    /// <summary>Görüntülenen kullanıcı dostu durum adı. (ör. "Aktif Geliştirme")</summary>
    public required string Name { get; set; }

    /// <summary>Programatik kullanım için sabit kod. (ör. "ACTIVE_DEVELOPMENT")</summary>
    public required string Code { get; set; }

    /// <summary>Durumun ne anlama geldiğini açıklayan kısa metin.</summary>
    public string? Description { get; set; }

    /// <summary>Arayüzdeki listeleme sırası.</summary>
    public int DisplayOrder { get; set; }

    // Navigation
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
