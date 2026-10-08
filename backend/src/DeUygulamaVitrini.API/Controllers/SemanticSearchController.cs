using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Anlamsal (vektörel) proje arama denetleyicisi.
/// Yetkilendirme duyarlı çalışır; normal kullanıcılar yalnızca onaylı ve yayınlanmış projelere erişebilir.
/// Generative LLM çağırmaz; doğrudan vektör benzerliği hesaplar.
/// </summary>
[ApiController]
[Route("api/ai/semantic-search")]
[Authorize]
[Produces("application/json")]
public class SemanticSearchController : ControllerBase
{
    private readonly IProjectSemanticSearchService _semanticSearchService;
    private readonly ILogger<SemanticSearchController> _logger;

    public SemanticSearchController(
        IProjectSemanticSearchService semanticSearchService,
        ILogger<SemanticSearchController> logger)
    {
        _semanticSearchService = semanticSearchService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    private bool IsAdminUser()
    {
        return User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.SuperAdmin);
    }

    /// <summary>
    /// GET parametreleri ile anlamsal arama yürütür.
    /// GET /api/ai/semantic-search?query=...&topK=5&minSimilarity=0.50
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SemanticSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<SemanticSearchResultDto>>> SearchGet(
        [FromQuery] SemanticSearchQueryDto queryDto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queryDto.Query))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz İstek",
                Detail = "Arama sorgusu boş olamaz.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _semanticSearchService.SearchAsync(
            queryDto,
            currentUserId: GetCurrentUserId(),
            isAdmin: IsAdminUser(),
            cancellationToken);

        return Ok(results);
    }

    /// <summary>
    /// POST gövdesi ile anlamsal arama yürütür (filtreler ve uzun sorgular için).
    /// POST /api/ai/semantic-search
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyList<SemanticSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<SemanticSearchResultDto>>> SearchPost(
        [FromBody] SemanticSearchQueryDto queryDto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queryDto.Query))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz İstek",
                Detail = "Arama sorgusu boş olamaz.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _semanticSearchService.SearchAsync(
            queryDto,
            currentUserId: GetCurrentUserId(),
            isAdmin: IsAdminUser(),
            cancellationToken);

        return Ok(results);
    }
}
