# Demir Export — Proje Kütüphanesi / Project Hub
## Phase 18: Proje Bilgi İndeksi, Çok Dilli BGE-M3 Embedding & Semantik Arama Mimarisi

Bu doküman, Phase 18 kapsamında geliştirilen Proje Bilgi İndeksi (Project Knowledge Index), çok dilli BGE-M3 embedding entegrasyonu, veritabanı testi izolasyonu (`DeUygulamaVitriniTestDb`), vektör saklama stratejisi, anlamsal benzerlik arama altyapısı ve yetkilendirme tabanlı erişim kontrolü mimarisini tanımlar.

---

## 1. Mimari Genel Bakış ve Temel İlkeler

Phase 18, gelecekteki RAG (Retrieval-Augmented Generation) sohbet asistanı için bağımsız **arama ve erişim (retrieval)** temelini oluşturur.

### 1.1 Temel Ayrım (Generative LLM vs Embedding Model)
- **Embedding Modeli (`text-embedding-bge-m3`):** Metinlerin çok dilli anlamsal temsilini (**1024 boyutlu** yoğun vektör) üretir ve arama sorgusunu vektör uzayına dönüştürür.
- **Üretken LLM (`deepseek-r1-distill-qwen-7b`):** Yalnızca metin üretimi yapar. **Semantik arama esnasında üretken LLM ASLA çağrılmaz.** Arama tamamen vektör benzerlik matematiği ve veritabanı yetki filtreleri ile yürütülür.

### 1.2 Veri Akışı
```
[Proje SQL Veritabanı: DeUygulamaVitriniDb] 
        ↓
[ProjectKnowledgeDocumentBuilder (Deterministik Domain Parçalama)]
        ↓
[KnowledgeChunkDraft (OVERVIEW, TECHNICAL, ORGANIZATION_USAGE)]
        ↓
[ContentHash (SHA256) & Model/Dimension Eşleşme Kontrolü]
        ↓
[LocalEmbeddingProvider (BGE-M3 / 1024-D / LM Studio POST /v1/embeddings)]
        ↓
[SQL Server Tablosu: ProjectKnowledgeChunks (varbinary(max) IEEE 754 float32)]
        ↓
───────────────────────────────────────────────────────────────────────────
[Kullanıcı Arama Sorgusu (TR veya EN)]
        ↓
[IProjectSemanticSearchService]
        ↓
[Query Embedding: BGE-M3 -> 1024-D Vektör]
        ↓
[Bellek-İçi SIMD Donanım Hızlandırmalı Cosine Similarity]
        ↓
[Kullanıcı Yetki Filtresi (Taslak/Gizli Proje İzolasyonu & Silinmiş Kayıt Engeli)]
        ↓
[Sıralı İlgili Projeler + Benzerlik Skorları + İlgili Parça Metinleri]
```

---

## 2. Test Veritabanı İzolasyonu ve Güvenlik Kalkanı (Safety Guard)

Entegrasyon ve regresyon testlerinin normal geliştirme veritabanını (`DeUygulamaVitriniDb`) kirletmesini önlemek amacıyla katı bir veritabanı izolasyonu uygulanmıştır:

### 2.1 Ortam ve Veritabanı Ayrımı
- **Development Ortamı (`ASPNETCORE_ENVIRONMENT=Development`):**
  - Veritabanı: `DeUygulamaVitriniDb` (Port 5000)
  - Yalnızca geliştirme ve kullanıcı kabul verilerini barındırır.
- **Testing Ortamı (`ASPNETCORE_ENVIRONMENT=Testing`):**
  - Veritabanı: `DeUygulamaVitriniTestDb` (Port 5001)
  - Otomatik regresyon testleri, Excel import testleri ve yük testleri yalnızca bu veritabanında çalışır.

### 2.2 Startup Safety Guard (Fail-Fast Güvenlik Kalkanı)
`DependencyInjection.cs` içinde altyapı servisleri kaydedilirken ortam kontrolü yapılır:
```csharp
if (environment != null && environment.IsEnvironment("Testing"))
{
    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
    var initialCatalog = builder.InitialCatalog;
    if (string.IsNullOrWhiteSpace(initialCatalog) ||
        initialCatalog.Equals("DeUygulamaVitriniDb", StringComparison.OrdinalIgnoreCase) ||
        !initialCatalog.EndsWith("TestDb", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            $"TEST DATABASE ISOLATION SAFETY GUARD VIOLATION: Environment is 'Testing' but connection string targets database '{initialCatalog}'. " +
            $"Testing environment MUST target a dedicated test database (e.g., 'DeUygulamaVitriniTestDb'). Startup aborted to protect Development database.");
    }
}
```
Bu sayede olası bir yapılandırma hatasında testler geliştirme veritabanına dokunamadan uygulama başlatılması derhal durdurulur.

