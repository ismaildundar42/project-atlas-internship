using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Yetkilendirme duyarlı (authorization-aware) anlamsal proje arama servisi.
/// Sadece vektör benzerliği hesaplar; yanıt üretimi için Generative LLM çağırmaz.
/// </summary>
public interface IProjectSemanticSearchService
{
    /// <summary>
    /// Kullanıcı bağlamına ve yetkilerine göre en alakalı projeleri anlamsal benzerlik sırasıyla getirir.
    /// </summary>
    Task<IReadOnlyList<SemanticSearchResultDto>> SearchAsync(
        SemanticSearchQueryDto queryDto,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
