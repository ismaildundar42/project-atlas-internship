namespace DeUygulamaVitrini.Application.Common.Interfaces;

public interface IAuditLogService
{
    /// <summary>
    /// Aktif kullanıcı bağlamını otomatik çözerek bir denetim kaydı (AuditLog) oluşturur.
    /// </summary>
    Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        string? entityDisplayName,
        string description,
        object? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir aktör kullanıcısı için açık olarak denetim kaydı (AuditLog) oluşturur.
    /// </summary>
    Task LogWithActorAsync(
        int? actorUserId,
        string action,
        string entityType,
        string? entityId,
        string? entityDisplayName,
        string description,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}
