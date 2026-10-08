namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Project ↔ Technology many-to-many ilişkisi için join entity.
/// </summary>
public class ProjectTechnology
{
    public int ProjectId { get; set; }
    public int TechnologyId { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Technology Technology { get; set; } = null!;
}
