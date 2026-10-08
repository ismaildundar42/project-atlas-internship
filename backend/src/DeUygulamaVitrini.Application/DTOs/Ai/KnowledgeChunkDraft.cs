namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Bilgi dokümanı derleyicisi tarafından üretilen normalize edilmiş ham parça taslağı.
/// </summary>
public class KnowledgeChunkDraft
{
    public required string ChunkKey { get; set; }
    public required string Content { get; set; }
    public required string ContentHash { get; set; }
}