---

## 3. SQL Server Vektör Saklama ve Model Uyumluluğu Güvencesi

### 3.1 Saklama Stratejisi
1024 boyutlu `float[]` vektörler, `ProjectKnowledgeChunks` tablosunda sıkıştırılmamış IEEE 754 float32 ikili dizisi (`varbinary(max)`, parça başına 4096 bayt) olarak saklanır:
- `EmbeddingModel`: Vektörü üreten model kimliği (örn: `text-embedding-bge-m3`)
- `EmbeddingDimension`: Vektör boyutu (örn: 1024)
- `ContentHash`: Parça metninin SHA256 özeti
- `IndexedAtUtc`: İndekslenme zamanı

### 3.2 Model Değişikliğinde Vektör Uzayı Güvenliği
Model veya boyut değiştiğinde (örn. Nomic 768D'den BGE-M3 1024D'ye geçiş):
- `ProjectSemanticSearchService`, sorgu anında yalnızca aktif model ve boyutla uyuşan parçaları karşılaştırır; uyumsuz eski vektörleri sorgu uzayına dahil etmez.
- `ProjectKnowledgeIndexService.RebuildIndexAsync` eski model vektörlerini otomatik geçersiz kılarak yeni model ile baştan embed eder.

---

## 4. İndeks Durumu Hesaplama Semantiği (Status Fix)

Eski `(totalEligibleProjects * 3) - totalChunks` formülü kaldırılmıştır. Çünkü `ProjectKnowledgeDocumentBuilder`, içeriği yetersiz veya boş olan parçaları (örn. organizasyon/ekip/lokasyon bilgisi bulunmayan projeler için `ORGANIZATION_USAGE` parçasını) kasıtlı olarak üretmez.

### 4.1 Doğru İndeks Durumu Algoritması
1. Aktif projeler için güncel doküman derleyicinin ürettiği **anlamlı taslak parçalar (expected drafts)** belirlenir.
2. Her taslak için veritabanında aynı `ChunkKey`, aktif `EmbeddingModel`, aktif `EmbeddingDimension`, eşleşen `ContentHash` ve geçerli `EmbeddingVector` bulunup bulunmadığı kontrol edilir.
3. Tam bir yeniden indeksleme sonrasında `staleOrMissingChunks` değeri **TAM OLARAK 0** olur.

---

## 5. Çok Dilli Embedding Modeli: BGE-M3

### 5.1 Nomic Embed v1.5 Sınırlaması ve BGE-M3 Geçiş Gerekçesi
Phase 18.2 araştırmasında, `text-embedding-nomic-embed-text-v1.5` modelinin Türkçe içi (TR → TR) aramalarda başarılı olmasına rağmen, İngilizce sorgularla Türkçe dokümanları eşleştirmede (EN → TR çapraz dil) yetersiz kaldığı; *"Which projects are related to predictive maintenance?"* sorgusunda *"Akıllı Bakım Tahmin Sistemi"* projesini ilgisiz Türkçe dokümanların altına sıraladığı tespit edilmiştir.

Bu nedenle 100'den fazla dilde çapraz anlamsal hizalama yeteneğine sahip **BGE-M3 (BAAI General Embedding M3)** modeline geçilmiştir.

### 5.2 BGE-M3 Çalışma Zamanı Bilgileri
- **LM Studio Sürümü:** 0.3.14
- **Yerel Uç Nokta:** `http://127.0.0.1:1234/v1/embeddings`
- **LM Studio Model Tanımlayıcısı (API Identifier):** `text-embedding-bge-m3`
- **GGUF Dosyası:** `bge-m3-Q4_K_M.gguf`
- **Vektör Boyutu (Dimension):** **1024 float** (Canlı API yanıtı ile doğrulanmıştır)

### 5.3 BGE-M3 Canlı Kontrollü Karşılaştırma Sonuçları (Benchmark)

