using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.DTOs.Ai;
using DeUygulamaVitrini.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Yerel embedding modelleri için (LM Studio / Ollama / OpenAI-uyumlu) HTTP sağlayıcı implementasyonu.
/// </summary>
public class LocalEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<LocalEmbeddingProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public LocalEmbeddingProvider(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<LocalEmbeddingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    private string GetEndpointUrl(string relativePath)
    {
        var effectiveBaseUrl = !string.IsNullOrWhiteSpace(_options.Local.EmbeddingBaseUrl)
            ? _options.Local.EmbeddingBaseUrl
            : _options.Local.BaseUrl;
        var baseUrl = effectiveBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{relativePath.TrimStart('/')}";
    }

    private string FormatInputWithPrefix(string text, EmbeddingType type)
    {
        var model = _options.Local.EmbeddingModel.ToLowerInvariant();
        var isNomic = model.Contains("nomic");

        if (isNomic)
        {
            var prefix = type == EmbeddingType.Document ? "search_document:" : "search_query:";
            return $"{prefix} {text.Trim()}";
        }

        return text.Trim();
    }

    public async Task<EmbeddingResult> GenerateEmbeddingAsync(
        string text,
        EmbeddingType type = EmbeddingType.Document,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return EmbeddingResult.Failed("AI provider is disabled in configuration.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return EmbeddingResult.Failed("Input text cannot be empty.");
        }

        var results = await GenerateEmbeddingsAsync(new[] { text }, type, cancellationToken);
        if (results.Count > 0 && results[0].Success)
        {
            return results[0];
        }

        return results.Count > 0
            ? results[0]
            : EmbeddingResult.Failed("No embedding returned from provider.", _options.Local.EmbeddingModel);
    }

    public async Task<IReadOnlyList<EmbeddingResult>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        EmbeddingType type = EmbeddingType.Document,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return texts.Select(_ => EmbeddingResult.Failed("AI provider is disabled in configuration.")).ToList();
        }

        if (texts == null || texts.Count == 0)
        {
            return Array.Empty<EmbeddingResult>();
        }

        var modelName = _options.Local.EmbeddingModel;
        var formattedInputs = texts.Select(t => FormatInputWithPrefix(t, type)).ToList();

        try
        {
            if (_options.Local.ApiFormat.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                return await GenerateOllamaEmbeddingsAsync(formattedInputs, modelName, cancellationToken);
            }

            return await GenerateOpenAiEmbeddingsAsync(formattedInputs, modelName, cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            _logger.LogWarning("Embedding generation was cancelled by caller.");
            return texts.Select(_ => EmbeddingResult.Failed("Request was cancelled.", modelName)).ToList();
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Embedding generation timed out after {Timeout} seconds.", _options.RequestTimeoutSeconds);
            return texts.Select(_ => EmbeddingResult.Failed("Embedding request timed out.", modelName)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during embedding generation.");
            return texts.Select(_ => EmbeddingResult.Failed($"Embedding error: {ex.Message}", modelName)).ToList();
        }
    }

    private async Task<IReadOnlyList<EmbeddingResult>> GenerateOpenAiEmbeddingsAsync(
        List<string> formattedInputs,
        string modelName,
        CancellationToken cancellationToken)
    {
        var url = GetEndpointUrl("embeddings");
        var results = new List<EmbeddingResult>();

        foreach (var input in formattedInputs)
        {
            var singleRequest = new OpenAiEmbeddingRequest
            {
                Model = modelName,
                Input = input
            };

            var response = await _httpClient.PostAsJsonAsync(url, singleRequest, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Local embedding endpoint returned HTTP {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                results.Add(EmbeddingResult.Failed($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", modelName));
                continue;
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>(JsonOptions, cancellationToken);
            if (result?.Data == null || result.Data.Count == 0 || result.Data[0].Embedding == null || result.Data[0].Embedding.Length == 0)
            {
                results.Add(EmbeddingResult.Failed("Empty embedding response received.", modelName));
                continue;
            }

            var vector = result.Data[0].Embedding;
            if (_options.Local.EmbeddingDimension > 0 && vector.Length != _options.Local.EmbeddingDimension)
            {
                _logger.LogError("Embedding dimension mismatch: configured {ConfiguredDim}, but provider returned {ReturnedDim} for model {Model}.",
                    _options.Local.EmbeddingDimension, vector.Length, modelName);
                results.Add(EmbeddingResult.Failed($"Embedding dimension mismatch: expected {_options.Local.EmbeddingDimension}, got {vector.Length}", modelName));
                continue;
            }

            results.Add(EmbeddingResult.Succeeded(vector, modelName));
        }

        return results;
    }

    private async Task<IReadOnlyList<EmbeddingResult>> GenerateOllamaEmbeddingsAsync(
        List<string> formattedInputs,
        string modelName,
        CancellationToken cancellationToken)
    {
        var results = new List<EmbeddingResult>();
        var url = GetEndpointUrl("api/embeddings");

        foreach (var input in formattedInputs)
        {
            var requestBody = new OllamaEmbeddingRequest
            {
                Model = modelName,
                Prompt = input
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                results.Add(EmbeddingResult.Failed($"Ollama HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", modelName));
                continue;
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(JsonOptions, cancellationToken);
            if (result?.Embedding != null && result.Embedding.Length > 0)
            {
                var vector = result.Embedding;
                if (_options.Local.EmbeddingDimension > 0 && vector.Length != _options.Local.EmbeddingDimension)
                {
                    _logger.LogError("Ollama embedding dimension mismatch: configured {ConfiguredDim}, but returned {ReturnedDim} for model {Model}.",
                        _options.Local.EmbeddingDimension, vector.Length, modelName);
                    results.Add(EmbeddingResult.Failed($"Embedding dimension mismatch: expected {_options.Local.EmbeddingDimension}, got {vector.Length}", modelName));
                }
                else
                {
                    results.Add(EmbeddingResult.Succeeded(vector, modelName));
                }
            }
            else
            {
                results.Add(EmbeddingResult.Failed("Empty vector returned from Ollama.", modelName));
            }
        }

        return results;
    }

    public async Task<AiHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new AiHealthStatus
            {
                Enabled = false,
                IsReachable = false,
                Provider = _options.Provider,
                Model = _options.Local.EmbeddingModel,
                StatusMessage = "Embedding provider is disabled in configuration."
            };
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var relative = _options.Local.ApiFormat.Equals("Ollama", StringComparison.OrdinalIgnoreCase) ? "api/tags" : "models";
            var url = GetEndpointUrl(relative);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new AiHealthStatus
                {
                    Enabled = true,
                    IsReachable = true,
                    Provider = _options.Provider,
                    Model = _options.Local.EmbeddingModel,
                    LatencyMs = sw.ElapsedMilliseconds,
                    StatusMessage = "Local embedding provider is online and reachable."
                };
            }

            return new AiHealthStatus
            {
                Enabled = true,
                IsReachable = false,
                Provider = _options.Provider,
                Model = _options.Local.EmbeddingModel,
                LatencyMs = sw.ElapsedMilliseconds,
                StatusMessage = $"Endpoint returned HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Health check failed for local embedding provider at {BaseUrl}", _options.Local.BaseUrl);
            return new AiHealthStatus
            {
                Enabled = true,
                IsReachable = false,
                Provider = _options.Provider,
                Model = _options.Local.EmbeddingModel,
                LatencyMs = sw.ElapsedMilliseconds,
                StatusMessage = $"Connection failed: {ex.Message}"
            };
        }
    }

    // ─── JSON İstek / Yanıt Modelleri (OpenAI & Ollama) ──────────────────────

    private class OpenAiEmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public object Input { get; set; } = string.Empty;
    }

    private class OpenAiEmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<OpenAiEmbeddingData> Data { get; set; } = new();

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    }

    private class OpenAiEmbeddingData
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    private class OllamaEmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;
    }

    private class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
