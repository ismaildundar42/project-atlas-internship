using DeUygulamaVitrini.Domain.Common;
using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Bir projenin dış sistemlerle entegrasyonunu açıklar.
/// Her entegrasyon kaydı bir projeye aittir (owned relationship).
///
/// NOT: Proje-proje referans (self-reference) bu entity'de desteklenmemektedir.
/// "Başka sistemlerle entegrasyon" sade bir açıklama kaydıdır.
/// </summary>
public class ProjectIntegration : BaseEntity
{
    public int ProjectId { get; set; }

    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>Entegrasyon türü. (ör. REST API, Database, Message Queue)</summary>
    public IntegrationType IntegrationType { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
}
