using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class CreateLocationRequestDto
{
    public required string Name { get; set; }
    public LocationType LocationType { get; set; } = LocationType.MineSite;
    public string? Description { get; set; }
}
