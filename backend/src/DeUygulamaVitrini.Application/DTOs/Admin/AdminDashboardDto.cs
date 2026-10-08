namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class AdminDashboardDto
{
    public int TotalProjects { get; set; }
    public int PublishedProjects { get; set; }
    public int DraftProjects { get; set; }
    public int FeaturedProjects { get; set; }

    public AdminContentAttentionDto ContentAttention { get; set; } = new();
    public List<AdminRecentProjectDto> RecentProjects { get; set; } = new();
}

public class AdminContentAttentionDto
{
    public int DraftProjectsCount { get; set; }
    public int MissingDescriptionCount { get; set; }
    public int MissingTeamCount { get; set; }
    public int MissingLocationCount { get; set; }
}

public class AdminRecentProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public string? PrimaryTeamName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
