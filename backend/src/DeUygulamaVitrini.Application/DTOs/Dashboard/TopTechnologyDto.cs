namespace DeUygulamaVitrini.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard en çok kullanılan teknoloji verisi.
/// </summary>
public class TopTechnologyDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required int ProjectCount { get; set; }
}
