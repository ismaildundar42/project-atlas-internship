using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Search;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeUygulamaVitrini.Application.Services;

/// <summary>
/// ISearchService implementasyonu.
/// EF Core ve SQL Server üzerinde deterministik, çok boyutlu ve yüksek performanslı
/// proje arama ve keşif operasyonlarını yürütür.
/// </summary>
public class SearchService : ISearchService
{
    private readonly IApplicationDbContext _context;

    public SearchService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResultDto> SearchProjectsAsync(
        string? query,
        int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var rawTerm = query?.Trim() ?? string.Empty;

        // Anlamsız veya çok kısa sorgularda veritabanı yükü oluşturmadan boş dön
        if (string.IsNullOrWhiteSpace(rawTerm) || rawTerm.Length < 2)
        {
            return new SearchResultDto
            {
                Query = rawTerm,
                TotalCount = 0,
                Items = new List<ProjectSearchResultDto>()
            };
        }

        // Limit sınırlandırması (1 - 30 arası)
        var clampedLimit = Math.Clamp(limit, 1, 30);

        // Maksimum sorgu uzunluğu koruması (100 karakter)
        var term = rawTerm.Length > 100 ? rawTerm[..100] : rawTerm;

        // ─── Güvenlik & Yayın Filtresi ─────────────────────────────────────────
        // Yalnızca yayınlanmış (IsPublished = true) ve silinmemiş projeler aranır.
        // Taslak, arşivlenmiş ve silinmiş projeler ASLA genel aramaya sızamaz.
        var baseQuery = _context.Projects
            .AsNoTracking()
            .Where(p => p.IsPublished);

        // ─── Çok Boyutlu Arama Kapsamı ───────────────────────────────────────
        var filteredQuery = baseQuery.Where(p =>
            EF.Functions.Like(p.Name, $"%{term}%") ||
            EF.Functions.Like(p.ShortDescription, $"%{term}%") ||
            (p.Description != null && EF.Functions.Like(p.Description, $"%{term}%")) ||
            (p.Purpose != null && EF.Functions.Like(p.Purpose, $"%{term}%")) ||
            (p.ProblemSolved != null && EF.Functions.Like(p.ProblemSolved, $"%{term}%")) ||
            (p.NonTechnicalDescription != null && EF.Functions.Like(p.NonTechnicalDescription, $"%{term}%")) ||
            (p.TechnicalDescription != null && EF.Functions.Like(p.TechnicalDescription, $"%{term}%")) ||
            (p.BusinessImpact != null && EF.Functions.Like(p.BusinessImpact, $"%{term}%")) ||
            (p.TargetAudience != null && EF.Functions.Like(p.TargetAudience, $"%{term}%")) ||
            EF.Functions.Like(p.Category.Name, $"%{term}%") ||
            EF.Functions.Like(p.Status.Name, $"%{term}%") ||
            p.ProjectTechnologies.Any(pt => EF.Functions.Like(pt.Technology.Name, $"%{term}%")) ||
            p.ProjectTags.Any(pt => EF.Functions.Like(pt.Tag.Name, $"%{term}%")) ||
            p.ProjectLocations.Any(pl => EF.Functions.Like(pl.Location.Name, $"%{term}%")) ||
            p.ProjectTeams.Any(pt => EF.Functions.Like(pt.Team.Name, $"%{term}%")) ||
            p.ProjectMembers.Any(pm => EF.Functions.Like(pm.Member.FirstName, $"%{term}%") || EF.Functions.Like(pm.Member.LastName, $"%{term}%"))
        );

        // ─── Deterministik Eşleşme Önceliği (Relevance Ranking) ─────────────────
        // 1. İsim tam eşleşmesi: 100
        // 2. İsim önek (prefix) eşleşmesi: 80
        // 3. İsim içeren (contains) eşleşmesi: 60
        // 4. Kısa açıklama eşleşmesi: 40
        // 5. Teknoloji, Etiket veya Kategori eşleşmesi: 30
        // 6. Lokasyon, Ekip veya Ekip Üyesi eşleşmesi: 20
        // 7. Detaylı içerik anlatımları eşleşmesi: 10
        var orderedQuery = filteredQuery.OrderByDescending(p =>
            (p.Name == term ? 100 :
             EF.Functions.Like(p.Name, $"{term}%") ? 80 :
             EF.Functions.Like(p.Name, $"%{term}%") ? 60 :
             EF.Functions.Like(p.ShortDescription, $"%{term}%") ? 40 :
             (p.ProjectTechnologies.Any(pt => EF.Functions.Like(pt.Technology.Name, $"%{term}%")) ||
              p.ProjectTags.Any(pt => EF.Functions.Like(pt.Tag.Name, $"%{term}%")) ||
              EF.Functions.Like(p.Category.Name, $"%{term}%")) ? 30 :
             (p.ProjectLocations.Any(pl => EF.Functions.Like(pl.Location.Name, $"%{term}%")) ||
              p.ProjectTeams.Any(pt => EF.Functions.Like(pt.Team.Name, $"%{term}%")) ||
              p.ProjectMembers.Any(pm => EF.Functions.Like(pm.Member.FirstName, $"%{term}%") || EF.Functions.Like(pm.Member.LastName, $"%{term}%"))) ? 20 : 10))
            .ThenByDescending(p => p.UpdatedAt);

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        // ─── Projeksiyon (Veritabanı Düzeyinde DTO Seçimi) ───────────────────
        var projectedItems = await orderedQuery
            .Take(clampedLimit)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.ShortDescription,
                CategoryName = p.Category.Name,
                CategoryCode = p.Category.Code,
                StatusName = p.Status.Name,
                StatusCode = p.Status.Code,
                CoverImageUrl = p.CoverImageUrl ?? p.ProjectMediaItems
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.FileUrl)
                    .FirstOrDefault(),
                PrimaryTeamName = p.ProjectTeams
                    .Where(pt => pt.IsPrimary)
                    .Select(pt => pt.Team.Name)
                    .FirstOrDefault() ?? p.ProjectTeams.Select(pt => pt.Team.Name).FirstOrDefault(),
                Technologies = p.ProjectTechnologies.Select(pt => pt.Technology.Name).ToList(),
                Tags = p.ProjectTags.Select(pt => pt.Tag.Name).ToList(),
                Locations = p.ProjectLocations.Select(pl => pl.Location.Name).ToList(),
                Teams = p.ProjectTeams.Select(pt => pt.Team.Name).ToList(),
                Members = p.ProjectMembers.Select(pm => pm.Member.FirstName + " " + pm.Member.LastName).ToList(),
                p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // ─── Kompakt Eşleşme Sebebi (Match Reason) Çözümlemesi ───────────────
        var items = projectedItems.Select(item =>
        {
            string? matchReason = null;

            // Teknoloji eşleşti mi?
            var matchedTech = item.Technologies.FirstOrDefault(t => t.Contains(term, StringComparison.OrdinalIgnoreCase));
            if (matchedTech != null)
            {
                matchReason = $"Teknoloji: {matchedTech}";
            }
            else
            {
                // Lokasyon eşleşti mi?
                var matchedLoc = item.Locations.FirstOrDefault(l => l.Contains(term, StringComparison.OrdinalIgnoreCase));
                if (matchedLoc != null)
                {
                    matchReason = $"Lokasyon: {matchedLoc}";
                }
                else
                {
                    // Ekip eşleşti mi?
                    var matchedTeam = item.Teams.FirstOrDefault(tm => tm.Contains(term, StringComparison.OrdinalIgnoreCase));
                    if (matchedTeam != null)
                    {
                        matchReason = $"Ekip: {matchedTeam}";
                    }
                    else
                    {
                        // Etiket eşleşti mi?
                        var matchedTag = item.Tags.FirstOrDefault(tg => tg.Contains(term, StringComparison.OrdinalIgnoreCase));
                        if (matchedTag != null)
                        {
                            matchReason = $"Etiket: {matchedTag}";
                        }
                        else if (item.CategoryName.Contains(term, StringComparison.OrdinalIgnoreCase))
                        {
                            matchReason = $"Kategori: {item.CategoryName}";
                        }
                        else
                        {
                            // Ekip üyesi eşleşti mi?
                            var matchedMember = item.Members.FirstOrDefault(m => m.Contains(term, StringComparison.OrdinalIgnoreCase));
                            if (matchedMember != null)
                            {
                                matchReason = $"Ekip Üyesi: {matchedMember}";
                            }
                        }
                    }
                }
            }

            return new ProjectSearchResultDto
            {
                Id = item.Id,
                Name = item.Name,
                Slug = item.Slug,
                ShortDescription = item.ShortDescription,
                CategoryName = item.CategoryName,
                CategoryCode = item.CategoryCode,
                StatusName = item.StatusName,
                StatusCode = item.StatusCode,
                CoverImageUrl = item.CoverImageUrl,
                PrimaryTeamName = item.PrimaryTeamName,
                Technologies = item.Technologies,
                Tags = item.Tags,
                Locations = item.Locations,
                Teams = item.Teams,
                MatchReason = matchReason,
                UpdatedAt = item.UpdatedAt
            };
        }).ToList();

        return new SearchResultDto
        {
            Query = term,
            TotalCount = totalCount,
            Items = items
        };
    }
}
