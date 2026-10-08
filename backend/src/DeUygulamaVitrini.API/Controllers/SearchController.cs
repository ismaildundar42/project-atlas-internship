using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Search;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Kurumsal genel arama ve proje keşif API uç noktası.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Genel arama sorgusu çalıştırır. Yayınlanmış projeler üzerinde çok boyutlu arama yapar.
    /// </summary>
    /// <param name="q">Arama sorgusu</param>
    /// <param name="limit">Azami sonuç sayısı (varsayılan: 8)</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchProjectsAsync(q, limit, cancellationToken);
        return Ok(result);
    }
}
