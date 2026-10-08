using System.Diagnostics;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Entities;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Proje Bilgi İndeksi yaşam döngüsü yönetim servisi implementasyonu.
/// </summary>
public class ProjectKnowledgeIndexService : IProjectKnowledgeIndexService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IProjectKnowledgeDocumentBuilder _documentBuilder;
    private readonly AiOptions _options;
    private readonly ILogger<ProjectKnowledgeIndexService> _logger;

    public ProjectKnowledgeIndexService(
        IApplicationDbContext context,
        IEmbeddingProvider embeddingProvider,
        IProjectKnowledgeDocumentBuilder documentBuilder,
        IOptions<AiOptions> options,
        ILogger<ProjectKnowledgeIndexService> logger)
    {
        _context = context;
        _embeddingProvider = embeddingProvider;
        _documentBuilder = documentBuilder;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IndexRebuildResultDto> RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new IndexRebuildResultDto();

        try
        {
            // Tüm aktif projeleri ve ilişkili bilgilerini Cartesian patlaması olmadan yükle
            var projects = await _context.Projects
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectIntegrations)
                .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team)
                .Include(p => p.ProjectLocations).ThenInclude(pl => pl.Location)
                .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProjectKnowledgeChunks)
                .ToListAsync(cancellationToken);

            result.ProjectsProcessed = projects.Count;
            var currentModel = _options.Local.EmbeddingModel;
            var currentDimension = _options.Local.EmbeddingDimension;

            // 1. Tüm projelerin taslaklarını hazırla ve değişmeyenleri ayıkla
            var pendingWork = new List<(Project Project, KnowledgeChunkDraft Draft, ProjectKnowledgeChunk? Existing)>();

            foreach (var project in projects)
            {
                var drafts = _documentBuilder.BuildChunks(project);
                var existingChunks = project.ProjectKnowledgeChunks.ToList();

                // Artık mevcut olmayan veya silinen parçaları temizle
                var draftKeys = drafts.Select(d => d.ChunkKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var chunksToDelete = existingChunks.Where(c => !draftKeys.Contains(c.ChunkKey)).ToList();

                foreach (var chunk in chunksToDelete)
                {
                    _context.ProjectKnowledgeChunks.Remove(chunk);
                    result.ChunksDeleted++;
                }

                foreach (var draft in drafts)
                {
                    var existing = existingChunks.FirstOrDefault(c => c.ChunkKey.Equals(draft.ChunkKey, StringComparison.OrdinalIgnoreCase));

                    if (existing != null &&
                        existing.ContentHash == draft.ContentHash &&
                        existing.EmbeddingModel == currentModel &&
                        (currentDimension <= 0 || existing.EmbeddingDimension == currentDimension) &&
                        existing.EmbeddingVector != null &&
                        existing.EmbeddingVector.Length > 0)
                    {
                        // Cache hit
                        result.ChunksUnchanged++;
                        continue;
                    }

                    pendingWork.Add((project, draft, existing));
                }
            }

            // 2. Değişen ve yeni parçaları toplu (batch) olarak embed et (yüksek başarım)
            if (pendingWork.Count > 0)
            {
                const int batchSize = 32;
                for (int i = 0; i < pendingWork.Count; i += batchSize)
                {
                    var currentBatch = pendingWork.Skip(i).Take(batchSize).ToList();
                    var textsToEmbed = currentBatch.Select(w => w.Draft.Content).ToList();

                    var embResults = await _embeddingProvider.GenerateEmbeddingsAsync(textsToEmbed, EmbeddingType.Document, cancellationToken);

                    for (int j = 0; j < currentBatch.Count; j++)
                    {
                        var (project, draft, existing) = currentBatch[j];
                        var embResult = j < embResults.Count ? embResults[j] : null;

                        if (embResult == null || !embResult.Success || embResult.Vector.Length == 0)
                        {
                            _logger.LogWarning("Batch embedding failed for project {ProjectId} chunk {ChunkKey}: {Error}",
                                project.Id, draft.ChunkKey, embResult?.Error ?? "No result");
                            continue;
                        }

                        if (existing != null)
                        {
                            existing.Content = draft.Content;
                            existing.ContentHash = draft.ContentHash;
                            existing.EmbeddingModel = currentModel;
                            existing.EmbeddingDimension = embResult.Dimension;
                            existing.EmbeddingVector = VectorUtils.ToBytes(embResult.Vector);
                            existing.IndexedAtUtc = DateTime.UtcNow;
                        }
                        else
                        {
                            var newChunk = new ProjectKnowledgeChunk
                            {
                                ProjectId = project.Id,
                                ChunkKey = draft.ChunkKey,
                                Content = draft.Content,
                                ContentHash = draft.ContentHash,
                                EmbeddingModel = currentModel,
                                EmbeddingDimension = embResult.Dimension,
                                EmbeddingVector = VectorUtils.ToBytes(embResult.Vector),
                                IndexedAtUtc = DateTime.UtcNow
                            };
                            _context.ProjectKnowledgeChunks.Add(newChunk);
                        }

                        result.ChunksCreatedOrUpdated++;
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            sw.Stop();

            result.Success = true;
            result.DurationMs = sw.ElapsedMilliseconds;
            _logger.LogInformation("Knowledge index rebuild completed in {Duration} ms. Processed: {Projects}, Updated: {Updated}, Unchanged: {Unchanged}, Deleted: {Deleted}",
                result.DurationMs, result.ProjectsProcessed, result.ChunksCreatedOrUpdated, result.ChunksUnchanged, result.ChunksDeleted);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Failed to rebuild project knowledge index.");
            result.Success = false;
            result.DurationMs = sw.ElapsedMilliseconds;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<IndexProjectResultDto> IndexProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var result = new IndexProjectResultDto { ProjectId = projectId };

        var project = await _context.Projects
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
            .Include(p => p.ProjectIntegrations)
            .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team)
            .Include(p => p.ProjectLocations).ThenInclude(pl => pl.Location)
            .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProjectKnowledgeChunks)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null || project.IsDeleted)
        {
            await RemoveProjectIndexAsync(projectId, cancellationToken);
            result.Success = true;
            return result;
        }

        var drafts = _documentBuilder.BuildChunks(project);
        var existingChunks = project.ProjectKnowledgeChunks.ToList();
        var currentModel = _options.Local.EmbeddingModel;
        var currentDimension = _options.Local.EmbeddingDimension;

        var draftKeys = drafts.Select(d => d.ChunkKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var chunk in existingChunks.Where(c => !draftKeys.Contains(c.ChunkKey)))
        {
            _context.ProjectKnowledgeChunks.Remove(chunk);
        }

        foreach (var draft in drafts)
        {
            var existing = existingChunks.FirstOrDefault(c => c.ChunkKey.Equals(draft.ChunkKey, StringComparison.OrdinalIgnoreCase));

            if (existing != null &&
                existing.ContentHash == draft.ContentHash &&
                existing.EmbeddingModel == currentModel &&
                (currentDimension <= 0 || existing.EmbeddingDimension == currentDimension) &&
                existing.EmbeddingVector != null &&
                existing.EmbeddingVector.Length > 0)
            {
                result.ChunksUnchanged++;
                continue;
            }

            var embResult = await _embeddingProvider.GenerateEmbeddingAsync(draft.Content, EmbeddingType.Document, cancellationToken);
            if (!embResult.Success || embResult.Vector.Length == 0)
            {
                result.ErrorMessage = embResult.Error;
                continue;
            }

            if (existing != null)
            {
                existing.Content = draft.Content;
                existing.ContentHash = draft.ContentHash;
                existing.EmbeddingModel = currentModel;
                existing.EmbeddingDimension = embResult.Dimension;
                existing.EmbeddingVector = VectorUtils.ToBytes(embResult.Vector);
                existing.IndexedAtUtc = DateTime.UtcNow;
            }
            else
            {
                var newChunk = new ProjectKnowledgeChunk
                {
                    ProjectId = project.Id,
                    ChunkKey = draft.ChunkKey,
                    Content = draft.Content,
                    ContentHash = draft.ContentHash,
                    EmbeddingModel = currentModel,
                    EmbeddingDimension = embResult.Dimension,
                    EmbeddingVector = VectorUtils.ToBytes(embResult.Vector),
                    IndexedAtUtc = DateTime.UtcNow
                };
                _context.ProjectKnowledgeChunks.Add(newChunk);
            }

            result.ChunksIndexed++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        result.Success = true;
        return result;
    }

    public async Task RemoveProjectIndexAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var chunks = await _context.ProjectKnowledgeChunks
            .Where(c => c.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        if (chunks.Count > 0)
        {
            _context.ProjectKnowledgeChunks.RemoveRange(chunks);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed {Count} knowledge chunks for project {ProjectId}.", chunks.Count, projectId);
        }
    }

    public async Task<ReconciliationResultDto> ReconcileBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new ReconciliationResultDto
        {
            BatchSizeRequested = batchSize
        };

        try
        {
            if (!_options.Enabled)
            {
                result.ProviderUnavailable = true;
                result.ErrorMessage = "AI service is disabled in configuration.";
                return result;
            }

            var currentModel = _options.Local.EmbeddingModel;
            var currentDimension = _options.Local.EmbeddingDimension;

            // 1. Silinmiş projelerin artık chunk'ları varsa temizle (Ineligible cleanup)
            var deletedProjectIds = await _context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (deletedProjectIds.Count > 0)
            {
                var orphanedChunks = await _context.ProjectKnowledgeChunks
                    .IgnoreQueryFilters()
                    .Where(c => deletedProjectIds.Contains(c.ProjectId))
                    .ToListAsync(cancellationToken);

                if (orphanedChunks.Count > 0)
                {
                    _context.ProjectKnowledgeChunks.RemoveRange(orphanedChunks);
                    await _context.SaveChangesAsync(cancellationToken);
                    result.ChunksDeleted += orphanedChunks.Count;
                    _logger.LogInformation("Reconciliation: Cleaned up {Count} orphaned chunks from deleted projects.", orphanedChunks.Count);
                }
            }

            // 2. Tüm uygun projeleri ve chunk'larını çek
            var eligibleProjects = await _context.Projects
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.Status)
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectIntegrations)
                .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team)
                .Include(p => p.ProjectLocations).ThenInclude(pl => pl.Location)
                .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProjectKnowledgeChunks)
                .Where(p => !p.IsDeleted)
                .ToListAsync(cancellationToken);

            result.TotalEligibleProjects = eligibleProjects.Count;

            // 3. Durum analizi (CURRENT, MISSING, STALE)
            var missingProjects = new List<Project>();
            var staleProjects = new List<Project>();

            foreach (var project in eligibleProjects)
            {
                var expectedDrafts = _documentBuilder.BuildChunks(project);
                var existingChunks = project.ProjectKnowledgeChunks.ToList();

                if (existingChunks.Count == 0)
                {
                    missingProjects.Add(project);
                    continue;
                }

                bool isStale = false;
                if (existingChunks.Count != expectedDrafts.Count)
                {
                    isStale = true;
                }
                else
                {
                    foreach (var draft in expectedDrafts)
                    {
                        var match = existingChunks.FirstOrDefault(c =>
                            c.ChunkKey.Equals(draft.ChunkKey, StringComparison.OrdinalIgnoreCase) &&
                            c.EmbeddingModel == currentModel &&
                            (currentDimension <= 0 || c.EmbeddingDimension == currentDimension) &&
                            c.ContentHash == draft.ContentHash &&
                            c.EmbeddingVector != null &&
                            c.EmbeddingVector.Length > 0);

                        if (match == null)
                        {
                            isStale = true;
                            break;
                        }
                    }
                }

                if (isStale)
                {
                    staleProjects.Add(project);
                }
                else
                {
                    result.CurrentProjectsCount++;
                }
            }

            result.MissingProjectsCount = missingProjects.Count;
            result.StaleProjectsCount = staleProjects.Count;

            // 4. Sınırlandırılmış grup (bounded batch) seçimi (Önce Missing, sonra Stale)
            var batch = missingProjects
                .Concat(staleProjects)
                .Take(batchSize > 0 ? batchSize : 5)
                .ToList();

            if (batch.Count == 0)
            {
                sw.Stop();
                result.Success = true;
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.ProjectsProcessedInBatch = batch.Count;

            // 5. Batch içindeki projeleri güvenli şekilde işle
            foreach (var project in batch)
            {
                var drafts = _documentBuilder.BuildChunks(project);
                var existingChunks = project.ProjectKnowledgeChunks.ToList();

                var draftKeys = drafts.Select(d => d.ChunkKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var toDelete = existingChunks.Where(c => !draftKeys.Contains(c.ChunkKey)).ToList();
                foreach (var c in toDelete)
                {
                    _context.ProjectKnowledgeChunks.Remove(c);
                    result.ChunksDeleted++;
                }

                var pendingDrafts = new List<(KnowledgeChunkDraft Draft, ProjectKnowledgeChunk? Existing)>();
                foreach (var draft in drafts)
                {
                    var existing = existingChunks.FirstOrDefault(c => c.ChunkKey.Equals(draft.ChunkKey, StringComparison.OrdinalIgnoreCase));
                    if (existing != null &&
                        existing.ContentHash == draft.ContentHash &&
                        existing.EmbeddingModel == currentModel &&
                        (currentDimension <= 0 || existing.EmbeddingDimension == currentDimension) &&
                        existing.EmbeddingVector != null &&
                        existing.EmbeddingVector.Length > 0)
                    {
                        result.ChunksUnchanged++;
                        continue;
                    }

                    pendingDrafts.Add((draft, existing));
                }

                if (pendingDrafts.Count == 0)
                {
                    continue;
                }

                var texts = pendingDrafts.Select(p => p.Draft.Content).ToList();
                IReadOnlyList<EmbeddingResult> embResults;
                try
                {
                    embResults = await _embeddingProvider.GenerateEmbeddingsAsync(texts, EmbeddingType.Document, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Reconciliation: Embedding provider failed for project {ProjectId} ({Slug}): {Message}",
                        project.Id, project.Slug, ex.Message);
                    result.FailedProjectsCount++;
                    continue;
                }

                bool allDraftsSucceeded = true;
                for (int i = 0; i < pendingDrafts.Count; i++)
                {
                    var (draft, existing) = pendingDrafts[i];
                    var emb = i < embResults.Count ? embResults[i] : null;

                    if (emb == null || !emb.Success || emb.Vector.Length == 0)
                    {
                        allDraftsSucceeded = false;
                        _logger.LogWarning("Reconciliation: Embedding failed for project {ProjectId} chunk {ChunkKey}: {Error}",
                            project.Id, draft.ChunkKey, emb?.Error ?? "No result");
                        continue;
                    }

                    if (existing != null)
                    {
                        existing.Content = draft.Content;
                        existing.ContentHash = draft.ContentHash;
                        existing.EmbeddingModel = currentModel;
                        existing.EmbeddingDimension = emb.Dimension;
                        existing.EmbeddingVector = VectorUtils.ToBytes(emb.Vector);
                        existing.IndexedAtUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        var newChunk = new ProjectKnowledgeChunk
                        {
                            ProjectId = project.Id,
                            ChunkKey = draft.ChunkKey,
                            Content = draft.Content,
                            ContentHash = draft.ContentHash,
                            EmbeddingModel = currentModel,
                            EmbeddingDimension = emb.Dimension,
                            EmbeddingVector = VectorUtils.ToBytes(emb.Vector),
                            IndexedAtUtc = DateTime.UtcNow
                        };
                        _context.ProjectKnowledgeChunks.Add(newChunk);
                    }

                    result.ChunksCreatedOrUpdated++;
                }

                if (!allDraftsSucceeded)
                {
                    result.FailedProjectsCount++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            sw.Stop();

            result.Success = result.FailedProjectsCount == 0;
            result.DurationMs = sw.ElapsedMilliseconds;
            _logger.LogInformation(
                "Semantic Reconciliation completed in {Duration} ms. Batch: {Batch}, Updated: {Updated}, Unchanged: {Unchanged}, Failed: {Failed}",
                result.DurationMs, result.ProjectsProcessedInBatch, result.ChunksCreatedOrUpdated, result.ChunksUnchanged, result.FailedProjectsCount);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Reconciliation batch execution encountered an unexpected error.");
            result.Success = false;
            result.DurationMs = sw.ElapsedMilliseconds;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<IndexStatusDto> GetIndexStatusAsync(CancellationToken cancellationToken = default)
    {
        var health = await _embeddingProvider.CheckHealthAsync(cancellationToken);
        var currentModel = _options.Local.EmbeddingModel;
        var currentDimension = _options.Local.EmbeddingDimension;

        var projects = await _context.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
            .Include(p => p.ProjectIntegrations)
            .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team)
            .Include(p => p.ProjectLocations).ThenInclude(pl => pl.Location)
            .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProjectKnowledgeChunks)
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);

        int totalEligible = projects.Count;
        int totalValidChunks = 0;
        int indexedProjectsCount = 0;
        int staleOrMissingChunks = 0;
        DateTime? latestChunkDate = null;

        foreach (var project in projects)
        {
            var expectedDrafts = _documentBuilder.BuildChunks(project);
            var chunks = project.ProjectKnowledgeChunks.ToList();
            bool projectHasValidChunks = false;

            foreach (var draft in expectedDrafts)
            {
                var matchingChunk = chunks.FirstOrDefault(c =>
                    c.ChunkKey.Equals(draft.ChunkKey, StringComparison.OrdinalIgnoreCase) &&
                    c.EmbeddingModel == currentModel &&
                    (currentDimension <= 0 || c.EmbeddingDimension == currentDimension) &&
                    c.ContentHash == draft.ContentHash &&
                    c.EmbeddingVector != null &&
                    c.EmbeddingVector.Length > 0);

                if (matchingChunk != null)
                {
                    totalValidChunks++;
                    projectHasValidChunks = true;
                    if (latestChunkDate == null || matchingChunk.IndexedAtUtc > latestChunkDate)
                    {
                        latestChunkDate = matchingChunk.IndexedAtUtc;
                    }
                }
                else
                {
                    staleOrMissingChunks++;
                }
            }

            if (projectHasValidChunks)
            {
                indexedProjectsCount++;
            }
        }

        return new IndexStatusDto
        {
            IsProviderOnline = health.IsReachable,
            EmbeddingModel = currentModel,
            EmbeddingDimension = currentDimension,
            TotalEligibleProjects = totalEligible,
            TotalIndexedProjects = indexedProjectsCount,
            TotalChunks = totalValidChunks,
            StaleOrMissingChunks = staleOrMissingChunks,
            LastRebuiltAtUtc = latestChunkDate,
            StatusMessage = health.StatusMessage
        };
    }
}
