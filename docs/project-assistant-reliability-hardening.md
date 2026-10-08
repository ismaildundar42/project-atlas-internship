# Project Assistant Reliability Hardening — Teknik Dokümantasyon

**Tarih:** 7 Ekim 2026  
**Proje:** Demir Export Proje Havuzu (DeUygulamaVitrini)  
**Kapsam:** Project Assistant Dayanıklılık & Güvenilirlik Sertleştirme (Reliability Hardening)

---

## 1. Problem Tanımı (Problem Statement)

Project Hub bünyesinde yer alan Proje Asistanı (Project Assistant), kullanıcıların portföydeki projelere dair doğal dil veya yapılandırılmış sorgular ile bilgi edinmesini sağlayan temel etkileşim katmanıdır. 

Sistem dondurulma (feature-freeze) aşamasına gelmiş olmasına rağmen, gerçek kullanımda iki kritik güvenilirlik zaafı tespit edilmiştir:
1. **Case A (False Empty - Yanıltıcı Boş Yanıt):** Kullanıcı "SAP teknolojisi kullanılan projeler nelerdir?" diye sorduğunda, veritabanında "Kurumsal SAP ERP ve Saha Üretim Sistemleri Entegrasyonu" gibi erişilebilir projeler bulunmasına ve Global Arama'da listelenmesine rağmen asistan *"Belirtilen kriterlere uygun yetkili bir proje bulunamadı."* yanıtı dönmekteydi.
2. **Case B (Hard Assistant Failure - LLM Hata Kırılganlığı):** "Kestirimci bakım alanında hangi projelerimiz var?" gibi semantik arama ile kanıt toplanan sorgularda, LLM üretim adımı (`IAiProvider.GenerateAsync`) zaman aşımı veya sağlayıcı hatası verdiğinde, arka planda erişilebilir 2 proje ve kanıt parçacıkları hazır olmasına rağmen asistan çökmekte ve kullanıcıya *"Proje Asistanı şu anda kullanılamıyor. Lütfen biraz sonra tekrar deneyin."* (503 Service Unavailable) hatası dönmekteydi.

---

## 2. Kök Neden Analizi (Root Cause Analysis)

### Case A: SAP Yanıltıcı Boş Sonuç (False Empty)
- **Tespit Edilen Yol:** Yapılandırılmış Sorgu Yorumlayıcısı (`StructuredQueryInterpreter`), "SAP" kelimesini regex üzerinden bir `TechnologyKeyword` olarak eşleştirdi (`TechnologyIntent`).
- **Veri Uyuşmazlığı:** `Technologies` lookup tablosunda "SAP" adında bağımsız bir teknoloji kaydı veya `ProjectTechnology` ilişkisi yoktu; ancak "SAP" anahtar kelimesi projenin `Name`, `ShortDescription`, `Description`, `TechnicalDescription` ve `ProjectIntegrations` alanlarında yoğun olarak geçmekteydi.
- **Tıkanma Noktası:** `ProjectAssistantService`, yapılandırılmış teknoloji filtresi 0 sonuç döndüğünde sorguyu "Çözümlenememiş / Yetersiz" (Unresolved) olarak sınıflandırmak yerine "Kesin Boş Sonuç" kabul edip işlemi sonlandırıyordu. Semantik RAG ve metin tabanlı erişilebilir arama fallback hattı tetiklenmiyordu.

### Case B: LLM Üretim Hatasında Çökme (Hard Assistant Failure)
- **Tespit Edilen Yol:** Semantik RAG Pipeline başarıyla çalışıyor, embedding üretimi ve kosinüs benzerliği eşleşmesi sonucunda 2 adet yetkilendirilmiş proje kanıtı (`ProjectKnowledgeChunk` & kaynak projeler) başarıyla çekiliyordu.
- **Tıkanma Noktası:** `IAiProvider.GenerateAsync` çağrısı başarısız olduğunda veya istisna fırlattığında `ProjectAssistantService`, toplanan kanıtları ve kaynak proje kartlarını tamamen çöpe atarak doğrudan `GenerationProviderUnavailableException` fırlatıyordu.
- **Sonuç:** LLM üretimi, projenin tek hata noktası (Single Point of Failure - SPOF) haline gelmişti.

---

## 3. Eski ve Yeni Mimari Karşılaştırması

