using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.ModuleAccess;
using DeUygulamaVitrini.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Kullanıcı modül erişim durumu sorgulama ve erişim talebi oluşturma API uç noktaları.
/// </summary>
[ApiController]
[Route("api/module-access")]
[Authorize]
[Produces("application/json")]
public class ModuleAccessController : ControllerBase
{
    private readonly IModuleAccessService _moduleAccessService;

    public ModuleAccessController(IModuleAccessService moduleAccessService)
    {
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

    /// <summary>
    /// Oturum açmış kullanıcının tüm modüller için erişim durumunu ve bekleyen taleplerini getirir.
    /// </summary>
    [HttpGet("my-access")]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, ModuleAccessStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyDictionary<string, ModuleAccessStatusDto>>> GetMyAccessSummary(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var summary = await _moduleAccessService.GetUserModuleAccessSummaryAsync(userId, isAdmin, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Kısıtlı bir modül (Raporlar, Ekipler) için yöneticilere yeni bir erişim talebi iletir.
    /// </summary>
    [HttpPost("requests")]
    [ProducesResponseType(typeof(ModuleAccessRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ModuleAccessRequestDto>> CreateAccessRequest(
        [FromBody] CreateModuleAccessRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        try
        {
            var result = await _moduleAccessService.CreateAccessRequestAsync(userId, request.Module, request.Reason, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Talep Oluşturulamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }
}
