using System.Text.RegularExpressions;
using DeUygulamaVitrini.Application.DTOs.Ai;

namespace DeUygulamaVitrini.Infrastructure.Services.Ai;

/// <summary>
/// Doğal dil soru metinlerini deterministik ve güvenli StructuredProjectQuery modellerine ayrıştırır.
/// Hiçbir SQL kodu üretmez; yalnızca sunucu tarafından izin verilen filtre ve sıralama alanlarını tespit eder.
/// </summary>
public static class ProjectQueryInterpreter
{
    private static readonly string[] Locations =
    {
        "kangal", "divriği", "divrigi", "sivas", "ankara", "liman", "iliç", "ilic", "hakkari", "amasya"
    };

    private static readonly (string Canonical, string[] Synonyms)[] Technologies =
    {
        ("Python", new[] { "python", "py" }),
        (".NET", new[] { ".net", "dotnet", ".net core", "c#", "csharp", "asp.net" }),
        ("React", new[] { "react", "reactjs", "react.js" }),
        ("SAP", new[] { "sap", "sap pm", "sap erp", "sap mm", "sap fi" }),
        ("SCADA", new[] { "scada", "plc" }),
        ("IoT", new[] { "iot", "nesnelerin interneti", "telemetri", "mqtt" }),
        ("SQL Server", new[] { "sql server", "mssql" }),
        ("TimescaleDB", new[] { "timescaledb", "timescale" }),
        ("Docker", new[] { "docker", "container" }),
        ("Kubernetes", new[] { "kubernetes", "k8s" }),
        ("TensorFlow", new[] { "tensorflow", "pytorch", "keras" }),
        ("Angular", new[] { "angular", "angularjs" }),
        ("Vue", new[] { "vue", "vuejs" }),
        ("Node.js", new[] { "node.js", "nodejs", "node" }),
        ("RFID", new[] { "rfid" }),
        ("Spektrometre", new[] { "spektrometre", "spektrometri" }),
        ("TPMS", new[] { "tpms" })
    };

    private static readonly (string Canonical, string[] Synonyms)[] Categories =
    {
        ("Yapay Zeka", new[] { "yapay zeka", "yapay zekâ", "ai", "artificial intelligence", "makine öğrenmesi", "machine learning", "derin öğrenme" }),
        ("IoT & Telemetri", new[] { "iot", "nesnelerin interneti", "telemetri" }),
        ("Yazılım", new[] { "yazılım geliştirme", "yazılım projeleri", "software" }),
        ("Madencilik Teknolojileri", new[] { "madencilik teknolojileri", "madencilik projeleri", "mining" }),
        ("Ar-Ge & İnovasyon", new[] { "ar-ge", "arge", "inovasyon", "r&d" }),
        ("İş Sağlığı & Güvenliği", new[] { "isg", "iş sağlığı", "iş güvenliği", "safety", "hse" }),
        ("Enerji & Sürdürülebilirlik", new[] { "enerji verimliliği", "sürdürülebilirlik", "kompanzasyon" }),
        ("Otomasyon", new[] { "otomasyon projeleri", "scada otomasyon" }),
        ("Lojistik & Sevkiyat", new[] { "lojistik", "sevkiyat", "kantar" })
    };

    private static readonly (string Canonical, string[] Synonyms)[] Statuses =
    {
        ("Canlıda", new[] { "canlıda", "canlı", "aktif", "production", "live" }),
        ("Planlama", new[] { "planlama", "planlanan", "planning" }),
        ("Geliştirme", new[] { "geliştirme", "geliştirilen", "development" }),
        ("Test", new[] { "test aşamasında", "pilot" }),
        ("Tamamlandı", new[] { "tamamlandı", "tamamlanan", "tamamlanmış", "completed", "biten" }),
        ("Taslak", new[] { "taslak", "taslaklar", "draft" })
    };

