namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class ProjectStatusDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
