using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using DeUygulamaVitrini.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Seeders;

/// <summary>
/// Phase 19.6B-2R: 20 adet zengin, derinlikli ve tutarlı kurumsal projeyi veritabanına ekleyen deterministik ve idempotent seeder.
/// </summary>
public static class CuratedProjectDatasetSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // İdempotency kontrolü: Veritabanında zaten projeler varsa mükerrer kayıt oluşturma
        var existingCount = await context.Projects.IgnoreQueryFilters().CountAsync();
        if (existingCount > 0)
        {
            logger.LogInformation(
                "CuratedProjectDatasetSeeder: Veritabanında zaten {Count} proje mevcut. Seeding atlandı.",
                existingCount);
            return;
        }

        logger.LogInformation("CuratedProjectDatasetSeeder: 20 adet zengin kurumsal proje ekleniyor...");

        // Canonical lookupları yükle
        var categories = await context.ProjectCategories.ToDictionaryAsync(c => c.Code, c => c);
        var statuses = await context.ProjectStatuses.ToDictionaryAsync(s => s.Code, s => s);
        var technologies = await context.Technologies.ToListAsync();
        var locations = await context.Locations.ToListAsync();
        var teams = await context.Teams.ToListAsync();
        var tags = await context.Tags.ToListAsync();
        var members = await context.Members.ToListAsync();

        var adminUser = await userManager.FindByEmailAsync("admin@demirexport.com");
        var defaultUserId = adminUser?.Id ?? 1;

        var blueprints = CuratedProjectDefinitions.GetProjects();
        var newProjects = new List<Project>();

        foreach (var bp in blueprints)
        {
            if (!categories.TryGetValue(bp.CategoryCode, out var category))
            {
                logger.LogWarning("Kategori bulunamadı: {Code}. Proje atlandı: {Name}", bp.CategoryCode, bp.Name);
                continue;
            }

            if (!statuses.TryGetValue(bp.StatusCode, out var status))
            {
                logger.LogWarning("Durum bulunamadı: {Code}. Proje atlandı: {Name}", bp.StatusCode, bp.Name);
                continue;
            }

            var project = new Project
            {
                Name = bp.Name,
                Slug = bp.Slug,
                ShortDescription = bp.ShortDescription,
                Description = bp.Description,
                Purpose = bp.Purpose,
                ProblemSolved = bp.ProblemSolved,
                NonTechnicalDescription = bp.NonTechnicalDescription,
                TechnicalDescription = bp.TechnicalDescription,
                BusinessImpact = bp.BusinessImpact,
                TargetAudience = bp.TargetAudience,
                AccessInstructions = bp.AccessInstructions,
                CoverImageUrl = $"/uploads/projects/demo/{bp.Slug}.svg",
                CategoryId = category.Id,
                StatusId = status.Id,
                DevelopmentType = bp.DevelopmentType,
                IsFeatured = bp.IsFeatured,
                IsPublished = bp.IsPublished,
                ApprovalStatus = bp.ApprovalStatus,
                StartDate = bp.StartDate,
                EndDate = bp.EndDate,
                CreatedAt = bp.CreatedAt,
                UpdatedAt = bp.UpdatedAt,
                CreatedByUserId = defaultUserId,
                ReviewedByUserId = bp.ReviewedByUserId.HasValue ? defaultUserId : null,
                SubmittedForReviewAt = bp.ApprovalStatus != ProjectApprovalStatus.Draft ? bp.CreatedAt : null,
                ReviewedAt = bp.ApprovalStatus == ProjectApprovalStatus.Approved ? bp.CreatedAt : null
            };

            // Lokasyon İlişkileri
            foreach (var locName in bp.LocationNames)
            {
                var loc = locations.FirstOrDefault(l => string.Equals(l.Name, locName, StringComparison.OrdinalIgnoreCase));
                if (loc != null)
                {
                    project.ProjectLocations.Add(new ProjectLocation
                    {
                        LocationId = loc.Id
                    });
                }
            }

            // Teknoloji İlişkileri
            foreach (var techName in bp.TechnologyNames)
            {
                var tech = technologies.FirstOrDefault(t => string.Equals(t.Name, techName, StringComparison.OrdinalIgnoreCase));
                if (tech != null)
                {
                    project.ProjectTechnologies.Add(new ProjectTechnology
                    {
                        TechnologyId = tech.Id
                    });
                }
            }

            // Ekip İlişkileri (İlk ekip Primary, diğerleri Supporting)
            for (var i = 0; i < bp.TeamNames.Count; i++)
            {
                var teamName = bp.TeamNames[i];
                var team = teams.FirstOrDefault(t => string.Equals(t.Name, teamName, StringComparison.OrdinalIgnoreCase));
                if (team != null)
                {
                    project.ProjectTeams.Add(new ProjectTeam
                    {
                        TeamId = team.Id,
                        IsPrimary = (i == 0)
                    });
                }
            }

            // Etiket İlişkileri
            foreach (var tagName in bp.TagNames)
            {
                var tag = tags.FirstOrDefault(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));
                if (tag != null)
                {
                    project.ProjectTags.Add(new ProjectTag
                    {
                        TagId = tag.Id
                    });
                }
            }

            // Entegrasyonlar
            foreach (var (intName, intDesc, intType) in bp.Integrations)
            {
                project.ProjectIntegrations.Add(new ProjectIntegration
                {
                    Name = intName,
                    Description = intDesc,
                    IntegrationType = intType
                });
            }

            // Üyeler
            foreach (var (fullName, role) in bp.Members)
            {
                var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var mem = members.FirstOrDefault(m =>
                        string.Equals(m.FirstName, parts[0], StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(m.LastName, parts[1], StringComparison.OrdinalIgnoreCase));

                    if (mem != null)
                    {
                        project.ProjectMembers.Add(new ProjectMember
                        {
                            MemberId = mem.Id,
                            ProjectRole = role
                        });
                    }
                }
            }

            // Medya Öğeleri (ProjectMediaItems)
            var displayOrder = 1;
            foreach (var (title, theme, caption) in bp.MediaItems)
            {
                project.ProjectMediaItems.Add(new ProjectMedia
                {
                    MediaType = MediaType.Image,
                    FileName = $"{theme}.svg",
                    FileUrl = $"/uploads/projects/demo/{theme}.svg",
                    AltText = title,
                    Caption = caption,
                    DisplayOrder = displayOrder++
                });
            }

            // Dokümanlar (ProjectDocuments)
            foreach (var (docName, docFile, docType, docDesc) in bp.Documents)
            {
                project.ProjectDocuments.Add(new ProjectDocument
                {
                    Name = docName,
                    FileName = docFile,
                    FileUrl = $"/uploads/docs/{docFile}",
                    DocumentType = docType,
                    Description = docDesc
                });
            }

            newProjects.Add(project);
        }

        if (newProjects.Count > 0)
        {
            context.Projects.AddRange(newProjects);
            await context.SaveChangesAsync();
            logger.LogInformation("CuratedProjectDatasetSeeder: {Count} adet proje başarıyla veritabanına kaydedildi.", newProjects.Count);
        }
    }
}
