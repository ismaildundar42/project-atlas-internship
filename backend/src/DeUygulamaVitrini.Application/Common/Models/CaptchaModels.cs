namespace DeUygulamaVitrini.Application.Common.Models;

/// <summary>
/// Sunucu tarafında üretilen ve geçici bellekte saklanan CAPTCHA doğrulama verisi.
/// Düz metin (plaintext) parola/cevap saklanmaz; yalnızca kriptografik özet (hash) tutulur.
/// </summary>
public class CaptchaChallengeData
{
    public string ChallengeId { get; set; } = string.Empty;
    public string HashedAnswer { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsConsumed { get; set; }
}

/// <summary>
/// İstemciye (tarayıcıya) iletilen genel CAPTCHA meydan okuma modeli.
/// Düz metin cevabı KESİNLİKLE içermez.
/// </summary>
public class CaptchaChallengeResponseDto
{
    public string ChallengeId { get; set; } = string.Empty;
    public string ImageDataUrl { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}

/// <summary>
/// CAPTCHA doğrulama denemesinin sonucunu ifade eden model.
/// </summary>
public class CaptchaValidationResult
{
    public bool IsValid { get; set; }
    public string? FailureReason { get; set; }
    public string? ErrorMessage { get; set; }

    public static CaptchaValidationResult Success() => new() { IsValid = true };

    public static CaptchaValidationResult Failed(string reason, string message) => new()
    {
        IsValid = false,
        FailureReason = reason,
        ErrorMessage = message
    };
}

/// <summary>
/// CAPTCHA yapılandırma ayarları.
/// </summary>
public class CaptchaOptions
{
    public const string SectionName = "Captcha";

    /// <summary>
    /// CAPTCHA doğrulamasının aktif olup olmadığını belirler.
    /// Kurumsal SSO (Entra ID, ADFS vb.) durumunda false yapılabilir.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Üretilecek alfanümerik kodun uzunluğu (Varsayılan: 5 karakter).
    /// </summary>
    public int Length { get; set; } = 5;

    /// <summary>
    /// CAPTCHA meydan okumasının geçerlilik süresi (dakika cinsinden, Varsayılan: 3).
    /// </summary>
    public int ExpirationMinutes { get; set; } = 3;

    /// <summary>
    /// Büyük/küçük harf duyarlılığı. Kullanıcı deneyimi için varsayılan: false (büyük/küçük harf farksız).
    /// </summary>
    public bool CaseSensitive { get; set; } = false;
}
