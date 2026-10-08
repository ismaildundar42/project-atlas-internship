namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Arka plan anlamsal indeks eşitleme (reconciliation) döngüsü sonuç DTO'su.
/// </summary>
public class ReconciliationResultDto
{
    public bool Success { get; set; }
    public bool ProviderUnavailable { get; set; }
    public int TotalEligibleProjects { get; set; }
    public int CurrentProjectsCount { get; set; }
    public int MissingProjectsCount { get; set; }
    public int StaleProjectsCount { get; set; }
    public int BatchSizeRequested { get; set; }
    public int ProjectsProcessedInBatch { get; set; }
    public int ChunksCreatedOrUpdated { get; set; }
    public int ChunksUnchanged { get; set; }
    public int ChunksDeleted { get; set; }
    public int FailedProjectsCount { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}
