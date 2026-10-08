using DeUygulamaVitrini.Application.DTOs.Lookups;

namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class AdminProjectTeamEditDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class AdminProjectMemberEditDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string? ProjectRole { get; set; }
}

public class AdminProjectIntegrationEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
}

public class AdminProjectDocumentEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
}

public class AdminProjectMediaEditDto
{
    public int Id { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}

public class AdminProjectEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Purpose { get; set; }
    public string? ProblemSolved { get; set; }
    public string? NonTechnicalDescription { get; set; }
    public string? TechnicalDescription { get; set; }
    public string? BusinessImpact { get; set; }
    public string? TargetAudience { get; set; }
    public string? AccessInstructions { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? RepositoryUrl { get; set; }

    public int StatusId { get; set; }
    public int CategoryId { get; set; }
    public string DevelopmentType { get; set; } = "Internal";

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public string? CoverImageUrl { get; set; }

    // Phase 14 Approval Workflow fields
    public DeUygulamaVitrini.Domain.Enums.ProjectApprovalStatus ApprovalStatus { get; set; }
    public string ApprovalStatusName { get; set; } = string.Empty;
    public DateTime? SubmittedForReviewAt { get; set; }
    public int? SubmittedForReviewByUserId { get; set; }
    public string? SubmittedForReviewByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public string? RejectionReason { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<AdminProjectTeamEditDto> Teams { get; set; } = new();
    public List<AdminProjectMemberEditDto> Members { get; set; } = new();
    public List<int> LocationIds { get; set; } = new();
    public List<int> TechnologyIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();

    public List<AdminProjectIntegrationEditDto> Integrations { get; set; } = new();
    public List<AdminProjectDocumentEditDto> Documents { get; set; } = new();
    public List<AdminProjectMediaEditDto> MediaItems { get; set; } = new();
}
