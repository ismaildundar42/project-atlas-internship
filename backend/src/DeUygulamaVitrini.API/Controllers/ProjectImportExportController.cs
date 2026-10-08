using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Admin;
using DeUygulamaVitrini.Application.DTOs.ImportExport;
using DeUygulamaVitrini.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/admin/projects/import")]
[Authorize]
[Produces("application/json")]
public class ProjectImportExportController : ControllerBase
{
    private readonly IProjectExcelService _excelService;
    private readonly IProjectAuthorizationService _projectAuthService;
    private readonly ILogger<ProjectImportExportController> _logger;

    public ProjectImportExportController(
        IProjectExcelService excelService,
        IProjectAuthorizationService projectAuthService,
        ILogger<ProjectImportExportController> logger)
    {
        _excelService = excelService;
        _projectAuthService = projectAuthService;
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
    /// Yüklenen Excel (.xlsx) çalışma kitabını güvenlik, yapı ve başlık açısından inceler;
    /// geçerli ise sonraki adımlarda kullanılmak üzere geçici FileToken üretir.
    /// </summary>
    [HttpPost("inspect")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(InspectWorkbookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InspectWorkbook(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
        if (!canCreate)
        {
            _logger.LogWarning("Unauthorized attempt to inspect workbook. UserId={UserId}, IsAdmin={IsAdmin}", userId, isAdmin);
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Proje içe aktarma yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz Dosya",
                Detail = "Lütfen geçerli ve içi dolu bir Excel (.xlsx) dosyası yükleyiniz.",
                Instance = HttpContext.Request.Path
            });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dosya Çok Büyük",
                Detail = "Excel dosyası en fazla 10 MB olabilir.",
                Instance = HttpContext.Request.Path
            });
        }

        await using var stream = file.OpenReadStream();
        var response = await _excelService.InspectWorkbookAsync(stream, file.FileName, userId, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Canlı veritabanı referans değerleri (Kategoriler, Durumlar, Ekipler vb.) ve
    /// veri doğrulama listeleri içeren resmi Proje İçe Aktarım Excel Şablonunu üretir.
    /// </summary>
    [HttpGet("template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadTemplate(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
        if (!canCreate)
        {
            _logger.LogWarning("Unauthorized attempt to download import template. UserId={UserId}, IsAdmin={IsAdmin}", userId, isAdmin);
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Proje şablonu indirme yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        var templateBytes = await _excelService.GenerateImportTemplateAsync(cancellationToken);
        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        const string fileName = "Proje-Kutuphanesi-Import-Sablonu.xlsx";

        return File(templateBytes, contentType, fileName);
    }

    /// <summary>
    /// FileToken ve sütun eşleştirmelerini alarak veritabanına yazmadan tam bir proje doğrulama ve önizleme işlemi yürütür.
    /// </summary>
    [HttpPost("validate")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ValidateImportResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidateImport(
        [FromBody] ValidateImportRequestDto? request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
        if (!canCreate)
        {
            _logger.LogWarning("Unauthorized attempt to validate import. UserId={UserId}, IsAdmin={IsAdmin}", userId, isAdmin);
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Proje doğrulama yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        if (request == null || string.IsNullOrWhiteSpace(request.FileToken))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = "Lütfen geçerli bir FileToken belirtiniz.",
                Instance = HttpContext.Request.Path
            });
        }

        var response = await _excelService.ValidateImportAsync(request, userId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// FileToken ve sütun eşleştirmelerini alarak veritabanı durumunu yeniden doğrular;
    /// geçerliyse tek bir işlem (transaction) içinde projeleri taslak olarak içe aktarır.
    /// </summary>
    [HttpPost("confirm")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ConfirmImportResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfirmImport(
        [FromBody] ConfirmImportRequestDto? request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
        if (!canCreate)
        {
            _logger.LogWarning("Unauthorized attempt to confirm import. UserId={UserId}, IsAdmin={IsAdmin}", userId, isAdmin);
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Proje içe aktarma yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        if (request == null || string.IsNullOrWhiteSpace(request.FileToken))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = "Lütfen geçerli bir FileToken belirtiniz.",
                Instance = HttpContext.Request.Path
            });
        }

        var response = await _excelService.ConfirmImportAsync(request, userId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Filtrelenmiş aktif proje listesini Excel (.xlsx) çalışma kitabı olarak dışa aktarır.
    /// Admin: Tüm filtrelenmiş projeleri dışa aktarabilir.
    /// Proje Girişi Yetkilisi: Yalnızca yönetiminde yetkili olduğu (kendi oluşturduğu) filtrelenmiş projeleri dışa aktarabilir.
    /// </summary>
    [HttpGet("/api/admin/projects/export")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportProjects(
        [FromQuery] AdminProjectQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canAccess = await _projectAuthService.CanAccessManagementAsync(userId, isAdmin, cancellationToken);
        if (!canAccess)
        {
            _logger.LogWarning("Unauthorized attempt to export projects. UserId={UserId}, IsAdmin={IsAdmin}", userId, isAdmin);
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Proje listesini dışa aktarma yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        int? createdByFilter = isAdmin ? null : userId;
        var excelBytes = await _excelService.ExportProjectsAsync(queryParams, createdByFilter, cancellationToken);
        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"DemirExport_Projeler_{DateTime.UtcNow:yyyy-MM-dd}.xlsx";

        return File(excelBytes, contentType, fileName);
    }
}
