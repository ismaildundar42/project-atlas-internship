namespace DeUygulamaVitrini.Application.Common.Exceptions;

/// <summary>
/// Metin üretim yapay zeka sağlayıcısının (DeepSeek, LM Studio, Ollama vb.) erişilemez, kapalı veya zaman aşımına uğramış olduğunu belirten istisna.
/// API seviyesinde güvenli ve steril bir HTTP 503 Service Unavailable yanıtına dönüştürülür.
/// </summary>
public class GenerationProviderUnavailableException : Exception
{
    public GenerationProviderUnavailableException()
        : base("Proje Asistanı şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.")
    {
    }

    public GenerationProviderUnavailableException(string message)
        : base(message)
    {
    }

    public GenerationProviderUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
