using DeUygulamaVitrini.Application.Common.Models;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Kimlik doğrulama sağlayıcısından (Identity, AD, LDAP, Entra ID) bağımsız
/// CAPTCHA meydan okuması üretme ve yanıtlama servisi soyutlaması.
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// Yeni bir görsel CAPTCHA meydan okuması üretir, veriyi güvenli şekilde saklar
    /// ve istemciye iletilecek güvenli DTO'yu döner.
    /// </summary>
    Task<CaptchaChallengeResponseDto> CreateChallengeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// İstemci tarafından gönderilen CAPTCHA yanıtını doğrular ve tek kullanımlık semantiği uygular.
    /// </summary>
    Task<CaptchaValidationResult> ValidateChallengeAsync(string? challengeId, string? answer, CancellationToken cancellationToken = default);

    /// <summary>
    /// CAPTCHA özelliğinin aktif olup olmadığını döner.
    /// </summary>
    bool IsEnabled { get; }
}
