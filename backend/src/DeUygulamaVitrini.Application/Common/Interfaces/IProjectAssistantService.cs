using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Yetkilendirme duyarlı (authorization-aware) RAG Proje Kütüphanesi Asistanı servis arayüzü.
/// Kullanıcının doğal dil sorularını alır, yetkili proje bilgisini getirir, bağlam oluşturur ve LLM ile yanıt üretir.
/// </summary>
public interface IProjectAssistantService
{
    /// <summary>
    /// Kullanıcının sorduğu soruyu RAG boru hattı üzerinden yanıtlar.
    /// </summary>
    Task<ProjectAssistantResponseDto> AskAsync(
        ProjectAssistantRequestDto request,
        int currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
