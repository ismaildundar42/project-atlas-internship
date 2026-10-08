using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Yetkilendirme duyarlı tekil proje yapay zeka özetleme servisi arayüzü.
/// Belirli bir proje için doğrudan SQL verisi üzerinden topraklanmış (grounded) özet üretir.
/// </summary>
public interface IProjectAiSummaryService
{
    /// <summary>
    /// Kimlik numarası verilen projenin yetki kontrolünü yaparak doğrulanmış SQL verisi üzerinden AI özeti üretir.
    /// </summary>
    /// <param name="projectId">Özetlenecek projenin ID'si.</param>
    /// <param name="language">İstenen özet dili (opsiyonel, "tr" veya "en").</param>
    /// <param name="currentUserId">İstekte bulunan kullanıcının ID'si (anonim ise 0).</param>
    /// <param name="isAdmin">Kullanıcının Admin / SuperAdmin yetkisine sahip olup olmadığı.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Yapılandırılmış AI özet yanıt DTO'su.</returns>
    Task<ProjectAiSummaryResponseDto> GenerateSummaryAsync(
        int projectId,
        string? language,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
