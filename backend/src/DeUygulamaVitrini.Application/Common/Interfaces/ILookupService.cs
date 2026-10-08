using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Referans ve sözlük verilerini okumak için servis arayüzü.
/// Frontend filtreleme ve açılır menü öğelerini besler.
/// </summary>
public interface ILookupService
{
    Task<IReadOnlyList<ProjectStatusDto>> GetProjectStatusesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectCategoryDto>> GetProjectCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamDto>> GetTeamsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagDto>> GetTagsAsync(CancellationToken cancellationToken = default);
}
