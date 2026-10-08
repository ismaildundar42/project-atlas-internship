using DeUygulamaVitrini.Domain.Enums;

namespace DeUygulamaVitrini.Application.DTOs.Reports;

/// <summary>
/// Proje durumu dağılım öğesi.
/// </summary>
public class ReportStatusDistributionDto
{
    public int StatusId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Proje kategorisi dağılım öğesi.
/// </summary>
public class ReportCategoryDistributionDto
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Geliştirme modeli dağılım öğesi.
/// </summary>
public class ReportDevelopmentTypeDistributionDto
{
    public DevelopmentType DevelopmentType { get; set; }
    public required string Name { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}
