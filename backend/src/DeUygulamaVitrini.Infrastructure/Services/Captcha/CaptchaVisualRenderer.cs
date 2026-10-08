using System.Security.Cryptography;
using System.Text;

namespace DeUygulamaVitrini.Infrastructure.Services.Captcha;

/// <summary>
/// Platformdan bağımsız (Linux/Windows/Azure App Service/Container %100 uyumlu)
/// yüksek kaliteli, kurumsal temalı SVG CAPTCHA görsel oluşturucu.
/// Harici C/C++ ikili kütüphanelere veya Windows-only System.Drawing bileşenlerine ihtiyaç duymaz.
/// </summary>
public static class CaptchaVisualRenderer
{
    private static readonly string[] TextColors =
    [
        "#ffffff",
        "#f8fafc",
        "#f1f5f9",
        "#e2e8f0",
        "#cbd5e1",
        "#93c5fd",
        "#ff8a7a"
    ];

    private static readonly string[] InterferenceLineColors =
    [
        "rgba(59, 130, 246, 0.4)",
        "rgba(197, 22, 5, 0.35)",
        "rgba(148, 163, 184, 0.3)",
        "rgba(99, 102, 241, 0.35)"
    ];

    /// <summary>
    /// Belirtilen metin için SVG formatında CAPTCHA görseli üretir ve Data URL olarak döner.
    /// </summary>
    public static string RenderAsDataUrl(string code)
    {
        var svg = RenderSvg(code);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }

    /// <summary>
    /// Ham SVG dizgisi üretir (140x48 px, kurumsal koyu tema ile birebir uyumlu).
    /// </summary>
    public static string RenderSvg(string code)
    {
        const int width = 140;
        const int height = 48;
        var length = code.Length;

        var sb = new StringBuilder();
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\" width=\"{width}\" height=\"{height}\" style=\"background:#0b132b;border-radius:6px;overflow:hidden;user-select:none;\">");

        // 1. Arka plan Gradyanı
        sb.Append("<defs>");
        sb.Append("<linearGradient id=\"bg\" x1=\"0%\" y1=\"0%\" x2=\"100%\" y2=\"100%\">");
        sb.Append("<stop offset=\"0%\" stop-color=\"#08101d\" />");
        sb.Append("<stop offset=\"100%\" stop-color=\"#0d1b2e\" />");
        sb.Append("</linearGradient>");
        sb.Append("</defs>");

        sb.Append($"<rect width=\"{width}\" height=\"{height}\" fill=\"url(#bg)\" />");

        // İnce arka plan ızgara çizgileri
        for (int x = 18; x < width; x += 22)
        {
            sb.Append($"<line x1=\"{x}\" y1=\"0\" x2=\"{x}\" y2=\"{height}\" stroke=\"rgba(255,255,255,0.04)\" stroke-width=\"1\" />");
        }
        for (int y = 12; y < height; y += 16)
        {
            sb.Append($"<line x1=\"0\" y1=\"{y}\" x2=\"{width}\" y2=\"{y}\" stroke=\"rgba(255,255,255,0.04)\" stroke-width=\"1\" />");
        }

        // 2. Rastgele Arka Plan Gürültü Noktaları
        for (int i = 0; i < 18; i++)
        {
            var nx = RandomNumberGenerator.GetInt32(4, width - 4);
            var ny = RandomNumberGenerator.GetInt32(4, height - 4);
            var nr = (RandomNumberGenerator.GetInt32(6, 16) / 10.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            var opacity = (RandomNumberGenerator.GetInt32(20, 50) / 100.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            sb.Append($"<circle cx=\"{nx}\" cy=\"{ny}\" r=\"{nr}\" fill=\"rgba(148, 163, 184, {opacity})\" />");
        }

        // 3. Arka Plan Bezier Parazit Eğrileri
        for (int i = 0; i < 2; i++)
        {
            var startX = 0;
            var startY = RandomNumberGenerator.GetInt32(8, height - 8);
            var cp1X = RandomNumberGenerator.GetInt32(30, 60);
            var cp1Y = RandomNumberGenerator.GetInt32(4, height - 4);
            var cp2X = RandomNumberGenerator.GetInt32(80, 110);
            var cp2Y = RandomNumberGenerator.GetInt32(4, height - 4);
            var endX = width;
            var endY = RandomNumberGenerator.GetInt32(8, height - 8);
            var color = InterferenceLineColors[RandomNumberGenerator.GetInt32(0, InterferenceLineColors.Length)];
            var strokeWidth = (RandomNumberGenerator.GetInt32(10, 18) / 10.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

            sb.Append($"<path d=\"M {startX} {startY} C {cp1X} {cp1Y}, {cp2X} {cp2Y}, {endX} {endY}\" stroke=\"{color}\" stroke-width=\"{strokeWidth}\" fill=\"none\" />");
        }

        // 4. Karakterlerin Konumlandırılması, Döndürülmesi ve Çizimi (Kenarlardan güvenli mesafe)
        var leftMargin = 22.0;
        var availableWidth = width - (leftMargin * 2); // 96px
        var charSpacing = length > 1 ? availableWidth / (length - 1) : 0; // ~24px adım

        for (int i = 0; i < length; i++)
        {
            var ch = code[i];
            var posX = leftMargin + (i * charSpacing) + RandomNumberGenerator.GetInt32(-1, 2);
            var posY = 32 + RandomNumberGenerator.GetInt32(-2, 3); // baseline ~32px (dikeyde ortalanmış)
            var angle = RandomNumberGenerator.GetInt32(-8, 9); // -8 ile +8 derece arası dengeli açı
            var color = TextColors[RandomNumberGenerator.GetInt32(0, TextColors.Length)];
            var fontSize = RandomNumberGenerator.GetInt32(21, 24);

            sb.Append($"<g transform=\"rotate({angle} {posX.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} {posY})\">");
            
            // İnce gölge efekti
            sb.Append($"<text x=\"{(posX + 1.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}\" y=\"{posY + 1}\" font-family=\"'Consolas', 'Menlo', 'Monaco', monospace\" font-size=\"{fontSize}\" font-weight=\"800\" fill=\"rgba(0,0,0,0.65)\" text-anchor=\"middle\">{ch}</text>");
            
            // Ana karakter
            sb.Append($"<text x=\"{posX.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}\" y=\"{posY}\" font-family=\"'Consolas', 'Menlo', 'Monaco', monospace\" font-size=\"{fontSize}\" font-weight=\"800\" fill=\"{color}\" text-anchor=\"middle\">{ch}</text>");
            
            sb.Append("</g>");
        }

        // 5. Ön Parazit İnce Çizgisi
        {
            var fx1 = RandomNumberGenerator.GetInt32(6, 18);
            var fy1 = RandomNumberGenerator.GetInt32(12, height - 12);
            var fx2 = RandomNumberGenerator.GetInt32(width - 18, width - 6);
            var fy2 = RandomNumberGenerator.GetInt32(12, height - 12);
            sb.Append($"<line x1=\"{fx1}\" y1=\"{fy1}\" x2=\"{fx2}\" y2=\"{fy2}\" stroke=\"rgba(255, 255, 255, 0.15)\" stroke-width=\"1.0\" stroke-dasharray=\"3 3\" />");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}
