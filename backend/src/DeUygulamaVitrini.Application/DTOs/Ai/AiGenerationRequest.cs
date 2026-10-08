namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Sağlayıcıdan bağımsız metin üretim isteği modeli.
/// </summary>
public record AiGenerationRequest
{
    /// <summary>
    /// Kullanıcıdan veya çağırıcı servisinden gelen asıl istem metni.
    /// </summary>
    public string UserPrompt { get; init; } = string.Empty;

    /// <summary>
    /// Modele rolünü ve davranış kurallarını belirten opsiyonel sistem talimatı.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Üretim rastgeleliği / yaratıcılık derecesi (0.0 = deterministik, 1.0 = yaratıcı).
    /// </summary>
    public double? Temperature { get; init; }

    /// <summary>
    /// Üretilecek maksimum belirteç (token) sınırı.
    /// </summary>
    public int? MaxTokens { get; init; }
}
