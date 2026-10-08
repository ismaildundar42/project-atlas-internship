# Yapay Zeka Mimari ve Sağlayıcı Temeli (AI Architecture & Provider Foundation)

Bu doküman, **Demir Export — Proje Kütüphanesi / Project Hub** platformunun yapay zeka (AI) katmanının mimari sınırlarını, yerel geliştirme ortamını, sağlayıcı bağımsızlık modelini, güvenlik ilkelerini ve gelecek fazlara (Semantik Arama, RAG Asistanı, Proje Özeti vb.) yönelik stratejik kararlarını tanımlar.

---

## 1. Hedefler ve Kapsam Dışı Konular (Goals & Non-Goals)

### Hedefler (Goals — Phase 17)
- **Sağlayıcı Bağımsızlığı (Provider-Agnostic Abstraction):** Uygulama iş katmanını (`Application` ve `API`) belirli bir yapay zeka kütüphanesine, SDK'sına veya satıcısına (Ollama, LM Studio, OpenAI, Azure, Gemini, Anthropic vb.) doğrudan bağımlı kılmadan soyut bir sözleşme (`IAiProvider`) arkasına almak.
- **Yerel-Önce (Local-First Development):** Geliştirme ortamında ücretli bulut API'lerine ihtiyaç duymadan, geliştirici makinesindeki yerel donanım üzerinde (CPU/GPU) çalışan modellerle tam işlevsel metin üretimi gerçekleştirmek.
- **Güvenli ve Dayanıklı Entegrasyon:** Model çalışmadığında, çöktüğünde veya zaman aşımına uğradığında platformun ana işlevlerinin (Proje Yönetimi, Onay İşlemleri, Bildirimler, Excel İçe/Dışa Aktarma) kesintiye uğramamasını sağlamak; kontrollü hata ve durum yönetimi sunmak.
- **Geleceğe Hazırlık:** Phase 18 (Semantik Arama), Phase 19 (RAG Asistanı) ve Phase 20 (AI Proje Özeti) için embedding modeli ve vektör saklama stratejisini bugünden belirlemek.

### Kapsam Dışı Konular (Non-Goals — Gelecek Fazlar)
- **AI İçerik Çevirisi (Phase 22):** Çeviri özellikleri Phase 17'de uygulanmaz.
- **RAG / Chatbot Asistanı (Phase 19):** Çok turlu sohbet, bağlam zenginleştirme ve proje asistanı UI'ı bu fazın konusu değildir.
- **AI Proje Özeti Üretimi (Phase 20):** Projeler için otomatik özet çıkarma henüz entegre edilmez.
- **Vektör Veritabanı ve Proje İndeksleme (Phase 18):** Veritabanına vektör tabloları eklenmez veya tüm projeler vektörize edilmez.

---

## 2. Sağlayıcı Bağımsız Mimari Sınırı (Architectural Boundary)

Sistem katmanları arasındaki bağımlılık hiyerarşisi aşağıdaki gibidir:

```
┌─────────────────────────────────────────────────────────┐
│                    API / Controllers                    │
│      [AiDiagnosticsController, Gelecek AI Endpoint'leri] │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                    Application Katmanı                   │
│   - IAiProvider (Soyut Sözleşme)                        │
│   - AiGenerationRequest & AiGenerationResult (DTO'lar)   │
│   - AiHealthStatus & Diagnostic Modelleri               │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                   Infrastructure Katmanı                │
│   - LocalAiProvider (Doğrudan HTTP / Typed HttpClient) │
│   - Gelecekte: AzureOpenAiProvider / CorporateAiProvider│
│   - AiOptions (appsettings.json Bağlantısı)             │
└────────────────────────────┬────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────┐
│                 Yerel / Kurumsal AI Motoru              │
│   - LM Studio (OpenAI Uyumlu REST: localhost:1234)       │
│   - VEYA Ollama (REST: localhost:11434)                 │
│   - Model: deepseek-r1-distill-qwen-7b (Qwen2 / 4.68 GB) │
└─────────────────────────────────────────────────────────┘
```

