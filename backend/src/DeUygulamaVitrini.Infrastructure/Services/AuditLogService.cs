using System.Security.Claims;
using System.Text.Json;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace DeUygulamaVitrini.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuditLogService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    private int? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated) return null;

        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (int.TryParse(idClaim, out var id) && id > 0) return id;

        // Fallback: resolve from username/email if ID claim was not found
        var name = user.Identity.Name;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var appUser = _userManager.FindByNameAsync(name).GetAwaiter().GetResult()
                ?? _userManager.FindByEmailAsync(name).GetAwaiter().GetResult();
            if (appUser != null) return appUser.Id;
        }

        return null;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        string? entityDisplayName,
        string description,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = GetCurrentUserId();
        await LogWithActorAsync(actorUserId, action, entityType, entityId, entityDisplayName, description, metadata, cancellationToken);
    }

    public async Task LogWithActorAsync(
        int? actorUserId,
        string action,
        string entityType,
        string? entityId,
        string? entityDisplayName,
        string description,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedUserId = actorUserId ?? GetCurrentUserId();
        string? actorName = null;
        string? actorEmail = null;

        if (resolvedUserId.HasValue && resolvedUserId.Value > 0)
        {
            var user = await _userManager.FindByIdAsync(resolvedUserId.Value.ToString());
            if (user != null)
            {
                actorEmail = user.Email;

                // AUTHORITATIVE ACTOR = ApplicationUser identity.
                // Domain rule: Member.UserId is an organisational link and MUST NOT
                // override the authenticated account's display identity in audit history.
                // A linked Member profile is irrelevant for security audit purposes.
                //
                // Resolution order (per spec):
                //   1. ApplicationUser.FirstName + LastName (trimmed, when meaningful)
                //   2. ApplicationUser.UserName
                //   3. ApplicationUser.Email
                var userFullName = $"{user.FirstName} {user.LastName}".Trim();
                if (!string.IsNullOrWhiteSpace(userFullName))
                {
                    actorName = userFullName;
                }
                else if (!string.IsNullOrWhiteSpace(user.UserName))
                {
                    actorName = user.UserName;
                }
                else
                {
                    actorName = user.Email;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(actorName))
        {
            actorName = "Sistem";
        }

        string? metadataJson = null;
        if (metadata != null)
        {
            try
            {
                metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                metadataJson = null;
            }
        }

        var log = new AuditLog
        {
            OccurredAtUtc = DateTime.UtcNow,
            ActorUserId = resolvedUserId,
            ActorDisplayNameSnapshot = actorName,
            ActorEmailSnapshot = actorEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityDisplayNameSnapshot = entityDisplayName,
            Description = description,
            MetadataJson = metadataJson
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
