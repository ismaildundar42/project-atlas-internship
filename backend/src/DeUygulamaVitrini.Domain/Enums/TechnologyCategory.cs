namespace DeUygulamaVitrini.Domain.Enums;

/// <summary>
/// Teknoloji kategorisini belirtir.
/// Ayrı tablo yerine enum: Bu değerler uygulamanın filtreleme ve
/// gruplama mantığı için gerekli olduğundan kapalı küme olarak tutulur.
/// </summary>
public enum TechnologyCategory
{
    Frontend = 1,
    Backend = 2,
    Database = 3,
    Analytics = 4,
    Cloud = 5,
    DevOps = 6,
    Mobile = 7,
    IoT = 8,
    AI = 9,
    Other = 99
}
