using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Exceptions;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Application.DTOs.Projects;
using DeUygulamaVitrini.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IProjectAiSummaryService _aiSummaryService;
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public ProjectsController(
        IProjectService projectService,
        IProjectAiSummaryService aiSummaryService,
        IApplicationDbContext context,
        IFileStorageService fileStorageService)
    {
        _projectService = projectService;
        _aiSummaryService = aiSummaryService;
        _context = context;
        _fileStorageService = fileStorageService;
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
    /// Yayınlanmış projeleri filtreleme, arama, sıralama ve sayfalama ile listeler.
    /// </summary>
    /// <param name="queryParams">Filtreleme, arama, sıralama ve sayfalama parametreleri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Sayfalanmış proje listesi.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProjectListItemDto>>> GetProjects(
        [FromQuery] ProjectQueryParameters queryParams,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(queryParams, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kimlik numarasına göre yayınlanmış bir projenin tüm detaylarını getirir.
    /// </summary>
    /// <param name="id">Proje benzersiz kimlik numarası (integer).</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Proje detay DTO'su.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetProjectById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Proje Bulunamadı",
                Detail = $"ID değeri '{id}' olan yayınlanmış bir proje bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(project);
    }

    /// <summary>
    /// SEO dostu slug ifadesine göre yayınlanmış bir projenin tüm detaylarını getirir.
    /// </summary>
    /// <param name="slug">Proje slug kelimesi (Örn: "saha-veri-takip-sistemi").</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Proje detay DTO'su.</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetProjectBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectBySlugAsync(slug, cancellationToken);
        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Proje Bulunamadı",
                Detail = $"Slug değeri '{slug}' olan yayınlanmış bir proje bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(project);
    }

    /// <summary>
    /// Kimlik numarasına göre yetkilendirilmiş proje için yapay zeka destekli kurumsal özet üretir.
    /// </summary>
    /// <param name="id">Proje benzersiz kimlik numarası.</param>
    /// <param name="request">İsteğe bağlı dil parametresini içeren DTO.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Yapılandırılmış AI proje özeti.</returns>
    [HttpPost("{id:int}/ai-summary")]
    [ProducesResponseType(typeof(ProjectAiSummaryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ProjectAiSummaryResponseDto>> GetAiSummary(
        [FromRoute] int id,
        [FromBody] ProjectAiSummaryRequestDto? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _aiSummaryService.GenerateSummaryAsync(
                projectId: id,
                language: request?.Language,
                currentUserId: GetCurrentUserId(),
                isAdmin: IsAdminUser(),
                cancellationToken: cancellationToken);

            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Proje Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Erişim Engellendi",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (GenerationProviderUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Yapay Zeka Servisi Kullanılamıyor",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz Parametre",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Kimlik doğrulaması ve proje yetkilendirmesi yapılmış kullanıcılar için özel proje dokümanını güvenli şekilde sunar (stream).
    /// Anonim veya yetkisiz erişimler engellenir; sunucu içi fiziksel dosya yolları ifşa edilmez.
    /// </summary>
    /// <param name="projectId">Proje benzersiz kimlik numarası.</param>
    /// <param name="documentId">Doküman benzersiz kimlik numarası.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>İndirilebilir dosya akışı (FileResult).</returns>
    [HttpGet("{projectId:int}/documents/{documentId:int}/download")]
    [HttpGet("{projectId:int}/documents/{documentId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(
        [FromRoute] int projectId,
        [FromRoute] int documentId,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.ProjectDocuments)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted, cancellationToken);

        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Proje Bulunamadı",
                Detail = $"ID={projectId} olan proje bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        var document = project.ProjectDocuments.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Doküman Bulunamadı",
                Detail = $"ID={documentId} olan doküman bu projede bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        // Yetkilendirme Kontrolü
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsAdminUser();

        var isAuthorized = isAdmin ||
            (project.IsPublished && project.ApprovalStatus == Domain.Enums.ProjectApprovalStatus.Approved) ||
            (currentUserId > 0 && project.CreatedByUserId == currentUserId);

        if (!isAuthorized)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Erişim Engellendi",
                Detail = "Bu projenin özel dokümanlarını indirme yetkiniz bulunmamaktadır.",
                Instance = HttpContext.Request.Path
            });
        }

        var physicalPath = _fileStorageService.GetPrivatePhysicalFilePath(document.FileUrl);
        if (string.IsNullOrEmpty(physicalPath) || !System.IO.File.Exists(physicalPath))
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Dosya Bulunamadı",
                Detail = "İstenen doküman dosyası depolama alanında bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        var contentType = ResolveSafeContentType(document.FileName, document.DocumentType);
        var downloadFileName = ResolveSafeDownloadFileName(document.FileName, document.Name);

        Response.Headers["X-Content-Type-Options"] = "nosniff";

        var fileStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(fileStream, contentType, downloadFileName, enableRangeProcessing: true);
    }

    private static string ResolveSafeContentType(string fileName, string? declaredType)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".txt" => "text/plain; charset=utf-8",
            _ => "application/octet-stream"
        };
    }

    private static string ResolveSafeDownloadFileName(string fileName, string documentName)
    {
        var rawName = !string.IsNullOrWhiteSpace(fileName) ? fileName : documentName;
        var ext = Path.GetExtension(rawName);
        var baseName = Path.GetFileNameWithoutExtension(rawName);

        var cleanBase = System.Text.RegularExpressions.Regex.Replace(baseName, @"[^\w\.\-\s]", "_").Trim();
        if (string.IsNullOrWhiteSpace(cleanBase)) cleanBase = "document";
        if (cleanBase.Length > 80) cleanBase = cleanBase.Substring(0, 80);

        var cleanExt = string.IsNullOrWhiteSpace(ext) ? ".pdf" : ext.ToLowerInvariant();
        return $"{cleanBase}{cleanExt}";
    }
}
