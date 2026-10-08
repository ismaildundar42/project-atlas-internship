namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Tekil proje için yapay zeka özetleme isteği modeli.
/// </summary>
public class ProjectAiSummaryRequestDto
{
    /// <summary>
    /// İstenen özet dili (opsiyonel, örn. "tr" veya "en"). Belirtilmezse varsayılan olarak "tr" kullanılır.
    /// </summary>
    public string? Language { get; set; }
}
