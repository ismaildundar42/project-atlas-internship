namespace DeUygulamaVitrini.Application.Common.Models;

/// <summary>
/// Sayfalanmış liste sonuçlarını sarmalayan jenerik model.
/// API tarafında standart pagination kontratını sağlar.
/// </summary>
/// <typeparam name="T">Döndürülen veri öğesinin DTO tipi.</typeparam>
public class PagedResult<T>
{
    /// <summary>Mevcut sayfadaki veri öğeleri.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Aktif sayfa numarası (1-indexed).</summary>
    public int PageNumber { get; }

    /// <summary>Sayfa başına düşen öğe sayısı.</summary>
    public int PageSize { get; }

    /// <summary>Filtreye uyan toplam kayıt sayısı.</summary>
    public int TotalCount { get; }

    /// <summary>Toplam sayfa sayısı.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>Önceki sayfa var mı?</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Sonraki sayfa var mı?</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items ?? Array.Empty<T>();
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 12 : pageSize;
        TotalCount = totalCount < 0 ? 0 : totalCount;
    }
}
