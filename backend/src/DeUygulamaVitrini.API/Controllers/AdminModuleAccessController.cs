using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.ModuleAccess;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Yönetici modül erişim talepleri ve yetkilendirme yönetimi API uç noktaları.
/// </summary>
[ApiController]
[Route("api/admin/module-access")]
[Authorize(Policy = "AdminAccess")]
[Produces("application/json")]
public class AdminModuleAccessController : ControllerBase
{
    private readonly IModuleAccessService _moduleAccessService;

    public AdminModuleAccessController(IModuleAccessService moduleAccessService)
    {
        _moduleAccessService = moduleAccessService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    /// <summary>
    /// Sistemdeki tüm modül erişim taleplerini filtreleme, arama ve sayfalama ile listeler.
    /// </summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(PagedResult<ModuleAccessRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ModuleAccessRequestDto>>> GetRequests(
        [FromQuery] AccessRequestQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var result = await _moduleAccessService.GetAccessRequestsAsync(queryParams, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Onay bekleyen modül erişim talebi sayısını döner (Yönetici menüsü rozeti için).
    /// </summary>
    [HttpGet("pending-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingCount(CancellationToken cancellationToken)
    {
        var count = await _moduleAccessService.GetPendingRequestCountAsync(cancellationToken);
        return Ok(new { count });
    }

    /// <summary>
    /// Bekleyen bir erişim talebini onaylar ve kullanıcıya ilgili modül yetkisini tanımlar.
    /// </summary>
    [HttpPost("requests/{id:int}/approve")]
    [ProducesResponseType(typeof(ModuleAccessRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuleAccessRequestDto>> ApproveRequest(
        int id,
        [FromBody] ReviewModuleAccessRequestDto request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var result = await _moduleAccessService.ApproveAccessRequestAsync(id, adminUserId, request.ReviewNote, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Talep Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İşlem",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Bekleyen bir erişim talebini gerekçe belirterek reddeder.
    /// </summary>
    [HttpPost("requests/{id:int}/reject")]
    [ProducesResponseType(typeof(ModuleAccessRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuleAccessRequestDto>> RejectRequest(
        int id,
        [FromBody] ReviewModuleAccessRequestDto request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            var result = await _moduleAccessService.RejectAccessRequestAsync(id, adminUserId, request.ReviewNote, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Talep Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İşlem",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Bir kullanıcıya talep olmaksızın doğrudan modül yetkisi atar.
    /// </summary>
    [HttpPost("users/{userId:int}/grant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GrantModuleAccess(
        int userId,
        [FromBody] GrantModuleAccessRequestDto request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();

        try
        {
            await _moduleAccessService.GrantDirectModuleAccessAsync(userId, request.Module, adminUserId, cancellationToken);
            return Ok(new { message = "Modül yetkisi başarıyla tanımlandı." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Yetki Atanamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Bir kullanıcının mevcut modül yetkisini kaldırır.
    /// </summary>
    [HttpDelete("users/{userId:int}/revoke/{module}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeModuleAccess(
        int userId,
        ApplicationModule module,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();

        await _moduleAccessService.RevokeModuleAccessAsync(userId, module, adminUserId, cancellationToken);
        return Ok(new { message = "Modül yetkisi başarıyla kaldırıldı." });
    }
}
