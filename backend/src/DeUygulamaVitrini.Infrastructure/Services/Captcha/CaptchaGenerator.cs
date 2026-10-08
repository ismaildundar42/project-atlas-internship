using System.Security.Cryptography;
using System.Text;

namespace DeUygulamaVitrini.Infrastructure.Services.Captcha;

/// <summary>
/// Kriptografik olarak güvenli rastgele kod ve özet (hash) üretim yardımcı sınıfı.
/// </summary>
public static class CaptchaGenerator
{
    /// <summary>
    /// Karışıklığa yol açmayan (0/O/o, 1/I/l, 8/B gibi benzer karakterler ayıklanmış) güvenli alfabe.
    /// </summary>
    public const string SafeAlphabet = "2345679ACDEFGHJKLMNPQRSTUVWXYZ";

    private const string SaltKey = "DeUygulamaVitrini_Captcha_Internal_Salt_2026";

    /// <summary>
    /// Kriptografik rastgelelikle belirtilen uzunlukta alfanümerik metin üretir.
    /// </summary>
    public static string GenerateCode(int length = 5)
    {
        if (length < 3) length = 3;
        if (length > 10) length = 10;

        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(0, SafeAlphabet.Length);
            result[i] = SafeAlphabet[idx];
        }

        return new string(result);
    }

    /// <summary>
    /// Benzersiz, şeffaf olmayan meydan okuma anahtarı üretir.
    /// </summary>
    public static string GenerateChallengeId()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Cevap metnini normalize eder ve SHA256 ile özetler.
    /// </summary>
    public static string ComputeHash(string answer, string challengeId, bool caseSensitive = false)
    {
        var normalized = answer.Trim();
        if (!caseSensitive)
        {
            normalized = normalized.ToUpperInvariant();
        }

        var payload = $"{normalized}:{challengeId.Trim()}:{SaltKey}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Sabit zamanlı (constant-time) güvenli özet karşılaştırması yapar (timing attack koruması).
    /// </summary>
    public static bool VerifyHash(string submittedAnswer, string challengeId, string storedHash, bool caseSensitive = false)
    {
        if (string.IsNullOrWhiteSpace(submittedAnswer) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        var computedHash = ComputeHash(submittedAnswer, challengeId, caseSensitive);
        var computedBytes = Encoding.UTF8.GetBytes(computedHash);
        var storedBytes = Encoding.UTF8.GetBytes(storedHash);

        return CryptographicOperations.FixedTimeEquals(computedBytes, storedBytes);
    }
}
