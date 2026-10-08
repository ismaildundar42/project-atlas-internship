using System.Security.Claims;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Profile;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AuthenticatedUser")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(
        IProfileService profileService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _profileService = profileService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Oturum açmış kullanıcının detaylı profilini, organizasyonel bağlantısını ve projelerini getirir.
    /// </summary>
    [HttpGet]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponseDto>> GetProfile(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Oturum Açılmadı",
                Detail = "Geçerli bir kullanıcı kimliği bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            var profile = await _profileService.GetProfileAsync(userId, cancellationToken);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Profil Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Oturum açmış kullanıcının kendi şifresini değiştirmesini sağlar.
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Oturum Açılmadı",
                Detail = "Geçerli bir kullanıcı kimliği bulunamadı.",
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            await _profileService.ChangePasswordAsync(userId, request, cancellationToken);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
            return Ok(new { message = "Şifreniz başarıyla değiştirildi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Şifre Değiştirilemedi",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Kullanıcı Bulunamadı",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }
}
