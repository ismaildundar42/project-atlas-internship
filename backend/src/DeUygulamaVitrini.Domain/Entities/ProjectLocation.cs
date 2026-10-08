namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Project ↔ Location many-to-many ilişkisi için join entity.
/// </summary>
public class ProjectLocation
{
    public int ProjectId { get; set; }
    public int LocationId { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
