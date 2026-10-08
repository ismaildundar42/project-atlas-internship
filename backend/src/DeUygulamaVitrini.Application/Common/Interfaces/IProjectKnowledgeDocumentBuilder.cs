using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Domain.Entities;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Bir Proje varlığından deterministik, temizlenmiş ve anlamsal bilgi parçaları (chunks) üreten derleyici arayüzü.
/// Herhangi bir LLM çağrısı yapmaz; saf C# domain mantığı ile kararlı metin blokları ve SHA256 karma üretir.
/// </summary>
public interface IProjectKnowledgeDocumentBuilder
{
    /// <summary>
    /// Verilen proje nesnesinden yapılandırılmış bilgi parçaları üretir.
    /// </summary>
    IReadOnlyList<KnowledgeChunkDraft> BuildChunks(Project project);
}
