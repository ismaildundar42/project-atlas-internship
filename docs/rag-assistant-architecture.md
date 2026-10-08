# Demir Export Proje Kütüphanesi — Hibrit & RAG Proje Asistanı Mimarisi (Phase 19.5)

Bu doküman, **Demir Export — Proje Kütüphanesi (Project Hub)** kurumsal yapay zeka asistanı olan **Proje Asistanı (Project Library Assistant)**'nın hibrit sorgu yönlendirme mimarisini, deterministik veritabanı işlemlerini, konuşma takip ve düzeltme mekanizmasını, sunucu kontrollü sistem bilgisini, güvenlik sınırlarını ve alıntı doğrulama akışını detaylandırmaktadır.

---

## 1. Altın Kural (The Golden Rule)

> [!IMPORTANT]
> **USE THE CHEAPEST CORRECT EXECUTION PATH (En Ucuz ve Doğru Yürütme Yolunu Kullan).**
> Doğal dilde yazılmış olması bir sorunun mutlaka LLM / Üretici Yapay Zeka ile yanıtlanması gerektiği anlamına gelmez.
> Kullanıcının isteği veritabanındaki yapısal alanlarla (tarih, durum, lokasyon, teknoloji, kategori, ekip, sayı vb.) kesin, güvenli ve tam olarak karşılanabiliyorsa deterministik SQL kullanılır.
> RAG ve LLM (Qwen 3B) yalnızca serbest metin anlamsal çıkarımı veya doğal dilde sentez gerektiğinde devreye girer.

---

## 2. Uçtan Uca Hibrit Asistan Mimarisi (Phase 19.5 Flow)

```
                                KULLANICI SORUSU (TR / EN)
                                            |
                                            v
                             +-----------------------------+
                             |   PROJE SORGU AYRIŞTIRICI   |
                             |  (ProjectQueryInterpreter)  |
                             +-----------------------------+
                                            |
         +------------------+---------------+------------------+------------------+
         |                  |                                  |                  |
         v                  v                                  v                  v
 [1. HIZLI / SİSTEM] [2. YAPISAL PROJE SORGUSU]         [3. HİBRİT SORGU]   [4. ANLAMSAL RAG]
   • Selamlama         • "Son 5 proje"                    • "Kangal sahasında • "Kestirimci bakımla
   • Asistan Kimliği   • "Kangal'daki aktif projeler"       kestirimci bakım    ilgili hangi
   • Asistan Amacı     • "Python kullanan projeler"         projeleri"          çalışmalar var?"
   • Kütüphane Amacı   • "Kaç yapay zeka projesi var?"         |                  |
   • Yetenekler        • "5 dedim ama 3 getirdin"              v                  v
   • Kapsam Dışı       • "Bunlardan Kangal'dakiler"      SQL Ön Filtreleme      BGE-M3
         |                  |                            (Loc = Kangal)     Vektör Arama
         |                  v                                  |                  |
         |           SQL SERVER VERİTABANI                     v                  v
         |           (Pre-Generation Auth +             BGE-M3 Vektör         Yetkilendirme
         |            Deterministic Query)              Arama & Sıralama         Filtresi
         |                  |                                  |                  |
         |                  |                                  v                  v
         |                  |                            Qwen 3B Sentez     Qwen 3B Üretim
         |                  |                                  |                  |
         |                  |                                  v                  v
         |                  |                           Relevance Filter   Kalite Koruması
         |                  |                                  |                  |
         +------------------+----------------------------------+------------------+
                                            |
                                            v
                                 SONUÇ VE ALINTI KARTLARI
                                (Doğal Yanıt + Proje Kartları)
```

---

## 3. Beş Yürütme Yolu (5 Execution Paths)

| Yürütme Yolu (`ExecutionPath`) | Tetiklenme Şartı | Kullanılan Motor | Tipik Yanıt Süresi | Embedding Çağrısı | LLM Çağrısı |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`FastPath` / `SystemKnowledge`** | Selamlama, asistan kimliği, kütüphane amacı, kapsam dışı veya öznel sorular. | Sunucu Sabit Bilgi Tabanı | **0–4 ms** | **0** | **0** |
| **`StructuredQuery`** | En son/en yeni, durum, lokasyon, teknoloji, kategori, ekip, kombinasyon veya adet (count) sorguları. | SQL Server (EF Core + Pre-Gen Auth) | **10–50 ms** | **0** | **0** |
| **`ConversationFollowUp`** | Önceki sonuç kümesi üzerinde filtreleme veya adet düzeltmesi (*"Bunlardan Kangal'da olanlar"*, *"5 dedim"*). | SQL Server (Oturum `ReferencedProjectIds`) | **10–50 ms** | **0** | **0** |
| **`HybridQuery`** | Yapısal kısıt ile serbest anlamsal konunun birleşimi (*"Kangal sahasında kestirimci bakım"*). | SQL Ön Kısıt + BGE-M3 + Qwen 3B | **2.5–6.0 s** | **1** | **1** |
| **`SemanticRag`** | Tam metinsel anlamsal arama ve akıl yürütme gerektiren sorular (*"Arızaları önceden tahmin eden sistemler"*). | BGE-M3 (1024D) + Qwen 3B + Quality Guard | **3.5–12.0 s** | **1** | **1** |

---

## 4. Kesin Güvenlik Kuralı: LLM Asla SQL Üretmez

> [!CAUTION]
> **Qwen2.5-3B veya harici herhangi bir LLM modeline veritabanı şeması verilip SQL ürettirilmez.**
> Tüm SQL sorguları sunucu tarafında C# kodunda, tip güvenli `StructuredProjectQuery` modeli ve Entity Framework Core LINQ ifadeleri ile güvenli biçimde derlenir.

