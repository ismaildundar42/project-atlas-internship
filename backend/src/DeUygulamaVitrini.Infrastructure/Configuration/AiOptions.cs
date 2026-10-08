namespace DeUygulamaVitrini.Infrastructure.Configuration;

/// <summary>
/// AI altyapı yapılandırma seçenekleri.
/// appsettings.json içerisindeki "AI" bölümünden bağlanır.
/// </summary>
public class AiOptions
{
    public const string SectionName = "AI";

    /// <summary>
    /// AI servisinin etkin olup olmadığı. Kapalıysa uygulama sorunsuz çalışmaya devam eder.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Seçilen sağlayıcı türü: "Local", "Disabled" vb.
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// AI istekleri için zaman aşımı süresi (saniye cinsinden, varsayılan: 60).
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Yerel (Local / Ollama / LM Studio) sağlayıcı yapılandırma detayları.
    /// </summary>
    public LocalAiOptions Local { get; set; } = new();

    /// <summary>
    /// Otomatik anlamsal indeks yaşam döngüsü ve arka plan eşitleme ayarları.
    /// </summary>
    public SemanticIndexOptions SemanticIndex { get; set; } = new();

    /// <summary>
    /// AI sağlayıcı cold-start önleme ve başlangıç ısınma (warm-up) ayarları.
    /// </summary>
    public AiWarmupOptions Warmup { get; set; } = new();
}

/// <summary>
/// AI sağlayıcı cold-start önleme ve başlangıç ısınma seçenekleri.
/// </summary>
public class AiWarmupOptions
{
    /// <summary>
    /// AI sağlayıcıları için başlangıçta arka planda warm-up yapılmasının etkin olup olmadığı.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// API başlangıcında warm-up çağrısının yapılmasından önceki bekleme süresi (saniye, varsayılan: 5).
    /// </summary>
    public int DelaySeconds { get; set; } = 5;

    /// <summary>
    /// Metin üretimi warm-up isteği için maksimum token sınırı (varsayılan: 8).
    /// </summary>
    public int MaxGenerationTokens { get; set; } = 8;
}

/// <summary>
/// Anlamsal indeks arka plan eşitleme seçenekleri.
/// </summary>
public class SemanticIndexOptions
{
    /// <summary>
    /// Arka planda otomatik indeks eşitleme servisinin etkin olup olmadığı.
    /// </summary>
    public bool BackgroundIndexingEnabled { get; set; } = true;

    /// <summary>
    /// Eşitleme döngüsünün periyodu (saniye cinsinden, varsayılan: 30).
    /// </summary>
    public int ReconciliationIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Her bir eşitleme döngüsünde işlenecek maksimum proje sayısı (sınırlı toplu iş).
    /// </summary>
    public int BatchSize { get; set; } = 5;

    /// <summary>
    /// API başlangıcında arka plan görevinin başlamasından önceki bekleme süresi (saniye).
    /// </summary>
    public int InitialDelaySeconds { get; set; } = 5;
}

/// <summary>
/// Yerel AI sağlayıcısı için özel ayarlar.
/// </summary>
public class LocalAiOptions
{
    /// <summary>
    /// Yerel model çalışma zamanı temel HTTP adresi.
    /// Örnekler:
    /// - LM Studio OpenAI uyumlu: "http://localhost:1234/v1"
    /// - Ollama: "http://localhost:11434"
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";

    /// <summary>
    /// Metin üretimi (chat/generation) için ayrı bir temel HTTP adresi tanımlanmak istenirse kullanılır (Örn: "http://127.0.0.1:1234/v1").
    /// Boş veya null bırakıldığında varsayılan olarak BaseUrl değeri kullanılır.
    /// </summary>
    public string? ChatBaseUrl { get; set; }

    /// <summary>
    /// Vektör üretimi (embedding) için ayrı bir temel HTTP adresi tanımlanmak istenirse kullanılır (Örn: "http://127.0.0.1:1235/v1").
    /// Boş veya null bırakıldığında varsayılan olarak BaseUrl değeri kullanılır.
    /// </summary>
    public string? EmbeddingBaseUrl { get; set; }

    /// <summary>
    /// Metin üretimi için kullanılacak model adı / tanımlayıcısı.
    /// </summary>
    public string ChatModel { get; set; } = "deepseek-r1-distill-qwen-7b";

    /// <summary>
    /// Vektör üretimi (embedding) için kullanılacak model adı / tanımlayıcısı.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-nomic-embed-text-v1.5";

    /// <summary>
    /// Beklenen embedding vektör boyutu (Nomic v1.5 için varsayılan 768).
    /// </summary>
    public int EmbeddingDimension { get; set; } = 768;

    /// <summary>
    /// API iletişim protokolü: "OpenAiCompatible" veya "Ollama".
    /// </summary>
    public string ApiFormat { get; set; } = "OpenAiCompatible";
}
