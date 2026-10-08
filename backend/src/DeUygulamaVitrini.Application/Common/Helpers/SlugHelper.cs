using System.Text;
using System.Text.RegularExpressions;

namespace DeUygulamaVitrini.Application.Common.Helpers;

public static class SlugHelper
{
    private static readonly Dictionary<char, char> TurkishCharMap = new()
    {
        { 'ç', 'c' }, { 'Ç', 'c' },
        { 'ğ', 'g' }, { 'Ğ', 'g' },
        { 'ı', 'i' }, { 'I', 'i' }, { 'İ', 'i' },
        { 'ö', 'o' }, { 'Ö', 'o' },
        { 'ş', 's' }, { 'Ş', 's' },
        { 'ü', 'u' }, { 'Ü', 'u' }
    };

    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var sb = new StringBuilder();
        foreach (var c in text)
        {
            if (TurkishCharMap.TryGetValue(c, out var mapped))
                sb.Append(mapped);
            else
                sb.Append(c);
        }

        var lower = sb.ToString().ToLowerInvariant();
        var cleaned = Regex.Replace(lower, @"[^a-z0-9\s-]", "");
        cleaned = Regex.Replace(cleaned.Trim(), @"\s+", "-");
        cleaned = Regex.Replace(cleaned, @"-+", "-");
        return cleaned;
    }
}
