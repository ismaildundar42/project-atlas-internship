using DeUygulamaVitrini.Application.DTOs.Notifications;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(
        int recipientUserId,
        string type,
        string title,
        string message,
        int? projectId = null,
        string? targetUrl = null,
        CancellationToken cancellationToken = default);

    Task CreateNotificationsForAdminsAsync(
        string type,
        string title,
        string message,
        int? projectId = null,
        string? targetUrl = null,
        CancellationToken cancellationToken = default);

    Task<List<NotificationDto>> GetUserNotificationsAsync(
        int userId,
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        long notificationId,
        int userId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteNotificationAsync(
        long notificationId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<int> ClearReadNotificationsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
