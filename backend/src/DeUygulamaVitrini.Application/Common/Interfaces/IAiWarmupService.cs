using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// AI sağlayıcı cold-start (soğuk başlangıç) önleme ve ısınma servisi soyutlaması.
/// Sağlayıcıdan bağımsız (Provider-Agnostic) olarak metin üretimi ve embedding
/// modellerine başlangıçta hafif, şirket verisi içermeyen test istekleri göndererek
/// model yükleme gecikmelerini kullanıcı etkileşiminden önce absorbe eder.
/// </summary>
public interface IAiWarmupService
{
    /// <summary>
    /// AI sağlayıcıları için hafif ve yan etkisiz (side-effect free) ısınma sürecini yürütür.
    /// </summary>
    Task<AiWarmupResult> WarmupAsync(CancellationToken cancellationToken = default);
}