    private static readonly (string Canonical, string[] Synonyms)[] Teams =
    {
        ("Yazılım Geliştirme", new[] { "yazılım geliştirme ekibi", "yazılım ekibi", "software team" }),
        ("Dijital Dönüşüm", new[] { "dijital dönüşüm ekibi", "digital transformation team" }),
        ("Kestirimci Bakım", new[] { "kestirimci bakım ekibi", "bakım ekibi", "predictive maintenance team" }),
        ("Otomasyon & SCADA", new[] { "otomasyon ekibi", "scada ekibi" }),
        ("Saha Operasyonları", new[] { "saha ekibi", "maden ekibi" }),
        ("Veri Analitiği", new[] { "veri analitiği ekibi", "veri ekibi", "data team" })
    };

    private static readonly string[] SemanticTopics =
    {
        "kestirimci bakım", "arıza tahmin", "erken uyarı", "titreşim", "yağ analizi",
        "üretim verimliliği", "verimlilik artışı", "enerji tasarrufu", "makine sağlığı",
        "duruş süresi", "plansız duruş", "optimizasyon", "görüntü işleme"
    };

    /// <summary>
    /// Verilen kullanıcı sorusunu analiz ederek yapılandırılmış bir sorgu olup olmadığını tespit eder.
    /// </summary>
    public static StructuredProjectQuery? TryParse(string question, IReadOnlyList<ProjectAssistantMessageDto>? history = null)
    {
        if (string.IsNullOrWhiteSpace(question)) return null;

        var normalized = Normalize(question);
        bool hasHistory = history != null && history.Count > 0;

        // 0. AÇIKLAMA / DETAYLI BİLGİ İSTEĞİ KONTROLÜ (Explanation / Deep-dive queries must route to Semantic RAG)
        if (HasExplanationIntent(normalized))
        {
            return null; // Detaylı anlatım/açıklama soruları doğrudan LLM anlamsal RAG boru hattına yönlendirilir
        }

        // 1. KULLANICI DÜZELTME KONTROLÜ (örn: "5 dedim ama 3 tane getirdin", "5 istemiştim")
        if (hasHistory && IsCorrectionQuery(normalized, out int correctedLimit))
        {
            return new StructuredProjectQuery
            {
                Limit = correctedLimit,
                SortField = "CreatedAt",
                SortDirection = "Desc",
                IsCorrection = true
            };
        }

        // 2. TAKİP FİLTRELEME KONTROLÜ (örn: "bunlardan Kangal'da olanları göster", "ilk 3'ünü göster")
        if (hasHistory && IsFollowUpFilterQuery(normalized))
        {
            var followUp = ParseFilters(normalized);
            int followUpLimit = ExtractExplicitLimit(normalized) ?? 5;
            return followUp with
            {
                Limit = followUpLimit,
                IsFollowUpFilter = true
            };
        }

        // 3. ADET / SAYI SORGUSU (Count: "Kangal'da kaç aktif proje var?")
        bool isCount = IsCountQuery(normalized);

        // 4. SIRALAMA VE YÖN TESPİTİ
        bool hasRecencyIntent = HasRecencyIntent(normalized);
        bool hasOldestIntent = HasOldestIntent(normalized);
        bool hasUpdateIntent = HasUpdateIntent(normalized);
        bool hasAlphabeticalIntent = HasAlphabeticalIntent(normalized);

        string sortField = "CreatedAt";
        string sortDir = "Desc";

        if (hasOldestIntent)
        {
            sortField = "CreatedAt";
            sortDir = "Asc";
        }
        else if (hasUpdateIntent)
        {
            sortField = "UpdatedAt";
            sortDir = "Desc";
        }
        else if (hasAlphabeticalIntent)
        {
            sortField = "Name";
            sortDir = "Asc";
        }

        // 5. YAPISAL FİLTRELERİ AYRIŞTIR
        var query = ParseFilters(normalized);
        int? explicitLimit = ExtractExplicitLimit(normalized);

        // 6. ANLAMSAL (SEMANTIC) KONU TESPİTİ
        string? semanticTopic = DetectSemanticTopic(normalized);

        // 7. KARAR MEKANİZMASI:
        // Bir sorgunun Structured sayılması için:
        // a) Açık bir liste/arama/filtre eylemi VEYA açık limit/sıralama/count bulunmalıdır.
        bool hasActionVerb = HasStructuredActionVerb(normalized) || hasRecencyIntent || hasOldestIntent || hasUpdateIntent || hasAlphabeticalIntent || isCount || explicitLimit.HasValue;

        bool hasStructuredFilter = !string.IsNullOrEmpty(query.LocationKeyword) ||
                                   !string.IsNullOrEmpty(query.TechnologyKeyword) ||
                                   !string.IsNullOrEmpty(query.CategoryKeyword) ||
                                   !string.IsNullOrEmpty(query.StatusKeyword) ||
                                   !string.IsNullOrEmpty(query.TeamKeyword);

        if (!hasActionVerb && !hasStructuredFilter)
        {
            return null; // Saf RAG
        }

        // Eğer sadece filtre kelimesi geçip eylem yoksa ve genel bir soru soruluyorsa (örn: "Kestirimci bakımla ilgili hangi çalışmalarımız var?"):
        if (hasStructuredFilter && !hasActionVerb && !explicitLimit.HasValue && !isCount)
        {
            // Sadece konum/teknoloji varsa ve kavramsal konu yoksa structured kabul et
            if (string.IsNullOrEmpty(semanticTopic))
            {
                return query with
                {
                    Limit = 5,
                    SortField = sortField,
                    SortDirection = sortDir,
                    IsCountOnly = false
                };
            }
            return null; // Saf RAG
        }

        // Hibrit sorgu: Yapısal filtre (örn: Kangal) + Anlamsal konu (örn: kestirimci bakım)
        if (hasStructuredFilter && !string.IsNullOrEmpty(semanticTopic))
        {
            return query with
            {
                Limit = explicitLimit ?? 5,
                SortField = sortField,
                SortDirection = sortDir,
                IsCountOnly = isCount,
                SemanticTopic = semanticTopic
            };
        }

        // Deterministik Structured Query
        if (hasStructuredFilter || hasRecencyIntent || explicitLimit.HasValue || isCount)
        {
            return query with
            {
                Limit = explicitLimit ?? 5,
                SortField = sortField,
                SortDirection = sortDir,
                IsCountOnly = isCount
            };
        }

        return null;
    }

