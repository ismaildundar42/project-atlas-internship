using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Yetkilendirme duyarlı (authorization-aware) RAG Proje Kütüphanesi Asistanı denetleyicisi.
/// Kullanıcının yetkili olduğu proje bağlamını getirerek DeepSeek LLM üzerinden topraklanmış yanıt ve alıntılar üretir.
/// </summary>
[ApiController]
[Route("api/ai/assistant")]
[Authorize]
[Produces("application/json")]
public class ProjectAssistantController : ControllerBase
{
    private readonly IProjectAssistantService _assistantService;
    private readonly ILogger<ProjectAssistantController> _logger;

    public ProjectAssistantController(
        IProjectAssistantService assistantService,
        ILogger<ProjectAssistantController> logger)
    {
        _assistantService = assistantService;
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
    /// Doğal dilde sorulan soruya yetkilendirilmiş proje kütüphanesi bağlamı üzerinden topraklanmış yanıt üretir.
    /// POST /api/ai/assistant
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectAssistantResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ProjectAssistantResponseDto>> Ask(
        [FromBody] ProjectAssistantRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz İstek",
                Detail = "Soru metni boş olamaz.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        var response = await _assistantService.AskAsync(
            request,
            currentUserId: GetCurrentUserId(),
            isAdmin: IsAdminUser(),
            cancellationToken);

        return Ok(response);
    }
}
