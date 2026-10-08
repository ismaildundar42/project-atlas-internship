using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

/// <summary>
/// Yönetici Organizasyon (Ekip, Departman, Kişi) yönetimi API uç noktaları.
/// </summary>
[ApiController]
[Route("api/admin/organization")]
[Authorize(Policy = "OrganizationManagementAccess")]
[Produces("application/json")]
public class AdminOrganizationController : ControllerBase
{
    private readonly IAdminOrganizationService _adminOrganizationService;

    public AdminOrganizationController(IAdminOrganizationService adminOrganizationService)
    {
        _adminOrganizationService = adminOrganizationService;
    }

    // ─── DEPARTMENTS ─────────────────────────────────────────────────────────────

    [HttpGet("departments")]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentAdminDto>>> GetDepartments(CancellationToken cancellationToken)
    {
        var result = await _adminOrganizationService.GetDepartmentsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("departments")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _adminOrganizationService.CreateDepartmentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetDepartments), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpPut("departments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _adminOrganizationService.UpdateDepartmentAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Departman Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpDelete("departments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _adminOrganizationService.DeleteDepartmentAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Departman Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    // ─── TEAMS ───────────────────────────────────────────────────────────────────

    [HttpGet("teams")]
    [ProducesResponseType(typeof(IReadOnlyList<TeamAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamAdminDto>>> GetTeams(CancellationToken cancellationToken)
    {
        var result = await _adminOrganizationService.GetTeamsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("teams")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _adminOrganizationService.CreateTeamAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTeams), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpPut("teams/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _adminOrganizationService.UpdateTeamAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Ekip Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpDelete("teams/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteTeam(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _adminOrganizationService.DeleteTeamAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Ekip Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    // ─── MEMBERS ─────────────────────────────────────────────────────────────────

    [HttpGet("members")]
    [ProducesResponseType(typeof(IReadOnlyList<MemberAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MemberAdminDto>>> GetMembers(CancellationToken cancellationToken)
    {
        var result = await _adminOrganizationService.GetMembersAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("members")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _adminOrganizationService.CreateMemberAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetMembers), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpPut("members/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] UpdateMemberRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _adminOrganizationService.UpdateMemberAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Üye Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpDelete("members/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMember(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _adminOrganizationService.DeleteMemberAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Üye Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Çakışma",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    // ─── MEMBER PROJECT ACCESS & ACCOUNT ENDPOINTS (Phase 13.2) ──────────────────

    [HttpPut("members/{memberId:int}/project-access")]
    [ProducesResponseType(typeof(MemberAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberAdminDto>> UpdateMemberProjectAccess(
        int memberId,
        [FromBody] UpdateMemberProjectAccessRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adminOrganizationService.UpdateMemberProjectAccessAsync(memberId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Üye Bulunamadı",
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

    [HttpPost("members/{memberId:int}/account")]
    [ProducesResponseType(typeof(MemberAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberAdminDto>> CreateMemberAccount(
        int memberId,
        [FromBody] CreateMemberAccountRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adminOrganizationService.CreateMemberAccountAsync(memberId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Üye Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz İstek",
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

    [HttpPost("members/{memberId:int}/link-account")]
    [ProducesResponseType(typeof(MemberAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberAdminDto>> LinkMemberAccount(
        int memberId,
        [FromBody] LinkMemberAccountRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adminOrganizationService.LinkMemberAccountAsync(memberId, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Üye Bulunamadı",
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

    // ─── MEMBER ADMIN ROLE MANAGEMENT (Phase 19.7 - SuperAdmin Only) ──────────────

    /// <summary>
    /// Bir kullanıcıya Admin rolü atar veya kaldırır.
    /// Yalnızca Süper Yönetici (SuperAdmin) yetkisine sahip kullanıcılar erişebilir.
    /// </summary>
    [HttpPut("members/{memberId:int}/admin-role")]
    [Authorize(Policy = "SuperAdminAccess")]
    [ProducesResponseType(typeof(MemberAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberAdminDto>> UpdateMemberAdminRole(
        int memberId,
        [FromBody] UpdateMemberAdminRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        var actingUserIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(actingUserIdStr, out var actingUserId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Oturum Açılmadı",
                Detail = "Geçersiz oturum kimliği.",
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            var result = await _adminOrganizationService.UpdateMemberAdminRoleAsync(memberId, request, actingUserId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Üye Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Yetkisiz İşlem",
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
}

