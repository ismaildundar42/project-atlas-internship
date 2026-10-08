namespace DeUygulamaVitrini.Application.Common.Exceptions;

/// <summary>
/// Vektörel embedding sağlayıcısının (LM Studio, Ollama vb.) erişilemez, kapalı veya zaman aşımına uğramış olduğunu belirten istisna.
/// API seviyesinde güvenli ve steril bir HTTP 503 Service Unavailable yanıtına dönüştürülür.
/// </summary>
public class EmbeddingProviderUnavailableException : Exception
{
    public EmbeddingProviderUnavailableException()
        : base("Anlamsal arama servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.")
    {
    }

    public EmbeddingProviderUnavailableException(string message)
        : base(message)
    {
    }

    public EmbeddingProviderUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
