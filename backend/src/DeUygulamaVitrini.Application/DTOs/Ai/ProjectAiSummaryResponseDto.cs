namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Yetkilendirilmiş proje için üretilen AI özet yanıt DTO'su.
/// </summary>
public class ProjectAiSummaryResponseDto
{
    /// <summary>
    /// Özetlenen projenin benzersiz kimlik numarası.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Projenin adı.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Model tarafından üretilen yapılandırılmış ve topraklanmış özet metni (Markdown formatında).
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Özetin üretildiği UTC zaman damgası.
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Özeti üreten sağlayıcı adı (örn. "Local", "CorporateAzure").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Özeti üreten model adı (örn. "qwen2.5-3b-instruct").
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Üretim süresi (milisaniye cinsinden).
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// AI sağlayıcısının erişilebilirlik durumu.
    /// </summary>
    public bool ProviderAvailable { get; set; } = true;

    /// <summary>
    /// Üretimin doğal sonlanma nedeni ("stop", "length" vb.).
    /// </summary>
    public string FinishReason { get; set; } = "stop";

    /// <summary>
    /// Özetin kesilmeden eksiksiz tamamlanıp tamamlanmadığı.
    /// </summary>
    public bool IsComplete { get; set; } = true;

    /// <summary>
    /// Kullanılan girdi token sayısı.
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Üretilen çıktı token sayısı.
    /// </summary>
    public int CompletionTokens { get; set; }
}
