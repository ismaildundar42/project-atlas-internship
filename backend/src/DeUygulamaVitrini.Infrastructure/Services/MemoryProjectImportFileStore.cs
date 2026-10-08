using System.Security.Cryptography;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DeUygulamaVitrini.Infrastructure.Services;

public class MemoryProjectImportFileStore : IProjectImportFileStore
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryProjectImportFileStore> _logger;
    private const string CacheKeyPrefix = "ProjectImportSession_";

    public MemoryProjectImportFileStore(
        IMemoryCache cache,
        ILogger<MemoryProjectImportFileStore> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<string> StoreAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        int ownerUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var fileToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        var now = DateTime.UtcNow;
        var session = new ProjectImportSession
        {
            FileToken = fileToken,
            FileName = fileName,
            OwnerUserId = ownerUserId,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(ttl),
            FileBytes = fileBytes,
            ContentType = contentType
        };

        var cacheKey = CacheKeyPrefix + fileToken;
        _cache.Set(cacheKey, session, ttl);

        _logger.LogInformation(
            "Project import session created. TokenPrefix={TokenPrefix}, OwnerUserId={OwnerUserId}, FileBytesLength={Length}, TTL={TTL}",
            fileToken[..8], ownerUserId, fileBytes.Length, ttl);

        return Task.FromResult(fileToken);
    }

    public Task<ProjectImportSession?> GetAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileToken))
        {
            return Task.FromResult<ProjectImportSession?>(null);
        }

        var cacheKey = CacheKeyPrefix + fileToken.Trim().ToLowerInvariant();
        if (_cache.TryGetValue(cacheKey, out ProjectImportSession? session) && session != null)
        {
            if (session.OwnerUserId == currentUserId && session.Status != ProjectImportSessionStatus.Consumed)
            {
                return Task.FromResult<ProjectImportSession?>(session);
            }

            _logger.LogWarning(
                "Access denied for import session. TokenPrefix={TokenPrefix}, RequestedUserId={RequestedUserId}, ActualOwnerUserId={ActualOwnerUserId}",
                fileToken[..Math.Min(8, fileToken.Length)], currentUserId, session.OwnerUserId);
        }

        return Task.FromResult<ProjectImportSession?>(null);
    }

    public Task<ProjectImportSession?> TryAcquireForConfirmAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileToken))
        {
            return Task.FromResult<ProjectImportSession?>(null);
        }

        var cacheKey = CacheKeyPrefix + fileToken.Trim().ToLowerInvariant();
        if (_cache.TryGetValue(cacheKey, out ProjectImportSession? session) && session != null)
        {
            if (session.OwnerUserId != currentUserId)
            {
                _logger.LogWarning(
                    "TryAcquireForConfirm access denied. TokenPrefix={TokenPrefix}, RequestedUserId={RequestedUserId}, ActualOwnerUserId={ActualOwnerUserId}",
                    fileToken[..Math.Min(8, fileToken.Length)], currentUserId, session.OwnerUserId);
                return Task.FromResult<ProjectImportSession?>(null);
            }

            lock (session)
            {
                if (session.Status == ProjectImportSessionStatus.Available)
                {
                    session.Status = ProjectImportSessionStatus.Confirming;
                    _logger.LogInformation(
                        "Import session acquired for confirm. TokenPrefix={TokenPrefix}, OwnerUserId={OwnerUserId}",
                        fileToken[..Math.Min(8, fileToken.Length)], currentUserId);
                    return Task.FromResult<ProjectImportSession?>(session);
                }
                else
                {
                    _logger.LogWarning(
                        "Import session acquisition rejected due to active state {Status}. TokenPrefix={TokenPrefix}, OwnerUserId={OwnerUserId}",
                        session.Status, fileToken[..Math.Min(8, fileToken.Length)], currentUserId);
                    return Task.FromResult<ProjectImportSession?>(null);
                }
            }
        }

        return Task.FromResult<ProjectImportSession?>(null);
    }

    public Task ReleaseConfirmLockAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileToken))
        {
            return Task.CompletedTask;
        }

        var cacheKey = CacheKeyPrefix + fileToken.Trim().ToLowerInvariant();
        if (_cache.TryGetValue(cacheKey, out ProjectImportSession? session) && session != null)
        {
            if (session.OwnerUserId == currentUserId)
            {
                lock (session)
                {
                    if (session.Status == ProjectImportSessionStatus.Confirming)
                    {
                        session.Status = ProjectImportSessionStatus.Available;
                        _logger.LogInformation(
                            "Import session confirm lock released. TokenPrefix={TokenPrefix}, OwnerUserId={OwnerUserId}",
                            fileToken[..Math.Min(8, fileToken.Length)], currentUserId);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileToken))
        {
            return Task.FromResult(false);
        }

        var cacheKey = CacheKeyPrefix + fileToken.Trim().ToLowerInvariant();
        if (_cache.TryGetValue(cacheKey, out ProjectImportSession? session) && session != null)
        {
            if (session.OwnerUserId == currentUserId)
            {
                lock (session)
                {
                    session.Status = ProjectImportSessionStatus.Consumed;
                }
                _cache.Remove(cacheKey);
                _logger.LogInformation("Project import session removed. TokenPrefix={TokenPrefix}, OwnerUserId={OwnerUserId}",
                    fileToken[..Math.Min(8, fileToken.Length)], currentUserId);
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}
