using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Captcha;

/// <summary>
/// Kimlik doğrulama katmanından tamamen bağımsız CAPTCHA yönetim servisi.
/// Meydan okuma üretir, görseli çizer, tek kullanımlık kontrolü uygular ve doğrular.
/// </summary>
public class CaptchaService : ICaptchaService
{
    private readonly ICaptchaChallengeStore _store;
    private readonly CaptchaOptions _options;
    private readonly ILogger<CaptchaService> _logger;

    public CaptchaService(
        ICaptchaChallengeStore store,
        IOptions<CaptchaOptions> options,
        ILogger<CaptchaService> logger)
    {
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsEnabled => _options.Enabled;

    public async Task<CaptchaChallengeResponseDto> CreateChallengeAsync(CancellationToken cancellationToken = default)
    {
        var code = CaptchaGenerator.GenerateCode(_options.Length);
        var challengeId = CaptchaGenerator.GenerateChallengeId();
        var hashedAnswer = CaptchaGenerator.ComputeHash(code, challengeId, _options.CaseSensitive);
        var imageDataUrl = CaptchaVisualRenderer.RenderAsDataUrl(code);

        var ttl = TimeSpan.FromMinutes(_options.ExpirationMinutes > 0 ? _options.ExpirationMinutes : 3);
        var challengeData = new CaptchaChallengeData
        {
            ChallengeId = challengeId,
            HashedAnswer = hashedAnswer,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl),
            IsConsumed = false
        };

        await _store.StoreChallengeAsync(challengeId, challengeData, ttl, cancellationToken);
        _logger.LogInformation("New CAPTCHA challenge created. ChallengeId={ChallengeId}, TTLSeconds={TTLSeconds}", challengeId, ttl.TotalSeconds);

        return new CaptchaChallengeResponseDto
        {
            ChallengeId = challengeId,
            ImageDataUrl = imageDataUrl,
            ExpiresInSeconds = (int)ttl.TotalSeconds
        };
    }

    public async Task<CaptchaValidationResult> ValidateChallengeAsync(string? challengeId, string? answer, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("CAPTCHA is disabled in configuration. Skipping validation.");
            return CaptchaValidationResult.Success();
        }

        if (string.IsNullOrWhiteSpace(challengeId) || string.IsNullOrWhiteSpace(answer))
        {
            _logger.LogWarning("CAPTCHA validation failed: Missing challengeId or answer.");
            return CaptchaValidationResult.Failed(
                "MissingInput",
                "Doğrulama kodu gereklidir.");
        }

        // Atomik tek kullanımlık tüketim (Replay Attack ve brute-force engelleme)
        var challenge = await _store.ConsumeChallengeAsync(challengeId.Trim(), cancellationToken);
        if (challenge == null)
        {
            _logger.LogWarning("CAPTCHA validation failed: Challenge not found, already consumed, or expired. ChallengeIdPrefix={Prefix}",
                challengeId.Length > 8 ? challengeId[..8] : challengeId);
            return CaptchaValidationResult.Failed(
                "NotFoundOrConsumed",
                "Doğrulama kodu hatalı veya süresi dolmuş. Lütfen yeni kodu deneyin.");
        }

        if (challenge.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("CAPTCHA validation failed: Challenge expired. ChallengeIdPrefix={Prefix}",
                challengeId.Length > 8 ? challengeId[..8] : challengeId);
            return CaptchaValidationResult.Failed(
                "Expired",
                "Doğrulama kodunun süresi dolmuş. Lütfen yeni kodu deneyin.");
        }

        var isCorrect = CaptchaGenerator.VerifyHash(answer, challengeId.Trim(), challenge.HashedAnswer, _options.CaseSensitive);
        if (!isCorrect)
        {
            _logger.LogWarning("CAPTCHA validation failed: Answer mismatch. ChallengeIdPrefix={Prefix}",
                challengeId.Length > 8 ? challengeId[..8] : challengeId);
            return CaptchaValidationResult.Failed(
                "AnswerMismatch",
                "Doğrulama kodu hatalı veya süresi dolmuş. Lütfen yeni kodu deneyin.");
        }

        _logger.LogInformation("CAPTCHA validation succeeded. ChallengeIdPrefix={Prefix}",
            challengeId.Length > 8 ? challengeId[..8] : challengeId);
        return CaptchaValidationResult.Success();
    }
}
