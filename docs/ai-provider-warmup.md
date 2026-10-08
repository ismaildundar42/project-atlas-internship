# Provider-Agnostic AI Cold-Start Warm-Up Layer

## 1. Problem ve Cold-Start Nedir?
Modern yapay zeka çıkarım (inference) ve gömme (embedding) runtime'ları (örn. LM Studio, Ollama, ONNX Runtime veya Azure AI Dedicated/Serverless endpoint'leri), bilgisayar veya model ilk açıldığında belleğe yüklenmemiş (cold) durumdadır.

İlk kullanıcı isteği geldiğinde:
- Model ağırlıkları diskten GPU/CPU VRAM'e aktarılır,
- Model bağlamı (context/KV cache) ilklendirilir,
- İlk çıkarım veya gömme hesaplaması gerçekleşir.

Bu durum ilk istekte belirgin bir cold-start gecikmesine yol açar:
- **İlk İstek (Cold):** ~23 saniye
- **Sonraki İstekler (Warm):** ~8-9 saniye

## 2. Neden Warm-Up Yaptık?
Warm-up mekanizması modelin temel çalışma hızını değiştirmeyi değil, **bu ilk gecikme maliyetini kullanıcıdan önce, uygulamanın arka plan yaşam döngüsü sırasında (background lifecycle) absorbe etmeyi** hedefler.

Kullanıcı arayüze gelip ilk AI özetini veya asistan sorgusunu çalıştırmadan önce sistem arka planda minimal bir warmup çağrısı yaparak modeli warm duruma geçirir.

> **Önemli İlke:** Warm-up cold-start maliyetini ortadan kaldırmaz. Cold-start maliyetini mümkün olduğunca kullanıcı isteğinden önce, application background lifecycle sırasında öder.

---

## 3. Mimari ve Bağımlılık Akışı (Architecture)

Warm-up katmanı tamamen **Provider-Agnostic** olarak tasarlanmıştır. Belirli bir local model adı, localhost portu, LM Studio veya Azure OpenAI SDK'sına doğrudan bağımlılığı **yoktur**.

```
ASP.NET Core Web API (Program.cs)
       │
       ▼
AiWarmupBackgroundService (IHostedService, BackgroundService)
       │ (Küçük Configurable Delay: örn. 5s)
       ▼
IAiWarmupService (AiWarmupService)
       ├── IAiProvider (GenerateAsync)
       └── IEmbeddingProvider (GenerateEmbeddingAsync)
```

- **Bugün (Local Development):**
  `IAiProvider` -> `LocalAiProvider` (LM Studio / local inference)
  `IEmbeddingProvider` -> `LocalEmbeddingProvider` (BGE-M3 embedding)
- **Gelecekte (Azure / Corporate Cloud Provider):**
  `IAiProvider` -> `AzureAiProvider` / Kurumsal API
  `IEmbeddingProvider` -> `AzureEmbeddingProvider` / Kurumsal API

Kodda hiçbir değişiklik yapılması gerekmez; aynı `IAiWarmupService` arayüzler üzerinden çalışmaya devam eder.

---

## 4. Warm-Up Operasyonları

### A. Generation Warm-Up
- **Hedef:** Metin üretim (LLM) modelini warm hale getirmek.
- **Prompt:** `"Reply only with OK."`
- **Max Tokens:** `8` (minimum token tüketimi, düşük gecikme).
- **Gizlilik/Güvenlik:** Şirket verisi, proje bilgisi, kullanıcı kimliği veya confidential veri kesinlikle içermez.

### B. Embedding Warm-Up
- **Hedef:** Vektör gömme modelini warm hale getirmek.
- **Girdi:** `"warmup"` (`EmbeddingType.Document`)
- **Semantic Index / DB İzolasyonu:** Dönen vektör semantic index'e (`ProjectKnowledgeChunks`) **kesinlikle yazılmaz**, veritabanında hiçbir kayıt üretilmez veya değiştirilmez. Tamamen side-effect free bir operasyondur.

---

## 5. Yapılandırma (Configuration)

`appsettings.json` altındaki `AI:Warmup` bölümünden yönetilir:

```json
{
  "AI": {
    "Warmup": {
      "Enabled": true,
      "DelaySeconds": 5,
      "MaxGenerationTokens": 8
    }
  }
}
```

- `Enabled` (`bool`, default: `true`): Warm-up'ın çalışıp çalışmayacağını belirler. Gelecekte daima warm olan bir Azure servisine geçildiğinde veya warm-up istenmediğinde `false` yapılarak tamamen devre dışı bırakılabilir.
- `DelaySeconds` (`int`, default: `5`): Uygulama ayağa kalktıktan sonra ana web sunucusu işlemlerinin kilitlenmemesi için beklenen süre.
- `MaxGenerationTokens` (`int`, default: `8`): Generation warm-up için ayrılan maksimum yanıt token bütçesi.

