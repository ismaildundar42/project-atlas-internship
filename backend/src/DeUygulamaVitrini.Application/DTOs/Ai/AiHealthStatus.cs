namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// AI altyapısının sağlık ve teşhis durumu modeli.
/// </summary>
public record AiHealthStatus
{
    /// <summary>
    /// Yapılandırmada AI servisinin aktif olup olmadığı.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// AI sağlayıcı uç noktasına erişilip erişilemediği.
    /// </summary>
    public bool IsReachable { get; init; }

    /// <summary>
    /// Yapılandırılmış sağlayıcı adı.
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Yapılandırılmış model adı.
    /// </summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Sağlık kontrolü durum özeti veya hata detayı.
    /// </summary>
    public string? StatusMessage { get; init; }

    /// <summary>
    /// Sağlık yoklama gecikmesi (milisaniye).
    /// </summary>
    public long LatencyMs { get; init; }
}
