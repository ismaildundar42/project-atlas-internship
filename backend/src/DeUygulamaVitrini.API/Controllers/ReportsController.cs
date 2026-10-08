using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Reports;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IModuleAccessService _moduleAccessService;

    public ReportsController(
        IReportService reportService,
        IModuleAccessService moduleAccessService)
    {
        _reportService = reportService;
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
    /// Portföy raporlama ve analitik içgörü verilerini getirir.
    /// Yalnızca yayınlanmış (IsPublished = true) ve silinmemiş (IsDeleted = false) projeleri kapsar.
    /// Kullanıcının Reports modülü erişim yetkisi doğrulanır.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ReportOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReportOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var hasAccess = await _moduleAccessService.CanAccessModuleAsync(userId, isAdmin, ApplicationModule.Reports, cancellationToken);
        if (!hasAccess)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Erişim Engellendi",
                Detail = "Raporlar modülünü görüntüleme yetkiniz bulunmuyor. Yöneticinizden erişim talep edebilirsiniz.",
                Instance = HttpContext.Request.Path
            });
        }

        var overview = await _reportService.GetPortfolioOverviewAsync(cancellationToken);
        return Ok(overview);
    }
}
