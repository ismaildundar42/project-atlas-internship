namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Tüm bilgi indeksinin yeniden oluşturulma işleminin sonucunu temsil eden DTO.
/// </summary>
public class IndexRebuildResultDto
{
    public bool Success { get; set; }
    public int ProjectsProcessed { get; set; }
    public int ChunksCreatedOrUpdated { get; set; }
    public int ChunksUnchanged { get; set; }
    public int ChunksDeleted { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tek bir projenin indeksleme sonucunu temsil eden DTO.
/// </summary>
public class IndexProjectResultDto
{
    public bool Success { get; set; }
    public int ProjectId { get; set; }
    public int ChunksIndexed { get; set; }
    public int ChunksUnchanged { get; set; }
    public string? ErrorMessage { get; set; }
}
