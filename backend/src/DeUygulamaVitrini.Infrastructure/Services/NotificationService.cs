using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Notifications;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task CreateNotificationAsync(
        int recipientUserId,
        string type,
        string title,
        string message,
        int? projectId = null,
        string? targetUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (recipientUserId <= 0) return;

        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            ProjectId = projectId,
            TargetUrl = targetUrl,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateNotificationsForAdminsAsync(
        string type,
        string title,
        string message,
        int? projectId = null,
        string? targetUrl = null,
        CancellationToken cancellationToken = default)
    {
        var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);

        var activeAdmins = admins
            .Concat(superAdmins)
            .Where(u => u.IsActive)
            .DistinctBy(u => u.Id)
            .ToList();

        if (activeAdmins.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var admin in activeAdmins)
        {
            _context.Notifications.Add(new Notification
            {
                RecipientUserId = admin.Id,
                Type = type,
                Title = title,
                Message = message,
                ProjectId = projectId,
                TargetUrl = targetUrl,
                IsRead = false,
                CreatedAtUtc = now
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(
        int userId,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return new List<NotificationDto>();

        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(take)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                ProjectId = n.ProjectId,
                TargetUrl = n.TargetUrl,
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc,
                ReadAtUtc = n.ReadAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return 0;

        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientUserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(
        long notificationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (notificationId <= 0 || userId <= 0) return;

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId, cancellationToken);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return;

        var unreadNotifications = await _context.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadNotifications.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                n.ReadAtUtc = now;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> DeleteNotificationAsync(
        long notificationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (notificationId <= 0 || userId <= 0) return false;

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId, cancellationToken);

        if (notification == null) return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> ClearReadNotificationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return 0;

        var readNotifications = await _context.Notifications
            .Where(n => n.RecipientUserId == userId && n.IsRead)
            .ToListAsync(cancellationToken);

        if (readNotifications.Count == 0) return 0;

        _context.Notifications.RemoveRange(readNotifications);
        await _context.SaveChangesAsync(cancellationToken);
        return readNotifications.Count;
    }
}
