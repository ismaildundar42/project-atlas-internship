namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class AdminProjectQueryParams
{
    public string? Search { get; set; }
    public string PublicationState { get; set; } = "all"; // "all", "published", "draft"
    public string ApprovalState { get; set; } = "all"; // "all", "draft", "pendingreview", "approved", "rejected"
    public string LifecycleState { get; set; } = "active"; // "active", "archived", "all"
    public int? StatusId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "updatedat";
    public string SortDirection { get; set; } = "desc";
}
