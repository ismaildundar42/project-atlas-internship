using System.Diagnostics;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Sağlayıcıdan bağımsız (Provider-Agnostic) AI ısınma servisi uygulaması.
/// Somut AI veya Embedding sağlayıcısından (LM Studio, Ollama, Azure vb.) tamamen bağımsızdır;
/// sadece IAiProvider ve IEmbeddingProvider arayüzlerini tüketir.
/// Şirket veya proje verisi içermeyen minimal ve yan etkisiz (side-effect free) istekler göndererek
/// olası cold-start (soğuk başlangıç) gecikmelerini kullanıcıdan önce absorbe eder.
/// </summary>
public class AiWarmupService : IAiWarmupService
{
    private readonly IAiProvider _aiProvider;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IOptions<AiOptions> _options;
    private readonly ILogger<AiWarmupService> _logger;

    public AiWarmupService(
        IAiProvider aiProvider,
        IEmbeddingProvider embeddingProvider,
        IOptions<AiOptions> options,
        ILogger<AiWarmupService> logger)
    {
        _aiProvider = aiProvider;
        _embeddingProvider = embeddingProvider;
        _options = options;
        _logger = logger;
    }

    public async Task<AiWarmupResult> WarmupAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogDebug("AI warm-up atlandı: AI servisi genel yapılandırmada devre dışı.");
            return new AiWarmupResult
            {
                Skipped = true,
                SkipReason = "AI is disabled in configuration."
            };
        }

        if (options.Warmup != null && !options.Warmup.Enabled)
        {
            _logger.LogDebug("AI warm-up atlandı: Warmup seçeneği yapılandırmada devre dışı.");
            return new AiWarmupResult
            {
                Skipped = true,
                SkipReason = "Warmup is disabled in configuration."
            };
        }

        _logger.LogInformation("AI sağlayıcı warm-up işlemi başlatılıyor (Provider-Agnostic)...");
        var totalStopwatch = Stopwatch.StartNew();

        // 1. Generation Warm-up (minimum maliyetli, veri içermeyen neutral istem)
        bool genSuccess = false;
        long genDuration = 0;
        string? genError = null;

        var genStopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var maxTokens = options.Warmup?.MaxGenerationTokens ?? 8;
            var request = new AiGenerationRequest
            {
                UserPrompt = "Reply only with OK.",
                Temperature = 0.0,
                MaxTokens = maxTokens
            };

            var genResult = await _aiProvider.GenerateAsync(request, cancellationToken);
            genDuration = genStopwatch.ElapsedMilliseconds;

            if (genResult.Success)
            {
                genSuccess = true;
                _logger.LogInformation("AI metin üretimi (generation) warm-up tamamlandı ({DurationMs} ms).", genDuration);
            }
            else
            {
                genError = genResult.ErrorMessage ?? "Generation failed without message.";
                _logger.LogWarning("AI metin üretimi warm-up tamamlanamadı ({DurationMs} ms): {Error}", genDuration, genError);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("AI metin üretimi warm-up iptal edildi.");
            throw;
        }
        catch (Exception ex)
        {
            genDuration = genStopwatch.ElapsedMilliseconds;
            genError = ex.Message;
            _logger.LogWarning(ex, "AI metin üretimi warm-up sırasında istisna oluştu ({DurationMs} ms): {Error}", genDuration, genError);
        }

        // 2. Embedding Warm-up (şirket verisi içermeyen kısa neutral string, veritabanına YAZILMAZ)
        bool embSuccess = false;
        long embDuration = 0;
        string? embError = null;

        var embStopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var embResult = await _embeddingProvider.GenerateEmbeddingAsync("warmup", EmbeddingType.Document, cancellationToken);
            embDuration = embStopwatch.ElapsedMilliseconds;

            if (embResult.Success)
            {
                embSuccess = true;
                _logger.LogInformation("AI vektör (embedding) warm-up tamamlandı ({DurationMs} ms).", embDuration);
            }
            else
            {
                embError = embResult.Error ?? "Embedding generation failed without message.";
                _logger.LogWarning("AI vektör warm-up tamamlanamadı ({DurationMs} ms): {Error}", embDuration, embError);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("AI vektör warm-up iptal edildi.");
            throw;
        }
        catch (Exception ex)
        {
            embDuration = embStopwatch.ElapsedMilliseconds;
            embError = ex.Message;
            _logger.LogWarning(ex, "AI vektör warm-up sırasında istisna oluştu ({DurationMs} ms): {Error}", embDuration, embError);
        }

        totalStopwatch.Stop();
        var totalMs = totalStopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "AI warm-up süreci tamamlandı ({TotalDurationMs} ms). Metin Üretimi={GenSuccess} ({GenDurationMs} ms), Vektör={EmbSuccess} ({EmbDurationMs} ms)",
            totalMs, genSuccess, genDuration, embSuccess, embDuration);

        return new AiWarmupResult
        {
            GenerationSuccess = genSuccess,
            GenerationDurationMs = genDuration,
            GenerationError = genError,
            EmbeddingSuccess = embSuccess,
            EmbeddingDurationMs = embDuration,
            EmbeddingError = embError,
            TotalDurationMs = totalMs,
            Skipped = false
        };
    }
}
