using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projeye ait görsel veya video medya kaydı.
///
/// Binary dosya SQL Server içinde SAKLANMAZ.
/// Bu tablo yalnızca metadata ve dosya yolu/URL bilgisini tutar.
/// Gerçek dosya storage çözümü (Azure Blob, yerel dosya sistemi vb.)
/// ileriki bir phase'te belirlenecektir.
/// </summary>
public class ProjectMedia : BaseEntity
{
    public int ProjectId { get; set; }

    /// <summary>Medya türü: Image veya Video.</summary>
    public MediaType MediaType { get; set; }

    /// <summary>Orijinal dosya adı. (ör. "drone-scan-overview.png")</summary>
    public required string FileName { get; set; }

    /// <summary>Dosyanın erişim URL'si veya depolama yolu.</summary>
    public required string FileUrl { get; set; }

    /// <summary>Görsel için screen reader / erişilebilirlik açıklaması.</summary>
    public string? AltText { get; set; }

    /// <summary>Galeride gösterilecek başlık/açıklama.</summary>
    public string? Caption { get; set; }

    /// <summary>Medya öğesinin galerindeki sıralama pozisyonu.</summary>
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Project Project { get; set; } = null!;
}