### Önceki Mimari (Previous Execution Model)
```
[Kullanıcı Sorgusu]
       │
       ▼
[Structured Interpreter] ──(Teknoloji/Kategori Bulundu)──► [DB Filtreleme] ──► (0 Sonuç) ──► "Proje Bulunamadı" (HATALI BOŞ)
       │ (Bulunamadı)
       ▼
[Embedding + Vektör Arama]
       │
       ▼
[Yetkilendirilmiş Kanıtlar]
       │
       ▼
[LLM Üretim (GenerateAsync)] ──(HATA)──► Exception fırlat ──► 503 "Asistan Kullanılamıyor" (HATALI ÇÖKME)
       │ (BAŞARI)
       ▼
[Zengin RAG Yanıtı]
```

### Yeni Dayanıklı Yürütme Modeli (Revised Execution Model)
```
                           KULLANICI SORGUSU
                                  │
                                  ▼
                         Sorgu Analizi & Yorumlama
                                  │
               ┌──────────────────┴──────────────────┐
               │                                     │
               ▼                                     ▼
        YAPILANDIRILMIŞ YOL                    SEMANTİK RAG YOLU
               │                                     │
               ├─► Eşleşti (Matched)                 │
               │   (Sonuç > 0) ────────┐             │
               │                       │             │
               └─► Çözümlenemedi       │             │
                   (Unresolved, 0) ────┼────────────►│
                                       │             ▼
                                       │      Yetkilendirilmiş Kanıt Toplama
                                       │      (Vektör + Çok Alanlı Metin Arama)
                                       │             │
                                       │      ┌──────┴──────┐
                                       │      │             │
                                       │      ▼             ▼
                                       │   Kanıt Var     Kanıt Yok (True Empty)
                                       │      │             │
                                       │      ▼             ▼
                                       │   LLM Üretim   "Kriterlere uygun
                                       │   Girişimi      proje bulunamadı."
                                       │      │
                                       │   ┌──┴──┐
                                       │   │     │
                                       │   ▼     ▼
                                       │ Başarı Hata
                                       │   │     │
                                       │   │     ▼
                                       │   │  DETERMİNİSTİK FALLBACK (Seviye 3)
                                       │   │  - Kaynak proje kartları korunur
                                       │   │  - Açıklayıcı durum mesajı eklenir
                                       │   │     │
                                       └───┼─────┘
                                           ▼
                                   KULLANICI YANITI
```

---

## 4. Seviyelendirilmiş Yanıt & Fallback Stratejisi (Fallback Hierarchy)

1. **Seviye 1 (Yapılandırılmış Deterministik Yanıt):**
   - Kategori, Teknoloji vb. filtre kriteri güvenle çözümlenip en az 1 yetkili proje ile eşleşirse LLM'e ihtiyaç duymadan doğrudan deterministik yanıt üretilir (`Matched`).
2. **Seviye 2 (Semantik Arama + Zenginleştirilmiş RAG Yanıtı):**
   - Yapılandırılmış filtreleme çözümlenemediğinde (`Unresolved`) veya semantik sorgularda vektör benzerliği ve çok alanlı metin araması ile yetkilendirilmiş kanıtlar toplanır. LLM başarıyla çalıştırılarak zengin yanıt ve kaynak alıntıları sunulur.
3. **Seviye 3 (Deterministik Proje Fallback):**
   - Yetkilendirilmiş proje kanıtları mevcut ancak LLM üretim adımı başarısız olduysa; toplanan projeler atılmaz. Proje kartları korunarak kullanıcıya *"X ile ilişkili erişebildiğiniz N proje bulundu. AI tarafından ayrıntılı açıklama şu anda oluşturulamadı."* şeklinde deterministik ve kullanışlı yanıt dönülür.
4. **Seviye 4 (Gerçek Boş Sonuç - True Empty):**
   - Yapılan semantik, metin ve ilişkisel aramalarda kullanıcının erişebileceği hiçbir proje kanıtı bulunamadığında dürüstçe boş sonuç dönülür (`NoEvidenceFound`).
5. **Seviye 5 (Altyapı Hizmet Dışı - Generic Unavailable):**
   - Yalnızca veritabanı veya yetkilendirme katmanına ulaşılamadığı gerçek altyapı çöküşlerinde 503 dönülür.

---

## 5. Yetkilendirme & Güvenlik Garantileri (Authorization Guarantees)

