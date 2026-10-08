using System.ComponentModel.DataAnnotations;

namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Anlamsal (vektörel) arama sorgusu DTO'su.
/// </summary>
public class SemanticSearchQueryDto
{
    [Required(ErrorMessage = "Arama sorgusu boş olamaz.")]
    [StringLength(500, MinimumLength = 2, ErrorMessage = "Arama sorgusu 2 ile 500 karakter arasında olmalıdır.")]
    public string Query { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "TopK değeri 1 ile 50 arasında olmalıdır.")]
    public int TopK { get; set; } = 5;

    [Range(0.0, 1.0, ErrorMessage = "MinSimilarity 0.0 ile 1.0 arasında olmalıdır.")]
    public float MinSimilarity { get; set; } = 0.50f;

    // ─── İsteğe Bağlı Yapılandırılmış Filtreler (Structured Metadata Filters) ──
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? Technology { get; set; }
    public string? Location { get; set; }
    public string? DevelopmentType { get; set; }
}

/// <summary>
/// Anlamsal arama sonuç öğesi DTO'su.
/// </summary>
public class SemanticSearchResultDto
{
    public int ProjectId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string ChunkKey { get; set; } = string.Empty;
    public string MatchedSnippet { get; set; } = string.Empty;
    public float SimilarityScore { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string DevelopmentType { get; set; } = string.Empty;
    public IReadOnlyList<string> Technologies { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Locations { get; set; } = Array.Empty<string>();
}
