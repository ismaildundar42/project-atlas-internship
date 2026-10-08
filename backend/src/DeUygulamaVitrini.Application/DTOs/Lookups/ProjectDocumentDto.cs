namespace DeUygulamaVitrini.Application.DTOs.Lookups;

public class ProjectDocumentDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string FileName { get; set; }
    public required string FileUrl { get; set; }
    public string? DocumentType { get; set; }
}
