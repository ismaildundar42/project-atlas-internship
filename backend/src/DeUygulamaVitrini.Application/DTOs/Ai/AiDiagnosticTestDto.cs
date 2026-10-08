namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Geliştirme ve yönetici teşhis paneli için test isteği.
/// </summary>
public record AiDiagnosticTestRequest(
    string Prompt,
    string? SystemPrompt = null,
    double? Temperature = 0.3
);

/// <summary>
/// Geliştirme ve yönetici teşhis paneli için test yanıtı.
/// </summary>
public record AiDiagnosticTestResponse(
    bool Success,
    string Provider,
    string Model,
    string? Response,
    long DurationMs,
    string? ErrorMessage = null
);
