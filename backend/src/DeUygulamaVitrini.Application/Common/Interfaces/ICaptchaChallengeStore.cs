using DeUygulamaVitrini.Application.Common.Models;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// CAPTCHA meydan okuma oturumlarını geçici olarak saklayan ve tek kullanımlık (single-use)
/// semantiği ile tüketen depolama soyutlaması.
/// Tekil sunucu (Memory) veya çoklu bulut örneği (Distributed/Redis) ile değiştirilebilir.
/// </summary>
public interface ICaptchaChallengeStore
{
    /// <summary>
    /// Yeni bir CAPTCHA meydan okumasını belirlenen TTL süresince saklar.
    /// </summary>
    Task StoreChallengeAsync(string challengeId, CaptchaChallengeData challengeData, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut bir meydan okumayı okur (tüketmeden).
    /// </summary>
    Task<CaptchaChallengeData?> GetChallengeAsync(string challengeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir meydan okumayı atomik olarak getirir ve depodan kaldırır/tüketir (Single-use / Tek kullanımlık koruması).
    /// </summary>
    Task<CaptchaChallengeData?> ConsumeChallengeAsync(string challengeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Meydan okumayı depodan kaldırır.
    /// </summary>
    Task RemoveChallengeAsync(string challengeId, CancellationToken cancellationToken = default);
}
