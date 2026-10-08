using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Yerel AI çalışma zamanı sağlayıcı uygulaması (LM Studio, Ollama vb. ile HTTP üzerinden iletişim).
/// Doğrudan HTTP REST protokolü kullanır ve üçüncü taraf SDK bağımlılığı içermez.
/// </summary>
public class LocalAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<LocalAiProvider> _logger;

    private static readonly Regex ThinkTagRegex = new(@"<think>[\s\S]*?</think>", RegexOptions.Compiled);

    public string ProviderName => "Local";

    public LocalAiProvider(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<LocalAiProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return AiGenerationResult.Failed(
                "AI servisi yapılandırmada devre dışı bırakılmıştır.",
                ProviderName,
                _options.Local.ChatModel);
        }

        if (string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            return AiGenerationResult.Failed(
                "İstem (prompt) metni boş olamaz.",
                ProviderName,
                _options.Local.ChatModel);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var isOllama = string.Equals(_options.Local.ApiFormat, "Ollama", StringComparison.OrdinalIgnoreCase);

            var effectiveBaseUrl = !string.IsNullOrWhiteSpace(_options.Local.ChatBaseUrl)
                ? _options.Local.ChatBaseUrl
                : _options.Local.BaseUrl;
            var baseUrl = (effectiveBaseUrl ?? "http://localhost:1234/v1").TrimEnd('/');
            var endpointUrl = isOllama
                ? $"{baseUrl}/api/chat"
                : $"{baseUrl}/chat/completions";

            var isQwen3 = _options.Local.ChatModel.Contains("qwen3", StringComparison.OrdinalIgnoreCase);

            var effectiveUserPrompt = isQwen3 && !request.UserPrompt.StartsWith("/no_think", StringComparison.OrdinalIgnoreCase)
                ? $"/no_think\n{request.UserPrompt}"
                : request.UserPrompt;

            object requestPayload;

            if (isOllama)
            {
                var messages = new List<object>();
                if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
                {
                    messages.Add(new { role = "system", content = request.SystemPrompt });
                }
                messages.Add(new { role = "user", content = effectiveUserPrompt });

                requestPayload = new
                {
                    model = _options.Local.ChatModel,
                    messages,
                    stream = false,
                    options = new
                    {
                        temperature = request.Temperature ?? 0.3,
                        num_predict = request.MaxTokens
                    }
                };
            }
            else
            {
                var messages = new List<object>();
                if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
                {
                    messages.Add(new { role = "system", content = request.SystemPrompt });
                }
                messages.Add(new { role = "user", content = effectiveUserPrompt });

                if (isQwen3)
                {
                    requestPayload = new
                    {
                        model = _options.Local.ChatModel,
                        messages,
                        temperature = request.Temperature ?? 0.3,
                        max_tokens = request.MaxTokens,
                        enable_thinking = false
                    };
                }
                else
                {
                    requestPayload = new
                    {
                        model = _options.Local.ChatModel,
                        messages,
                        temperature = request.Temperature ?? 0.3,
                        max_tokens = request.MaxTokens
                    };
                }
            }

            using var response = await _httpClient.PostAsJsonAsync(endpointUrl, requestPayload, cancellationToken);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning(
                    "Yerel AI sağlayıcısından başarısız HTTP yanıtı alındı: Status={StatusCode}, Provider={Provider}, Model={Model}",
                    statusCode,
                    ProviderName,
                    _options.Local.ChatModel);

                return AiGenerationResult.Failed(
                    $"Yerel AI çalışma zamanı hata döndürdü (HTTP {statusCode}).",
                    ProviderName,
                    _options.Local.ChatModel,
                    stopwatch.ElapsedMilliseconds);
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var details = ExtractGenerationDetails(responseJson, isOllama);
            var content = details.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                return AiGenerationResult.Failed(
                    "Yerel AI modelinden boş yanıt alındı.",
                    ProviderName,
                    _options.Local.ChatModel,
                    stopwatch.ElapsedMilliseconds);
            }

            // DeepSeek-R1 veya reasoning modelleri için <think>...</think> bloklarını temizle
            if (content.Contains("</think>", StringComparison.OrdinalIgnoreCase))
            {
                var thinkEndIndex = content.IndexOf("</think>", StringComparison.OrdinalIgnoreCase);
                content = content.Substring(thinkEndIndex + 8).Trim();
            }
            else
            {
                content = ThinkTagRegex.Replace(content, string.Empty).Trim();
            }

            _logger.LogInformation(
                "Yerel AI metin üretimi başarılı: Provider={Provider}, Model={Model}, DurationMs={DurationMs}, FinishReason={FinishReason}, CompTokens={CompTokens}",
                ProviderName,
                _options.Local.ChatModel,
                stopwatch.ElapsedMilliseconds,
                details.FinishReason,
                details.CompletionTokens);

            return AiGenerationResult.Succeeded(
                content,
                ProviderName,
                _options.Local.ChatModel,
                stopwatch.ElapsedMilliseconds,
                details.FinishReason,
                details.PromptTokens,
                details.CompletionTokens);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogInformation("AI metin üretimi isteği iptal edildi.");
            return AiGenerationResult.Failed("İstek iptal edildi.", ProviderName, _options.Local.ChatModel, stopwatch.ElapsedMilliseconds);
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("AI sağlayıcı isteği zaman aşımına uğradı ({Timeout}s).", _options.RequestTimeoutSeconds);
            return AiGenerationResult.Failed(
                $"Yerel AI sağlayıcısı zaman aşımına uğradı ({_options.RequestTimeoutSeconds}s).",
                ProviderName,
                _options.Local.ChatModel,
                stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "Yerel AI çalışma zamanına bağlanılamadı: BaseUrl={BaseUrl}, Model={Model}",
                _options.Local.BaseUrl,
                _options.Local.ChatModel);

            return AiGenerationResult.Failed(
                "Yerel AI çalışma zamanına bağlanılamadı. Servisin aktif olduğunu doğrulayın.",
                ProviderName,
                _options.Local.ChatModel,
                stopwatch.ElapsedMilliseconds);
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Yerel AI sağlayıcısının JSON yanıtı çözümlenemedi.");
            return AiGenerationResult.Failed(
                "Yerel AI sağlayıcısından geçersiz veri yapısı alındı.",
                ProviderName,
                _options.Local.ChatModel,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "AI metin üretimi sırasında beklenmeyen bir hata oluştu.");
            return AiGenerationResult.Failed(
                "AI işlemi sırasında beklenmeyen bir hata oluştu.",
                ProviderName,
                _options.Local.ChatModel,
                stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<AiHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new AiHealthStatus
            {
                Enabled = false,
                IsReachable = false,
                Provider = ProviderName,
                Model = _options.Local.ChatModel,
                StatusMessage = "AI servisi yapılandırmada devre dışı bırakılmıştır.",
                LatencyMs = 0
            };
        }

        var isOllama = string.Equals(_options.Local.ApiFormat, "Ollama", StringComparison.OrdinalIgnoreCase);
        var effectiveBaseUrl = !string.IsNullOrWhiteSpace(_options.Local.ChatBaseUrl)
            ? _options.Local.ChatBaseUrl
            : _options.Local.BaseUrl;
        var baseUrl = (effectiveBaseUrl ?? "http://localhost:1234/v1").TrimEnd('/');
        var checkEndpoint = isOllama ? $"{baseUrl}/api/tags" : $"{baseUrl}/models";

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var response = await _httpClient.GetAsync(checkEndpoint, linkedCts.Token);
            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new AiHealthStatus
                {
                    Enabled = true,
                    IsReachable = true,
                    Provider = ProviderName,
                    Model = _options.Local.ChatModel,
                    StatusMessage = "Yerel AI servisi aktif ve erişilebilir durumda.",
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }

            return new AiHealthStatus
            {
                Enabled = true,
                IsReachable = false,
                Provider = ProviderName,
                Model = _options.Local.ChatModel,
                StatusMessage = $"Yerel AI servisi HTTP {(int)response.StatusCode} döndürdü.",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AiHealthStatus
            {
                Enabled = true,
                IsReachable = false,
                Provider = ProviderName,
                Model = _options.Local.ChatModel,
                StatusMessage = $"Yerel AI servisine ulaşılamıyor ({ex.GetType().Name}).",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private record GenerationDetails(string? Content, string FinishReason, int PromptTokens, int CompletionTokens);

    private static GenerationDetails ExtractGenerationDetails(string jsonString, bool isOllama)
    {
        using var document = JsonDocument.Parse(jsonString);
        var root = document.RootElement;
        string? content = null;
        string finishReason = "stop";
        int promptTokens = 0;
        int completionTokens = 0;

        if (root.TryGetProperty("usage", out var usageElement))
        {
            if (usageElement.TryGetProperty("prompt_tokens", out var pt))
                promptTokens = pt.GetInt32();
            if (usageElement.TryGetProperty("completion_tokens", out var ct))
                completionTokens = ct.GetInt32();
        }

        if (isOllama)
        {
            if (root.TryGetProperty("done_reason", out var dr))
                finishReason = dr.GetString() ?? "stop";
            else if (root.TryGetProperty("done", out var done) && done.GetBoolean())
                finishReason = "stop";

            if (root.TryGetProperty("message", out var messageElement) &&
                messageElement.TryGetProperty("content", out var messageContent))
            {
                content = messageContent.GetString();
            }
            else if (root.TryGetProperty("response", out var legacyResponse))
            {
                content = legacyResponse.GetString();
            }
        }
        else
        {
            if (root.TryGetProperty("choices", out var choicesElement) &&
                choicesElement.ValueKind == JsonValueKind.Array &&
                choicesElement.GetArrayLength() > 0)
            {
                var firstChoice = choicesElement[0];
                if (firstChoice.TryGetProperty("finish_reason", out var fr))
                {
                    finishReason = fr.GetString() ?? "stop";
                }

                if (firstChoice.TryGetProperty("message", out var messageElement) &&
                    messageElement.TryGetProperty("content", out var contentElement))
                {
                    content = contentElement.GetString();
                }
                else if (firstChoice.TryGetProperty("text", out var textElement))
                {
                    content = textElement.GetString();
                }
            }
        }

        return new GenerationDetails(content, finishReason, promptTokens, completionTokens);
    }
}
