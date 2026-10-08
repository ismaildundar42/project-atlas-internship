using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DeUygulamaVitrini.Application.Common.Exceptions;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Yetkilendirme duyarlı tekil proje yapay zeka özetleme servisi.
/// Yetkili kullanıcının talep ettiği proje için doğrudan SQL verisi üzerinden
/// sınırlı ve topraklanmış (bounded & grounded) bağlam oluşturarak IAiProvider üzerinden özet üretir.
/// Vektör/RAG aramasına ihtiyaç duymaz.
/// </summary>
public class ProjectAiSummaryService : IProjectAiSummaryService
{
    private readonly IApplicationDbContext _context;
    private readonly IAiProvider _aiProvider;
    private readonly ILogger<ProjectAiSummaryService> _logger;

    public ProjectAiSummaryService(
        IApplicationDbContext context,
        IAiProvider aiProvider,
        ILogger<ProjectAiSummaryService> logger)
    {
        _context = context;
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<ProjectAiSummaryResponseDto> GenerateSummaryAsync(
        int projectId,
        string? language,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. Yetkilendirme ve Proje Yükleme (Authoritative SQL Source of Truth)
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProjectTeams).ThenInclude(pt => pt.Team).ThenInclude(t => t.Department)
            .Include(p => p.ProjectLocations).ThenInclude(pl => pl.Location)
            .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
            .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProjectIntegrations)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("AI Proje Özeti: Proje bulunamadı veya silinmiş. ProjectId={ProjectId}", projectId);
            throw new KeyNotFoundException($"ID değeri '{projectId}' olan bir proje bulunamadı.");
        }

        // Yetkilendirme kuralı: Yayınlanmış ve Onaylanmış projeler herkese açıktır.
        // Taslak veya Onay Bekleyen projeler yalnızca Admin/SuperAdmin veya proje oluşturan kullanıcı tarafından özetlenebilir.
        var isAuthorized = isAdmin ||
            (project.IsPublished && project.ApprovalStatus == ProjectApprovalStatus.Approved) ||
            (currentUserId > 0 && project.CreatedByUserId == currentUserId);

        if (!isAuthorized)
        {
            _logger.LogWarning("AI Proje Özeti: Yetkisiz erişim denemesi engellendi. ProjectId={ProjectId}, UserId={UserId}, IsAdmin={IsAdmin}",
                projectId, currentUserId, isAdmin);
            throw new UnauthorizedAccessException("Bu projenin özetini görüntüleme yetkiniz bulunmamaktadır.");
        }

        // 2. Dil Belirleme (Language Detection)
        var isEnglish = string.Equals(language?.Trim(), "en", StringComparison.OrdinalIgnoreCase);

        // 3. Sınırlı ve Topraklanmış Bağlam Oluşturma (Bounded Context Construction)
        var projectContext = BuildProjectContext(project, isEnglish);

        // 4. Sistem ve Kullanıcı İstemlerinin Hazırlanması (Prompts)
        var (systemPrompt, userPrompt) = BuildPrompts(project.Name, projectContext, isEnglish);

        // 5. IAiProvider Çağrısı (Direct Grounded Generation)
        _logger.LogInformation("AI Proje Özeti: Model çağrısı başlatılıyor. ProjectId={ProjectId}, Language={Lang}",
            projectId, isEnglish ? "en" : "tr");

        var generationRequest = new AiGenerationRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = 0.2, // Yüksek tutarlılık ve deterministik yanıtlar için düşük sıcaklık
            MaxTokens = 500    // Phase 20.2B: 5 yapılandırılmış bölüm için güvenli ve tamamlama garantili bütçe
        };

        var aiResult = await _aiProvider.GenerateAsync(generationRequest, cancellationToken);
        stopwatch.Stop();

        if (!aiResult.Success || string.IsNullOrWhiteSpace(aiResult.Content))
        {
            _logger.LogWarning("AI Proje Özeti: AI sağlayıcısı yanıt üretemedi. ProjectId={ProjectId}, DurationMs={DurationMs}, Error={Error}",
                projectId, stopwatch.ElapsedMilliseconds, aiResult.ErrorMessage);

            throw new GenerationProviderUnavailableException(
                "Yapay zeka özet servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyiniz.");
        }

        var finishReason = string.IsNullOrWhiteSpace(aiResult.FinishReason) ? "stop" : aiResult.FinishReason.ToLowerInvariant();
        var isTruncated = finishReason == "length" || IsTruncatedText(aiResult.Content, finishReason);
        var isComplete = !isTruncated;

        // Sanitizasyon: Sarkık veya yarım kalmış markdown kalıntılarını temizle
        var cleanedSummary = CleanDanglingMarkdown(aiResult.Content.Trim());

        if (isTruncated)
        {
            _logger.LogWarning("AI Proje Özeti token sınırına ulaştı (FinishReason={FinishReason}). ProjectId={ProjectId}, Tokens={Tokens}",
                finishReason, projectId, aiResult.CompletionTokens);
        }
        else
        {
            _logger.LogInformation("AI Proje Özeti başarıyla üretildi. ProjectId={ProjectId}, Provider={Provider}, Model={Model}, DurationMs={DurationMs}, Tokens={Tokens}",
                projectId, aiResult.Provider, aiResult.Model, stopwatch.ElapsedMilliseconds, aiResult.CompletionTokens);
        }

        return new ProjectAiSummaryResponseDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Summary = cleanedSummary,
            GeneratedAtUtc = DateTime.UtcNow,
            Provider = aiResult.Provider,
            Model = aiResult.Model,
            DurationMs = stopwatch.ElapsedMilliseconds,
            ProviderAvailable = true,
            FinishReason = finishReason,
            IsComplete = isComplete,
            PromptTokens = aiResult.PromptTokens,
            CompletionTokens = aiResult.CompletionTokens
        };
    }

    /// <summary>
    /// Projenin yetkilendirilmiş alanlarından sınırlı, gereksiz tekrarlardan arındırılmış güvenli metin bağlamı oluşturur.
    /// Phase 20.2: Token bütçesini optimize eder (~1,200-1,500 karakter / ~300 token).
    /// </summary>
    private static string BuildProjectContext(Domain.Entities.Project project, bool isEnglish)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Proje Adı: {project.Name}");
        if (project.Category != null) sb.AppendLine($"Kategori: {project.Category.Name}");
        if (project.Status != null) sb.AppendLine($"Durum: {project.Status.Name}");
        sb.AppendLine($"Geliştirme Tipi: {project.DevelopmentType}");

        // Açıklama: Öncelik ShortDescription, yoksa Description'ın ilk 250 karakteri
        if (!string.IsNullOrWhiteSpace(project.ShortDescription))
        {
            sb.AppendLine($"Özet Açıklama: {project.ShortDescription.Trim()}");
        }
        else if (!string.IsNullOrWhiteSpace(project.Description))
        {
            var desc = project.Description.Trim();
            sb.AppendLine($"Özet Açıklama: {(desc.Length > 250 ? desc.Substring(0, 250) + "..." : desc)}");
        }

        if (!string.IsNullOrWhiteSpace(project.Purpose))
            sb.AppendLine($"Amacı: {project.Purpose.Trim()}");

        if (!string.IsNullOrWhiteSpace(project.ProblemSolved))
            sb.AppendLine($"Çözülen Problem: {project.ProblemSolved.Trim()}");

        if (!string.IsNullOrWhiteSpace(project.TechnicalDescription))
        {
            var techDesc = project.TechnicalDescription.Trim();
            sb.AppendLine($"Teknik Altyapı Özeti: {(techDesc.Length > 350 ? techDesc.Substring(0, 350) + "..." : techDesc)}");
        }

        if (!string.IsNullOrWhiteSpace(project.BusinessImpact))
            sb.AppendLine($"İş Kazancı / Beklenen Katkı: {project.BusinessImpact.Trim()}");

        var teams = project.ProjectTeams
            .Where(pt => pt.Team != null)
            .Select(pt => pt.Team.Department != null ? $"{pt.Team.Name} ({pt.Team.Department.Name})" : pt.Team.Name)
            .Distinct()
            .ToList();
        if (teams.Count > 0)
            sb.AppendLine($"Sorumlu Ekipler: {string.Join(", ", teams)}");

        var locations = project.ProjectLocations
            .Where(pl => pl.Location != null)
            .Select(pl => pl.Location.Name)
            .Distinct()
            .ToList();
        if (locations.Count > 0)
            sb.AppendLine($"Uygulandığı Lokasyonlar: {string.Join(", ", locations)}");

        var techs = project.ProjectTechnologies
            .Where(pt => pt.Technology != null)
            .Select(pt => pt.Technology.Name)
            .Distinct()
            .ToList();
        if (techs.Count > 0)
            sb.AppendLine($"Kullanılan Teknolojiler: {string.Join(", ", techs)}");

        var integrations = project.ProjectIntegrations
            .Where(pi => !string.IsNullOrWhiteSpace(pi.Name))
            .Select(pi => pi.Name)
            .Distinct()
            .ToList();
        if (integrations.Count > 0)
            sb.AppendLine($"Entegrasyonlar: {string.Join(", ", integrations)}");

        return sb.ToString();
    }

    /// <summary>
    /// Dil tercihlerine uygun, öz ve hızlı üretimi teşvik eden kurumsal sistem ve kullanıcı istemlerini oluşturur.
    /// Phase 20.2B: 5 başlık için 1-2 cümlelik kesin sınır koyarak token tavanına takılmayı önler.
    /// </summary>
    private static (string SystemPrompt, string UserPrompt) BuildPrompts(string projectName, string projectContext, bool isEnglish)
    {
        if (isEnglish)
        {
            var systemPrompt =
@"You are an authoritative enterprise project summary specialist for the Demir Export Project Library.
TASK:
Using ONLY the verified project information provided below, prepare a concise, structured, and enterprise-grade summary (120-180 words) for employees and managers.

RULES:
1. Ground your response STRICTLY in the provided project data. NEVER invent missing facts, quantitative metrics, or assumptions.
2. Distinguish expected/planned impacts from achieved results. Do not transform planned benefits into proven facts.
3. Start directly with the structured output. Do NOT write conversational greetings, introductions, or sign-offs.
4. Write at most 1-2 concise sentences under each heading. Total response must remain within 120-180 words.
5. Respond in English.

OUTPUT STRUCTURE:
- **Overview**: 1-2 concise sentences explaining what the project does.
- **Purpose & Problem Solved**: 1-2 concise sentences on core objective and main challenge.
- **Users & Operational Scope**: 1-2 concise sentences on responsible teams and locations.
- **Technology Stack**: 1-2 concise sentences or bullets on key technologies and integrations.
- **Business Impact**: 1-2 concise sentences on enterprise value.";

            var userPrompt =
$@"Please summarize the following enterprise project concisely based strictly on its verified data:

--- PROJECT DATA ---
{projectContext.Trim()}
--- END OF DATA ---";

            return (systemPrompt, userPrompt);
        }
        else
        {
            var systemPrompt =
@"Sen Demir Export Proje Kütüphanesi için yetkili bir kurumsal proje özetleme uzmanısın.
GÖREV:
Aşağıda verilen doğrulanmış proje bilgilerini kullanarak net, öz, yapılandırılmış kurumsal bir özet (120-180 kelime) hazırla.

KURALLAR:
1. SADECE sağlanan proje bilgilerine dayan. Asla metinde yer almayan bilgi, sayısal veri veya varsayım uydurma.
2. 'Beklenen' veya 'hedeflenen' faydaları planlanan/beklenen olarak belirt; gerçekleşmiş kesin başarı gibi gösterme.
3. Giriş-çıkış nezaket cümleleri yazma; doğrudan aşağıdaki 5 başlıkla başla.
4. Her başlık altında en fazla 1-2 kısa ve net cümle yaz. Toplam yanıt 120-180 kelimeyi kesinlikle aşmamalıdır.
5. Türkçe yanıt ver.

ÇIKTI YAPISI:
- **Genel Bakış**: 1-2 kısa cümle.
- **Amaç ve Çözülen Problem**: 1-2 kısa cümle.
- **Kullanıcılar ve Saha/Kapsam**: 1-2 kısa cümle.
- **Teknolojik Altyapı**: 1-2 kısa madde veya cümle.
- **İş Katkısı**: 1-2 kısa cümle.";

            var userPrompt =
$@"Lütfen aşağıdaki kurumsal projeyi yalnızca verilen doğrulanmış verilerine dayanarak öz ve net bir şekilde özetle:

--- PROJE BİLGİLERİ ---
{projectContext.Trim()}
--- BİLGİLERİN SONU ---";

            return (systemPrompt, userPrompt);
        }
    }

    public static string CleanDanglingMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var result = text.TrimEnd();

        // 1. Remove dangling trailing headings without content or closing, e.g., "\n**Teknolojik Altyapı" or "\n### Teknolojik"
        result = Regex.Replace(result, @"\n+(\*{2,3}|#{1,6})\s*[A-Za-zÇĞİÖŞÜçğıöşü\s]*$", string.Empty, RegexOptions.Multiline).TrimEnd();

        // 2. Remove dangling bullet markers at the very end (e.g., "\n- " or "\n* ")
        result = Regex.Replace(result, @"\n+[\-\*]\s*$", string.Empty, RegexOptions.Multiline).TrimEnd();

        // 3. Balance unclosed bold/italic markdown markers (e.g., odd number of '**')
        int boldCount = Regex.Matches(result, @"\*\*").Count;
        if (boldCount % 2 != 0)
        {
            var lastIdx = result.LastIndexOf("**", StringComparison.Ordinal);
            if (lastIdx >= 0)
            {
                result = result.Substring(0, lastIdx).TrimEnd();
            }
        }

        return result;
    }

    private static bool IsTruncatedText(string text, string? finishReason)
    {
        if (finishReason != null && finishReason.Equals("length", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(text)) return false;
        var lastChar = text.TrimEnd()[^1];
        return lastChar != '.' && lastChar != '!' && lastChar != '?' && lastChar != ')' && lastChar != '*' && lastChar != ':' && lastChar != '"';
    }
}
