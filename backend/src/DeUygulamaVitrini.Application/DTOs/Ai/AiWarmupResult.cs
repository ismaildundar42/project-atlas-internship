namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// AI warm-up (ısınma) işlemi sonuç modeli.
/// Sağlayıcıdan bağımsız metin üretimi ve embedding test durumlarını içerir.
/// </summary>
public record AiWarmupResult
{
    /// <summary>
    /// Metin üretimi (generation) warm-up çağrısının başarılı olup olmadığı.
    /// </summary>
    public bool GenerationSuccess { get; init; }

    /// <summary>
    /// Metin üretimi warm-up çağrısı süresi (milisaniye).
    /// </summary>
    public long GenerationDurationMs { get; init; }

    /// <summary>
    /// Metin üretimi warm-up başarısız olduysa hata mesajı.
    /// </summary>
    public string? GenerationError { get; init; }

    /// <summary>
    /// Vektör (embedding) warm-up çağrısının başarılı olup olmadığı.
    /// </summary>
    public bool EmbeddingSuccess { get; init; }

    /// <summary>
    /// Vektör warm-up çağrısı süresi (milisaniye).
    /// </summary>
    public long EmbeddingDurationMs { get; init; }

    /// <summary>
    /// Vektör warm-up başarısız olduysa hata mesajı.
    /// </summary>
    public string? EmbeddingError { get; init; }

    /// <summary>
    /// Toplam warm-up işlem süresi (milisaniye).
    /// </summary>
    public long TotalDurationMs { get; init; }

    /// <summary>
    /// Warm-up işleminin yapılandırma nedeniyle atlanıp atlanmadığı.
    /// </summary>
    public bool Skipped { get; init; }

    /// <summary>
    /// Warm-up atlandıysa gerekçesi.
    /// </summary>
    public string? SkipReason { get; init; }

    /// <summary>
    /// Tüm bileşenlerin başarıyla ısınıp ısınmadığı.
    /// </summary>
    public bool OverallSuccess => !Skipped && GenerationSuccess && EmbeddingSuccess;
}
