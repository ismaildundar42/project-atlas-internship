using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Admin;
using DeUygulamaVitrini.Application.DTOs.Lookups;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IProjectAuthorizationService _projectAuthService;
    private readonly UserManager<ApplicationUser> _userManager;

    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt"
    };

    private static readonly HashSet<string> AllowedDocumentMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain"
    };

    private static readonly HashSet<string> AllowedMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp"
    };

    private static readonly HashSet<string> AllowedMediaMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp"
    };

    private const long MaxDocumentSize = 25 * 1024 * 1024; // 25 MB
    private const long MaxMediaSize = 10 * 1024 * 1024;    // 10 MB

    public AdminController(
        IAdminService adminService,
        IFileStorageService fileStorageService,
        IProjectAuthorizationService projectAuthService,
        UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _fileStorageService = fileStorageService;
        _projectAuthService = projectAuthService;
        _userManager = userManager;
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
    /// Yönetici/Yetkili erişim yetkisini doğrular.
    /// Admin veya proje girişi yetkisi olan kullanıcılar erişebilir.
    /// </summary>
    [HttpGet("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyAccess(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canAccess = await _projectAuthService.CanAccessManagementAsync(userId, isAdmin, cancellationToken);
        if (!canAccess)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Erişim Engellendi",
                Detail = "Yönetim paneline erişim yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new
        {
            status = "authorized",
            user = User.Identity?.Name,
            isAdmin,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Yönetici paneli özet metriklerini getirir (Yalnızca Admin).
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _adminService.GetAdminDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Projeleri listeler.
    /// Admin: Tüm projeleri görür.
    /// Proje Girişi Yetkilisi: Yalnızca kendi oluşturduğu projeleri görür.
    /// </summary>
    [HttpGet("projects")]
    [ProducesResponseType(typeof(PagedResult<AdminProjectListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminProjectListItemDto>>> GetAdminProjects(
        [FromQuery] AdminProjectQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canAccess = await _projectAuthService.CanAccessManagementAsync(userId, isAdmin, cancellationToken);
        if (!canAccess)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Erişim Engellendi",
                Detail = "Proje yönetim paneline erişim yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        int? createdByFilter = isAdmin ? null : userId;
        var result = await _adminService.GetAdminProjectsAsync(queryParams, createdByFilter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Düzenleme editörü için projenin detaylarını getirir (yetki kontrolü ile).
    /// </summary>
    [HttpGet("projects/{id:int}")]
    [ProducesResponseType(typeof(AdminProjectEditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProjectEditDto>> GetProjectForEdit(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canEdit = await _projectAuthService.CanEditProjectAsync(userId, isAdmin, id, cancellationToken);
        if (!canEdit)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Erişim Engellendi",
                Detail = "Bu projeyi görüntüleme veya düzenleme yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        var project = await _adminService.GetProjectForEditAsync(id, cancellationToken);
        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Proje Bulunamadı",
                Detail = $"Id={id} olan proje bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }
        return Ok(project);
    }

    /// <summary>
    /// Yeni bir proje kaydı oluşturur.
    /// Admin veya CanCreateProjects = true olan kullanıcılar erişebilir.
    /// </summary>
    [HttpPost("projects")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
        if (!canCreate)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Proje oluşturma yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        // Admin olmayan kullanıcılar projeyi doğrudan yayınlayamaz (taslak olarak oluşturulur)
        if (!isAdmin)
        {
            request.IsPublished = false;
            request.IsFeatured = false;
        }

        try
        {
            var id = await _adminService.CreateProjectAsync(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetProjectForEdit), new { id }, new { id, slug = request.Slug });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Mevcut bir proje kaydını günceller.
    /// Admin: Tüm projeleri güncelleyebilir.
    /// Proje Girişi Yetkilisi: Yalnızca kendi oluşturduğu projeyi güncelleyebilir.
    /// </summary>
    [HttpPut("projects/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var canEdit = await _projectAuthService.CanEditProjectAsync(userId, isAdmin, id, cancellationToken);
        if (!canEdit)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
                Detail = "Bu proje üzerinde düzenleme yetkiniz bulunmuyor.",
                Instance = HttpContext.Request.Path
            });
        }

        // Admin olmayan kullanıcılar yayın durumunu değiştiremez
        if (!isAdmin)
        {
            var existing = await _adminService.GetProjectForEditAsync(id, cancellationToken);
            if (existing != null)
            {
                request.IsPublished = existing.IsPublished;
                request.IsFeatured = existing.IsFeatured;
            }
        }

        try
        {
            await _adminService.UpdateProjectAsync(id, request, userId, cancellationToken);
            return Ok(new { id, message = "Proje başarıyla güncellendi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Projeyi arşivler (Yalnızca Admin).
    /// </summary>
    [HttpDelete("projects/{id:int}")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            await _adminService.DeleteProjectAsync(id, currentUserId, cancellationToken);
            return Ok(new { id, message = "Proje başarıyla arşivlendi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Arşivlenmiş projeyi geri yükler (Yalnızca Admin).
    /// </summary>
    [HttpPost("projects/{id:int}/restore")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProject(int id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            await _adminService.RestoreProjectAsync(id, currentUserId, cancellationToken);
            return Ok(new { id, message = "Proje başarıyla geri yüklendi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Projenin yayın durumunu değiştirir (Yalnızca Admin).
    /// </summary>
    [HttpPut("projects/{id:int}/publish")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetProjectPublished(int id, [FromBody] SetPublishedRequestDto request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            await _adminService.SetProjectPublishedAsync(id, request.IsPublished, currentUserId, cancellationToken);
            var statusMessage = request.IsPublished ? "Proje başarıyla yayına alındı." : "Proje taslağa çekildi.";
            return Ok(new { id, isPublished = request.IsPublished, message = statusMessage });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Projeyi yönetici incelemesine gönderir (Sahibi veya Admin).
    /// </summary>
    [HttpPost("projects/{id:int}/submit-for-review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitProjectForReview(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        try
        {
            await _adminService.SubmitProjectForReviewAsync(id, userId, isAdmin, cancellationToken);
            return Ok(new { id, message = "Proje yönetici incelemesine gönderildi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Projeyi onaylar ve yayına alır (Yalnızca Admin).
    /// </summary>
    [HttpPost("projects/{id:int}/approve")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveProject(int id, CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        try
        {
            await _adminService.ApproveProjectAsync(id, adminUserId, cancellationToken);
            return Ok(new { id, message = "Proje onaylandı ve yayına alındı." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Projeyi reddeder ve gerekçe kaydeder (Yalnızca Admin).
    /// </summary>
    [HttpPost("projects/{id:int}/reject")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectProject(int id, [FromBody] RejectProjectRequestDto request, CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        try
        {
            await _adminService.RejectProjectAsync(id, request.GetEffectiveReason(), adminUserId, cancellationToken);
            return Ok(new { id, message = "Proje reddedildi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Sistem geneli denetim kayıtlarını (Audit Log) listeler (Yalnızca Admin).
    /// </summary>
    [HttpGet("audit-logs")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs([FromQuery] AuditLogQueryParams queryParams, CancellationToken cancellationToken)
    {
        var logs = await _adminService.GetAuditLogsAsync(queryParams, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Belirli bir projeye ait denetim kayıtlarını listeler (Yalnızca Admin).
    /// </summary>
    [HttpGet("projects/{projectId:int}/audit-logs")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLogDto>>> GetProjectAuditLogs(int projectId, CancellationToken cancellationToken)
    {
        var logs = await _adminService.GetProjectAuditLogsAsync(projectId, cancellationToken);
        return Ok(logs);
    }

    /// <summary>
    /// Projeye doküman dosyası yükler.
    /// </summary>
    [HttpPost("projects/{projectId:int}/documents/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResultDto>> UploadDocument(int projectId, IFormFile file, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        if (projectId > 0)
        {
            var canEdit = await _projectAuthService.CanEditProjectAsync(userId, isAdmin, projectId, cancellationToken);
            if (!canEdit)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Yetkisiz İşlem",
                    Detail = "Bu projeye dosya yükleme yetkiniz bulunmuyor.",
                    Instance = HttpContext.Request.Path
                });
            }
        }
        else
        {
            var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
            if (!canCreate)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Yetkisiz İşlem",
                    Detail = "Dosya yükleme yetkiniz bulunmuyor.",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = "Lütfen geçerli bir dosya seçin." });

        if (file.Length > MaxDocumentSize)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new ProblemDetails { Status = StatusCodes.Status413PayloadTooLarge, Detail = "Doküman boyutu azami 25 MB olabilir." });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedDocumentExtensions.Contains(ext) || !AllowedDocumentMimeTypes.Contains(file.ContentType))
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = "Desteklenmeyen doküman formatı. Yalnızca PDF, Word, Excel, PowerPoint ve TXT dosyaları yüklenebilir." });
        }

        var folder = projectId > 0 ? $"projects/{projectId}/documents" : "projects/temp/documents";
        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType, folder, isPrivate: true, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Projeye medya / görsel dosyası yükler.
    /// </summary>
    [HttpPost("projects/{projectId:int}/media/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResultDto>> UploadMedia(int projectId, IFormFile file, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        if (projectId > 0)
        {
            var canEdit = await _projectAuthService.CanEditProjectAsync(userId, isAdmin, projectId, cancellationToken);
            if (!canEdit)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Yetkisiz İşlem",
                    Detail = "Bu projeye görsel yükleme yetkiniz bulunmuyor.",
                    Instance = HttpContext.Request.Path
                });
            }
        }
        else
        {
            var canCreate = await _projectAuthService.CanCreateProjectAsync(userId, isAdmin, cancellationToken);
            if (!canCreate)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Yetkisiz İşlem",
                    Detail = "Görsel yükleme yetkiniz bulunmuyor.",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = "Lütfen geçerli bir medya dosyası seçin." });

        if (file.Length > MaxMediaSize)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new ProblemDetails { Status = StatusCodes.Status413PayloadTooLarge, Detail = "Görsel/medya boyutu azami 10 MB olabilir." });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedMediaExtensions.Contains(ext) || !AllowedMediaMimeTypes.Contains(file.ContentType))
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = "Desteklenmeyen görsel formatı. Yalnızca PNG, JPG, JPEG ve WEBP formatları yüklenebilir." });
        }

        var folder = projectId > 0 ? $"projects/{projectId}/media" : "projects/temp/media";
        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType, folder, isPrivate: false, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni bir teknoloji tanımı oluşturur (Yalnızca Admin).
    /// </summary>
    [HttpPost("technologies")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(typeof(TechnologyDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TechnologyDto>> CreateTechnology([FromBody] CreateTechnologyRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adminService.CreateTechnologyAsync(request, cancellationToken);
            return CreatedAtAction(nameof(CreateTechnology), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = ex.Message });
        }
    }

    /// <summary>
    /// Yeni bir lokasyon tanımı oluşturur (Yalnızca Admin).
    /// </summary>
    [HttpPost("locations")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adminService.CreateLocationAsync(request, cancellationToken);
            return CreatedAtAction(nameof(CreateLocation), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = ex.Message });
        }
    }
}
