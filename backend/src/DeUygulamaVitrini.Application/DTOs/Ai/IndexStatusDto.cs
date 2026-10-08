namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Bilgi İndeksi durum ve istatistiklerini raporlayan DTO.
/// </summary>
public class IndexStatusDto
{
    public bool IsProviderOnline { get; set; }
    public string EmbeddingModel { get; set; } = string.Empty;
    public int EmbeddingDimension { get; set; }
    public int TotalEligibleProjects { get; set; }
    public int TotalIndexedProjects { get; set; }
    public int TotalChunks { get; set; }
    public int StaleOrMissingChunks { get; set; }
    public DateTime? LastRebuiltAtUtc { get; set; }
    public string? StatusMessage { get; set; }
}
