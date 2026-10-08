using System.Text;
using System.Text.RegularExpressions;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Entities;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Proje varlığından deterministik, temiz ve anlamsal bilgi parçaları (knowledge chunks) üreten servis.
/// </summary>
public class ProjectKnowledgeDocumentBuilder : IProjectKnowledgeDocumentBuilder
{
    private static readonly Regex MultipleSpacesRegex = new(@"[ \t]+", RegexOptions.Compiled);
    private static readonly Regex MultipleNewlinesRegex = new(@"(\r?\n){3,}", RegexOptions.Compiled);

    public IReadOnlyList<KnowledgeChunkDraft> BuildChunks(Project project)
    {
        if (project == null)
            return Array.Empty<KnowledgeChunkDraft>();

        var chunks = new List<KnowledgeChunkDraft>();

        // ─── 1. OVERVIEW (Genel Bakış, Amaç, Problem, İş Etkisi) ───────────────
        var overviewContent = BuildOverviewChunk(project);
        if (IsSubstantive(overviewContent))
        {
            chunks.Add(new KnowledgeChunkDraft
            {
                ChunkKey = "OVERVIEW",
                Content = overviewContent,
                ContentHash = VectorUtils.ComputeSha256(overviewContent)
            });
        }

        // ─── 2. TECHNICAL (Teknik Mimari, Teknolojiler, Entegrasyonlar) ───────
        var technicalContent = BuildTechnicalChunk(project);
        if (IsSubstantive(technicalContent))
        {
            chunks.Add(new KnowledgeChunkDraft
            {
                ChunkKey = "TECHNICAL",
                Content = technicalContent,
                ContentHash = VectorUtils.ComputeSha256(technicalContent)
            });
        }

        // ─── 3. ORGANIZATION_USAGE (Organizasyon, Saha Kullanımı, Lokasyonlar) ─
        var orgUsageContent = BuildOrgUsageChunk(project);
        if (IsSubstantive(orgUsageContent))
        {
            chunks.Add(new KnowledgeChunkDraft
            {
                ChunkKey = "ORGANIZATION_USAGE",
                Content = orgUsageContent,
                ContentHash = VectorUtils.ComputeSha256(orgUsageContent)
            });
        }

        return chunks;
    }

    private string BuildOverviewChunk(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Proje: {project.Name}");

        if (project.Category != null && !string.IsNullOrWhiteSpace(project.Category.Name))
            sb.AppendLine($"Kategori: {project.Category.Name}");

        if (project.Status != null && !string.IsNullOrWhiteSpace(project.Status.Name))
            sb.AppendLine($"Durum: {project.Status.Name}");

        if (!string.IsNullOrWhiteSpace(project.ShortDescription))
            sb.AppendLine($"Özet: {project.ShortDescription}");

        if (!string.IsNullOrWhiteSpace(project.Purpose))
            sb.AppendLine($"Amaç: {project.Purpose}");

        if (!string.IsNullOrWhiteSpace(project.ProblemSolved))
            sb.AppendLine($"Çözülen Problem: {project.ProblemSolved}");

        if (!string.IsNullOrWhiteSpace(project.BusinessImpact))
            sb.AppendLine($"İş Etkisi ve Kazanımlar: {project.BusinessImpact}");

        if (!string.IsNullOrWhiteSpace(project.TargetAudience))
            sb.AppendLine($"Hedef Kitle: {project.TargetAudience}");

        if (!string.IsNullOrWhiteSpace(project.Description))
            sb.AppendLine($"Açıklama: {project.Description}");

        return NormalizeText(sb.ToString());
    }

    private string BuildTechnicalChunk(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Proje: {project.Name}");
        sb.AppendLine($"Geliştirme Tipi: {project.DevelopmentType}");

        if (!string.IsNullOrWhiteSpace(project.TechnicalDescription))
            sb.AppendLine($"Teknik Mimari ve Detaylar: {project.TechnicalDescription}");

        if (project.ProjectTechnologies != null && project.ProjectTechnologies.Count > 0)
        {
            var techs = project.ProjectTechnologies
                .Where(pt => pt.Technology != null && !string.IsNullOrWhiteSpace(pt.Technology.Name))
                .Select(pt => pt.Technology.Name)
                .Distinct();
            var techList = string.Join(", ", techs);
            if (!string.IsNullOrWhiteSpace(techList))
                sb.AppendLine($"Kullanılan Teknolojiler: {techList}");
        }

        if (project.ProjectIntegrations != null && project.ProjectIntegrations.Count > 0)
        {
            var integrations = project.ProjectIntegrations
                .Where(pi => !string.IsNullOrWhiteSpace(pi.Name))
                .Select(pi => $"{pi.Name} ({pi.IntegrationType}: {pi.Description})");
            var intList = string.Join("; ", integrations);
            if (!string.IsNullOrWhiteSpace(intList))
                sb.AppendLine($"Sistem Entegrasyonları: {intList}");
        }

        return NormalizeText(sb.ToString());
    }

    private string BuildOrgUsageChunk(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Proje: {project.Name}");

        if (!string.IsNullOrWhiteSpace(project.NonTechnicalDescription))
            sb.AppendLine($"Kullanıcı Açıklaması: {project.NonTechnicalDescription}");

        if (!string.IsNullOrWhiteSpace(project.AccessInstructions))
            sb.AppendLine($"Erişim ve Saha Talimatları: {project.AccessInstructions}");

        if (project.ProjectTeams != null && project.ProjectTeams.Count > 0)
        {
            var teams = project.ProjectTeams
                .Where(pt => pt.Team != null && !string.IsNullOrWhiteSpace(pt.Team.Name))
                .Select(pt => $"{pt.Team.Name} ({(pt.IsPrimary ? "Ana Sorumlu" : "Destekçi")})");
            var teamList = string.Join(", ", teams);
            if (!string.IsNullOrWhiteSpace(teamList))
                sb.AppendLine($"Sorumlu Ekipler: {teamList}");
        }

        if (project.ProjectLocations != null && project.ProjectLocations.Count > 0)
        {
            var locations = project.ProjectLocations
                .Where(pl => pl.Location != null && !string.IsNullOrWhiteSpace(pl.Location.Name))
                .Select(pl => pl.Location.Name)
                .Distinct();
            var locList = string.Join(", ", locations);
            if (!string.IsNullOrWhiteSpace(locList))
                sb.AppendLine($"Uygulanan Lokasyonlar: {locList}");
        }

        if (project.ProjectTags != null && project.ProjectTags.Count > 0)
        {
            var tags = project.ProjectTags
                .Where(pt => pt.Tag != null && !string.IsNullOrWhiteSpace(pt.Tag.Name))
                .Select(pt => pt.Tag.Name)
                .Distinct();
            var tagList = string.Join(", ", tags);
            if (!string.IsNullOrWhiteSpace(tagList))
                sb.AppendLine($"Etiketler: {tagList}");
        }

        return NormalizeText(sb.ToString());
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        normalized = MultipleSpacesRegex.Replace(normalized, " ");
        normalized = MultipleNewlinesRegex.Replace(normalized, "\n\n");
        return normalized.Trim();
    }

    private static bool IsSubstantive(string chunkContent)
    {
        if (string.IsNullOrWhiteSpace(chunkContent))
            return false;

        // "Proje: [Name]" başlığı dışındaki içerik en az 15 karakter olmalıdır
        return chunkContent.Length >= 25;
    }
}
