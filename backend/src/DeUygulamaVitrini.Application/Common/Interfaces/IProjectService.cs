using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Projects;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Proje okuma işlemlerini yöneten servis arayüzü.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Filtreleme, sayfalama ve sıralama parametrelerine göre yayınlanmış projeleri listeler.
    /// </summary>
    Task<PagedResult<ProjectListItemDto>> GetProjectsAsync(
        ProjectQueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kimlik numarasına göre yayınlanmış tek bir projenin detayını getirir.
    /// Bulunamazsa veya yayınlanmamışsa null döner.
    /// </summary>
    Task<ProjectDetailDto?> GetProjectByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// SEO/URL slug bilgisine göre yayınlanmış tek bir projenin detayını getirir.
    /// Bulunamazsa veya yayınlanmamışsa null döner.
    /// </summary>
    Task<ProjectDetailDto?> GetProjectBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