| Test Senaryosu | Sorgu Metni | Beklenen Hedef Doküman | Ölçülen Cosine Benzerlik | Sıralama / Sonuç |
| :--- | :--- | :--- | :--- | :--- |
| **TR → TR Semantik** | *"Arızaları gerçekleşmeden tahmin eden sistemler"* | Akıllı Bakım Tahmin Sistemi (TR) | **0.6373** (İlgisiz Enerji: 0.4958) | **BAŞARILI (#1 Sıra)** |
| **EN → TR Çapraz Dil** | *"Which projects are related to predictive maintenance?"* | Akıllı Bakım Tahmin Sistemi (TR) | **0.5528** (İlgisiz TR: 0.3988, EN doc: 0.5465) | **BAŞARILI (#1 TR Hedef)** |
| **TR → EN Çapraz Dil** | *"Ekipman arızalarını önceden tahmin eden projeler"* | Predictive Maintenance System (EN) | **0.5513** | **BAŞARILI** |
| **TR Anlamsal Yorumlama** | *"Sahadaki makinelerin durumunu uzaktan takip eden çözümler"* | Saha Telemetri & İzleme (TR) | **0.6370** | **YÜKSEK BAŞARI** |
| **EN Anlamsal Yorumlama** | *"Solutions that improve operational visibility in mining sites"* | Saha Telemetri & İzleme (TR) | **0.5841** | **BAŞARILI** |
| **İlgisiz Negatif Sorgu** | *"çalışan yemek menüsü"* | Akıllı Bakım Tahmin Sistemi (TR) | **0.3872** (Eşik altı, ayrışma tam) | **BAŞARILI (Elenir)** |
| **Teknik Kavram Eşleşmesi** | *"SAP entegrasyonu kullanan projeler"* | SAP PM & ERP Entegrasyonu | **0.5498** (Enerji: 0.4348) | **BAŞARILI** |

---

## 6. Güvenlik ve Yetkilendirme Kuralları

Proje Kütüphanesi genelinde **ASP.NET Core Identity Cookie Authentication** mimarisi esastır (JWT veya Bearer token KULLANILMAZ).

- **Oturum Çerezi:** HTTP-Only `.DemirExport.Auth`
- **Anonim İstekler:** `GET/POST /api/ai/semantic-search` uç noktasına oturumsuz istekler `401 Unauthorized` alır.
- **Normal Kullanıcı:** Yalnızca `IsPublished == true` ve `ApprovalStatus == Approved` olan onaylanmış projelerin parçalarını görebilir. Admin tanı ve indeks uç noktalarına erişmeye çalıştığında `403 Forbidden` alır.
- **Proje Girişi Yetkilisi (Creator):** Onaylanmış projelerin yanı sıra kendi oluşturduğu taslak (`Draft`) ve incelemedeki projelerini de arama sonuçlarında görebilir.
- **Yönetici (Admin):** Tüm silinmemiş projeleri görebilir; indeks yönetimi (`/api/admin/ai/index/*`) uç noktalarını tetikleyebilir.
- **Silinmiş Projeler (`IsDeleted == true`):** Hiçbir kullanıcı (Admin dahil) tarafından arama sonuçlarında getirilemez.

### 6.1 Hata Durumları ve HTTP 503 Servis Erişilemezlik Semantiği

Anlamsal arama altyapısında "sonuç bulunamadı" ile "sağlayıcı erişilemez" durumları kesin olarak ayrılmıştır:

1. **CASE A — Sağlayıcı Aktif, Ancak Eşik Üstü Sonuç Yok:**
   - İstemciye: `HTTP 200 OK` ve `[]` (boş dizi) döner.
   - Anlamı: Arama başarıyla icra edilmiş, ancak verilen benzerlik eşiğini (`minSimilarity`) geçen proje bulunamamıştır.
2. **CASE B — Embedding Sağlayıcısı Erişilemez / Kapalı / Zaman Aşımı:**
   - İstemciye: `HTTP 503 Service Unavailable` ve steril `ProblemDetails` döner.
   - Başlık: `"Anlamsal Arama Servisi Kullanılamıyor"`
   - Açıklama: `"Anlamsal arama servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin."`
   - Güvenlik: Yanıtta hiçbir dahili IP adresi, port, bağlantı dizesi veya stack trace ifşa edilmez.
3. **CASE C — Anonim İstek:**
   - İstemciye: `HTTP 401 Unauthorized` döner.
4. **CASE D — Normal Kullanıcının Admin Tanı Uç Noktalarına Erişimi:**
   - İstemciye: `HTTP 403 Forbidden` döner.

---

## 7. Gelecekteki Kurumsal Genişletilebilirlik (Corporate Cloud / On-Prem)

Mevcut LM Studio yerel çalışma zamanı geliştirme ve test ortamı içindir. `IEmbeddingProvider` soyutlaması sayesinde ileride kurumsal Azure OpenAI, AWS Bedrock veya merkezi kurum içi embedding servislerine geçişte iş mantığı ve denetleyicilerde hiçbir değişiklik yapılmasına gerek kalmaz.

