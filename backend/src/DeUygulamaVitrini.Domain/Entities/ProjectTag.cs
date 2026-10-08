namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Project ↔ Tag many-to-many ilişkisi için join entity.
/// </summary>
public class ProjectTag
{
    public int ProjectId { get; set; }
    public int TagId { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
