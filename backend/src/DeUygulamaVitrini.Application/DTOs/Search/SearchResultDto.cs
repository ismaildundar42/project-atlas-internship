namespace DeUygulamaVitrini.Application.DTOs.Search;

/// <summary>
/// Genel arama yanıt DTO'su.
/// </summary>
public class SearchResultDto
{
    public string Query { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public List<ProjectSearchResultDto> Items { get; set; } = new();
}

/// <summary>
/// Hızlı arama ve keşif dropdown'ı için optimize edilmiş kompakt proje arama sonucu DTO'su.
/// </summary>
public class ProjectSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? PrimaryTeamName { get; set; }
    public List<string> Technologies { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public List<string> Teams { get; set; } = new();

    /// <summary>
    /// Arama sorgusunun projede neden eşleştiğine dair kompakt bağlam/çip bilgisi (örn. "Teknoloji: React", "Lokasyon: Divriği").
    /// </summary>
    public string? MatchReason { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
