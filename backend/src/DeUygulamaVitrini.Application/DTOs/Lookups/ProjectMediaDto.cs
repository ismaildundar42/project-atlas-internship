namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class ProjectMediaDto
{
    public int Id { get; set; }
    public required string MediaType { get; set; }
    public required string FileName { get; set; }
    public required string FileUrl { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}
