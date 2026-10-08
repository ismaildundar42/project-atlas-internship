using DeUygulamaVitrini.Application.Common.Exceptions;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Domain.Enums;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Yetkilendirme duyarlı (authorization-aware) anlamsal proje arama servisi implementasyonu.
/// </summary>
public class ProjectSemanticSearchService : IProjectSemanticSearchService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly AiOptions _options;
    private readonly ILogger<ProjectSemanticSearchService> _logger;

    public ProjectSemanticSearchService(
        IApplicationDbContext context,
        IEmbeddingProvider embeddingProvider,
        IOptions<AiOptions> options,
        ILogger<ProjectSemanticSearchService> logger)
    {
        _context = context;
        _embeddingProvider = embeddingProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(
        SemanticSearchQueryDto queryDto,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (queryDto == null || string.IsNullOrWhiteSpace(queryDto.Query))
        {
            return Array.Empty<SemanticSearchResultDto>();
        }

        var minSimilarity = queryDto.MinSimilarity > 0 ? queryDto.MinSimilarity : 0.50f;
        var topK = queryDto.TopK is >= 1 and <= 50 ? queryDto.TopK : 5;

        // 1. Kullanıcı sorgusu için embedding üret (search_query: ön-eki ile)
        var queryEmbResult = await _embeddingProvider.GenerateEmbeddingAsync(
            queryDto.Query,
            EmbeddingType.Query,
            cancellationToken);

        if (!queryEmbResult.Success || queryEmbResult.Vector == null || queryEmbResult.Vector.Length == 0)
        {
            _logger.LogWarning("Failed to generate query embedding for search query: {Error}", queryEmbResult.Error);
            throw new EmbeddingProviderUnavailableException("Anlamsal arama servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.");
        }

        var queryVector = queryEmbResult.Vector;

        // 2. Yetkilendirme ve yapılandırılmış filtrelere göre aday parçaları veritabanından getir
        var chunksQuery = _context.ProjectKnowledgeChunks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Project)
                .ThenInclude(p => p.Status)
            .Include(c => c.Project)
                .ThenInclude(p => p.Category)
            .Include(c => c.Project)
                .ThenInclude(p => p.ProjectTechnologies)
                    .ThenInclude(pt => pt.Technology)
            .Include(c => c.Project)
                .ThenInclude(p => p.ProjectLocations)
                    .ThenInclude(pl => pl.Location)
            .Where(c => !c.Project.IsDeleted);

        // Yetkilendirme filtresi:
        // Admin: Tüm indeksli projeleri görebilir.
        // Normal kullanıcı: Yalnızca IsPublished = true VE ApprovalStatus = Approved olan (veya kendi oluşturduğu) projeleri görebilir.
        if (!isAdmin)
        {
            chunksQuery = chunksQuery.Where(c =>
                (c.Project.IsPublished && c.Project.ApprovalStatus == ProjectApprovalStatus.Approved) ||
                (currentUserId > 0 && c.Project.CreatedByUserId == currentUserId));
        }

        // ─── Yapılandırılmış Filtreler (Structured Metadata Filters) ───────────
        if (!string.IsNullOrWhiteSpace(queryDto.Status))
        {
            var statusCode = queryDto.Status.Trim();
            chunksQuery = chunksQuery.Where(c => c.Project.Status.Code.ToLower() == statusCode.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Category))
        {
            var categoryCode = queryDto.Category.Trim();
            chunksQuery = chunksQuery.Where(c => c.Project.Category.Code.ToLower() == categoryCode.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(queryDto.DevelopmentType) &&
            Enum.TryParse<DevelopmentType>(queryDto.DevelopmentType.Trim(), true, out var devType))
        {
            chunksQuery = chunksQuery.Where(c => c.Project.DevelopmentType == devType);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Technology))
        {
            var tech = queryDto.Technology.Trim().ToLower();
            chunksQuery = chunksQuery.Where(c => c.Project.ProjectTechnologies.Any(pt => pt.Technology.Name.ToLower() == tech));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Location))
        {
            var loc = queryDto.Location.Trim().ToLower();
            chunksQuery = chunksQuery.Where(c => c.Project.ProjectLocations.Any(pl => pl.Location.Name.ToLower() == loc));
        }

        var candidateChunks = await chunksQuery.ToListAsync(cancellationToken);
        if (candidateChunks.Count == 0)
        {
            return Array.Empty<SemanticSearchResultDto>();
        }

        // 3. Kosinüs Benzerliğini hesapla ve eşik üstü sonuçları topla
        var scoredCandidates = new List<(ProjectKnowledgeChunk Chunk, float Score)>();

        foreach (var chunk in candidateChunks)
        {
            if (chunk.EmbeddingVector == null || chunk.EmbeddingVector.Length == 0)
                continue;

            if (chunk.EmbeddingModel != _options.Local.EmbeddingModel || 
                (_options.Local.EmbeddingDimension > 0 && chunk.EmbeddingDimension != _options.Local.EmbeddingDimension))
                continue;

            var chunkVector = VectorUtils.ToFloats(chunk.EmbeddingVector);
            if (chunkVector.Length != queryVector.Length)
                continue;

            var score = VectorUtils.CosineSimilarity(queryVector, chunkVector);
            if (score >= minSimilarity)
            {
                scoredCandidates.Add((chunk, score));
            }
        }

        // 4. Proje bazında tekilleştir (En yüksek puanlı parçayı seç)
        var projectGroups = scoredCandidates
            .GroupBy(sc => sc.Chunk.ProjectId)
            .Select(g => g.OrderByDescending(sc => sc.Score).First())
            .OrderByDescending(sc => sc.Score)
            .Take(topK)
            .ToList();

        // 5. DTO'lara dönüştür
        var results = projectGroups.Select(sc =>
        {
            var p = sc.Chunk.Project;
            var snippet = ExtractRelevantSnippet(sc.Chunk.Content);

            return new SemanticSearchResultDto
            {
                ProjectId = p.Id,
                Slug = p.Slug,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                CoverImageUrl = p.CoverImageUrl,
                ChunkKey = sc.Chunk.ChunkKey,
                MatchedSnippet = snippet,
                SimilarityScore = MathF.Round(sc.Score, 4),
                StatusCode = p.Status?.Code ?? string.Empty,
                StatusName = p.Status?.Name ?? string.Empty,
                CategoryCode = p.Category?.Code ?? string.Empty,
                CategoryName = p.Category?.Name ?? string.Empty,
                DevelopmentType = p.DevelopmentType.ToString(),
                Technologies = p.ProjectTechnologies?
                    .Where(pt => pt.Technology != null)
                    .Select(pt => pt.Technology.Name)
                    .ToList() ?? new List<string>(),
                Locations = p.ProjectLocations?
                    .Where(pl => pl.Location != null)
                    .Select(pl => pl.Location.Name)
                    .ToList() ?? new List<string>()
            };
        }).ToList();

        return results;
    }

    private static string ExtractRelevantSnippet(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // Metin zaten normalize edildiği için ilk 300 karakteri veya anlamlı başlangıcı kes
        if (content.Length <= 300)
            return content;

        var cut = content[..300];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > 200)
        {
            cut = cut[..lastSpace];
        }

        return cut + "...";
    }
}