### Sözleşme (Contract) Tasarımı
- `IAiProvider.GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken)`
- `IAiProvider.CheckHealthAsync(CancellationToken cancellationToken)`
- Application katmanında satıcıya özel hiçbir NuGet paketi bulunmaz; tüm HTTP ve JSON serileştirme detayları `Infrastructure` katmanında kapsüllenmiştir.

---

## 3. Donanım ve Model Seçim Raporu

### Keşfedilen Geliştirme Makinesi Kaynakları
- **İşlemci (CPU):** Intel(R) Core(TM) i5-10300H @ 2.50GHz (4 Fiziksel Çekirdek, 8 Mantıksal İşlemci)
- **Sistem Belleği (RAM):** 16.0 GB DDR4
- **Grafik İşlemci (GPU):** NVIDIA GeForce GTX 1650 (4 GB VRAM) + Intel UHD Graphics
- **Mevcut Yerel Çalışma Zamanı:** LM Studio CLI (`lms.exe`) & Ollama REST uyumluluğu

### Seçilen Model: `deepseek-r1-distill-qwen-7b` (GGUF Q4_K_M)
| Kriter | Değerlendirme & Neden |
| :--- | :--- |
| **Model Mimarisi** | Qwen 2.5 temelli, DeepSeek-R1 akıl yürütme (reasoning) damıtılmış modeli. |
| **Disk & VRAM Kullanımı** | 4.68 GB boyut; 4 GB VRAM'e kısmi GPU hızlandırma ile offload edilir ve 16 GB sistem belleğinde stabil çalışır. |
| **Türkçe Kalitesi** | Qwen 2.5 mimarisi çok dilli ve Türkçe morfolojik uyumda yüksek başarı gösterir. |
| **İngilizce Kalitesi** | Kurumsal terminoloji ve teknik dokümantasyon sorgularında yüksek doğruluk. |
| **Talimat Uyumu (Instruction Following)** | Sistem istemlerine (`SystemPrompt`) ve format kısıtlamalarına sıkı bağlılık. |
| **Lisans** | MIT / Apache 2.0 uyumlu açık ağırlıklı araştırma ve ticari kullanıma uygun lisans. |
| **Düşünme İzi Temizliği** | Reasoning modellerinden gelen `<think>...</think>` blokları altyapıda otomatik filtrelenerek iş katmanına yalnızca saf sonuç iletilir. |

---

## 4. Yapılandırma ve Hata Dayanıklılığı (Configuration & Resilience)

### `appsettings.json` Yapısı
```json
{
  "AI": {
    "Enabled": true,
    "Provider": "Local",
    "RequestTimeoutSeconds": 60,
    "Local": {
      "BaseUrl": "http://localhost:1234/v1",
      "ChatModel": "deepseek-r1-distill-qwen-7b",
      "ApiFormat": "OpenAiCompatible"
    }
  }
}
```

### Hata ve İzolasyon Kuralları
1. **AI Devre Dışı Modu (`Enabled: false`):** Platformun ana fonksiyonları (CRUD, Excel, Onay, Bildirim vb.) eksiksiz çalışır. AI çağrıları kontrollü `AiGenerationResult.Failed` döner; sunucu çökmez.
2. **Sağlayıcıya Ulaşılamama (Offline/Connection Refused):** Yerel motor açık değilse kullanıcı dostu hata mesajı üretilir, arka planda loglanır, HTTP 500 veya çökmeye yol açmaz.
3. **Zaman Aşımı ve İptal (`CancellationToken`):** İstekler `RequestTimeoutSeconds` süresi sonunda otomatik kesilir, asılı kalan thread veya kaynak sızıntısı oluşmaz.
4. **Log Gizliliği:** Kullanıcıların girdiği gizli proje prompt'ları sunucu loglarına ham içerik olarak dökülmez.

---

## 5. Güvenlik ve Gizlilik Prensipleri (Security & Privacy)

