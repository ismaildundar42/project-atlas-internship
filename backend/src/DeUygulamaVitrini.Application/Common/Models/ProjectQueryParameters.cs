namespace DeUygulamaVitrini.Application.Common.Models;

/// <summary>
/// Proje listeleme endpoint'i için tip korumalı sorgu parametreleri modeli.
/// Arama, filtreleme, sıralama ve sayfalama parametrelerini taşır.
/// </summary>
public class ProjectQueryParameters
{
    private int _pageNumber = 1;
    private int _pageSize = 12;
    private string? _search;
    private string? _sortBy = "updatedAt";
    private string? _sortDirection = "desc";

    /// <summary>Arama terimi (Ad, Kısa Açıklama ve Genel Açıklamada aranır).</summary>
    public string? Search
    {
        get => _search;
        set => _search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>Proje durum kodu (Örn: "ACTIVE", "COMPLETED").</summary>
    public string? Status { get; set; }

    /// <summary>Proje kategori kodu (Örn: "SOFTWARE", "DATA_ANALYTICS").</summary>
    public string? Category { get; set; }

    /// <summary>Geliştirme tipi (Internal, External, Hybrid).</summary>
    public string? DevelopmentType { get; set; }

    /// <summary>İlişkili ekip kimliği.</summary>
    public int? TeamId { get; set; }

    /// <summary>İlişkili departman kimliği (Team -> Department ilişkisi üzerinden filtrelenir).</summary>
    public int? DepartmentId { get; set; }

    /// <summary>İlişkili departman adı veya kodu.</summary>
    public string? Department { get; set; }

    /// <summary>İlişkili lokasyon kimliği.</summary>
    public int? LocationId { get; set; }

    /// <summary>İlişkili teknoloji kimliği.</summary>
    public int? TechnologyId { get; set; }

    /// <summary>İlişkili etiket kimliği.</summary>
    public int? TagId { get; set; }

    /// <summary>Öne çıkan projeler filtresi.</summary>
    public bool? IsFeatured { get; set; }

    /// <summary>Sayfa numarası (varsayılan: 1).</summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Sayfa boyutu (varsayılan: 12, maksimum: 100).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 12 : (value > 100 ? 100 : value);
    }

    /// <summary>Sıralama yapılacak alan (name, createdAt, updatedAt, startDate).</summary>
    public string? SortBy
    {
        get => _sortBy;
        set => _sortBy = string.IsNullOrWhiteSpace(value) ? "updatedAt" : value.Trim();
    }

    /// <summary>Sıralama yönü (asc, desc).</summary>
    public string? SortDirection
    {
        get => _sortDirection;
        set => _sortDirection = string.IsNullOrWhiteSpace(value) ? "desc" : value.Trim().ToLowerInvariant();
    }
}
