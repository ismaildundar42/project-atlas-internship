using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeUygulamaVitrini.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    /// <summary>
    /// Oturum açmış kullanıcının son bildirimlerini getirir.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        var safeTake = Math.Clamp(take, 1, 50);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, safeTake, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Oturum açmış kullanıcının okunmamış bildirim sayısını getirir.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        var unreadCount = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new { unreadCount });
    }

    /// <summary>
    /// Belirtilen bildirimi okundu olarak işaretler.
    /// </summary>
    [HttpPost("{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(long id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);
        return Ok(new { id, isRead = true });
    }

    /// <summary>
    /// Kullanıcının tüm bildirimlerini okundu olarak işaretler.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new { message = "Tüm bildirimler okundu olarak işaretlendi." });
    }

    /// <summary>
    /// Kullanıcının belirli bir bildirimini siler.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(long id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        var deleted = await _notificationService.DeleteNotificationAsync(id, userId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Bildirim bulunamadı",
                Detail = "Belirtilen bildirim bulunamadı veya silme yetkiniz yok."
            });
        }

        return Ok(new { id, message = "Bildirim silindi." });
    }

    /// <summary>
    /// Kullanıcının tüm okunmuş bildirimlerini siler.
    /// </summary>
    [HttpDelete("read")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearReadNotifications(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        var deletedCount = await _notificationService.ClearReadNotificationsAsync(userId, cancellationToken);
        return Ok(new { deletedCount, message = $"{deletedCount} adet okunmuş bildirim temizlendi." });
    }
}
