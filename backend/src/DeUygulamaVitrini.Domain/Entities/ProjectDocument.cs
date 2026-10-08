
using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Projeye ait teknik doküman, kullanıcı kılavuzu veya rapor kaydı.
///
/// Binary dosya SQL Server içinde SAKLANMAZ.
/// Yalnızca metadata ve dosya yolu/URL bilgisi tutulur.
/// </summary>
public class ProjectDocument : BaseEntity
{
    public int ProjectId { get; set; }

    /// <summary>Dokümanın görüntülenen adı. (ör. "Teknik Mimari Dokümanı")</summary>
    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>Orijinal dosya adı. (ör. "architecture-v2.pdf")</summary>
    public required string FileName { get; set; }

    /// <summary>Dosyanın erişim URL'si veya depolama yolu.</summary>
    public required string FileUrl { get; set; }

    /// <summary>
    /// Doküman türü serbest metin olarak tutulmuştur.
    /// Örnekler: "PDF", "Technical Spec", "User Manual", "Report"
    /// Bu alanda enum yerine serbest metin tercih edilmiştir çünkü
    /// doküman türleri öngörülemeyen çeşitliliktedir.
    /// </summary>
    public string? DocumentType { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
}
