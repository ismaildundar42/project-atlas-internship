using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Lookups;
using DeUygulamaVitrini.Application.DTOs.Projects;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

/// <summary>
/// IProjectService arayüzünün EF Core + LINQ projeksiyonlarına dayalı implementasyonu.
/// N+1 sorgu problemlerinden kaçınır ve veritabanı seviyesinde optimize sorgular çalıştırır.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IApplicationDbContext _context;

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "createdat", "updatedat", "startdate"
    };

    public ProjectService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProjectListItemDto>> GetProjectsAsync(
        ProjectQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished);

        // ─── Arama (Search) ──────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{search}%") ||
                EF.Functions.Like(p.ShortDescription, $"%{search}%") ||
                (p.Description != null && EF.Functions.Like(p.Description, $"%{search}%")));
        }

        // ─── Filtreler ───────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var statusCode = parameters.Status.Trim();
            query = query.Where(p => p.Status.Code.ToLower() == statusCode.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var categoryCode = parameters.Category.Trim();
            query = query.Where(p => p.Category.Code.ToLower() == categoryCode.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(parameters.DevelopmentType))
        {
            var devTypeInput = parameters.DevelopmentType.Trim();
            if (Enum.TryParse<DevelopmentType>(devTypeInput, true, out var parsedDevType))
            {
                query = query.Where(p => p.DevelopmentType == parsedDevType);
            }
            else
            {
                throw new ArgumentException(
                    $"Geçersiz Geliştirme Tipi: '{devTypeInput}'. Geçerli değerler: Internal, External, Hybrid.");
            }
        }

        if (parameters.TeamId.HasValue)
        {
            query = query.Where(p => p.ProjectTeams.Any(pt => pt.TeamId == parameters.TeamId.Value));
        }

        if (parameters.DepartmentId.HasValue)
        {
            query = query.Where(p => p.ProjectTeams.Any(pt => pt.Team.DepartmentId == parameters.DepartmentId.Value));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Department))
        {
            var dept = parameters.Department.Trim().ToLower();
            query = query.Where(p => p.ProjectTeams.Any(pt => pt.Team.Department.Name.ToLower() == dept));
        }

        if (parameters.LocationId.HasValue)
        {
            query = query.Where(p => p.ProjectLocations.Any(pl => pl.LocationId == parameters.LocationId.Value));
        }

        if (parameters.TechnologyId.HasValue)
        {
            query = query.Where(p => p.ProjectTechnologies.Any(pt => pt.TechnologyId == parameters.TechnologyId.Value));
        }

        if (parameters.TagId.HasValue)
        {
            query = query.Where(p => p.ProjectTags.Any(pt => pt.TagId == parameters.TagId.Value));
        }

        if (parameters.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == parameters.IsFeatured.Value);
        }

        // ─── Sıralama (Sorting - Allow-List) ─────────────────────────────────
        var sortBy = parameters.SortBy?.ToLowerInvariant() ?? "updatedat";
        if (!AllowedSortFields.Contains(sortBy))
        {
            throw new ArgumentException(
                $"Geçersiz sortBy parametresi: '{parameters.SortBy}'. İzin verilen alanlar: name, createdAt, updatedAt, startDate.");
        }

        var isAscending = string.Equals(parameters.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        query = sortBy switch
        {
            "name" => isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "createdat" => isAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            "startdate" => isAscending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
            _ => isAscending ? query.OrderBy(p => p.UpdatedAt ?? p.CreatedAt) : query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
        };

        // ─── Sayfalama ve Projeksiyon ─────────────────────────────────────────
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new ProjectListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                DevelopmentType = p.DevelopmentType.ToString(),
                IsFeatured = p.IsFeatured,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
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
                PrimaryTeam = p.ProjectTeams
                    .Where(pt => pt.IsPrimary)
                    .Select(pt => new TeamDto
                    {
                        Id = pt.Team.Id,
                        Name = pt.Team.Name,
                        Description = pt.Team.Description,
                        DepartmentId = pt.Team.DepartmentId,
                        DepartmentName = pt.Team.Department.Name,
                        IsPrimary = true
                    })
                    .FirstOrDefault() ?? p.ProjectTeams
                    .Select(pt => new TeamDto
                    {
                        Id = pt.Team.Id,
                        Name = pt.Team.Name,
                        Description = pt.Team.Description,
                        DepartmentId = pt.Team.DepartmentId,
                        DepartmentName = pt.Team.Department.Name,
                        IsPrimary = false
                    })
                    .FirstOrDefault(),
                Locations = p.ProjectLocations
                    .Select(pl => new LocationDto
                    {
                        Id = pl.Location.Id,
                        Name = pl.Location.Name,
                        Description = pl.Location.Description,
                        LocationType = pl.Location.LocationType.ToString()
                    })
                    .ToList(),
                Technologies = p.ProjectTechnologies
                    .Select(pt => new TechnologyDto
                    {
                        Id = pt.Technology.Id,
                        Name = pt.Technology.Name,
                        Category = pt.Technology.Category.ToString()
                    })
                    .ToList(),
                Tags = p.ProjectTags
                    .Select(pt => new TagDto
                    {
                        Id = pt.Tag.Id,
                        Name = pt.Tag.Name,
                        Slug = pt.Tag.Slug
                    })
                    .ToList(),
                CoverImageUrl = p.CoverImageUrl ?? p.ProjectMediaItems
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.FileUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectListItemDto>(items, parameters.PageNumber, parameters.PageSize, totalCount);
    }

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await GetProjectDetailQuery(_context.Projects.Where(p => p.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto?> GetProjectBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;

        var normalizedSlug = slug.Trim();
        return await GetProjectDetailQuery(_context.Projects.Where(p => p.Slug == normalizedSlug))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<ProjectDetailDto> GetProjectDetailQuery(IQueryable<Project> sourceQuery)
    {
        return sourceQuery
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .Select(p => new ProjectDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                Purpose = p.Purpose,
                ProblemSolved = p.ProblemSolved,
                NonTechnicalDescription = p.NonTechnicalDescription,
                TechnicalDescription = p.TechnicalDescription,
                BusinessImpact = p.BusinessImpact,
                TargetAudience = p.TargetAudience,
                AccessInstructions = p.AccessInstructions,
                ApplicationUrl = p.ApplicationUrl,
                RepositoryUrl = p.RepositoryUrl,
                DevelopmentType = p.DevelopmentType.ToString(),
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsFeatured = p.IsFeatured,
                CoverImageUrl = p.CoverImageUrl ?? p.ProjectMediaItems
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.FileUrl)
                    .FirstOrDefault(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
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
                Teams = p.ProjectTeams
                    .Select(pt => new TeamDto
                    {
                        Id = pt.Team.Id,
                        Name = pt.Team.Name,
                        Description = pt.Team.Description,
                        DepartmentId = pt.Team.DepartmentId,
                        DepartmentName = pt.Team.Department.Name,
                        IsPrimary = pt.IsPrimary
                    })
                    .ToList(),
                Members = p.ProjectMembers
                    .Select(pm => new MemberDto
                    {
                        Id = pm.Member.Id,
                        FirstName = pm.Member.FirstName,
                        LastName = pm.Member.LastName,
                        Title = pm.Member.Title,
                        Email = pm.Member.Email,
                        ProjectRole = pm.ProjectRole
                    })
                    .ToList(),
                Locations = p.ProjectLocations
                    .Select(pl => new LocationDto
                    {
                        Id = pl.Location.Id,
                        Name = pl.Location.Name,
                        Description = pl.Location.Description,
                        LocationType = pl.Location.LocationType.ToString()
                    })
                    .ToList(),
                Technologies = p.ProjectTechnologies
                    .Select(pt => new TechnologyDto
                    {
                        Id = pt.Technology.Id,
                        Name = pt.Technology.Name,
                        Category = pt.Technology.Category.ToString()
                    })
                    .ToList(),
                Tags = p.ProjectTags
                    .Select(pt => new TagDto
                    {
                        Id = pt.Tag.Id,
                        Name = pt.Tag.Name,
                        Slug = pt.Tag.Slug
                    })
                    .ToList(),
                Integrations = p.ProjectIntegrations
                    .Select(pi => new ProjectIntegrationDto
                    {
                        Id = pi.Id,
                        Name = pi.Name,
                        Description = pi.Description,
                        IntegrationType = pi.IntegrationType.ToString()
                    })
                    .ToList(),
                Media = p.ProjectMediaItems
                    .OrderBy(m => m.DisplayOrder)
                    .Select(pm => new ProjectMediaDto
                    {
                        Id = pm.Id,
                        MediaType = pm.MediaType.ToString(),
                        FileName = pm.FileName,
                        FileUrl = pm.FileUrl,
                        AltText = pm.AltText,
                        Caption = pm.Caption,
                        DisplayOrder = pm.DisplayOrder
                    })
                    .ToList(),
                Documents = p.ProjectDocuments
                    .Select(pd => new ProjectDocumentDto
                    {
                        Id = pd.Id,
                        Name = pd.Name,
                        Description = pd.Description,
                        FileName = pd.FileName,
                        FileUrl = $"/api/projects/{pd.ProjectId}/documents/{pd.Id}/download",
                        DocumentType = pd.DocumentType
                    })
                    .ToList()
            });
    }
}
