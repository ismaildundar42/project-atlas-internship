# First-Party CAPTCHA Challenge Security Layer — Architecture & Integration Report

**Tarih:** 05.10.2026  
**Durum:** PASS (Tamamlandı, 0 Uyarı, 0 Hata, 407/407 Test Başarılı)  
**Hedef Platform:** ASP.NET Core 10 (.NET 10.0.12, win-x64) + React / Vite / TypeScript

---

## 1. Neden Birinci Taraf (First-Party) CAPTCHA Tercih Edildi?

Demir Export Proje Kütüphanesi kurumsal bir bilgi ve Ar-Ge sistemidir. Üçüncü taraf CAPTCHA sağlayıcıları (Google reCAPTCHA, Cloudflare Turnstile vb.) yerine bağımsız, birinci taraf (first-party) bir doğrulama katmanı tasarlanmıştır. Bu tercihin nedenleri:

1. **Kurumsal Veri Gizliliği & Egemenlik:** Kullanıcı IP'si, tarayıcı izi veya kullanıcı hareketleri hiçbir harici üçüncü taraf servisle paylaşılmaz.
2. **Kimlik Sağlayıcı Bağımsızlığı (Auth-Provider Agnostic):** Mevcut ASP.NET Core Identity sistemi ileride Active Directory (AD), LDAP, Microsoft Entra ID veya kurumsal SAML/OIDC SSO ile değiştirildiğinde CAPTCHA mimarisinin baştan yazılması gerekmez.
3. **Azure & Çoklu Platform Desteği:** Windows-only `System.Drawing` bağımlılıklarından kaçınılarak, %100 C# ve cross-platform SVG tabanlı hafif ve yüksek çözünürlüklü görsel üretim mimarisi benimsenmiştir.
4. **Hafif ve Kesintisiz Kullanıcı Deneyimi:** Ağ gecikmesi, harici CDN yükleme süreleri ve iframe uyumsuzlukları ortadan kaldırılmıştır.

---

## 2. Güvenlik Mimarisi & Sorumluluk Ayrımı (Separation of Concerns)

```
[ Tarayıcı / Login UI ]
         |
         | (1) GET /api/auth/captcha
         v
[ AuthController ] ────> [ ICaptchaService ] ────> [ ICaptchaChallengeStore ]
                               |                         |
                               v                         v
                       [ CaptchaGenerator ]      [ MemoryCaptchaChallengeStore ]
                       [ CaptchaVisualRenderer ] (Gelecekte: DistributedStore)
         |
         | (2) POST /api/auth/login { Email, Password, CaptchaChallengeId, CaptchaAnswer }
         v
[ AuthController ] 
         │
         ├─── (A) ICaptchaService.ValidateChallengeAsync(...)
         │         ├─ Geçersiz/Süresi Dolmuş ──> [ 400 Bad Request (ProblemDetails) ]
         │         │                             [ Token Anında Tüketilir ]
         │         └─ Geçerli ──> [ Devam Et ]
         │
         └─── (B) Kimlik Doğrulama Sağlayıcısı (Authentication Provider)
                   │
                   ├── [ GÜNCEL ]: ASP.NET Core Identity (SignInManager / PasswordSignInAsync)
                   └── [ GELECEK ]: Kurumsal AD / LDAP / Microsoft Entra ID / SSO
```

### Temel Güvenlik İlkeleri:
1. **Düz Metin (Plaintext) İfşa Yasağı:** CAPTCHA metni hiçbir API DTO'sunda, HTTP başlığında, çerezde, HTML kodunda, DOM'da veya istemci durumunda yer almaz.
2. **Kriptografik Özetleme (Keyed SHA-256 Hashing):** Sunucu hafızasında yalnızca `SHA256(NormalizedAnswer + ":" + ChallengeId + ":" + InternalSalt)` tutulur.
3. **Sabit Zamanlı Karşılaştırma (Timing Attack Koruması):** `CryptographicOperations.FixedTimeEquals` ile zamanlama saldırıları engellenir.
4. **Tek Kullanımlık Semantik (Single-Use & Replay Attack Defense):** İster başarılı olsun ister başarısız, bir `ChallengeId` doğrulama denemesine girdiği an `ConsumeChallengeAsync` ile bellekten atomik olarak silinir. Aynı jetonla ikinci kez tahmin yapılamaz.
5. **Kullanıcı Numaralandırma (User Enumeration) Önleme:** Hatalı parola veya bulunamayan e-posta durumunda e-postanın varlığını ifşa etmeyen genel `"E-posta adresi veya parola hatalı."` mesajı döndürülür.
6. **Hız Sınırlama (Rate Limiting):** ASP.NET Core dahili `Microsoft.AspNetCore.RateLimiting` middleware'i ile `/api/auth/captcha` endpoint'ine IP bazlı dakikada 60 istek sınırı konulmuştur.

---

## 3. Kod Üretimi & Karışıklık Önleyici Alfabe

- **Karakter Seti (Safe Alphabet):** `2345679ACDEFGHJKLMNPQRSTUVWXYZ` (29 karakter).
  - Görsel benzerlik sebebiyle yanlış okunan `0 / O / o`, `1 / I / l`, `8 / B` karakterleri kasten ayıklanmıştır.
