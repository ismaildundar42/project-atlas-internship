using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Application.DTOs.Admin;
using DeUygulamaVitrini.Application.DTOs.Lookups;
using DeUygulamaVitrini.Application.DTOs.Projects;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly Microsoft.AspNetCore.Identity.UserManager<DeUygulamaVitrini.Domain.Entities.Identity.ApplicationUser> _userManager;

    public AdminService(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        Microsoft.AspNetCore.Identity.UserManager<DeUygulamaVitrini.Domain.Entities.Identity.ApplicationUser> userManager)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _userManager = userManager;
    }

    private async Task<string> ResolveUserDisplayNameAsync(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0) return "Kullanıcı";

        var member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        if (member != null && (!string.IsNullOrWhiteSpace(member.FirstName) || !string.IsNullOrWhiteSpace(member.LastName)))
        {
            return $"{member.FirstName} {member.LastName}".Trim();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
            return user.UserName ?? user.Email ?? "Kullanıcı";
        }

        return "Kullanıcı";
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.AsNoTracking();

        var totalProjects = await query.CountAsync(cancellationToken);
        var publishedProjects = await query.CountAsync(p => p.IsPublished, cancellationToken);
        var draftProjects = await query.CountAsync(p => !p.IsPublished, cancellationToken);
        var featuredProjects = await query.CountAsync(p => p.IsFeatured, cancellationToken);

        var missingDescriptionCount = await query.CountAsync(
            p => p.ShortDescription == null || p.ShortDescription.Trim() == "",
            cancellationToken);

        var missingTeamCount = await query.CountAsync(
            p => !p.ProjectTeams.Any(),
            cancellationToken);

        var missingLocationCount = await query.CountAsync(
            p => !p.ProjectLocations.Any(),
            cancellationToken);

        var recentProjects = await query
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(5)
            .Select(p => new AdminRecentProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                StatusName = p.Status != null ? p.Status.Name : string.Empty,
                StatusCode = p.Status != null ? p.Status.Code : string.Empty,
                IsPublished = p.IsPublished,
                IsFeatured = p.IsFeatured,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new AdminDashboardDto
        {
            TotalProjects = totalProjects,
            PublishedProjects = publishedProjects,
            DraftProjects = draftProjects,
            FeaturedProjects = featuredProjects,
            ContentAttention = new AdminContentAttentionDto
            {
                DraftProjectsCount = draftProjects,
                MissingDescriptionCount = missingDescriptionCount,
                MissingTeamCount = missingTeamCount,
                MissingLocationCount = missingLocationCount
            },
            RecentProjects = recentProjects
        };
    }

    public async Task<PagedResult<AdminProjectListItemDto>> GetAdminProjectsAsync(
        AdminProjectQueryParams parameters,
        int? createdByUserIdFilter = null,
        CancellationToken cancellationToken = default)
    {
        var isArchivedQuery = string.Equals(parameters.LifecycleState, "archived", StringComparison.OrdinalIgnoreCase);
        var isAllLifecycleQuery = string.Equals(parameters.LifecycleState, "all", StringComparison.OrdinalIgnoreCase);

        var query = isArchivedQuery || isAllLifecycleQuery
            ? _context.Projects.IgnoreQueryFilters().AsNoTracking()
            : _context.Projects.AsNoTracking();

        if (isArchivedQuery)
        {
            query = query.Where(p => p.IsDeleted);
        }
        else if (!isAllLifecycleQuery)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        // Yalnızca belirli kullanıcının oluşturduğu projeler filtresi
        if (createdByUserIdFilter.HasValue && createdByUserIdFilter.Value > 0)
        {
            query = query.Where(p => p.CreatedByUserId == createdByUserIdFilter.Value);
        }

        // Yayın durumu filtresi (all, published, draft)
        if (string.Equals(parameters.PublicationState, "published", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.IsPublished);
        }
        else if (string.Equals(parameters.PublicationState, "draft", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => !p.IsPublished);
        }

        // Approval State filtresi (all, draft, pending_review, pendingreview, approved, rejected)
        var normalizedApprovalState = parameters.ApprovalState?.Replace("_", "").Replace("-", "");
        if (string.Equals(normalizedApprovalState, "draft", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.Draft);
        }
        else if (string.Equals(normalizedApprovalState, "pendingreview", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.PendingReview);
        }
        else if (string.Equals(normalizedApprovalState, "approved", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.Approved);
        }
        else if (string.Equals(normalizedApprovalState, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.ApprovalStatus == ProjectApprovalStatus.Rejected);
        }

        // Proje durum filtresi (StatusId)
        if (parameters.StatusId.HasValue && parameters.StatusId > 0)
        {
            query = query.Where(p => p.StatusId == parameters.StatusId.Value);
        }

        // Metin arama filtresi
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.ShortDescription.ToLower().Contains(searchTerm));
        }

        // Sıralama
        var isAscending = string.Equals(parameters.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var sortBy = parameters.SortBy?.ToLowerInvariant() ?? "updatedat";

        query = sortBy switch
        {
            "name" => isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "createdat" => isAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            "startdate" => isAscending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
            _ => isAscending ? query.OrderBy(p => p.UpdatedAt ?? p.CreatedAt) : query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new AdminProjectListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                DevelopmentType = p.DevelopmentType.ToString(),
                ApprovalStatus = p.ApprovalStatus,
                ApprovalStatusName = p.ApprovalStatus == ProjectApprovalStatus.Draft ? "Taslak" :
                                     p.ApprovalStatus == ProjectApprovalStatus.PendingReview ? "İnceleme Bekliyor" :
                                     p.ApprovalStatus == ProjectApprovalStatus.Approved ? "Onaylandı" : "Reddedildi",
                IsPublished = p.IsPublished,
                IsFeatured = p.IsFeatured,
                CoverImageUrl = p.CoverImageUrl ?? p.ProjectMediaItems
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.FileUrl)
                    .FirstOrDefault(),
                CreatedByUserId = p.CreatedByUserId,
                CreatedByUserName = p.CreatedByUser != null ? (p.CreatedByUser.FirstName + " " + p.CreatedByUser.LastName).Trim() : null,
                IsDeleted = p.IsDeleted,
                DeletedAt = p.DeletedAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Status = new ProjectStatusDto
                {
                    Id = p.Status.Id,
                    Name = p.Status.Name,
                    Code = p.Status.Code,
                    Description = p.Status.Description,
                    DisplayOrder = p.Status.DisplayOrder
                },
                Category = new ProjectCategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Code = p.Category.Code,
                    Description = p.Category.Description,
                    DisplayOrder = p.Category.DisplayOrder
                },
                PrimaryTeamName = p.ProjectTeams
                    .Where(pt => pt.IsPrimary)
                    .Select(pt => pt.Team.Name)
                    .FirstOrDefault() ?? p.ProjectTeams
                    .Select(pt => pt.Team.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminProjectListItemDto>(items, parameters.PageNumber, parameters.PageSize, totalCount);
    }

    public async Task<AdminProjectEditDto?> GetProjectForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .Include(p => p.SubmittedForReviewByUser)
            .Include(p => p.ReviewedByUser)
            .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team)
            .Include(p => p.ProjectMembers).ThenInclude(pm => pm.Member)
            .Include(p => p.ProjectLocations)
            .Include(p => p.ProjectTechnologies)
            .Include(p => p.ProjectTags)
            .Include(p => p.ProjectIntegrations)
            .Include(p => p.ProjectDocuments)
            .Include(p => p.ProjectMediaItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
            return null;

        return new AdminProjectEditDto
        {
            Id = project.Id,
            Name = project.Name,
            Slug = project.Slug,
            ShortDescription = project.ShortDescription,
            Description = project.Description,
            Purpose = project.Purpose,
            ProblemSolved = project.ProblemSolved,
            NonTechnicalDescription = project.NonTechnicalDescription,
            TechnicalDescription = project.TechnicalDescription,
            BusinessImpact = project.BusinessImpact,
            TargetAudience = project.TargetAudience,
            AccessInstructions = project.AccessInstructions,
            ApplicationUrl = project.ApplicationUrl,
            RepositoryUrl = project.RepositoryUrl,
            CoverImageUrl = project.CoverImageUrl,
            StatusId = project.StatusId,
            CategoryId = project.CategoryId,
            DevelopmentType = project.DevelopmentType.ToString(),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            IsPublished = project.IsPublished,
            IsFeatured = project.IsFeatured,

            ApprovalStatus = project.ApprovalStatus,
            ApprovalStatusName = project.ApprovalStatus == ProjectApprovalStatus.Draft ? "Taslak" :
                                 project.ApprovalStatus == ProjectApprovalStatus.PendingReview ? "İnceleme Bekliyor" :
                                 project.ApprovalStatus == ProjectApprovalStatus.Approved ? "Onaylandı" : "Reddedildi",
            SubmittedForReviewAt = project.SubmittedForReviewAt,
            SubmittedForReviewByUserId = project.SubmittedForReviewByUserId,
            SubmittedForReviewByUserName = project.SubmittedForReviewByUser != null ? $"{project.SubmittedForReviewByUser.FirstName} {project.SubmittedForReviewByUser.LastName}".Trim() : null,
            ReviewedAt = project.ReviewedAt,
            ReviewedByUserId = project.ReviewedByUserId,
            ReviewedByUserName = project.ReviewedByUser != null ? $"{project.ReviewedByUser.FirstName} {project.ReviewedByUser.LastName}".Trim() : null,
            RejectionReason = project.RejectionReason,
            CreatedByUserId = project.CreatedByUserId,
            CreatedByUserName = project.CreatedByUser != null ? $"{project.CreatedByUser.FirstName} {project.CreatedByUser.LastName}".Trim() : null,

            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Teams = project.ProjectTeams.Select(pt => new AdminProjectTeamEditDto
            {
                TeamId = pt.TeamId,
                TeamName = pt.Team?.Name ?? string.Empty,
                IsPrimary = pt.IsPrimary
            }).ToList(),
            Members = project.ProjectMembers.Select(pm => new AdminProjectMemberEditDto
            {
                MemberId = pm.MemberId,
                MemberName = pm.Member != null ? $"{pm.Member.FirstName} {pm.Member.LastName}".Trim() : string.Empty,
                ProjectRole = pm.ProjectRole
            }).ToList(),
            LocationIds = project.ProjectLocations.Select(pl => pl.LocationId).ToList(),
            TechnologyIds = project.ProjectTechnologies.Select(pt => pt.TechnologyId).ToList(),
            TagIds = project.ProjectTags.Select(pt => pt.TagId).ToList(),
            Integrations = project.ProjectIntegrations.Select(pi => new AdminProjectIntegrationEditDto
            {
                Id = pi.Id,
                Name = pi.Name,
                Description = pi.Description,
                IntegrationType = pi.IntegrationType.ToString()
            }).ToList(),
            Documents = project.ProjectDocuments.Select(pd => new AdminProjectDocumentEditDto
            {
                Id = pd.Id,
                Name = pd.Name,
                Description = pd.Description,
                FileName = pd.FileName,
                FileUrl = pd.FileUrl,
                DocumentType = pd.DocumentType
            }).ToList(),
            MediaItems = project.ProjectMediaItems.Select(pm => new AdminProjectMediaEditDto
            {
                Id = pm.Id,
                MediaType = pm.MediaType.ToString(),
                FileName = pm.FileName,
                FileUrl = pm.FileUrl,
                AltText = pm.AltText,
                Caption = pm.Caption,
                DisplayOrder = pm.DisplayOrder
            }).ToList()
        };
    }

    public async Task<int> CreateProjectAsync(CreateProjectRequestDto request, int? createdByUserId = null, CancellationToken cancellationToken = default)
    {
        await ValidateProjectRequestAsync(request, null, cancellationToken);

        var developmentType = Enum.TryParse<DevelopmentType>(request.DevelopmentType, true, out var devType)
            ? devType
            : DevelopmentType.Internal;

        var project = new Project
        {
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim().ToLowerInvariant(),
            ShortDescription = request.ShortDescription.Trim(),
            Description = request.Description?.Trim(),
            Purpose = request.Purpose?.Trim(),
            ProblemSolved = request.ProblemSolved?.Trim(),
            NonTechnicalDescription = request.NonTechnicalDescription?.Trim(),
            TechnicalDescription = request.TechnicalDescription?.Trim(),
            BusinessImpact = request.BusinessImpact?.Trim(),
            TargetAudience = request.TargetAudience?.Trim(),
            AccessInstructions = request.AccessInstructions?.Trim(),
            ApplicationUrl = request.ApplicationUrl?.Trim(),
            RepositoryUrl = request.RepositoryUrl?.Trim(),
            CoverImageUrl = request.CoverImageUrl?.Trim(),
            StatusId = request.StatusId,
            CategoryId = request.CategoryId,
            DevelopmentType = developmentType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ApprovalStatus = ProjectApprovalStatus.Draft,
            IsPublished = false,
            IsFeatured = false,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        ApplyRelationsToProject(project, request);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        // Promote temp files to project-specific folder
        bool hasTempFiles = false;
        if (!string.IsNullOrEmpty(project.CoverImageUrl) && project.CoverImageUrl.Contains("/temp/"))
        {
            project.CoverImageUrl = _fileStorageService.PromoteFile(project.CoverImageUrl, project.Id, isPrivate: false);
            hasTempFiles = true;
        }
        foreach (var doc in project.ProjectDocuments)
        {
            if (!string.IsNullOrEmpty(doc.FileUrl) && doc.FileUrl.Contains("/temp/"))
            {
                doc.FileUrl = _fileStorageService.PromoteFile(doc.FileUrl, project.Id, isPrivate: true);
                hasTempFiles = true;
            }
        }
        foreach (var med in project.ProjectMediaItems)
        {
            if (!string.IsNullOrEmpty(med.FileUrl) && med.FileUrl.Contains("/temp/"))
            {
                med.FileUrl = _fileStorageService.PromoteFile(med.FileUrl, project.Id, isPrivate: false);
                hasTempFiles = true;
            }
        }
        if (hasTempFiles)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _auditLogService.LogWithActorAsync(
            createdByUserId,
            "ProjectCreated",
            "Project",
            project.Id.ToString(),
            project.Name,
            $"\"{project.Name}\" projesi taslak olarak oluşturuldu.",
            cancellationToken: cancellationToken);

        return project.Id;
    }

    public async Task UpdateProjectAsync(int id, UpdateProjectRequestDto request, int? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectTeams)
            .Include(p => p.ProjectMembers)
            .Include(p => p.ProjectLocations)
            .Include(p => p.ProjectTechnologies)
            .Include(p => p.ProjectTags)
            .Include(p => p.ProjectIntegrations)
            .Include(p => p.ProjectDocuments)
            .Include(p => p.ProjectMediaItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException($"Id={id} olan proje bulunamadı.");
        }

        await ValidateProjectRequestAsync(request, id, cancellationToken);

        var oldMediaUrls = project.ProjectMediaItems.Select(m => m.FileUrl).ToList();
        var oldDocUrls = project.ProjectDocuments.Select(d => d.FileUrl).ToList();

        var developmentType = Enum.TryParse<DevelopmentType>(request.DevelopmentType, true, out var devType)
            ? devType
            : DevelopmentType.Internal;

        project.Name = request.Name.Trim();
        project.Slug = request.Slug.Trim().ToLowerInvariant();
        project.ShortDescription = request.ShortDescription.Trim();
        project.Description = request.Description?.Trim();
        project.Purpose = request.Purpose?.Trim();
        project.ProblemSolved = request.ProblemSolved?.Trim();
        project.NonTechnicalDescription = request.NonTechnicalDescription?.Trim();
        project.TechnicalDescription = request.TechnicalDescription?.Trim();
        project.BusinessImpact = request.BusinessImpact?.Trim();
        project.TargetAudience = request.TargetAudience?.Trim();
        project.AccessInstructions = request.AccessInstructions?.Trim();
        project.ApplicationUrl = request.ApplicationUrl?.Trim();
        project.RepositoryUrl = request.RepositoryUrl?.Trim();

        var coverUrl = request.CoverImageUrl?.Trim();
        if (!string.IsNullOrEmpty(coverUrl) && coverUrl.Contains("/temp/"))
        {
            coverUrl = _fileStorageService.PromoteFile(coverUrl, project.Id);
        }
        project.CoverImageUrl = coverUrl;

        project.StatusId = request.StatusId;
        project.CategoryId = request.CategoryId;
        project.DevelopmentType = developmentType;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        // Phase 14 Rule: If project was Approved/Published and edited, reset to Draft / Unpublish
        if (project.ApprovalStatus == ProjectApprovalStatus.Approved || project.IsPublished)
        {
            project.ApprovalStatus = ProjectApprovalStatus.Draft;
            project.IsPublished = false;
            project.IsFeatured = false;
            project.ReviewedAt = null;
            project.ReviewedByUserId = null;
            project.RejectionReason = null;
        }

        project.UpdatedAt = DateTime.UtcNow;

        // Relation collections clear & repopulate
        project.ProjectTeams.Clear();
        project.ProjectMembers.Clear();
        project.ProjectLocations.Clear();
        project.ProjectTechnologies.Clear();
        project.ProjectTags.Clear();
        project.ProjectIntegrations.Clear();
        project.ProjectDocuments.Clear();
        project.ProjectMediaItems.Clear();

        ApplyRelationsToProject(project, request);

        // Promote newly uploaded media/documents with temp URLs
        foreach (var doc in project.ProjectDocuments)
        {
            if (!string.IsNullOrEmpty(doc.FileUrl) && doc.FileUrl.Contains("/temp/"))
            {
                doc.FileUrl = _fileStorageService.PromoteFile(doc.FileUrl, project.Id, isPrivate: true);
            }
        }
        foreach (var med in project.ProjectMediaItems)
        {
            if (!string.IsNullOrEmpty(med.FileUrl) && med.FileUrl.Contains("/temp/"))
            {
                med.FileUrl = _fileStorageService.PromoteFile(med.FileUrl, project.Id, isPrivate: false);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Delete orphaned local files safety check
        var newMediaUrls = project.ProjectMediaItems.Select(m => m.FileUrl).ToHashSet();
        foreach (var oldUrl in oldMediaUrls)
        {
            if (!newMediaUrls.Contains(oldUrl) && oldUrl != project.CoverImageUrl && oldUrl.StartsWith("/uploads/"))
            {
                _fileStorageService.DeleteFile(oldUrl);
            }
        }
        var newDocUrls = project.ProjectDocuments.Select(d => d.FileUrl).ToHashSet();
        foreach (var oldUrl in oldDocUrls)
        {
            if (!newDocUrls.Contains(oldUrl) && oldUrl.StartsWith("/uploads/"))
            {
                _fileStorageService.DeleteFile(oldUrl);
            }
        }

        await _auditLogService.LogWithActorAsync(
            actorUserId,
            "ProjectUpdated",
            "Project",
            project.Id.ToString(),
            project.Name,
            $"\"{project.Name}\" projesi güncellendi.",
            cancellationToken: cancellationToken);
    }

    public async Task SubmitProjectForReviewAsync(int id, int userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Id={id} olan aktif proje bulunamadı.");
        }

        if (!isAdmin && project.CreatedByUserId != userId)
        {
            throw new InvalidOperationException("Yalnızca kendi oluşturduğunuz projeleri incelemeye gönderebilirsiniz.");
        }

        if (project.ApprovalStatus == ProjectApprovalStatus.PendingReview)
        {
            throw new InvalidOperationException("Bu proje zaten yönetici incelemesindedir.");
        }

        var isResubmission = project.ApprovalStatus == ProjectApprovalStatus.Rejected || !string.IsNullOrEmpty(project.RejectionReason);

        project.ApprovalStatus = ProjectApprovalStatus.PendingReview;
        project.SubmittedForReviewAt = DateTime.UtcNow;
        project.SubmittedForReviewByUserId = userId;
        project.RejectionReason = null;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogWithActorAsync(
            userId,
            "ProjectSubmittedForReview",
            "Project",
            project.Id.ToString(),
            project.Name,
            isResubmission
                ? $"\"{project.Name}\" projesi yönetici incelemesine yeniden gönderildi."
                : $"\"{project.Name}\" projesi yönetici incelemesine gönderildi.",
            cancellationToken: cancellationToken);

        // Notify Admins
        var submitterName = await ResolveUserDisplayNameAsync(userId, cancellationToken);
        var notifMessage = isResubmission
            ? $"{submitterName}, '{project.Name}' projesini yeniden incelemeye gönderdi."
            : $"{submitterName}, '{project.Name}' projesini incelemeye gönderdi.";

        await _notificationService.CreateNotificationsForAdminsAsync(
            "ProjectSubmittedForReview",
            "Onay bekleyen proje var",
            notifMessage,
            project.Id,
            $"/admin/projects/{project.Id}/edit",
            cancellationToken);
    }

    public async Task ApproveProjectAsync(int id, int adminUserId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Id={id} olan aktif proje bulunamadı.");
        }

        project.ApprovalStatus = ProjectApprovalStatus.Approved;
        project.IsPublished = true;
        project.ReviewedAt = DateTime.UtcNow;
        project.ReviewedByUserId = adminUserId;
        project.RejectionReason = null;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogWithActorAsync(
            adminUserId,
            "ProjectApproved",
            "Project",
            project.Id.ToString(),
            project.Name,
            $"\"{project.Name}\" projesi onaylandı ve Proje Kütüphanesi'nde yayınlandı.",
            cancellationToken: cancellationToken);

        // Notify project creator
        if (project.CreatedByUserId.HasValue && project.CreatedByUserId.Value > 0)
        {
            await _notificationService.CreateNotificationAsync(
                project.CreatedByUserId.Value,
                "ProjectApproved",
                "Projeniz onaylandı",
                $"'{project.Name}' onaylandı ve Proje Kütüphanesi'nde yayınlandı.",
                project.Id,
                $"/projects/{project.Slug}",
                cancellationToken);
        }
    }

    public async Task RejectProjectAsync(int id, string reason, int adminUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Düzeltme notu belirtilmesi zorunludur.");
        }

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Id={id} olan aktif proje bulunamadı.");
        }

        project.ApprovalStatus = ProjectApprovalStatus.Rejected;
        project.IsPublished = false;
        project.ReviewedAt = DateTime.UtcNow;
        project.ReviewedByUserId = adminUserId;
        project.RejectionReason = reason.Trim();
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogWithActorAsync(
            adminUserId,
            "ProjectRejected",
            "Project",
            project.Id.ToString(),
            project.Name,
            $"\"{project.Name}\" projesi için düzeltme istendi. Yönetici notu: {reason.Trim()}",
            cancellationToken: cancellationToken);

        // Notify project creator
        if (project.CreatedByUserId.HasValue && project.CreatedByUserId.Value > 0)
        {
            await _notificationService.CreateNotificationAsync(
                project.CreatedByUserId.Value,
                "ProjectRejected",
                "Projeniz için düzeltme istendi",
                $"'{project.Name}' projesi için yönetici düzeltme talebinde bulundu.",
                project.Id,
                $"/admin/projects/{project.Id}/edit",
                cancellationToken);
        }
    }

    public async Task DeleteProjectAsync(int id, int? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Id={id} olan aktif proje bulunamadı.");
        }

        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        project.IsFeatured = false;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogWithActorAsync(
            actorUserId,
            "ProjectArchived",
            "Project",
            project.Id.ToString(),
            project.Name,
            $"\"{project.Name}\" projesi arşivlendi.",
            cancellationToken: cancellationToken);
    }

    public async Task RestoreProjectAsync(int id, int? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null || !project.IsDeleted)
        {
            throw new KeyNotFoundException($"Id={id} olan arşivlenmiş proje bulunamadı.");
        }

        project.IsDeleted = false;
        project.DeletedAt = null;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogWithActorAsync(
            actorUserId,
            "ProjectRestored",
            "Project",
            project.Id.ToString(),
            project.Name,
            $"\"{project.Name}\" projesi arşivden geri yüklendi.",
            cancellationToken: cancellationToken);
    }

    public async Task SetProjectPublishedAsync(int id, bool isPublished, int? actorUserId = null, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Id={id} olan proje bulunamadı.");
        }

        project.IsPublished = isPublished;
        if (!isPublished)
        {
            project.IsFeatured = false;
        }
        else
        {
            project.ApprovalStatus = ProjectApprovalStatus.Approved;
        }
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var action = isPublished ? "ProjectPublished" : "ProjectUnpublished";
        var desc = isPublished
            ? $"\"{project.Name}\" projesi yayına alındı."
            : $"\"{project.Name}\" projesi taslağa çekildi.";

        await _auditLogService.LogWithActorAsync(actorUserId, action, "Project", project.Id.ToString(), project.Name, desc, cancellationToken: cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var search = queryParams.Search.Trim().ToLower();
            query = query.Where(a =>
                a.Description.ToLower().Contains(search) ||
                (a.ActorDisplayNameSnapshot != null && a.ActorDisplayNameSnapshot.ToLower().Contains(search)) ||
                (a.EntityDisplayNameSnapshot != null && a.EntityDisplayNameSnapshot.ToLower().Contains(search))
            );
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Action))
        {
            query = query.Where(a => a.Action == queryParams.Action);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.EntityType))
        {
            query = query.Where(a => a.EntityType == queryParams.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.EntityId))
        {
            query = query.Where(a => a.EntityId == queryParams.EntityId);
        }

        if (queryParams.ActorUserId.HasValue && queryParams.ActorUserId.Value > 0)
        {
            query = query.Where(a => a.ActorUserId == queryParams.ActorUserId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.OccurredAtUtc)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                OccurredAtUtc = a.OccurredAtUtc,
                ActorUserId = a.ActorUserId,
                ActorDisplayName = a.ActorDisplayNameSnapshot,
                ActorEmail = a.ActorEmailSnapshot,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                EntityDisplayName = a.EntityDisplayNameSnapshot,
                Description = a.Description,
                MetadataJson = a.MetadataJson
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, queryParams.PageNumber, queryParams.PageSize, totalCount);
    }

    public async Task<List<AuditLogDto>> GetProjectAuditLogsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var entityIdStr = projectId.ToString();
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Project" && a.EntityId == entityIdStr)
            .OrderByDescending(a => a.OccurredAtUtc)
            .Take(100)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                OccurredAtUtc = a.OccurredAtUtc,
                ActorUserId = a.ActorUserId,
                ActorDisplayName = a.ActorDisplayNameSnapshot,
                ActorEmail = a.ActorEmailSnapshot,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                EntityDisplayName = a.EntityDisplayNameSnapshot,
                Description = a.Description,
                MetadataJson = a.MetadataJson
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyRequestDto request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Teknoloji adı zorunludur.");

        var exists = await _context.Technologies.AnyAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"'{name}' teknoloji tanımı zaten mevcut.");

        var tech = new Technology
        {
            Name = name,
            Category = (TechnologyCategory)request.Category
        };

        _context.Technologies.Add(tech);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("TechnologyCreated", "Technology", tech.Id.ToString(), tech.Name, $"'{tech.Name}' teknolojisi eklendi.", cancellationToken: cancellationToken);

        return new TechnologyDto
        {
            Id = tech.Id,
            Name = tech.Name,
            Category = tech.Category.ToString()
        };
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Lokasyon adı zorunludur.");

        var exists = await _context.Locations.AnyAsync(l => l.Name.ToLower() == name.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"'{name}' lokasyon tanımı zaten mevcut.");

        var location = new Location
        {
            Name = name,
            LocationType = request.LocationType,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("LocationCreated", "Location", location.Id.ToString(), location.Name, $"'{location.Name}' lokasyonu eklendi.", cancellationToken: cancellationToken);

        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationType = location.LocationType.ToString(),
            Description = location.Description
        };
    }

    private async Task ValidateProjectRequestAsync(CreateProjectRequestDto request, int? currentProjectId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Proje adı zorunludur.");

        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            request.Slug = DeUygulamaVitrini.Application.Common.Helpers.SlugHelper.GenerateSlug(request.Name);
        }

        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug adresi zorunludur.");

        // Check Slug Uniqueness
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var slugExists = await _context.Projects
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Slug.ToLower() == normalizedSlug && p.Id != currentProjectId, cancellationToken);

        if (slugExists)
        {
            throw new InvalidOperationException($"'{request.Slug}' slug adresi başka bir proje tarafından kullanılıyor.");
        }

        // Validate StatusId
        var statusExists = await _context.ProjectStatuses.AnyAsync(s => s.Id == request.StatusId, cancellationToken);
        if (!statusExists)
        {
            throw new ArgumentException("Geçersiz proje durumu.");
        }

        // Validate CategoryId
        var categoryExists = await _context.ProjectCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new ArgumentException("Geçersiz proje kategorisi.");
        }
    }

    private static void ApplyRelationsToProject(Project project, CreateProjectRequestDto request)
    {
        if (request.Teams != null && request.Teams.Any())
        {
            foreach (var teamReq in request.Teams)
            {
                project.ProjectTeams.Add(new ProjectTeam
                {
                    TeamId = teamReq.TeamId,
                    IsPrimary = teamReq.IsPrimary
                });
            }
        }

        if (request.Members != null && request.Members.Any())
        {
            foreach (var memReq in request.Members)
            {
                project.ProjectMembers.Add(new ProjectMember
                {
                    MemberId = memReq.MemberId,
                    ProjectRole = memReq.ProjectRole ?? "Geliştirici"
                });
            }
        }

        if (request.LocationIds != null && request.LocationIds.Any())
        {
            foreach (var locId in request.LocationIds.Distinct())
            {
                project.ProjectLocations.Add(new ProjectLocation { LocationId = locId });
            }
        }

        if (request.TechnologyIds != null && request.TechnologyIds.Any())
        {
            foreach (var techId in request.TechnologyIds.Distinct())
            {
                project.ProjectTechnologies.Add(new ProjectTechnology { TechnologyId = techId });
            }
        }

        if (request.TagIds != null && request.TagIds.Any())
        {
            foreach (var tagId in request.TagIds.Distinct())
            {
                project.ProjectTags.Add(new ProjectTag { TagId = tagId });
            }
        }

        if (request.Integrations != null && request.Integrations.Any())
        {
            foreach (var integ in request.Integrations)
            {
                var integType = Enum.TryParse<IntegrationType>(integ.IntegrationType, true, out var parsedType)
                    ? parsedType
                    : IntegrationType.RestApi;

                project.ProjectIntegrations.Add(new ProjectIntegration
                {
                    Name = integ.Name.Trim(),
                    Description = integ.Description?.Trim(),
                    IntegrationType = integType
                });
            }
        }

        if (request.Documents != null && request.Documents.Any())
        {
            foreach (var doc in request.Documents)
            {
                project.ProjectDocuments.Add(new ProjectDocument
                {
                    Name = doc.Name.Trim(),
                    Description = doc.Description?.Trim(),
                    FileName = doc.FileName.Trim(),
                    FileUrl = doc.FileUrl.Trim(),
                    DocumentType = doc.DocumentType?.Trim()
                });
            }
        }

        if (request.MediaItems != null && request.MediaItems.Any())
        {
            foreach (var med in request.MediaItems)
            {
                var medType = Enum.TryParse<MediaType>(med.MediaType, true, out var parsedType)
                    ? parsedType
                    : MediaType.Image;

                project.ProjectMediaItems.Add(new ProjectMedia
                {
                    MediaType = medType,
                    FileName = med.FileName.Trim(),
                    FileUrl = med.FileUrl.Trim(),
                    AltText = med.AltText?.Trim(),
                    Caption = med.Caption?.Trim(),
                    DisplayOrder = med.DisplayOrder
                });
            }
        }
    }
}