### Güvenli İstem Modeli (`StructuredProjectQuery`):
```csharp
public sealed record StructuredProjectQuery
{
    public int Limit { get; init; } = 5;
    public string SortField { get; init; } = "CreatedAt"; // CreatedAt, UpdatedAt, Name
    public string SortDirection { get; init; } = "Desc";   // Desc, Asc
    public string? StatusKeyword { get; init; }
    public string? CategoryKeyword { get; init; }
    public string? LocationKeyword { get; init; }
    public string? TechnologyKeyword { get; init; }
    public string? TeamKeyword { get; init; }
    public bool? IsFeatured { get; init; }
    public bool IsCountOnly { get; init; }
    public string? SemanticTopic { get; init; }
    public bool IsFollowUpFilter { get; init; }
    public bool IsCorrection { get; init; }
}
```

---

## 5. Yetkilendirme Değişmez Kuralı (Authorization Invariant)

Deterministik SQL veya Yapısal sorgu yolu hiçbir koşulda kullanıcı yetki sınırlarını aşamaz:
1. Normal kullanıcılar yalnızca **yayımlanmış (`IsPublished = true`) ve onaylanmış (`ApprovalStatus = Approved`)** projeleri veya kendilerine ait taslak projeleri görebilir.
2. Silinmiş (`IsDeleted = true`) kayıtlar hiçbir kullanıcıya (Admin dahil) gösterilmez.
3. Kullanıcı *"Son 5 taslak projeyi göster"* yazsa dahi yetkisiz taslaklar sunucu katmanında filtrelenir.

---

## 6. Soru Ayrıştırma ve Niyet Öncelik Hiyerarşisi

`ProjectQueryInterpreter` ve `ProjectAssistantService` sorguları şu katı öncelik sırasına göre sınıflandırır:

```
1. Kullanıcı Düzeltmesi (Correction: "5 dedim ama 3 tane getirdin")
      ↓
2. Önceki Sonuç Takibi (Follow-Up: "Bunlardan Kangal'da olanlar")
      ↓
3. Hibrit Sorgu (Yapısal Kısıt + Anlamsal Konu: "Kangal'da kestirimci bakım")
      ↓
4. Deterministik Yapısal Sorgu (Recency, Limit, Status, Tech, Loc, Cat, Team, Count)
      ↓
5. Kapsam Dışı / Öznel Sorular (OutOfDomain, Subjective)
      ↓
6. Sistem Bilgisi (AssistantPurpose, ProjectHubPurpose, Identity, Capability)
      ↓
7. Sosyal / Nezaket / Selamlama (Social, Courtesy, Greeting)
      ↓
8. Varsayılan Anlamsal Proje Bilgisi (SemanticRag)
```

> [!NOTE]
> **Karma İstek Örneği:** *"Merhaba, son 5 projeyi göster"* veya *"Sen ne yapabiliyorsun ve son 5 projeyi göster"* isteklerinde, somut yapısal istek (`StructuredQuery`) selamlama veya yetenek niyetini ezerek doğrudan deterministik SQL'e yönlendirilir.

---

## 7. Sonuç Adedi ve Sayım Davranışı

1. **İstenen Limit ile Eşleşme:**
   - Kullanıcı açıkça *"5 proje"* istediğinde ve en az 5 yetkili proje varsa tam olarak 5 alıntı kartı döner.
2. **Kriterlere Uyan Daha Az Kayıt Olduğunda:**
   - Kullanıcı 10 istediğinde ancak 6 kayıt varsa, asistan yanıltıcı davranmaz:
     - **TR:** *"Bu kriterlere uyan 6 proje bulundu:"*
     - **EN:** *"6 projects matched these criteria:"*
   - Alıntı kartı sayısı tam olarak 6 olur.
3. **Adet (Count) Sorguları:**
   - *"Kangal'da kaç aktif proje var?"* sorgusunda `IsCountOnly = true` olur.
   - Doğrudan `SELECT COUNT(*)` ile veritabanından sayı hesaplanır.
   - Yanıt: *"Kangal lokasyonundaki Canlıda durumunda 3 proje bulunmaktadır."*
   - Alıntı kartı listesi boş bırakılır (`Citations.Count == 0`), gereksiz yükleme yapılmaz.

---

## 8. Performans Karşılaştırması

| Senaryo | Phase 19.4 Öncesi (Salt RAG) | Phase 19.5 (Hibrit & Deterministik) | İyileşme Oranı |
| :--- | :--- | :--- | :--- |
| *"En son eklenen 5 projeyi getir"* | ~15.3 sn (BGE-M3 + Qwen 3B + 3 alıntı hatası) | **14 ms** (0 embedding, 0 LLM, tam 5 alıntı) | **~1000x Hızlanma** |
| *"Kangal'da kaç aktif proje var?"* | ~12.8 sn (LLM halüsinasyonu ve eksik sayım) | **8 ms** (Doğrudan SQL COUNT, kesin sayı) | **~1500x Hızlanma** |
| *"Bunlardan Kangal'da olanlar"* | ~14.1 sn (Bağlam kopukluğu, alakasız yanıt) | **11 ms** (Oturum sonuç kümesi filtreleme) | **~1200x Hızlanma** |
| *"Kestirimci bakım sistemleri"* | ~8.4 sn (BGE-M3 + Qwen 3B RAG) | **~8.4 sn** (RAG gerektiğinde korunur) | **Doğal Semantik Korundu** |