- **Rastgelelik:** `System.Security.Cryptography.RandomNumberGenerator.GetInt32` kullanılır.
- **Uzunluk:** Varsayılan 5 karakter (`CaptchaOptions.Length = 5`).
- **Geçerlilik Süresi (TTL):** Varsayılan 3 dakika (`CaptchaOptions.ExpirationMinutes = 3`).
- **Büyük/Küçük Harf Duyarlılığı:** Kullanıcı kolaylığı için varsayılan büyük/küçük harf duyarsızdır (`CaseSensitive = false`), girdiler `.Trim().ToUpperInvariant()` ile normalize edilir.

---

## 4. Sunucu Taraflı Görsel Üretimi (Cross-Platform SVG)

- Harici ikili bağımlılık (native libgdiplus / Skia binary) gerektirmeyen saf C# SVG renderer:
  - `viewBox="0 0 200 58"`
  - Koyu kurumsal arka plan gradyanı (`#08101d` → `#0d1b2e`) ve mikro ızgara çizgileri
  - Rastgele 28 adet opaklık dağıtımlı arka plan gürültü noktası (noise particles)
  - 3 adet dinamik çok noktalı kübik Bezier parazit eğrisi
  - Her bir karaktere özel açısal döndürme (-14° ile +14° arası), dikey basamaklama ve renk tonu varyasyonu
  - Çıktı: `data:image/svg+xml;base64,...` formatında Data URL.

---

## 5. Uygulama Katmanı & Soyutlamalar

| Katman | Dosya / Arayüz | Sorumluluk |
| :--- | :--- | :--- |
| **Application** | `ICaptchaService` | Meydan okuma üretme ve yanıtlama servis arayüzü |
| **Application** | `ICaptchaChallengeStore` | Bellek/Dağıtık oturum saklama ve atomik tüketim arayüzü |
| **Application** | `CaptchaModels.cs` | `CaptchaChallengeData`, `CaptchaChallengeResponseDto`, `CaptchaValidationResult`, `CaptchaOptions` |
| **Application** | `LoginRequestDto.cs` | `CaptchaChallengeId` ve `CaptchaAnswer` alanları ile genişletildi |
| **Infrastructure** | `MemoryCaptchaChallengeStore` | `IMemoryCache` tabanlı, `ConcurrentDictionary` kilit korumalı tek kullanımlık depo |
| **Infrastructure** | `CaptchaGenerator` | Kriptografik güvenli kod ve SHA256 özet üretim motoru |
| **Infrastructure** | `CaptchaVisualRenderer` | Cross-platform SVG ve Base64 Data URL oluşturucu |
| **Infrastructure** | `CaptchaService` | Yaşam döngüsü, TTL ve tek kullanımlık kontrol orkestrasyonu |
| **API** | `AuthController.cs` | `GET /api/auth/captcha` (NoStore, RateLimited) ve `POST /api/auth/login` entegrasyonu |

---

## 6. Login UI & Kullanıcı Deneyimi Entegrasyonu

- **Bileşen Hiyerarşisi:**
  1. Kurumsal E-posta Adresi (`Mail` ikonu)
  2. Parola (`Lock` ikonu + `Eye`/`EyeOff` görünürlük butonu)
  3. **Güvenlik Doğrulama Kodu:**
     - Sol tarafta kurumsal temalı SVG görsel alanı (`.login-page__captcha-visual-wrap`)
     - Sağ tarafta yenileme butonu (`RotateCw` animasyonlu ikon)
     - Altında 5 karakterli monospaced alfanümerik metin giriş kutusu (`ShieldCheck` ikonu)
  4. Giriş Yap Butonu (`Button`)
  5. Proje Kütüphanesine Dön Linki

- **Hata ve Yenileme Dinamikleri:**
  - Hatalı CAPTCHA girildiğinde veya parola yanlış olduğunda parola ve e-posta silinmez; yalnızca CAPTCHA cevabı temizlenir ve arka planda yeni bir CAPTCHA üretilir.
  - Yenile butonuna tıklandığında yalnızca CAPTCHA alanı güncellenir, form verileri korunur.
  - Açık/Koyu tema (`ThemeContext`) ve TR/EN (`LanguageSwitcher`) ile %100 uyumludur.

---

## 7. Azure ve Çoklu-Örnek (Multi-Instance) Hazırlığı

- Mevcut local ve tekil sunucu ortamı için `MemoryCaptchaChallengeStore` geliştirilmiştir.
- `ICaptchaChallengeStore` mimari arayüzü sayesinde, Azure App Service üzerinde birden fazla instance çalıştığında kod tabanında veya `AuthController`'da hiçbir değişiklik yapmadan `DistributedCaptchaChallengeStore` (Azure Cache for Redis / IDistributedCache) sınıfı DI katmanına eklenebilir.

---

## 8. Otomasyon & Test Sonuçları

| Test Paketi | Test Sayısı | Başarı |
| :--- | :--- | :--- |
| **`DeUygulamaVitrini.CaptchaTests`** | 41 | **41 / 41 (%100)** |
| **`DeUygulamaVitrini.AiTests`** | 184 | **184 / 184 (%100)** |
| **`DeUygulamaVitrini.ExcelTests`** | 182 | **182 / 182 (%100)** |
| **Toplam Otomasyon Testleri** | **407** | **407 / 407 (%100)** |
| **Backend Derleme** | `dotnet build` | **0 Hata, 0 Uyarı** |
| **Frontend Derleme** | `npm run build` | **0 Hata, Başarılı** |
