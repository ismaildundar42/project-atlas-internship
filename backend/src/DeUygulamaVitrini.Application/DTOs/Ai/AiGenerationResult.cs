namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Sağlayıcıdan bağımsız metin üretim sonucu modeli.
/// </summary>
public record AiGenerationResult
{
    /// <summary>
    /// İşlemin başarı durumu.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Model tarafından üretilen temiz metin içeriği.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Yanıtı üreten AI sağlayıcısının adı (örn. "Local", "CorporateAzure", vb.).
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Yanıtı üreten modelin adı/tanımlayıcısı.
    /// </summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Üretim süresi (milisaniye cinsinden).
    /// </summary>
    public long DurationMs { get; init; }

    /// <summary>
    /// Model tamamlama bitiş nedeni (örn. "stop", "length", "eos").
    /// </summary>
    public string FinishReason { get; init; } = "stop";

    /// <summary>
    /// Girdi istem jeton (token) sayısı.
    /// </summary>
    public int PromptTokens { get; init; }

    /// <summary>
    /// Üretilen çıktı jeton (token) sayısı.
    /// </summary>
    public int CompletionTokens { get; init; }

    /// <summary>
    /// Hata oluştuysa anlaşılır hata mesajı.
    /// </summary>
    public string? ErrorMessage { get; init; }

    public static AiGenerationResult Succeeded(
        string content,
        string provider,
        string model,
        long durationMs,
        string finishReason = "stop",
        int promptTokens = 0,
        int completionTokens = 0) =>
        new()
        {
            Success = true,
            Content = content,
            Provider = provider,
            Model = model,
            DurationMs = durationMs,
            FinishReason = finishReason,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens
        };

    public static AiGenerationResult Failed(string errorMessage, string provider, string model = "", long durationMs = 0) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            Provider = provider,
            Model = model,
            DurationMs = durationMs
        };
}
