# Demir Export Proje Kütüphanesi — API Tasarım Dokümanı (Phase 3)

Bu doküman, Demir Export Proje Kütüphanesi uygulaması için geliştirilen **Application Katmanı ve Read API** kontratlarını, endpoint'leri, filtreleme kurallarını, sayfalama ve hata yapısını açıklar.

---

## 1. Mimarî Yapı ve Veri Akışı

API katmanı temiz, katmanlı bir mimarî izler. Entity nesneleri doğrudan istemciye dönülmez; LINQ projeksiyonları (`Select`) ile DTO modellerine dönüştürülür.

```
HTTP İsteği
    ↓
ProjectsController / LookupsController
    ↓
IProjectService / ILookupService
    ↓
IApplicationDbContext (AsNoTracking)
    ↓
EF Core 9
    ↓
SQL Server (LocalDB)
```

---

## 2. Endpoint Listesi

### Proje Endpoint'leri (`ProjectsController`)

| HTTP Metodu | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/projects` | Filtreli, aramalı, sıralamalı ve sayfalanmış yayınlanmış proje listesi. |
| `GET` | `/api/projects/{id}` | Kimlik numarasına (integer) göre proje detay görünümü. |
| `GET` | `/api/projects/slug/{slug}` | SEO dostu slug ifadesine göre proje detay görünümü. |

### Referans / Sözlük Endpoint'leri (`LookupsController`)

| HTTP Metodu | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/project-statuses` | Tüm proje durumları (PLANNING, ACTIVE vb.). |
| `GET` | `/api/project-categories` | Tüm proje kategorileri (SOFTWARE, IOT vb.). |
| `GET` | `/api/technologies` | Kullanılan tüm teknolojiler ve kategorileri. |
| `GET` | `/api/locations` | Tüm maden sahaları ve lokasyonlar. |
| `GET` | `/api/teams` | Tüm Ar-Ge ve saha ekipleri. |
| `GET` | `/api/tags` | Tüm proje etiketleri. |

---

## 3. Sayfalama ve Kontrat (Pagination Contract)

`GET /api/projects` endpoint'i `PagedResult<T>` yapısını kullanır.

### İstek Parametreleri
- `pageNumber` (int, varsayılan: `1`, minimum: `1`)
- `pageSize` (int, varsayılan: `12`, maksimum: `100`)

### Örnek Yanıt Yapısı
```json
{
  "items": [ ... ],
  "pageNumber": 1,
  "pageSize": 12,
  "totalCount": 6,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

## 4. Filtreleme ve Arama (Filtering & Search)

### Desteklenen Filtreler
- `search`: Metin araması. Proje Adı (`Name`), Kısa Açıklama (`ShortDescription`) ve Detaylı Açıklama (`Description`) alanlarında harf büyüklüğüne duyarsız (case-insensitive) çalışır.
- `status`: Proje durum kodu (Örn: `active`, `completed`). Case-insensitive karşılaştırılır.
- `category`: Proje kategori kodu (Örn: `software`, `data_analytics`). Case-insensitive karşılaştırılır.
- `developmentType`: Geliştirme tipi (`Internal`, `External`, `Hybrid`).
- `teamId`: Belirli bir ekibe ait projeler.
- `locationId`: Belirli bir lokasyonda yürütülen projeler.
- `technologyId`: Belirli bir teknolojiyi kullanan projeler.
- `tagId`: Belirli bir etikete sahip projeler.
- `isFeatured`: Öne çıkan projeler (`true` / `false`).

### Örnek Sorgular
- `GET /api/projects?search=drone`
- `GET /api/projects?status=active&category=software`
- `GET /api/projects?technologyId=5&pageNumber=1&pageSize=12`

---

## 5. Sıralama (Sorting Allow-List)

Sistemi SQL injection ve geçersiz kolon sorgularından korumak için sıkı bir **allow-list** uygulanmaktadır:

- **İzin Verilen Sıralama Alanları (`sortBy`)**: `name`, `createdAt`, `updatedAt`, `startDate`.
- **Varsayılan Sıralama**: `updatedAt DESC` (Son güncellenen projeler en üstte).
- **Sıralama Yönü (`sortDirection`)**: `asc` veya `desc`.
- **Hatalı İzin Verilmeyen İstenim**: Geçersiz bir `sortBy` değeri gönderildiğinde API `400 Bad Request` ProblemDetails döndürür.

---

## 6. Yayınlanmış Proje Kuralı (Published Project Rule)

Public read endpoint'leri (`/api/projects`, `/api/projects/{id}`, `/api/projects/slug/{slug}`) yalnızca aşağıdaki şartı sağlayan projeleri döndürür:

```csharp
IsPublished == true AND IsDeleted == false
```

`IsDeleted == false` kontrolü EF Core **Global Query Filter** ile otomatik uygulanır. `IsPublished == true` kontrolü ise Application katmanındaki `ProjectService` sorgusunda açıkça uygulanır. Taslak (`IsPublished = false`) veya silinmiş (`IsDeleted = true`) kayıtlar `404 Not Found` döndürür.

---

## 7. Hata Yönetimi Strategy (ProblemDetails)

Sistemdeki tüm beklenmeyen durumlar ve validation hataları ASP.NET Core `IExceptionHandler` middleware'i ile yakalanır ve **RFC 7807 ProblemDetails** formatında yanıtlanır.

### Örnek 404 Not Found Yanıtı
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Proje Bulunamadı",
  "status": 404,
  "detail": "Slug değeri 'olmayan-proje' olan yayınlanmış bir proje bulunamadı.",
  "instance": "/api/projects/slug/olmayan-proje"
}
```

### Örnek 400 Bad Request Yanıtı
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Geçersiz Parametre / İstek",
  "status": 400,
  "detail": "Geçersiz sortBy parametresi: 'hataliKolon'. İzin verilen alanlar: name, createdAt, updatedAt, startDate.",
  "instance": "/api/projects"
}
```

---

## 8. Development Data Seeding

Yalnızca `Development` ortamında ve veritabanında henüz proje yoksa çalışır (`DevelopmentDataSeeder.cs`). 

Sisteme yüklenen örnek veriler tamamen kurgusal Ar-Ge projeleridir:
1. **Saha Veri Takip Sistemi** (Yazılım, Aktif, Kurum İçi)
2. **Drone Jeofizik Analizi** (Madencilik Teknolojisi, Aktif, Hibrit)
3. **Enerji İzleme Platformu** (IoT, Aktif, Kurum İçi)
4. **Entegre Lojistik Çözümleri** (Veri Analitiği, Tamamlandı, Dış Tedarik)
5. **Akıllı Bakım Tahmin Sistemi** (Yapay Zeka, Aktif Geliştirme, Kurum İçi)
6. **Operasyon Analitik Platformu** (Veri Analitiği, Aktif, Hibrit)
