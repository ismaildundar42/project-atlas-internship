using System.ComponentModel.DataAnnotations;

namespace DeUygulamaVitrini.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola zorunludur.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Sunucudan temin edilen şeffaf olmayan (opaque) meydan okuma kimliği.
    /// </summary>
    public string? CaptchaChallengeId { get; set; }

    /// <summary>
    /// Kullanıcının CAPTCHA görseline istinaden girdiği yanıt metni.
    /// </summary>
    public string? CaptchaAnswer { get; set; }
}
