namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Proje Kütüphanesi AI Asistanı tekil oturum mesaj modeli.
/// </summary>
public record ProjectAssistantMessageDto
{
    /// <summary>
    /// Mesajı gönderen rol: "user" veya "assistant".
    /// </summary>
    public string Role { get; init; } = "user";

    /// <summary>
    /// Mesaj metni.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Bu mesaja ait alıntılanan/dönen proje ID'leri (oturum içi takip ve filtreleme için).
    /// </summary>
    public IReadOnlyList<int>? ReferencedProjectIds { get; init; }
}

/// <summary>
/// Proje Kütüphanesi AI Asistanı soru sorma istek modeli.
/// </summary>
public record ProjectAssistantRequestDto
{
    /// <summary>
    /// Kullanıcının doğal dilde sorduğu soru.
    /// </summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>
    /// İsteğe bağlı tercih edilen yanıt dili (örn. "tr", "en").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// İsteğe bağlı oturum geçmişi (son 2-4 mesaj).
    /// Sunucu yetkilendirme kararlarını yalnızca doğrulanmış SQL verisiyle verir.
    /// </summary>
    public IReadOnlyList<ProjectAssistantMessageDto>? History { get; init; }
}

/// <summary>
/// AI Asistan yanıtına iliştirilen kaynak proje alıntısı (Citation).
/// Sunucu tarafında doğrulanmış ve kullanıcının görmeye yetkili olduğu gerçek projeleri temsil eder.
/// </summary>
public record ProjectAssistantCitationDto
{
    public int ProjectId { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string? CoverImageUrl { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public IReadOnlyList<string> Locations { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Technologies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MatchedChunkKeys { get; init; } = Array.Empty<string>();
}

/// <summary>
/// AI Asistan yanıtı gözlemlenebilirlik ve tanılama üst verisi.
/// </summary>
public record ProjectAssistantMetadataDto
{
    public int RetrievedChunksCount { get; init; }
    public int CitationCount { get; init; }
    public long RetrievalDurationMs { get; init; }
    public long GenerationDurationMs { get; init; }
    public long QueryDurationMs { get; init; }
    public long TotalDurationMs { get; init; }
    public bool GroundedFromContext { get; init; }
    public string ResponseLanguage { get; init; } = "tr";
    public string FinishReason { get; init; } = "stop";
    public bool IsComplete { get; init; } = true;
    public string Intent { get; init; } = "KnowledgeQuery";
    public string ExecutionPath { get; init; } = "SemanticRag";
    public string? FallbackReason { get; init; }
    public string? StructuredResolution { get; init; }
}

/// <summary>
/// Proje Kütüphanesi AI Asistanı yanıt modeli.
/// </summary>
public record ProjectAssistantResponseDto
{
    public string Answer { get; init; } = string.Empty;
    public IReadOnlyList<ProjectAssistantCitationDto> Citations { get; init; } = Array.Empty<ProjectAssistantCitationDto>();
    public ProjectAssistantMetadataDto Metadata { get; init; } = new();
}
