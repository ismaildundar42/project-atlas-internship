namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class AuditLogDto
{
    public long Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorDisplayName { get; set; }
    public string? ActorDisplayNameSnapshot => ActorDisplayName;
    public string? ActorEmail { get; set; }
    public string? ActorEmailSnapshot => ActorEmail;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? EntityDisplayName { get; set; }
    public string? EntityDisplayNameSnapshot => EntityDisplayName;
    public string Description { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
}
