using DeUygulamaVitrini.Application.Common.Models;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

public interface IProjectImportFileStore
{
    Task<string> StoreAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        int ownerUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task<ProjectImportSession?> GetAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectImportSession?> TryAcquireForConfirmAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task ReleaseConfirmLockAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        string fileToken,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
