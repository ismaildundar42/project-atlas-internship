namespace DeUygulamaVitrini.Application.DTOs.Ai;

/// <summary>
/// Embedding üretim isteğinin sonucunu temsil eden DTO.
/// </summary>
public class EmbeddingResult
{
    public bool Success { get; set; }
    public float[] Vector { get; set; } = Array.Empty<float>();
    public int Dimension { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? Error { get; set; }

    public static EmbeddingResult Succeeded(float[] vector, string model) => new()
    {
        Success = true,
        Vector = vector,
        Dimension = vector.Length,
        Model = model
    };

    public static EmbeddingResult Failed(string error, string model = "") => new()
    {
        Success = false,
        Error = error,
        Model = model
    };
}