    private static StructuredProjectQuery ParseFilters(string text)
    {
        string? location = null;
        string? technology = null;
        string? category = null;
        string? status = null;
        string? team = null;

        // Lokasyon
        foreach (var loc in Locations)
        {
            if (ContainsWordOrPattern(text, loc))
            {
                location = char.ToUpperInvariant(loc[0]) + loc[1..];
                if (loc == "divrigi") location = "Divriği";
                if (loc == "ilic") location = "İliç";
                break;
            }
        }

        // Teknoloji
        foreach (var (canonical, synonyms) in Technologies)
        {
            if (synonyms.Any(s => ContainsWordOrPattern(text, s)))
            {
                technology = canonical;
                break;
            }
        }

        // Kategori
        foreach (var (canonical, synonyms) in Categories)
        {
            if (synonyms.Any(s => ContainsWordOrPattern(text, s)))
            {
                category = canonical;
                break;
            }
        }

        // Durum
        foreach (var (canonical, synonyms) in Statuses)
        {
            if (synonyms.Any(s => ContainsWordOrPattern(text, s)))
            {
                status = canonical;
                break;
            }
        }

        // Ekip
        foreach (var (canonical, synonyms) in Teams)
        {
            if (synonyms.Any(s => ContainsWordOrPattern(text, s)))
            {
                team = canonical;
                break;
            }
        }

        return new StructuredProjectQuery
        {
            LocationKeyword = location,
            TechnologyKeyword = technology,
            CategoryKeyword = category,
            StatusKeyword = status,
            TeamKeyword = team
        };
    }

    private static bool IsCorrectionQuery(string text, out int requestedLimit)
    {
        requestedLimit = 5;
        string[] patterns = { "dedim ama", "demistim", "istemiştim", "istemistim", "tane istedim", "proje istedim", "i asked for", "i said" };
        if (patterns.Any(p => text.Contains(p)))
        {
            requestedLimit = ExtractExplicitLimit(text) ?? 5;
            if (requestedLimit == 3 && text.Contains("5 dedim")) requestedLimit = 5;
            return true;
        }
        return false;
    }

