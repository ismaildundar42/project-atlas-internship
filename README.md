# ğŸŒŸ ProjectAtlas â€” Kurumsal Ar-Ge Proje KÃ¼tÃ¼phanesi & AI Bilgi Havuzu

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.0-61DAFB?style=for-the-badge&logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![EF Core 10](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=for-the-badge&logo=microsoft&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![AI RAG](https://img.shields.io/badge/AI%20RAG-BGE--M3%20%2B%20Qwen-FF6F00?style=for-the-badge&logo=openai&logoColor=white)](https://huggingface.co/BAAI/bge-m3)

> ğŸ“Œ **Staj Projesi Bilgilendirmesi:**  
> Bu proje, **Demir Export A.Å. (KoÃ§ Holding)** bÃ¼nyesindeki YazÄ±lÄ±m MÃ¼hendisliÄŸi staj programÄ± kapsamÄ±nda geliÅŸtirilmiÅŸtir.  
> Bu portfÃ¶y sÃ¼rÃ¼mÃ¼nde kullanÄ±lan tÃ¼m projeler, organizasyonel birimler, metrikler ve dokÃ¼manlar **%100 sentetik (mock/fictional)** verilerden oluÅŸmaktadÄ±r. Åirket iÃ§i canlÄ± sistemlere, gizli kurumsal verilere veya dÄ±ÅŸ kimlik saÄŸlayÄ±cÄ±larÄ±na eriÅŸim iÃ§ermez.

---

## ğŸ“¸ Ekran GÃ¶rÃ¼ntÃ¼leri & GÃ¶rsel Vitrin

> ğŸ’¡ *AÅŸaÄŸÄ±daki ekran gÃ¶rÃ¼ntÃ¼leri projenin gerÃ§ek Ã§alÄ±ÅŸma anÄ±ndaki arayÃ¼z bileÅŸenlerini yansÄ±tmaktadÄ±r.*

<div align="center">

### 1. Ana Sayfa & Proje KeÅŸif Vitrini
![Ana Sayfa Vitrini](docs/screenshots/01-homepage-showcase.png)
*Modern kart tasarÄ±mlarÄ±, geliÅŸmiÅŸ filtreleme, arama ve istatistik Ã¶zet Ã§ubuklarÄ±.*

---

### 2. Proje DetayÄ± & Yerel Yapay Zeka (RAG) AsistanÄ±
![Proje Detay ve Asistan](docs/screenshots/02-project-detail-ai.png)
*KapsamlÄ± teknik/yÃ¶netsel proje detaylarÄ±, entegrasyon ÅŸemalarÄ± ve yerel Qwen/BGE-M3 destekli akÄ±llÄ± asistan widget'Ä±.*

---

### 3. Zengin Proje EditÃ¶rÃ¼ & DokÃ¼man YÃ¶netimi
![Proje EditÃ¶rÃ¼](docs/screenshots/03-project-editor.png)
*Ã‡ok adÄ±mlÄ± form validasyonlarÄ±, kapak gÃ¶rseli seÃ§ici, ekip Ã¼yesi baÄŸlama ve gÃ¼venli dokÃ¼man yÃ¼kleme paneli.*

---

### 4. ModÃ¼l EriÅŸim YÃ¶netimi & Talep Ä°ÅŸ AkÄ±ÅŸÄ±
![ModÃ¼l EriÅŸim Talepleri](docs/screenshots/04-module-access-requests.png)
*Raporlar ve Ekipler modÃ¼llerine Ã¶zel talep oluÅŸturma, onaylama, reddetme ve yetki kaldÄ±rma (revocation) paneli.*

---

### 5. YÃ¶netici Paneli & Denetim GÃ¼nlÃ¼kleri (Audit Logs)
![YÃ¶netici ve Audit Log Paneli](docs/screenshots/05-admin-audit-logs.png)
*Sistem genelindeki tÃ¼m veri mutasyonlarÄ±nÄ±, onay sÃ¼reÃ§lerini ve kullanÄ±cÄ± hareketlerini kaydeden denetim mekanizmasÄ±.*

---

### 6. Ä°nteraktif Raporlama & Analitik Grafikler
![Raporlama ve Analitik](docs/screenshots/06-reports-analytics.png)
*Departman daÄŸÄ±lÄ±mlarÄ±, teknoloji trendleri, bÃ¼tÃ§e/durum analizleri ve SVG/Canvas veri gÃ¶rselleÅŸtirmeleri.*

</div>

---

## ğŸ¯ Proje Vizyonu ve AmacÄ±

BÃ¼yÃ¼k Ã¶lÃ§ekli endÃ¼striyel kuruluÅŸlarda (Madencilik, IoT, AÄŸÄ±r Sanayi, Otomasyon) geliÅŸtirilen yazÄ±lÄ±m, yapay zeka, saha teknolojisi ve Ar-Ge projeleri Ã§oÄŸunlukla departman silolarÄ±nda sÄ±kÄ±ÅŸÄ±p kalmaktadÄ±r.

**ProjectAtlas**, kurum genelinde geliÅŸtirilen tÃ¼m inovasyon ve dijitalleÅŸme projelerini tek bir merkezde toplayarak:
1. **KeÅŸfedilebilirlik:** Teknik bilgisi az olan saha operasyon personelinden Ã¼st dÃ¼zey yÃ¶neticilere kadar herkesin projeleri kolayca incelemesini,
2. **Yapay Zeka Destekli Bilgi Ã‡Ä±karÄ±mÄ± (RAG):** DoÄŸal dille projeler hakkÄ±nda soru sorulabilmesini ve anlamsal (semantik) arama yapÄ±labilmesini,
3. **MÃ¼kerrer Efor Ã–nleme:** BaÅŸka bir sahadaki benzer ihtiyacÄ±n daha Ã¶nce nasÄ±l Ã§Ã¶zÃ¼ldÃ¼ÄŸÃ¼nÃ¼ teknoloji ve mimari etiketleriyle gÃ¶rÃ¼nÃ¼r kÄ±lmayÄ±,
4. **Kurumsal YÃ¶netiÅŸim:** Proje taslaÄŸÄ± oluÅŸturma, yÃ¶netici onayÄ±, modÃ¼l bazlÄ± eriÅŸim kontrolÃ¼ ve denetim loglarÄ±yla tam izlenebilirlik sunmayÄ± saÄŸlar.

---

## ğŸ›ï¸ Mimari TasarÄ±m & KatmanlÄ± YapÄ± (Clean Architecture)

Proje, **Temiz Mimari (Clean Architecture)** ve **Domain-Driven Design (DDD)** ilkeleri gÃ¶zetilerek 4 baÄŸÄ±msÄ±z katmanda inÅŸa edilmiÅŸtir:

```
ProjectAtlas/
â”œâ”€â”€ backend/
â”‚   â”œâ”€â”€ src/
â”‚   â”‚   â”œâ”€â”€ DeUygulamaVitrini.Domain/          # Saf iÅŸ kurallarÄ±, VarlÄ±klar, Enum'lar, Sabitler
â”‚   â”‚   â”œâ”€â”€ DeUygulamaVitrini.Application/     # CQRS/Use-Case servisleri, DTO'lar, ArayÃ¼zler, Excel/Import
â”‚   â”‚   â”œâ”€â”€ DeUygulamaVitrini.Infrastructure/  # EF Core 10, SQL Server, Local AI Provider, GÃ¼venlik
â”‚   â”‚   â””â”€â”€ DeUygulamaVitrini.API/             # REST Controller'lar, Rate Limiter, Middleware'ler
â”‚   â””â”€â”€ tests/
â”‚       â”œâ”€â”€ DeUygulamaVitrini.SecurityTests/   # SEC-001 Yetki ve GÃ¼venlik Regresyon Testleri
â”‚       â”œâ”€â”€ DeUygulamaVitrini.AiTests/         # RAG, Embedding ve Warm-Up Testleri
â”‚       â”œâ”€â”€ DeUygulamaVitrini.ExcelTests/      # Toplu Ä°Ã§e/DÄ±ÅŸa AktarÄ±m DoÄŸrulama Testleri
â”‚       â””â”€â”€ DeUygulamaVitrini.CaptchaTests/    # Birinci Taraf SVG CAPTCHA Testleri
â””â”€â”€ frontend/                                  # React 19 + TypeScript Single Page Application (SPA)
    â”œâ”€â”€ src/
    â”‚   â”œâ”€â”€ components/                        # UI TasarÄ±m Sistemi (Cards, Modals, Forms, Charts)
    â”‚   â”œâ”€â”€ context/                           # Auth, ModuleAccess, Theme, Accessibility Context'leri
    â”‚   â”œâ”€â”€ pages/                             # Sayfa BileÅŸenleri (Vitrin, EditÃ¶r, Admin, Raporlar)
    â”‚   â”œâ”€â”€ services/                          # Axios API Ä°stemcisi & Servis KatmanÄ±
    â”‚   â”œâ”€â”€ styles/                            # CSS DeÄŸiÅŸkenleri, Semantic Token'lar, Light/Dark Temalar
    â”‚   â””â”€â”€ i18n/                              # TÃ¼rkÃ§e / Ä°ngilizce Dil KaynaklarÄ±
    â””â”€â”€ vite.config.ts
```

---

## ğŸ”’ GÃ¼venlik & Yetkilendirme Mimarisi

ProjectAtlas, kurumsal gÃ¼venlik standartlarÄ±na uygun olarak tasarlanmÄ±ÅŸtÄ±r:

### 1. Kimlik DoÄŸrulama & Oturum YÃ¶netimi
- **ASP.NET Core Identity** altyapÄ±sÄ± ile gÃ¼venli parola hash'leme (PBKDF2).
- **HTTP-Only, Secure, SameSite=Lax Cookie (`.DemirExport.Auth`)** oturum yÃ¶netimi.
- Frontend'e kesinlikle hassas token sÄ±zdÄ±rÄ±lmaz; tÃ¼m API istekleri `withCredentials: true` ile taÅŸÄ±nÄ±r.
- REST API uyumlu `OnRedirectToLogin` $\rightarrow$ `401 Unauthorized` ve `OnRedirectToAccessDenied` $\rightarrow$ `403 Forbidden` JSON yanÄ±t mimarisi.

### 2. Ã‡ok Kademeli Rol & Yetki Sistemi (RBAC)
- **SuperAdmin:** TÃ¼m sistem konfigÃ¼rasyonu, kullanÄ±cÄ± yÃ¶netimi, onay sÃ¼reÃ§leri ve modÃ¼l atama yetkisi.
- **Admin:** Proje onaylama/reddetme, organizasyon birimlerini yÃ¶netme, raporlama ve eriÅŸim taleplerini deÄŸerlendirme.
- **Project Creator (`CanCreateProjects = true`):** Yeni proje oluÅŸturma, kendi taslaklarÄ±nÄ± dÃ¼zenleme ve onaya sunma.
- **Standard User (Viewer):** YalnÄ±zca onaylanmÄ±ÅŸ ve yayÄ±ndaki projeleri gÃ¶rÃ¼ntÃ¼leme, AI asistanÄ±nÄ± kullanma, yetkili olmadÄ±ÄŸÄ± modÃ¼ller iÃ§in eriÅŸim talebi oluÅŸturma.

### 3. ModÃ¼l Seviyesinde EriÅŸim KontrolÃ¼ & Talep Ä°ÅŸ AkÄ±ÅŸÄ±
- **Raporlama (`reports`)** ve **Ekipler (`teams`)** modÃ¼lleri hassas operasyonel veriler iÃ§erdiÄŸi iÃ§in kilitlenebilir.
- Yetkisi olmayan kullanÄ±cÄ± girdiÄŸinde ÅŸÄ±k bir eriÅŸim talep modalÄ± aÃ§Ä±lÄ±r; girilen gerekÃ§e yÃ¶neticinin onay kuyruÄŸuna dÃ¼ÅŸer.
- YÃ¶neticiler tek tÄ±kla onaylayabilir, gerekÃ§eli reddedebilir veya verilen yetkiyi geÃ§miÅŸi koruyarak geri alabilir (`Revocation`).

### 4. SertleÅŸtirilmiÅŸ Ã–zel DokÃ¼man GÃ¼venliÄŸi (SEC-001)
- Projeye eklenen teknik ÅŸartname, mimari rapor gibi Ã¶zel dokÃ¼manlar **`wwwroot` dÄ±ÅŸÄ±na (`App_Data/uploads/`)** izole edilmiÅŸtir.
- Statik dosya URL'leri Ã¼zerinden anonim eriÅŸim tamamen engellenmiÅŸtir.
- DokÃ¼manlar yalnÄ±zca yetkisi doÄŸrulanmÄ±ÅŸ kullanÄ±cÄ±lara Ã¶zel streaming endpoint'i (`GET /api/projects/{id}/documents/{docId}/download`) Ã¼zerinden gÃ¼venli baÅŸlÄ±klarla (`X-Content-Type-Options: nosniff`, `Content-Disposition: attachment`) sunulur.
- Dizin geÃ§iÅŸi (Path Traversal - `../../`) saldÄ±rÄ±larÄ±na karÅŸÄ± mutlak dosya yolu denetimi uygulanÄ±r.

### 5. Birinci Taraf SVG CAPTCHA SavunmasÄ±
- ÃœÃ§Ã¼ncÃ¼ parti takipÃ§i veya harici servis baÄŸÄ±mlÄ±lÄ±ÄŸÄ± olmayan, in-memory ve rate-limit korumalÄ± gÃ¶rsel SVG CAPTCHA katmanÄ±.
- Brute-force oturum aÃ§ma giriÅŸimlerine karÅŸÄ± endpoint seviyesinde koruma saÄŸlar.

---

## ğŸ§  Yerel Yapay Zeka & RAG (Retrieval-Augmented Generation)

ProjectAtlas, harici bulut API'lerine ÅŸirket verisi sÄ±zdÄ±rmadan Ã§alÄ±ÅŸabilecek **%100 yerel yapay zeka** entegrasyonuna sahiptir:

```
[KullanÄ±cÄ± DoÄŸal Dil Sorgusu]
              â”‚
              â–¼
[Yerel BGE-M3 Embedding Modeli (1024 Boyut)]
              â”‚
              â–¼ (KosinÃ¼s BenzerliÄŸi / VektÃ¶r Arama)
[MSSQL ProjectKnowledgeChunks VektÃ¶r Ä°ndeksi]
              â”‚
              â–¼ (Ä°lgili Proje BaÄŸlamÄ± + Sistem Promptu)
[Yerel Qwen 3 LLM Ã‡Ä±karÄ±m Motoru]
              â”‚
              â–¼
[Kaynak Proje ReferanslÄ± AkÄ±llÄ± YanÄ±t]
```

- **Hibrit Arama:** BaÅŸlÄ±k, Ã¶zet ve teknik etiketler Ã¼zerinden hem tam metin (lexical) hem de vektÃ¶rel anlamsal (semantic) benzerlik skorlamasÄ±.
- **Otomatik Ä°ndeks MutabakatÄ± (Reconciliation Background Worker):** Arka planda Ã§alÄ±ÅŸan hosted service, yeni eklenen veya gÃ¼ncellenen projelerin vektÃ¶r indeksini otomatik olarak senkronize eder.
- **Cold-Start Warmup Servisi:** Sunucu aÃ§Ä±lÄ±ÅŸÄ±nda AI modelini Ã¶nceden Ä±sÄ±tarak ilk sorgudaki gecikmeyi (cold-start latency) ortadan kaldÄ±rÄ±r.
- **Zarif Geri Ã‡ekilme (Graceful Fallback):** Yerel AI sunucusu kapalÄ± olsa dahi uygulama kesinlikle Ã§Ã¶kmez; klasik filtreleme ve arama moduna kesintisiz devam eder.

---

## ğŸ’» Teknoloji YÄ±ÄŸÄ±nÄ±

| Katman | Teknoloji / KÃ¼tÃ¼phane | AÃ§Ä±klama |
| :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core 10 Web API | YÃ¼ksek performanslÄ±, modern C# 13 RESTful mimari |
| **ORM & VeritabanÄ±** | EF Core 10 + Microsoft SQL Server | Split Queries, Global Soft-Delete filtreleri, Ä°ndekslemeler |
| **Kimlik & GÃ¼venlik** | ASP.NET Core Identity | Cookie Auth, PBKDF2 Hashing, Memory Rate Limiter |
| **Yapay Zeka / LLM** | BGE-M3 + Qwen 3 (OpenAI Compatible) | 1024-dim yerel vektÃ¶r embedding ve RAG soru-cevap |
| **Frontend Framework** | React 19 + TypeScript 5 | Tip gÃ¼venli, bileÅŸen tabanlÄ± modern SPA |
| **Build & Tooling** | Vite 8 + Rollup | Milisaniyeler mertebesinde HMR ve optimize Ã¼retim derlemesi |
| **Durum YÃ¶netimi** | TanStack Query v5 (React Query) | Sunucu durumu Ã¶nbellekleme, arkaplan senkronizasyonu |
| **YÃ¶nlendirme & Ä°konlar** | React Router v7 + Lucide React | GÃ¼venli rota koruyucularÄ± (ProtectedRoute), modern SVG ikonlar |
| **UluslararasÄ±laÅŸtÄ±rma** | i18next + react-i18next | Tam kapsamlÄ± TÃ¼rkÃ§e / Ä°ngilizce dil desteÄŸi |
| **TasarÄ±m & Tema** | Vanilla CSS Tokens | Glassmorphism, HSL renk paletleri, Dinamik AÃ§Ä±k/Koyu Tema |

---

## ğŸš€ HÄ±zlÄ± BaÅŸlangÄ±Ã§ (Quick Start)

### Gereksinimler
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js (v20+)](https://nodejs.org/) & `npm`
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) veya [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) ya da [Docker Desktop](https://www.docker.com/)

---

### AdÄ±m 1: VeritabanÄ±nÄ± BaÅŸlatÄ±n

Proje kÃ¶k dizininde yer alan `docker-compose.yml` ile tek komutla SQL Server baÅŸlatabilirsiniz:

```bash
# SQL Server 2022 Konteynerini BaÅŸlatÄ±n
docker-compose up -d
```

*(Alternatif olarak Visual Studio ile gelen `(localdb)\mssqllocaldb` doÄŸrudan kullanÄ±labilir; `appsettings.Development.json` varsayÄ±lan olarak LocalDB'ye ayarlÄ±dÄ±r).*

---

### AdÄ±m 2: Backend API'yi Ã‡alÄ±ÅŸtÄ±rÄ±n

```bash
cd backend/src/DeUygulamaVitrini.API

# BaÄŸÄ±mlÄ±lÄ±klarÄ± derleyin ve API'yi baÅŸlatÄ±n
# (Ä°lk aÃ§Ä±lÄ±ÅŸta 20 zengin sentetik proje ve referans veriler otomatik tohumlanÄ±r)
dotnet run
```

- API Adresi: `http://localhost:5000`
- Swagger OpenAPI ArayÃ¼zÃ¼: `http://localhost:5000/swagger`

---

### AdÄ±m 3: Frontend UygulamasÄ±nÄ± Ã‡alÄ±ÅŸtÄ±rÄ±n

```bash
cd frontend

# BaÄŸÄ±mlÄ±lÄ±klarÄ± yÃ¼kleyin
npm install

# GeliÅŸtirme sunucusunu baÅŸlatÄ±n
npm run dev
```

- Web ArayÃ¼zÃ¼: `http://localhost:5173`

---

## ğŸ‘¥ Ã–nceden TanÄ±mlÄ± Demo KullanÄ±cÄ± HesaplarÄ±

GeliÅŸtirme ortamÄ±nda veritabanÄ± ilk kez oluÅŸtuÄŸunda aÅŸaÄŸÄ±daki roller ve kullanÄ±cÄ±lar otomatik olarak hazÄ±rlanÄ±r:

| E-posta | Parola | Rol | Yetki KapsamÄ± |
| :--- | :--- | :--- | :--- |
| `admin@demirexport.com` | `AdminPassword123!` | **SuperAdmin** | Sistem genelinde tam yetki, onaylama, kullanÄ±cÄ± ve yetki yÃ¶netimi |
| `creator@demirexport.com` | `CreatorPassword123!` | **Proje GiriÅŸi** | Yeni proje taslaÄŸÄ± oluÅŸturma, kendi projelerini dÃ¼zenleme ve onaya sunma |
| `user@demirexport.com` | `UserPassword123!` | **Standart KullanÄ±cÄ±** | YayÄ±ndaki projeleri inceleme, AI asistanÄ±nÄ± kullanma, modÃ¼l eriÅŸim talebi iletme |
| `readonly@demirexport.com` | `ReadOnlyPassword123!` | **Salt Okunur** | Temel vitrin inceleme yetkisi |

*(GiriÅŸ ekranÄ±nda gÃ¼venli gÃ¶rsel CAPTCHA doÄŸrulamasÄ± aktiftir).*

---

## ğŸ§ª GÃ¼venlik ve Regresyon Testleri

Proje, kritik gÃ¼venlik ve iÅŸ kurallarÄ±nÄ± denetleyen otomatik test paketlerine sahiptir:

```bash
# SEC-001 GÃ¼venlik Testlerini Ã‡alÄ±ÅŸtÄ±rÄ±n (Ä°zole InMemory VeritabanÄ±)
dotnet test backend/tests/DeUygulamaVitrini.SecurityTests
```

**Test KapsamÄ± (25/25 BaÅŸarÄ±lÄ±):**
-  Anonim ve yetkisiz kullanÄ±cÄ±larÄ±n taslak projelere ve dokÃ¼manlara eriÅŸiminin engellenmesi (401/403).
-  Proje sahibi ve Admin kullanÄ±cÄ±larÄ±n taslak dokÃ¼manlara yetkili streaming eriÅŸimi (200 OK).
-  Dizin atlatma (Path Traversal: `../../../appsettings.json`) giriÅŸimlerinin engellenmesi.
-  MIME tipi ve `X-Content-Type-Options: nosniff` baÅŸlÄ±k doÄŸrulamalarÄ±.
-  Yetkisiz IDOR (farklÄ± projeye ait dokÃ¼man ID'si ile Ã§aÄŸÄ±rma) korumasÄ± (404 NotFound).

---

## ğŸ–¼ï¸ Ekran GÃ¶rÃ¼ntÃ¼leri Ekleme Rehberi (GeliÅŸtirici Notu)

README dosyasÄ±ndaki gÃ¶rsel yerleÅŸimlerini tamamlamak iÃ§in tarayÄ±cÄ±nÄ±zda uygulamayÄ± Ã§alÄ±ÅŸtÄ±rÄ±p aÅŸaÄŸÄ±daki ekran gÃ¶rÃ¼ntÃ¼lerini alarak `docs/screenshots/` klasÃ¶rÃ¼ne aynÄ± isimlerle kaydedebilirsiniz:

1. `01-homepage-showcase.png` $\rightarrow$ Ana sayfa filtreler ve proje kartlarÄ± vitrini.
2. `02-project-detail-ai.png` $\rightarrow$ Herhangi bir projenin detay sayfasÄ± ve saÄŸ altta aÃ§Ä±k olan AI Proje AsistanÄ± penceresi.
3. `03-project-editor.png` $\rightarrow$ `/admin/projects/new` veya dÃ¼zenleme ekranÄ±ndaki Ã§ok sekmeli form arayÃ¼zÃ¼.
4. `04-module-access-requests.png` $\rightarrow$ `/admin/module-access` yetki onaylama ve talep listesi ekranÄ±.
5. `05-admin-audit-logs.png` $\rightarrow$ `/admin/audit-logs` iÅŸlem geÃ§miÅŸi ve denetim kayÄ±tlarÄ± tablosu.
6. `06-reports-analytics.png` $\rightarrow$ `/reports` sayfasÄ±ndaki grafikler, donut chart ve daÄŸÄ±lÄ±m istatistikleri.

---

## ğŸ‘¨â€ğŸ’» GeliÅŸtirici & TeÅŸekkÃ¼r

- **GeliÅŸtirici:** Ä°smail DÃ¼ndar
- **Kurum:** Demir Export A.Å. â€” KoÃ§ Holding
- **Program:** YazÄ±lÄ±m MÃ¼hendisliÄŸi Staj ProgramÄ±

*Demir Export Ar-Ge ve Dijital DÃ¶nÃ¼ÅŸÃ¼m ekibine staj sÃ¼recindeki destek ve rehberlikleri iÃ§in teÅŸekkÃ¼r ederim.*