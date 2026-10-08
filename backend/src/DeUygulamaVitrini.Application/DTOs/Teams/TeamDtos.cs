namespace DeUygulamaVitrini.Application.DTOs.Teams;

public class TeamsSummaryDto
{
    public int TotalTeams { get; set; }
    public int TotalDepartments { get; set; }
    public int ActiveTeamsInPublishedProjects { get; set; }
    public int TotalMembersInProjects { get; set; }
}

public class TeamListItemDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int PublishedProjectCount { get; set; }
    public int MemberCount { get; set; }
    public List<string> Technologies { get; set; } = new();
    public List<string> Locations { get; set; } = new();
}

public class TeamProjectDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string ShortDescription { get; set; }
    public required string CategoryName { get; set; }
    public required string StatusName { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPrimaryTeam { get; set; }
}

public class TeamMemberDto
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public List<string> ProjectRoles { get; set; } = new();
}

public class TeamDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int PublishedProjectCount { get; set; }
    public int MemberCount { get; set; }
    public int TechnologyCount { get; set; }
    public int LocationCount { get; set; }

    public List<TeamProjectDto> Projects { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public List<TeamMemberDto> Members { get; set; } = new();
}
