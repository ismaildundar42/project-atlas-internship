namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Embedding üretiminde kullanılan amaç tipi.
/// Nomic gibi ön-ek (prefix) kullanan modeller için doküman veya sorgu ayrımını belirtir.
/// </summary>
public enum EmbeddingType
{
    /// <summary>İndekslenecek bilgi parçası (search_document:)</summary>
    Document = 1,

    /// <summary>Kullanıcı arama sorgusu (search_query:)</summary>
    Query = 2
}