---

## 6. Hata Toleransı ve Fail-Open Davranışı (Resilience)

Warm-up bir **best-effort arka plan optimizasyonudur**.

- AI runtime veya Azure endpoint'i kapalıysa,
- Model bulunamazsa veya timeout oluşursa,
- Ağ hatası alınırsa:

**Uygulama çökmeyecek, startup bloklanmayacak ve AI kalıcı olarak devre dışı bırakılmayacaktır.**
Warm-up servisi hatayı `ILogger` üzerinden warning olarak loglar ve arka plan görevini temiz bir şekilde tamamlar. Kullanıcı daha sonra bir istek attığında normal provider akışı olağan şekilde çalışmaya devam eder.

---

## 7. Gözlemlenebilirlik ve Loglama (Observability)

Süreler `Stopwatch` ile hassas şekilde ölçülür. Hassas veri veya prompt metinleri loglara yazılmaz:

```
[Information] AI warm-up background service scheduled to run in 5 seconds.
[Information] AI warm-up started.
[Information] AI generation warm-up completed in 1420 ms.
[Information] AI embedding warm-up completed in 310 ms.
[Information] AI warm-up completed successfully in 1735 ms.
```

Hata durumunda:
```
[Warning] AI generation warm-up failed: Connection refused.
```

---

## 8. Multi-Instance Azure Davranışı

Azure App Service veya Container Apps üzerinde birden fazla instance (scale-out) çalıştığında:
- **Her instance kendi startup'ında bağımsız olarak per-instance warm-up yürütür.**
- Bu istekler son derece küçük (8 token generation + 1 kelime embedding) olduğu için paylaşılan arka uçlarda ihmal edilebilir bir yük oluşturur.
- Dağıtık bir koordinatöre (distributed lock/coordinator) ihtiyaç duyulmaz.

---

## 9. Manuel Kabul Testi Prosedürü (Manual Acceptance Test)

### Test A — Baseline Cold
1. AI runtime / LM Studio servisini yeniden başlatın (modelleri bellekten boşaltın).
2. `appsettings.json` içinde `AI:Warmup:Enabled = false` yapın.
3. Backend uygulamasını başlatın.
4. Tarayıcıdan bir projenin "AI Özeti" butonuna tıklayın.
5. Süreyi ölçün (Beklenen: ~20-25 saniye cold-start).

### Test B — Automatic Warmup
1. AI runtime / LM Studio modellerini tekrar cold duruma getirin (veya runtime'ı yeniden başlatın).
2. `appsettings.json` içinde `AI:Warmup:Enabled = true` yapın.
3. Backend uygulamasını başlatın.
4. Konsolda `AI warm-up completed successfully in ... ms` logunu görün.
5. Tarayıcıdan ilk kullanıcı AI Özeti butonuna tıklayın.
6. Süreyi ölçün (Beklenen: ~8-10 saniye warm-state yanıtı).

### Test C — Provider Down (Fail-Open Testi)
1. AI runtime'ı tamamen kapatın.
2. Uygulamayı başlatın.
3. Warm-up warning logunun düştüğünü, uygulamanın sorunsuz açıldığını, kimlik doğrulama ve proje listeleme işlevlerinin eksiksiz çalıştığını doğrulayın.

---

## 10. Warm-Up'ın Çözmediği Sınırlar (Observed Limitations)
1. **İşlem Karmaşıklığı Süresi:** RAG arama, embedding mesafe hesaplaması ve uzun yanıt üretme gibi asıl iş yükünün kendi işlem süresini kısaltmaz; yalnızca ilk model/runtime yükleme ek yükünü ortadan kaldırır.
2. **Kullanıcı Erken İsteği:** Eğer kullanıcı uygulama ayağa kalkar kalkmaz ilk 5 saniye içinde (warm-up henüz tamamlanmadan) bir istek yaparsa, o istek kısmi cold-start gecikmesine maruz kalabilir.
3. **Bellekten Atılma (Eviction):** Yerel sistem veya cloud altyapısı uzun süreli hareketsizlik sonrasında modeli RAM/VRAM'den tahliye ederse (idle eviction), sonraki istekte yeniden cold-start oluşabilir. (Bu durum periyodik scheduler gerektirir, ancak mevcut faz kapsamı gereği gereksiz karmaşıklıktan kaçınılmıştır).
