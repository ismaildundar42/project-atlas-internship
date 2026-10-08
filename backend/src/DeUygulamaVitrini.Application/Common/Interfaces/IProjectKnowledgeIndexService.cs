using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Proje Bilgi İndeksi yaşam döngüsü yönetim servisi.
/// İndeksleme, güncelleme, silme ve yeniden oluşturma işlemlerini yönetir.
/// </summary>
public interface IProjectKnowledgeIndexService
{
    /// <summary>
    /// Uygun olan tüm projeleri baştan tarayarak bilgi indeksini sıfırdan veya artımsal olarak yeniden oluşturur.
    /// </summary>
    Task<IndexRebuildResultDto> RebuildIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir projenin bilgi parçalarını günceller veya indeksler.
    /// </summary>
    Task<IndexProjectResultDto> IndexProjectAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir projenin tüm indeks parçalarını siler (proje silindiğinde veya yayından kaldırıldığında).
    /// </summary>
    Task RemoveProjectIndexAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Arka plan veya istek üzerine, eksik (missing) veya güncelliğini yitirmiş (stale) projeleri
    /// sınırlandırılmış bir grup (batch) halinde tespit edip embedding indeksini günceller.
    /// </summary>
    Task<ReconciliationResultDto> ReconcileBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// İndeks durumunu, toplam parça ve proje istatistiklerini getirir.
    /// </summary>
    Task<IndexStatusDto> GetIndexStatusAsync(CancellationToken cancellationToken = default);
}
