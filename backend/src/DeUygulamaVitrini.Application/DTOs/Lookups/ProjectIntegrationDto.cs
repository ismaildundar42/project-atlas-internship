namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class ProjectIntegrationDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string IntegrationType { get; set; }
}
