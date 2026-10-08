using DeUygulamaVitrini.Domain.Common;

namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Proje Bilgi İndeksi (Project Knowledge Index) için vektör temsilini ve parça (chunk) verisini saklayan varlık.
/// Proje verisi tek doğruluk kaynağıdır (Single Source of Truth); bu varlık türetilmiş (derived) veridir.
/// </summary>
public class ProjectKnowledgeChunk : BaseEntity
{
    /// <summary>İlişkili Proje Kimliği (Foreign Key → Project).</summary>
    public int ProjectId { get; set; }

    /// <summary>Proje referansı.</summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Parça tipi ve anahtarı (Örn: "OVERVIEW", "TECHNICAL", "ORGANIZATION_USAGE").
    /// </summary>
    public required string ChunkKey { get; set; }

    /// <summary>
    /// Normalize edilmiş, anlamsal olarak yapılandırılmış metin içeriği.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// İçeriğin SHA256 karması (hash). İçerik değişmediğinde gereksiz embedding çağrılarını önler.
    /// </summary>
    public required string ContentHash { get; set; }

    /// <summary>
    /// Vektör üretiminde kullanılan embedding model adı (Örn: "text-embedding-nomic-embed-text-v1.5").
    /// </summary>
    public required string EmbeddingModel { get; set; }

    /// <summary>
    /// Vektör boyutu (Örn: 768).
    /// </summary>
    public int EmbeddingDimension { get; set; }

    /// <summary>
    /// IEEE 754 float32 vektör verisinin bayt dizisi temsili (EmbeddingDimension * 4 bayt).
    /// Yüksek performanslı bellek ve SIMD işlemleri için ikili (binary) formatta saklanır.
    /// </summary>
    public byte[] EmbeddingVector { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// İndekslenme UTC zaman damgası.
    /// </summary>
    public DateTime IndexedAtUtc { get; set; } = DateTime.UtcNow;
}
