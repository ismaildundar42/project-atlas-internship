namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class TeamDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsPrimary { get; set; }
}