- **Önce Yetkilendirme Kuralı:** Asistan asla önce tüm kanıtları toplayıp sonra LLM'e verip filtrelemez.
- `ProjectKnowledgeChunks` ve `Projects` tabloları arasında doğrudan LINQ Join uygulanarak kullanıcının okuma yetkisi olmayan projeler (`IsPublished == false`, `ApprovalStatus != Approved`, veya `CreatedByUserId != currentUserId`) henüz veritabanı sorgulama aşamasında elenir.
- Fallback kartlarında veya RAG bağlamında yetkisiz hiçbir proje adı, açıklaması veya metaverisi sızdırılmaz.

---

## 6. Gözlemlenebilirlik (Observability)

Sorgu yürütme sürecinin arka planda izlenebilmesi için `ILogger` üzerinden yapılandırılmış loglama ve DTO seviyesinde teşhis metaverileri sağlanmıştır:
- `AssistantPath`: `StructuredQuery`, `SemanticRag`, `SemanticFallback`, `DirectSearchFallback`
- `StructuredResolution`: `Matched`, `Unresolved`, `Skipped`
- `CandidateCount`, `AuthorizedProjectCount`, `RetrievedChunkCount`
- `GenerationAttempted`, `GenerationSucceeded`
- `FallbackReason`: `None`, `NoEvidence`, `GenerationUnavailableWithEvidence`, `EmbeddingUnavailable`

---

## 7. Test Database Fail-Fast Güvenlik Koruması

Geliştirme veritabanındaki 20 adetlik özenle hazırlanmış demo veri setinin testler tarafından kirletilmesini veya silinmesini engellemek amacıyla test altyapısında bağlantı dizesi doğrulama koruması (Fail-Fast Guard) teyit edilmiş ve `Test 22.0` altında otomatik teste bağlanmıştır:
- Test ortamında `DeUygulamaVitriniDb` doğrudan hedef alınamaz; yalnızca `_Test` son eki içeren izole test veritabanı veya InMemory sağlayıcı kabul edilir.

---

## 8. Test Sonuçları & Kalite Kapıları (Build & Test Gates)

| Test Süiti | Önceki Durum | Yeni Durum | Başarısız |
|---|---|---|---|
| **AiTests** | 202 | **210** | 0 |
| **CaptchaTests** | 41 | **41** | 0 |
| **ExcelTests** | 182 | **182** | 0 |
| **TOPLAM** | **425** | **433** | **0** |

- **Backend Build:** 0 Hata, 0 Uyarı (`dotnet build backend/DeUygulamaVitrini.sln --no-incremental`)
- **Frontend Build:** 0 Hata (`tsc -b && vite build`)
- **Frontend Lint:** 0 Hata (`npm run lint`)
- **Veritabanı Proje Sayısı:** Öncesi: 22 (20 demo + 2 manuel test), Sonrası: 22 (Sıfır mutasyon).
- **Migration/Şema Değişikliği:** 0.

---

## 9. Değiştirilen Dosyalar

1. `backend/src/DeUygulamaVitrini.Infrastructure/Services/Ai/ProjectAssistantService.cs` (Dayanıklı çok seviyeli pipeline, fallback, LINQ joins, metin arama)
2. `backend/src/DeUygulamaVitrini.Application/DTOs/Ai/ProjectAssistantDto.cs` (Teşhis ve fallback metaverileri)
3. `backend/tests/DeUygulamaVitrini.AiTests/Program.cs` (Phase 22 güvenilirlik ve regresyon testleri 22.0 - 22.8)
4. `frontend/src/types/assistant.ts` (Metadata arayüz alanları)
5. `frontend/src/components/search/GlobalSearch.tsx` (Lokalizasyon anahtarı düzeltmesi)
6. `frontend/src/i18n/resources/tr/projects.ts` & `en/projects.ts` (`resultsCount` ve arama lokalizasyon metinleri)

---

## 10. Bilinen Kısıtlamalar (Known Limitations)

- AI Project Summary ve Cold-Start performans yapılandırmaları yönerge doğrultusunda kapsam dışı tutulmuş ve mevcut stabil durumları korunmuştur.
- Proje Asistanı genel bir sohbet botu değil; Demir Export Proje Havuzu bilgi tabanına odaklı yetkilendirme-farkındalıklı bir kurumsal asistandır.
