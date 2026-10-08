using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class CreateTechnologyRequestDto
{
    public required string Name { get; set; }
    public TechnologyCategory Category { get; set; } = TechnologyCategory.Other;
}
