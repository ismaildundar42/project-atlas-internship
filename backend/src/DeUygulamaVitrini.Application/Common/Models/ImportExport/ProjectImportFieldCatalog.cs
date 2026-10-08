namespace DeUygulamaVitrini.Application.Common.Models.ImportExport;

public enum ImportFieldType
{
    ScalarString,
    LookupSingle,
    LookupMulti,
    Enum,
    Date,
    Boolean,
    Url
}

public class ProjectImportFieldDefinition
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public ImportFieldType FieldType { get; set; }
    public int? MaxLength { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = new();
}

/// <summary>
/// Proje içe aktarma alanları kataloğu — tek ve merkezi doğruluk kaynağı (Single Source of Truth).
/// İçe aktarılabilir alanların tanımları, veri tipleri, kısıtları ve eşleştirme takma adlarını yönetir.
/// </summary>
public static class ProjectImportFieldCatalog
{
    public static readonly IReadOnlyList<ProjectImportFieldDefinition> Fields = new List<ProjectImportFieldDefinition>
    {
        new()
        {
            Key = "name",
            DisplayName = "Proje Adı",
            IsRequired = true,
            FieldType = ImportFieldType.ScalarString,
            MaxLength = 200,
            Description = "Projenin resmi adı. Benzersiz olmalıdır.",
            Aliases = new() { "proje adı", "proje adi", "proje ad", "project name", "name", "proje", "title" }
        },
        new()
        {
            Key = "shortDescription",
            DisplayName = "Kısa Açıklama",
            IsRequired = true,
            FieldType = ImportFieldType.ScalarString,
            MaxLength = 500,
            Description = "Proje kartlarında görünen tek cümlelik kısa özet.",
            Aliases = new() { "kısa açıklama", "kisa aciklama", "kısa ozet", "kisa ozet", "short description", "summary" }
        },
        new()
        {
            Key = "category",
            DisplayName = "Kategori",
            IsRequired = true,
            FieldType = ImportFieldType.LookupSingle,
            Description = "Sistemde tanımlı proje kategorisi.",
            Aliases = new() { "kategori", "category", "proje kategorisi" }
        },
        new()
        {
            Key = "status",
            DisplayName = "Durum",
            IsRequired = true,
            FieldType = ImportFieldType.LookupSingle,
            Description = "Sistemde tanımlı proje operasyonel durumu.",
            Aliases = new() { "durum", "status", "proje durumu", "state" }
        },
        new()
        {
            Key = "primaryTeam",
            DisplayName = "Sorumlu Ekip",
            IsRequired = false,
            FieldType = ImportFieldType.LookupSingle,
            Description = "Projeden birinci derecede sorumlu ekip.",
            Aliases = new() { "sorumlu ekip", "birincil ekip", "ana ekip", "primary team", "lead team", "ekip", "team" }
        },
        new()
        {
            Key = "supportingTeams",
            DisplayName = "Destekleyen Ekipler",
            IsRequired = false,
            FieldType = ImportFieldType.LookupMulti,
            Description = "Projeye katkı sağlayan diğer ekipler (; ile ayrılmış).",
            Aliases = new() { "destekleyen ekipler", "yardımcı ekipler", "destek ekipleri", "supporting teams", "secondary teams" }
        },
        new()
        {
            Key = "members",
            DisplayName = "Proje Üyeleri",
            IsRequired = false,
            FieldType = ImportFieldType.LookupMulti,
            Description = "Projeden sorumlu üyelerin kurumsal e-posta adresleri (; ile ayrılmış).",
            Aliases = new() { "proje üyeleri", "proje uyeleri", "üyeler", "uyeler", "members", "team members", "katılımcılar" }
        },
        new()
        {
            Key = "developmentType",
            DisplayName = "Geliştirme Tipi",
            IsRequired = false,
            FieldType = ImportFieldType.Enum,
            Description = "Internal, External veya Hybrid geliştirme kaynağı.",
            Aliases = new() { "geliştirme tipi", "gelistirme tipi", "development type", "dev type", "kaynak tipi" }
        },
        new()
        {
            Key = "technologies",
            DisplayName = "Teknolojiler",
            IsRequired = false,
            FieldType = ImportFieldType.LookupMulti,
            Description = "Kullanılan teknolojiler (; ile ayrılmış).",
            Aliases = new() { "teknolojiler", "technologies", "tech stack", "teknoloji listesi" }
        },
        new()
        {
            Key = "locations",
            DisplayName = "Lokasyonlar",
            IsRequired = false,
            FieldType = ImportFieldType.LookupMulti,
            Description = "Uygulanan lokasyon veya maden sahaları (; ile ayrılmış).",
            Aliases = new() { "lokasyonlar", "locations", "sahalar", "konumlar" }
        },
        new()
        {
            Key = "tags",
            DisplayName = "Etiketler",
            IsRequired = false,
            FieldType = ImportFieldType.LookupMulti,
            Description = "Sistemde tanımlı arama etiketleri (; ile ayrılmış).",
            Aliases = new() { "etiketler", "tags", "anahtar kelimeler", "keywords" }
        },
        new()
        {
            Key = "description",
            DisplayName = "Genel Açıklama",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Detaylı proje genel kapsam metni.",
            Aliases = new() { "genel açıklama", "genel aciklama", "açıklama", "aciklama", "description", "details" }
        },
        new()
        {
            Key = "purpose",
            DisplayName = "Amaç",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Projenin geliştirilme amacı.",
            Aliases = new() { "amaç", "amac", "purpose", "hedef", "goal" }
        },
        new()
        {
            Key = "problemSolved",
            DisplayName = "Çözülen Problem",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Projenin çözdüğü operasyonel veya iş problemi.",
            Aliases = new() { "çözülen problem", "cozulen problem", "problem", "problem solved" }
        },
        new()
        {
            Key = "nonTechnicalDescription",
            DisplayName = "Teknik Olmayan Açıklama",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Teknik bilgisi olmayan çalışanlar için sade açıklama.",
            Aliases = new() { "teknik olmayan açıklama", "teknik olmayan aciklama", "sade açıklama", "non-technical description" }
        },
        new()
        {
            Key = "technicalDescription",
            DisplayName = "Teknik Açıklama",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Teknik mimari ve implementasyon detayları.",
            Aliases = new() { "teknik açıklama", "teknik aciklama", "technical description", "mimari" }
        },
        new()
        {
            Key = "businessImpact",
            DisplayName = "İş Etkisi",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Verimlilik, tasarruf ve iş kazancı etkisi.",
            Aliases = new() { "iş etkisi", "is etkisi", "business impact", "kazanç", "roi" }
        },
        new()
        {
            Key = "targetAudience",
            DisplayName = "Hedef Kitle",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            MaxLength = 1000,
            Description = "Projenin hedeflenen kullanıcı kitlesi.",
            Aliases = new() { "hedef kitle", "target audience", "kullanıcı kitlesi" }
        },
        new()
        {
            Key = "accessInstructions",
            DisplayName = "Erişim Talimatları",
            IsRequired = false,
            FieldType = ImportFieldType.ScalarString,
            Description = "Uygulamaya erişim ve kullanım talimatları.",
            Aliases = new() { "erişim talimatları", "erisim talimatlari", "access instructions", "erişim", "nasıl kullanılır" }
        },
        new()
        {
            Key = "startDate",
            DisplayName = "Başlangıç Tarihi",
            IsRequired = false,
            FieldType = ImportFieldType.Date,
            Description = "Proje başlangıç tarihi (YYYY-MM-DD).",
            Aliases = new() { "başlangıç tarihi", "baslangic tarihi", "başlama tarihi", "start date", "start" }
        },
        new()
        {
            Key = "endDate",
            DisplayName = "Bitiş Tarihi",
            IsRequired = false,
            FieldType = ImportFieldType.Date,
            Description = "Proje bitiş veya hedeflenen tamamlanma tarihi (YYYY-MM-DD).",
            Aliases = new() { "bitiş tarihi", "bitis tarihi", "tamamlanma tarihi", "end date", "end" }
        },
        new()
        {
            Key = "applicationUrl",
            DisplayName = "Canlı Uygulama URL",
            IsRequired = false,
            FieldType = ImportFieldType.Url,
            MaxLength = 500,
            Description = "Canlı uygulamaya doğrudan erişim web adresi.",
            Aliases = new() { "canlı uygulama url", "canli uygulama url", "uygulama url", "application url", "app url", "url" }
        },
        new()
        {
            Key = "repositoryUrl",
            DisplayName = "Repository URL",
            IsRequired = false,
            FieldType = ImportFieldType.Url,
            MaxLength = 500,
            Description = "Kaynak kod deposu bağlantısı.",
            Aliases = new() { "repository url", "repo url", "kaynak kod url", "git url", "github url" }
        },
        new()
        {
            Key = "coverImageUrl",
            DisplayName = "Kapak Görseli URL",
            IsRequired = false,
            FieldType = ImportFieldType.Url,
            MaxLength = 500,
            Description = "Özel kapak görseli web adresi.",
            Aliases = new() { "kapak görseli url", "kapak gorseli url", "kapak resmi", "cover image url", "image url" }
        },
        new()
        {
            Key = "isFeatured",
            DisplayName = "Öne Çıkan",
            IsRequired = false,
            FieldType = ImportFieldType.Boolean,
            Description = "Proje ana sayfada öne çıkarılsın mı? (EVET/HAYIR veya true/false).",
            Aliases = new() { "öne çıkan", "one cikan", "is featured", "featured", "öne çıkar" }
        }
    };

    private static readonly HashSet<string> ProtectedFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "slug", "approvalstatus", "ispublished", "createdbyuserid",
        "submittedforreviewat", "submittedforreviewbyuserid",
        "reviewedat", "reviewedbyuserid", "rejectionreason",
        "createdat", "createdby", "updatedat", "updatedby",
        "deletedat", "deletedby", "isdeleted",
        "integrations", "media", "documents", "projectintegrations", "projectmediaitems", "projectdocuments"
    };

    private static readonly Dictionary<string, ProjectImportFieldDefinition> KeyIndex =
        Fields.ToDictionary(f => f.Key, f => f, StringComparer.OrdinalIgnoreCase);

    public static ProjectImportFieldDefinition? GetByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        return KeyIndex.GetValueOrDefault(key.Trim());
    }

    public static bool IsProtectedField(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return ProtectedFieldKeys.Contains(key.Trim());
    }

    public static bool IsValidField(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return KeyIndex.ContainsKey(key.Trim());
    }
}
