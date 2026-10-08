using DeUygulamaVitrini.Application.Common.Constants;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Reports;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

/// <summary>
/// Proje portföyü raporlama ve içgörü metriklerini üreten uygulama servisi.
/// Yalnızca yayınlanmış (IsPublished = true) ve silinmemiş (IsDeleted = false) projeleri dikkate alır.
/// Performans için AsNoTracking ve SQL-side aggregation kullanır.
/// </summary>
public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    private static readonly string[] TurkishMonths =
    [
        "", "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"
    ];

    private static readonly Dictionary<DevelopmentType, string> DevTypeLabels = new()
    {
        [DevelopmentType.Internal] = "İç Geliştirme",
        [DevelopmentType.External] = "Dış Kaynak",
        [DevelopmentType.Hybrid] = "Karma"
    };

    public ReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportOverviewDto> GetPortfolioOverviewAsync(CancellationToken cancellationToken = default)
    {
        // Temel yayınlanmış ve silinmemiş projeler sorgusu
        var publishedQuery = _context.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished && !p.IsDeleted);

        // 1. KPI Metrikleri
        var totalProjects = await publishedQuery.CountAsync(cancellationToken);

        var activeProjects = await publishedQuery
            .CountAsync(p => p.Status.Code == ProjectStatusConstants.Active, cancellationToken);

        var inProgressProjects = await publishedQuery
            .CountAsync(p => ProjectStatusConstants.InProgressStatusCodes.Contains(p.Status.Code), cancellationToken);

        var completedProjects = await publishedQuery
            .CountAsync(p => p.Status.Code == ProjectStatusConstants.Completed, cancellationToken);

        var featuredProjects = await publishedQuery
            .CountAsync(p => p.IsFeatured, cancellationToken);

        var summary = new ReportSummaryDto
        {
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            InProgressProjects = inProgressProjects,
            CompletedProjects = completedProjects,
            FeaturedProjects = featuredProjects
        };

        // 2. Durum Dağılımı (SQL GroupBy)
        var statusGroups = await publishedQuery
            .GroupBy(p => new { p.Status.Id, p.Status.Name, p.Status.Code, p.Status.DisplayOrder })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                g.Key.Code,
                g.Key.DisplayOrder,
                Count = g.Count()
            })
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Id)
            .ToListAsync(cancellationToken);

        var statusDistribution = statusGroups.Select(s => new ReportStatusDistributionDto
        {
            StatusId = s.Id,
            Name = s.Name,
            Code = s.Code,
            Count = s.Count,
            Percentage = totalProjects > 0 ? Math.Round((double)s.Count / totalProjects * 100, 1) : 0
        }).ToList();

        // 3. Kategori Dağılımı (SQL GroupBy)
        var categoryGroups = await publishedQuery
            .GroupBy(p => new { p.Category.Id, p.Category.Name, p.Category.Code, p.Category.DisplayOrder })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                g.Key.Code,
                g.Key.DisplayOrder,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.DisplayOrder)
            .ToListAsync(cancellationToken);

        var categoryDistribution = categoryGroups.Select(c => new ReportCategoryDistributionDto
        {
            CategoryId = c.Id,
            Name = c.Name,
            Code = c.Code,
            Count = c.Count,
            Percentage = totalProjects > 0 ? Math.Round((double)c.Count / totalProjects * 100, 1) : 0
        }).ToList();

        // 4. Geliştirme Modeli Dağılımı (SQL GroupBy)
        var devTypeGroups = await publishedQuery
            .GroupBy(p => p.DevelopmentType)
            .Select(g => new
            {
                DevelopmentType = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ToListAsync(cancellationToken);

        var devTypeDistribution = devTypeGroups.Select(d => new ReportDevelopmentTypeDistributionDto
        {
            DevelopmentType = d.DevelopmentType,
            Name = DevTypeLabels.TryGetValue(d.DevelopmentType, out var label) ? label : d.DevelopmentType.ToString(),
            Count = d.Count,
            Percentage = totalProjects > 0 ? Math.Round((double)d.Count / totalProjects * 100, 1) : 0
        }).ToList();

        // 5. En Çok Kullanılan Teknolojiler (SQL GroupBy - Top 10)
        var topTechnologies = await publishedQuery
            .SelectMany(p => p.ProjectTechnologies)
            .GroupBy(pt => new { pt.Technology.Id, pt.Technology.Name, pt.Technology.Category })
            .Select(g => new ReportTechnologyDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Category = g.Key.Category.ToString(),
                ProjectCount = g.Count()
            })
            .OrderByDescending(g => g.ProjectCount)
            .ThenBy(g => g.Name)
            .Take(10)
            .ToListAsync(cancellationToken);

        // 6. Proje Lokasyonları Dağılımı (SQL GroupBy)
        var locationDistribution = await publishedQuery
            .SelectMany(p => p.ProjectLocations)
            .GroupBy(pl => new { pl.Location.Id, pl.Location.Name, pl.Location.LocationType })
            .Select(g => new ReportLocationDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Type = g.Key.LocationType.ToString(),
                ProjectCount = g.Count()
            })
            .OrderByDescending(g => g.ProjectCount)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        // 7. Ekiplerin Proje Katılımı (SQL GroupBy - Performans değerlendirmesi içermez)
        var teamData = await publishedQuery
            .SelectMany(p => p.ProjectTeams)
            .GroupBy(pt => new { pt.Team.Id, pt.Team.Name, DepartmentName = pt.Team.Department.Name })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                g.Key.DepartmentName,
                ProjectCount = g.Count(),
                PrimaryProjectCount = g.Count(pt => pt.IsPrimary)
            })
            .OrderByDescending(g => g.ProjectCount)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        var teamDistribution = teamData.Select(t => new ReportTeamDto
        {
            Id = t.Id,
            Name = t.Name,
            DepartmentName = t.DepartmentName,
            ProjectCount = t.ProjectCount,
            PrimaryProjectCount = t.PrimaryProjectCount
        }).ToList();

        // 8. Proje Zaman Çizgisi (Aylık Başlangıç Dağılımı)
        var timelineRaw = await publishedQuery
            .Where(p => p.StartDate != null)
            .GroupBy(p => new { p.StartDate!.Value.Year, p.StartDate!.Value.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        var timeline = timelineRaw.Select(t => new ReportTimelineDto
        {
            Year = t.Year,
            Month = t.Month,
            PeriodLabel = t.Month >= 1 && t.Month <= 12 ? $"{TurkishMonths[t.Month]} {t.Year}" : $"{t.Month}/{t.Year}",
            ProjectCount = t.Count
        }).ToList();

        return new ReportOverviewDto
        {
            Summary = summary,
            StatusDistribution = statusDistribution,
            CategoryDistribution = categoryDistribution,
            DevelopmentTypeDistribution = devTypeDistribution,
            TopTechnologies = topTechnologies,
            LocationDistribution = locationDistribution,
            TeamDistribution = teamDistribution,
            ProjectTimeline = timeline
        };
    }
}
