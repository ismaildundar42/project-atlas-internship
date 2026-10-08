using DeUygulamaVitrini.Application.DTOs.Search;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Kurumsal proje arama ve keşif servisi arayüzü.
/// Gelecekteki semantik / AI arama entegrasyonlarına uygun temiz servis soyutlaması sunar.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Proje bilgi alanları (ad, açıklama, detaylar, teknolojiler, etiketler, lokasyonlar, ekipler vb.)
    /// üzerinde deterministik veritabanı araması yürütür. Yalnızca yayınlanmış (IsPublished = true)
    /// ve silinmemiş projeleri döndürür.
    /// </summary>
    /// <param name="query">Arama terimi / anahtar kelime</param>
    /// <param name="limit">Döndürülecek azami sonuç sayısı (varsayılan: 8)</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    Task<SearchResultDto> SearchProjectsAsync(string? query, int limit = 8, CancellationToken cancellationToken = default);
}
