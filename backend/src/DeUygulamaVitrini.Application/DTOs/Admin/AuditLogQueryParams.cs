namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class AuditLogQueryParams
{
    public string? Search { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? ActorUserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
