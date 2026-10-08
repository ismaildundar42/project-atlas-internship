using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Lookups;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

/// <summary>
/// Referans ve sözlük verileri için salt-okunur arayüz sunan servis implementasyonu.
/// </summary>
public class LookupService : ILookupService
{
    private readonly IApplicationDbContext _context;

    public LookupService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProjectStatusDto>> GetProjectStatusesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProjectStatuses
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new ProjectStatusDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectCategoryDto>> GetProjectCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProjectCategories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ProjectCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Technologies
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TechnologyDto
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                LocationType = l.LocationType.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeamDto>> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DepartmentId = t.DepartmentId,
                DepartmentName = t.Department.Name,
                IsPrimary = false
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .AsNoTracking()
            .OrderBy(m => m.FirstName)
            .ThenBy(m => m.LastName)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Title = m.Title,
                Email = m.Email
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TagDto>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug
            })
            .ToListAsync(cancellationToken);
    }
}
