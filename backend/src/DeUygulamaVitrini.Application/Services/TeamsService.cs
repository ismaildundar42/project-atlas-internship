using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Teams;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

public class TeamsService : ITeamsService
{
    private readonly IApplicationDbContext _context;

    public TeamsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamsSummaryDto> GetTeamsSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalTeams = await _context.Teams.AsNoTracking().CountAsync(cancellationToken);
        var totalDepartments = await _context.Departments.AsNoTracking().CountAsync(cancellationToken);

        var activeTeamsInPublishedProjects = await _context.ProjectTeams
            .AsNoTracking()
            .Where(pt => pt.Project.IsPublished)
            .Select(pt => pt.TeamId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalMembersInProjects = await _context.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.Project.IsPublished)
            .Select(pm => pm.MemberId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new TeamsSummaryDto
        {
            TotalTeams = totalTeams,
            TotalDepartments = totalDepartments,
            ActiveTeamsInPublishedProjects = activeTeamsInPublishedProjects,
            TotalMembersInProjects = totalMembersInProjects
        };
    }

    public async Task<IReadOnlyList<TeamListItemDto>> GetTeamsAsync(
        string? search,
        int? departmentId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Teams
            .AsNoTracking()
            .AsQueryable();

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(t => t.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Name, $"%{term}%") ||
                (t.Department != null && EF.Functions.Like(t.Department.Name, $"%{term}%")) ||
                (t.Description != null && EF.Functions.Like(t.Description, $"%{term}%")));
        }

        var items = await query
            .OrderBy(t => t.Department != null ? t.Department.Name : t.Name)
            .ThenBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.DepartmentId,
                DepartmentName = t.Department != null ? t.Department.Name : null,

                // Yalnızca yayınlanmış projeler üzerinden hesaplanır
                PublishedProjectCount = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .Select(pt => pt.ProjectId)
                    .Distinct()
                    .Count(),

                MemberCount = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .SelectMany(pt => pt.Project.ProjectMembers)
                    .Select(pm => pm.MemberId)
                    .Distinct()
                    .Count(),

                Technologies = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .SelectMany(pt => pt.Project.ProjectTechnologies)
                    .Select(pt => pt.Technology.Name)
                    .Distinct()
                    .Take(4)
                    .ToList(),

                Locations = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .SelectMany(pt => pt.Project.ProjectLocations)
                    .Select(pl => pl.Location.Name)
                    .Distinct()
                    .Take(3)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return items.Select(x => new TeamListItemDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.DepartmentName,
            PublishedProjectCount = x.PublishedProjectCount,
            MemberCount = x.MemberCount,
            Technologies = x.Technologies,
            Locations = x.Locations
        }).ToList();
    }

    public async Task<TeamDetailDto?> GetTeamByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.DepartmentId,
                DepartmentName = t.Department != null ? t.Department.Name : null,

                // Sadece IsPublished = true olan projeler getirilir
                Projects = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .Select(pt => new
                    {
                        pt.Project.Id,
                        pt.Project.Name,
                        pt.Project.Slug,
                        pt.Project.ShortDescription,
                        CategoryName = pt.Project.Category.Name,
                        StatusName = pt.Project.Status.Name,
                        CoverImageUrl = pt.Project.CoverImageUrl ?? pt.Project.ProjectMediaItems
                            .Where(m => m.MediaType == MediaType.Image)
                            .OrderBy(m => m.DisplayOrder)
                            .Select(m => m.FileUrl)
                            .FirstOrDefault(),
                        IsPrimaryTeam = pt.IsPrimary
                    })
                    .ToList(),

                Technologies = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .SelectMany(pt => pt.Project.ProjectTechnologies)
                    .Select(pt => pt.Technology.Name)
                    .Distinct()
                    .ToList(),

                Locations = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .SelectMany(pt => pt.Project.ProjectLocations)
                    .Select(pl => pl.Location.Name)
                    .Distinct()
                    .ToList(),

                // Bu ekibin yer aldığı yayınlanmış projelerdeki üyeler ve rolleri
                MembersRaw = t.ProjectTeams
                    .Where(pt => pt.Project.IsPublished)
                    .SelectMany(pt => pt.Project.ProjectMembers)
                    .Select(pm => new
                    {
                        pm.Member.Id,
                        pm.Member.FirstName,
                        pm.Member.LastName,
                        pm.Member.Title,
                        pm.Member.Email,
                        pm.ProjectRole
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (team == null) return null;

        // Group members cleanly
        var membersGrouped = team.MembersRaw
            .GroupBy(m => m.Id)
            .Select(g => new TeamMemberDto
            {
                Id = g.Key,
                FirstName = g.First().FirstName,
                LastName = g.First().LastName,
                Title = g.First().Title,
                Email = g.First().Email,
                ProjectRoles = g.Select(x => x.ProjectRole)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r!)
                    .Distinct()
                    .ToList()
            })
            .OrderBy(m => m.FirstName)
            .ThenBy(m => m.LastName)
            .ToList();

        var projectDtos = team.Projects
            .OrderByDescending(p => p.IsPrimaryTeam)
            .ThenBy(p => p.Name)
            .Select(p => new TeamProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                CategoryName = p.CategoryName,
                StatusName = p.StatusName,
                CoverImageUrl = p.CoverImageUrl,
                IsPrimaryTeam = p.IsPrimaryTeam
            })
            .ToList();

        return new TeamDetailDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            DepartmentId = team.DepartmentId,
            DepartmentName = team.DepartmentName,
            PublishedProjectCount = projectDtos.Count,
            MemberCount = membersGrouped.Count,
            TechnologyCount = team.Technologies.Count,
            LocationCount = team.Locations.Count,

            Projects = projectDtos,
            Technologies = team.Technologies,
            Locations = team.Locations,
            Members = membersGrouped
        };
    }
}
