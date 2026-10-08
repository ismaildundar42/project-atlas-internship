using DeUygulamaVitrini.Application.DTOs.Teams;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Genel erişime açık Ekipler ve Organizasyon okuma servisi.
/// </summary>
public interface ITeamsService
{
    Task<TeamsSummaryDto> GetTeamsSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamListItemDto>> GetTeamsAsync(string? search, int? departmentId, CancellationToken cancellationToken = default);
    Task<TeamDetailDto?> GetTeamByIdAsync(int id, CancellationToken cancellationToken = default);
}