    private static bool IsFollowUpFilterQuery(string text)
    {
        string[] followUpMarkers = { "bunlardan", "bu projelerden", "aralarından", "içlerinden", "ilk üçünü", "ilk 3'ünü", "ilk 5'ini", "which of these", "from those" };
        return followUpMarkers.Any(m => text.Contains(m));
    }

    private static bool IsCountQuery(string text)
    {
        string[] countPatterns = { "kaç aktif", "kaç tane", "sayısı kaç", "kaç proje", "kac proje", "how many", "count of", "number of" };
        return countPatterns.Any(p => text.Contains(p));
    }

    private static bool HasStructuredActionVerb(string text)
    {
        string[] verbs = { "göster", "getir", "listele", "hangileri", "bul", "show", "show me", "list", "get", "display", "fetch" };
        return verbs.Any(v => ContainsWordOrPattern(text, v));
    }

    private static bool HasRecencyIntent(string text)
    {
        string[] recencyPatterns = { "en son", "son eklenen", "son ekledigimiz", "en yeni", "en yeni eklenen", "latest", "newest", "most recent" };
        return recencyPatterns.Any(p => text.Contains(p));
    }

    private static bool HasOldestIntent(string text)
    {
        string[] oldestPatterns = { "ilk eklenen", "en eski", "oldest", "first added" };
        return oldestPatterns.Any(p => text.Contains(p));
    }

    private static bool HasUpdateIntent(string text)
    {
        string[] updatePatterns = { "son güncellenen", "en güncel", "recently updated", "last modified" };
        return updatePatterns.Any(p => text.Contains(p));
    }

    private static bool HasAlphabeticalIntent(string text)
    {
        string[] alphaPatterns = { "alfabetik", "a'dan z'ye", "isim sırasına göre", "alphabetical" };
        return alphaPatterns.Any(p => text.Contains(p));
    }

    private static string? DetectSemanticTopic(string text)
    {
        foreach (var topic in SemanticTopics)
        {
            if (text.Contains(topic)) return topic;
        }
        return null;
    }

    private static int? ExtractExplicitLimit(string text)
    {
        var match = Regex.Match(text, @"\b(\d{1,2})\b");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int val) && val > 0)
        {
            return Math.Clamp(val, 1, 20);
        }

        var wordNumbers = new Dictionary<string, int>
        {
            { "bir", 1 }, { "one", 1 },
            { "iki", 2 }, { "two", 2 },
            { "üç", 3 }, { "uc", 3 }, { "three", 3 },
            { "dört", 4 }, { "dort", 4 }, { "four", 4 },
            { "beş", 5 }, { "bes", 5 }, { "five", 5 },
            { "altı", 6 }, { "alti", 6 }, { "six", 6 },
            { "yedi", 7 }, { "seven", 7 },
            { "sekiz", 8 }, { "eight", 8 },
            { "dokuz", 9 }, { "nine", 9 },
            { "on", 10 }, { "ten", 10 }
        };

        foreach (var (word, num) in wordNumbers)
        {
            if (Regex.IsMatch(text, $@"\b{word}\b"))
            {
                return num;
            }
        }

        return null;
    }

    private static bool ContainsWordOrPattern(string text, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return false;
        return text.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasExplanationIntent(string text)
    {
        string[] explanationMarkers = {
            "hakkında", "hakkinda", "detaylı bilgi", "detayli bilgi", "bilgi verir misin", "bilgi ver", "bilgi alabilir miyim",
            "nasıl çalışır", "nasil calisir", "mimarisi", "ne işe yarar", "ne ise yarar", "ne yapar",
            "detaylandır", "detaylandir", "açıklar mısın", "aciklar misin", "açıkla", "acikla",
            "anlatır mısın", "anlatir misin", "anlat", "tell me about", "explain", "how does", "what is the architecture of"
        };
        return explanationMarkers.Any(m => text.Contains(m));
    }

    private static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var lower = input.Trim()
            .Replace('İ', 'i')
            .ToLowerInvariant();
        lower = lower.TrimEnd('.', '!', '?');
        lower = Regex.Replace(lower, @"\s+", " ").Trim();
        return lower;
    }
}
