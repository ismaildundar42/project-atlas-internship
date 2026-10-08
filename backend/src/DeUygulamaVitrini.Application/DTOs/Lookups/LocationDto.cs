namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class LocationDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string LocationType { get; set; }
}
