using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// AI altyapı teşhis ve doğrulama denetleyicisi.
/// Yalnızca yönetici (Admin) rolündeki yetkili kullanıcıların erişimine açıktır.
/// Geliştirme ve operasyonel sağlık doğrulaması için kullanılır.
/// </summary>
[ApiController]
[Route("api/admin/ai")]
[Authorize(Policy = "AdminAccess")]
[Produces("application/json")]
public class AiDiagnosticsController : ControllerBase
{
    private readonly IAiProvider _aiProvider;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IProjectKnowledgeIndexService _indexService;
    private readonly IProjectSemanticSearchService _semanticSearchService;
    private readonly ILogger<AiDiagnosticsController> _logger;

    public AiDiagnosticsController(
        IAiProvider aiProvider,
        IEmbeddingProvider embeddingProvider,
        IProjectKnowledgeIndexService indexService,
        IProjectSemanticSearchService semanticSearchService,
        ILogger<AiDiagnosticsController> logger)
    {
        _aiProvider = aiProvider;
        _embeddingProvider = embeddingProvider;
        _indexService = indexService;
        _semanticSearchService = semanticSearchService;
        _logger = logger;
    }

    /// <summary>
    /// AI sağlayıcısının ve yapılandırılmış chat modelinin anlık erişilebilirlik durumunu döner.
    /// GET /api/admin/ai/status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(AiHealthStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<AiHealthStatus>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _aiProvider.CheckHealthAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Bilgi İndeksinin ve embedding modelinin durumunu, toplam parça ve proje istatistiklerini döner.
    /// GET /api/admin/ai/index/status
    /// </summary>
    [HttpGet("index/status")]
    [ProducesResponseType(typeof(IndexStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IndexStatusDto>> GetIndexStatus(CancellationToken cancellationToken)
    {
        var status = await _indexService.GetIndexStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Tüm aktif projeleri baştan tarayarak Bilgi İndeksini (Knowledge Index) yeniden oluşturur.
    /// POST /api/admin/ai/index/rebuild
    /// </summary>
    [HttpPost("index/rebuild")]
    [ProducesResponseType(typeof(IndexRebuildResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IndexRebuildResultDto>> RebuildIndex(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin initiated project knowledge index rebuild.");
        var result = await _indexService.RebuildIndexAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yönetici seviyesinde tam yetkili anlamsal arama testi yürütür.
    /// POST /api/admin/ai/semantic-search/test
    /// </summary>
    [HttpPost("semantic-search/test")]
    [ProducesResponseType(typeof(IReadOnlyList<SemanticSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<SemanticSearchResultDto>>> TestSemanticSearch(
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
            currentUserId: 0,
            isAdmin: true,
            cancellationToken);

        return Ok(results);
    }

    /// <summary>
    /// Yerel AI sağlayıcısına doğrudan güvenli bir test istemi gönderir ve yanıtı döner.
    /// POST /api/admin/ai/test
    /// </summary>
    [HttpPost("test")]
    [ProducesResponseType(typeof(AiDiagnosticTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AiDiagnosticTestResponse>> RunSmokeTest(
        [FromBody] AiDiagnosticTestRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz İstek",
                Detail = "Prompt metni boş olamaz.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.Prompt.Length > 2000)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz İstek",
                Detail = "Test istemi maksimum 2000 karakter olabilir.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var generationRequest = new AiGenerationRequest
        {
            UserPrompt = request.Prompt,
            SystemPrompt = request.SystemPrompt,
            Temperature = request.Temperature ?? 0.3
        };

        var result = await _aiProvider.GenerateAsync(generationRequest, cancellationToken);

        var response = new AiDiagnosticTestResponse(
            Success: result.Success,
            Provider: result.Provider,
            Model: result.Model,
            Response: result.Content,
            DurationMs: result.DurationMs,
            ErrorMessage: result.ErrorMessage
        );

        return Ok(response);
    }
}
