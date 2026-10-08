using DeUygulamaVitrini.Application.Common.Constants;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Dashboard;
using DeUygulamaVitrini.Application.DTOs.Lookups;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

/// <summary>
/// Dashboard metrikleri ve özet verilerini üreten servis.
/// Yalnızca yayınlanmış (IsPublished = true) projeleri dikkate alır.
/// SQL seviyesinde aggregation (AsNoTracking, GroupBy, Count) kullanır.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        // Temel yayınlanmış projeler sorgusu
        var publishedQuery = _context.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished);

        // 1. KPI Metrikleri
        var totalProjects = await publishedQuery.CountAsync(cancellationToken);

        var activeProjects = await publishedQuery
            .CountAsync(p => p.Status.Code == ProjectStatusConstants.Active, cancellationToken);

        var inProgressProjects = await publishedQuery
            .CountAsync(p => ProjectStatusConstants.InProgressStatusCodes.Contains(p.Status.Code), cancellationToken);

        var featuredProjects = await publishedQuery
            .CountAsync(p => p.IsFeatured, cancellationToken);

        // 2. Durum Dağılımı (GroupBy SQL aggregation)
        var statusDistribution = await publishedQuery
            .GroupBy(p => new { p.Status.Id, p.Status.Name, p.Status.Code, p.Status.DisplayOrder })
            .Select(g => new StatusDistributionDto
            {
                StatusId = g.Key.Id,
                Name = g.Key.Name,
                Code = g.Key.Code,
                Count = g.Count()
            })
            .OrderBy(g => g.StatusId)
            .ToListAsync(cancellationToken);

        // 3. Kategori Dağılımı (GroupBy SQL aggregation)
        var categoryDistribution = await publishedQuery
            .GroupBy(p => new { p.Category.Id, p.Category.Name, p.Category.Code, p.Category.DisplayOrder })
            .Select(g => new CategoryDistributionDto
            {
                CategoryId = g.Key.Id,
                Name = g.Key.Name,
                Code = g.Key.Code,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.CategoryId)
            .ToListAsync(cancellationToken);

        // 4. En Çok Kullanılan Teknolojiler (İlk 5)
        var topTechnologies = await publishedQuery
            .SelectMany(p => p.ProjectTechnologies)
            .GroupBy(pt => new { pt.Technology.Id, pt.Technology.Name, pt.Technology.Category })
            .Select(g => new TopTechnologyDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Category = g.Key.Category.ToString(),
                ProjectCount = g.Count()
            })
            .OrderByDescending(g => g.ProjectCount)
            .ThenBy(g => g.Name)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 5. Son Güncellenen Projeler (Son 5)
        var recentProjects = await publishedQuery
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(5)
            .Select(p => new RecentProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Status = new ProjectStatusDto
                {
                    Id = p.Status.Id,
                    Name = p.Status.Name,
                    Code = p.Status.Code,
                    Description = p.Status.Description,
                    DisplayOrder = p.Status.DisplayOrder
                },
                Category = new ProjectCategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Code = p.Category.Code,
                    Description = p.Category.Description,
                    DisplayOrder = p.Category.DisplayOrder
                },
                UpdatedAt = p.UpdatedAt ?? p.CreatedAt,
                CoverImageUrl = p.ProjectMediaItems
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.FileUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        // 6. Öne Çıkan Projeler (Son 4)
        var featuredProjectList = await publishedQuery
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(4)
            .Select(p => new FeaturedProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Status = new ProjectStatusDto
                {
                    Id = p.Status.Id,
                    Name = p.Status.Name,
                    Code = p.Status.Code,
                    Description = p.Status.Description,
                    DisplayOrder = p.Status.DisplayOrder
                },
                Category = new ProjectCategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Code = p.Category.Code,
                    Description = p.Category.Description,
                    DisplayOrder = p.Category.DisplayOrder
                },
                Technologies = p.ProjectTechnologies.Select(pt => pt.Technology.Name).ToList(),
                CoverImageUrl = p.ProjectMediaItems
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.FileUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            InProgressProjects = inProgressProjects,
            FeaturedProjectsCount = featuredProjects,
            StatusDistribution = statusDistribution,
            CategoryDistribution = categoryDistribution,
            TopTechnologies = topTechnologies,
            RecentProjects = recentProjects,
            FeaturedProjects = featuredProjectList
        };
    }
}
