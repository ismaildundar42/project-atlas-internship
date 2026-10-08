using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Auth;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICaptchaService _captchaService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ICaptchaService captchaService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _captchaService = captchaService;
    }

    /// <summary>
    /// Kimlik doğrulama için yeni bir görsel CAPTCHA meydan okuması üretir.
    /// Cevap metnini KESİNLİKLE sızdırmaz; yalnızca şeffaf olmayan ChallengeId ve SVG Data URL döner.
    /// </summary>
    [HttpGet("captcha")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("CaptchaRateLimitPolicy")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(CaptchaChallengeResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CaptchaChallengeResponseDto>> GetCaptcha(CancellationToken cancellationToken)
    {
        var challenge = await _captchaService.CreateChallengeAsync(cancellationToken);
        return Ok(challenge);
    }

    /// <summary>
    /// Kullanıcı e-posta, parola ve CAPTCHA yanıtı ile oturum açar.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthUserDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. CAPTCHA Doğrulaması (Kimlik doğrulama katmanından önce çalışır)
        if (_captchaService.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.CaptchaChallengeId) || string.IsNullOrWhiteSpace(request.CaptchaAnswer))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Doğrulama Hatası",
                    Detail = "Doğrulama kodu gereklidir.",
                    Instance = HttpContext.Request.Path
                });
            }

            var captchaResult = await _captchaService.ValidateChallengeAsync(
                request.CaptchaChallengeId,
                request.CaptchaAnswer,
                cancellationToken);

            if (!captchaResult.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Doğrulama Hatası",
                    Detail = captchaResult.ErrorMessage ?? "Doğrulama kodu hatalı veya süresi dolmuş. Lütfen yeni kodu deneyin.",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        // 2. Kimlik Doğrulama Sağlayıcısı (ASP.NET Core Identity / İleride AD/LDAP/SSO)
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Giriş Başarısız",
                Detail = "E-posta adresi veya parola hatalı.",
                Instance = HttpContext.Request.Path
            });
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName ?? user.Email!,
            request.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Giriş Başarısız",
                Detail = "E-posta adresi veya parola hatalı.",
                Instance = HttpContext.Request.Path
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
        var isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);

        return Ok(new AuthUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsSuperAdmin = isSuperAdmin,
            IsAdmin = isAdmin,
            CanCreateProjects = user.CanCreateProjects,
            Roles = roles.ToList()
        });
    }

    /// <summary>
    /// Mevcut aktif oturumu sonlandırır.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Oturum başarıyla sonlandırıldı." });
    }

    /// <summary>
    /// Mevcut oturum açmış kullanıcının temel profil bilgilerini getirir.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthUserDto>> GetCurrentUser()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Oturum Açılmadı",
                Detail = "Bu işlemi gerçekleştirmek için geçerli bir oturum gereklidir.",
                Instance = HttpContext.Request.Path
            });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            await _signInManager.SignOutAsync();
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin);
        var isAdmin = isSuperAdmin || roles.Contains(AppRoles.Admin);

        return Ok(new AuthUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsSuperAdmin = isSuperAdmin,
            IsAdmin = isAdmin,
            CanCreateProjects = user.CanCreateProjects,
            Roles = roles.ToList()
        });
    }
}
