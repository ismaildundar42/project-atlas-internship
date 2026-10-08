using System.Collections.Concurrent;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DeUygulamaVitrini.Infrastructure.Services.Captcha;

/// <summary>
/// Tekil süreç / yerel geliştirme için IMemoryCache tabanlı CAPTCHA meydan okuma deposu.
/// Tek kullanımlık (single-use) ve eşzamanlı istek yarışlarına (race condition) karşı
/// atomik tüketim garantisi sağlar.
/// Azure çoklu-örnek dağıtımında bu sınıf yerine DistributedCaptchaChallengeStore (Redis/Azure Cache) takılabilir.
/// </summary>
public class MemoryCaptchaChallengeStore : ICaptchaChallengeStore
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCaptchaChallengeStore> _logger;
    private static readonly ConcurrentDictionary<string, object> _locks = new();

    private const string KeyPrefix = "captcha_challenge_";

    public MemoryCaptchaChallengeStore(
        IMemoryCache cache,
        ILogger<MemoryCaptchaChallengeStore> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private static string GetCacheKey(string challengeId) => $"{KeyPrefix}{challengeId.Trim()}";

    public Task StoreChallengeAsync(string challengeId, CaptchaChallengeData challengeData, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(challengeId))
        {
            throw new ArgumentException("ChallengeId cannot be null or empty.", nameof(challengeId));
        }

        var key = GetCacheKey(challengeId);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            Priority = CacheItemPriority.High
        };

        // Cache'ten süre dolunca kaldırıldığında loglama veya temizlik callback'i
        options.RegisterPostEvictionCallback((evictedKey, value, reason, state) =>
        {
            if (reason != EvictionReason.Removed && reason != EvictionReason.Replaced)
            {
                _logger.LogDebug("CAPTCHA challenge expired or evicted. Key={Key}, Reason={Reason}", evictedKey, reason);
            }
        });

        _cache.Set(key, challengeData, options);
        _logger.LogDebug("CAPTCHA challenge stored. ChallengeId={ChallengeId}, TTL={TTL}", challengeId, ttl);

        return Task.CompletedTask;
    }

    public Task<CaptchaChallengeData?> GetChallengeAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(challengeId)) return Task.FromResult<CaptchaChallengeData?>(null);

        var key = GetCacheKey(challengeId);
        _cache.TryGetValue<CaptchaChallengeData>(key, out var data);
        return Task.FromResult(data);
    }

    public Task<CaptchaChallengeData?> ConsumeChallengeAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(challengeId)) return Task.FromResult<CaptchaChallengeData?>(null);

        var key = GetCacheKey(challengeId);
        var syncLock = _locks.GetOrAdd(key, _ => new object());

        lock (syncLock)
        {
            try
            {
                if (_cache.TryGetValue<CaptchaChallengeData>(key, out var data) && data != null)
                {
                    // Atomik olarak depodan kaldır
                    _cache.Remove(key);
                    data.IsConsumed = true;
                    _logger.LogDebug("CAPTCHA challenge consumed atomically. ChallengeId={ChallengeId}", challengeId);
                    return Task.FromResult<CaptchaChallengeData?>(data);
                }

                return Task.FromResult<CaptchaChallengeData?>(null);
            }
            finally
            {
                _locks.TryRemove(key, out _);
            }
        }
    }

    public Task RemoveChallengeAsync(string challengeId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(challengeId))
        {
            var key = GetCacheKey(challengeId);
            _cache.Remove(key);
            _locks.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
