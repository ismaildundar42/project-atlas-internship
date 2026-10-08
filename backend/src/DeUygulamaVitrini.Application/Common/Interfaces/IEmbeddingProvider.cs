using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Vektörel metin embedding sağlayıcı arayüzü.
/// Sağlayıcıdan bağımsız (Provider-Agnostic) çalışır; LM Studio, Ollama veya Azure OpenAI modellerini soyutlar.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Tek bir metin parçası için float32 embedding vektörü üretir.
    /// </summary>
    Task<EmbeddingResult> GenerateEmbeddingAsync(
        string text,
        EmbeddingType type = EmbeddingType.Document,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Toplu (batch) metin listesi için embedding vektörleri üretir.
    /// </summary>
    Task<IReadOnlyList<EmbeddingResult>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        EmbeddingType type = EmbeddingType.Document,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Embedding sağlayıcısının çalışma ve erişilebilirlik durumunu kontrol eder.
    /// </summary>
    Task<AiHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
}
