using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Teams;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Ekipler ve kurumsal organizasyon okuma API uç noktaları.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
    private readonly ITeamsService _teamsService;
    private readonly IModuleAccessService _moduleAccessService;

    public TeamsController(
        ITeamsService teamsService,
        IModuleAccessService moduleAccessService)
    {
        _teamsService = teamsService;
        _moduleAccessService = moduleAccessService;
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

    private async Task<bool> CheckTeamsAccessAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();
        return await _moduleAccessService.CanAccessModuleAsync(userId, isAdmin, ApplicationModule.Teams, cancellationToken);
    }

    private ObjectResult AccessDeniedResult()
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Erişim Engellendi",
            Detail = "Ekipler modülünü görüntüleme yetkiniz bulunmuyor. Yöneticinizden erişim talep edebilirsiniz.",
            Instance = HttpContext.Request.Path
        });
    }

    /// <summary>
    /// Ekipler modülü özet istatistiklerini getirir.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(TeamsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamsSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        if (!await CheckTeamsAccessAsync(cancellationToken))
        {
            return AccessDeniedResult();
        }

        var result = await _teamsService.GetTeamsSummaryAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ekipleri listeler. Arama ve departman filtresi destekler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<TeamListItemDto>>> GetTeams(
        [FromQuery] string? search,
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        if (!await CheckTeamsAccessAsync(cancellationToken))
        {
            return AccessDeniedResult();
        }

        var result = await _teamsService.GetTeamsAsync(search, departmentId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen ekibin detay profilini, projelerini, teknolojilerini ve ilişkili kişileri getirir.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDetailDto>> GetTeamById(int id, CancellationToken cancellationToken)
    {
        if (!await CheckTeamsAccessAsync(cancellationToken))
        {
            return AccessDeniedResult();
        }

        var result = await _teamsService.GetTeamByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { message = "Ekip bulunamadı." });

        return Ok(result);
    }
}
