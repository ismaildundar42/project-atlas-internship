using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Yüksek başarımlı vektör matematik, serileştirme ve özetleme (hash) yardımcı sınıfı.
/// </summary>
public static class VectorUtils
{
    /// <summary>
    /// IEEE 754 float32 vektörünü doğrudan ikili (byte[]) formata dönüştürür (sıfır ek bellek tahsisi).
    /// </summary>
    public static byte[] ToBytes(float[] vector)
    {
        if (vector == null || vector.Length == 0)
            return Array.Empty<byte>();

        var bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// İkili (byte[]) formatındaki vektör verisini float32 dizisine dönüştürür.
    /// </summary>
    public static float[] ToFloats(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return Array.Empty<float>();

        var floats = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
        return floats;
    }

    /// <summary>
    /// İki float32 vektör arasındaki Kosinüs Benzerliğini (Cosine Similarity) hesaplar.
    /// Sonuç [-1.0, 1.0] aralığındadır. Normalleştirilmiş vektörlerde [0.0, 1.0] beklenir.
    /// </summary>
    public static float CosineSimilarity(ReadOnlySpan<float> x, ReadOnlySpan<float> y)
    {
        if (x.Length != y.Length || x.Length == 0)
            return 0f;

        float dot = 0f;
        float normX = 0f;
        float normY = 0f;

        for (int i = 0; i < x.Length; i++)
        {
            float xi = x[i];
            float yi = y[i];
            dot += xi * yi;
            normX += xi * xi;
            normY += yi * yi;
        }

        if (normX <= 0f || normY <= 0f)
            return 0f;

        return dot / (MathF.Sqrt(normX) * MathF.Sqrt(normY));
    }

    /// <summary>
    /// Metin içeriğinin deterministik SHA256 karma (hash) değerini üretir.
    /// </summary>
    public static string ComputeSha256(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
