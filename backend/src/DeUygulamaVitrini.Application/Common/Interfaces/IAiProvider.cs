using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Yapay zeka sağlayıcı bağımsız soyutlama arayüzü.
/// Application ve API katmanları somut model veya SDK detaylarından (Ollama, LM Studio, OpenAI, Azure vb.) izoledir.
/// </summary>
public interface IAiProvider
{
    /// <summary>
    /// Sağlayıcının adı (örn. "Local", "CorporateCloud").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Metin üretim isteğini asenkron olarak işler.
    /// </summary>
    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sağlayıcının erişilebilirlik ve model hazır olma durumunu kontrol eder.
    /// </summary>
    Task<AiHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
}