- **Sıfır Dış Veri Sızıntısı:** Geliştirme ortamında hiçbir veri bulut sağlayıcılarına aktarılmaz; tüm çıkarım yerel makinede tamamlanır.
- **SQL Server İzolasyonu:** LLM modeline veritabanı bağlantı dizesi veya doğrudan SQL çalıştırma yetkisi verilmez. Model yalnızca API tarafından açıkça sunulan metin girdisini işler.
- **Otonom İşlem Yasağı:** Model kendi kendine proje onaylayamaz, silemez veya sistem ayarlarını değiştiremez.
- **Yetkilendirme:** AI teşhis ve test uç noktaları (`/api/admin/ai/test` ve `/api/admin/ai/status`) yalnızca **Admin** rolüne sahip kullanıcılara açıktır (`[Authorize(Policy = "AdminAccess")]`).

---

## 6. Gelecek Fazlar İçin Stratejik Kararlar

### 6.1 Phase 18 — Embedding Modeli Önerisi
- **Önerilen Model:** `text-embedding-nomic-embed-text-v1.5` (Nomic BERT, 84.11 MB) veya `bge-m3` (BAAI).
- **Gerekçe:** 
  - 84 MB ultra hafif boyut, yerel makinede neredeyse sıfır gecikme ile çalışma.
  - 8192 token bağlam uzunluğu (uzun proje açıklamaları ve dokümanları için ideal).
  - Türkçe ve İngilizce dahil 100+ dilde yüksek anlamsal eşleşme performansı.
  - Yerel LM Studio ve Ollama çalışma zamanlarında hazır destek.

### 6.2 Phase 18 — Vektör Saklama (Vector Store) Stratejisi
1. **Kaynak Otoritesi (Source of Truth):** SQL Server, projenin tek ve tartışmasız iş verisi kaynağı olarak kalacaktır.
2. **Değerlendirme:**
   - **Seçenek A (Önerilen — Hibrit / EF Core Uyumlu Vektör İndeksi):** Proje metin parçacıklarını ve embedding vektörlerini SQL Server tablolarında veya yerel gömülü bir vektör eklentisinde tutmak. Ek bir veritabanı sunucusu operasyonel maliyeti oluşturmaz, ACID işlemlerini ve yetkilendirme filtrelerini korur.
   - **Seçenek B (Harici Vektör DB — Qdrant / Milvus):** Büyük ölçekli kurumsal dağıtımda yüzbinlerce doküman seviyesine çıkıldığında değerlendirilebilir; ancak geliştirme fazında operasyonel karmaşıklık getireceğinden Phase 18 için önerilmez.
3. **Karar:** Phase 18'de SQL Server'ı ana veri kaynağı olarak koruyan, hafif ve ilişkisel modele entegre bir vektör indeksleme yapısı uygulanacaktır.

### 6.3 Gelecek RAG (Retrieval-Augmented Generation) Akışı
```
[Proje SQL Verisi] 
       ↓
[Bilgi Dokümanı Oluşturucu (Knowledge Document Builder)]
       ↓
[Parçalama (Chunking)]
       ↓
[Çok Dilli Embedding (nomic-embed-text-v1.5)]
       ↓
[Vektör İndeks & Anlamsal Arama]
       ↓
[Yetki Filtreleme (Authorized Project Retrieval)]
       ↓
[LLM (Yerel / Kurumsal Azure)]
       ↓
[Doğrulanmış Yanıt + Proje Kaynak Referansları (Citations)]
```

---

## 7. Sağlayıcı Değiştirme Stratejisi (Provider Transition Strategy)

Uygulama katmanı tamamen `IAiProvider` arayüzüne bağımlı olduğundan, kurum ileride Azure OpenAI, AWS Bedrock veya şirket içi özel bir API Gateway'e geçmek istediğinde:
1. `Infrastructure/Services/Ai/AzureOpenAiProvider.cs` sınıfı oluşturulur.
2. `appsettings.json` içindeki `AI:Provider` değeri `"AzureOpenAI"` olarak güncellenir.
3. `DependencyInjection.cs` içinde ilgili sağlayıcı kaydedilir.
4. **Application ve API katmanlarında tek bir satır kod bile değiştirilmeden** kurumsal bulut AI altyapısına geçiş sağlanır.
