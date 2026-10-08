# Yetkilendirme ve Proje Erişim Mimarisi (Phase 13.2)

## 1. Genel Bakış

Demir Export Proje Kütüphanesi yetkilendirme modeli sade, güvenli ve kurumsal organizasyon yapısıyla bütünleşiktir:

1. **Kimlik Doğrulama (Authentication):**
   * Tamamen **ASP.NET Core Identity Cookie** (`.DemirExport.Auth`) mekanizması ile çalışır (JWT kullanılmaz).
   * Tarayıcı çerezleri `HttpOnly`, `SameSite=Lax` ve güvenli olarak taşınır.

2. **Yetkilendirme Seviyeleri (Authorization):**
   * **Yönetici (Admin):** Uygulamanın tüm alanlarını (projeler, organizasyon yönetimi, kişi yetkilendirmeleri) tam yetkiyle yönetir.
   * **Proje Giriş Yetkisine Sahip Kullanıcı (`CanCreateProjects = true`):** Yönetim paneline erişebilir, yeni proje oluşturabilir ve **yalnızca kendi oluşturduğu projeleri** düzenleyebilir.
   * **Normal Kullanıcı:** Yönetim paneline erişemez; proje kütüphanesini salt okunur olarak görüntüler.

---

## 2. Model Ayrımı ve İlişki: `Member` ↔ `ApplicationUser`

* **`Member` (Domain Entity):** Şirket içi kurumsal çalışan metaverisidir (Ad, Soyad, Unvan, E-posta, Proje Ekip Katılımları). Organizasyon Yönetimi → Kişiler ekranında listelenir.
* **`ApplicationUser` (Identity Entity):** Sisteme giriş yapan kimlik doğrulama hesabıdır (Kullanıcı adı, şifre karması, `IsActive`, `CanCreateProjects`).
* **İlişki (`Member.UserId`):**
  * `Member` üzerinde nullable `UserId` (`int?`) dış anahtarı bulunur.
  * Bire-bir (1:1) opsiyonel ilişki tanımlıdır ve unique index ile korunur.
  * **Her `Member`'ın bir `ApplicationUser` hesabı olması gerekmez.** Yalnızca sisteme giriş yapması ve yetki verilmesi istenen kişiler hesaba bağlanır.

---

## 3. Yönetici Deneyimi (Admin UX: Organizasyon → Kişiler)

Yetki yönetimi ayrı bir kullanıcı yönetim sayfasında değil, doğrudan **Organizasyon → Kişiler** ekranından yürütülür:

1. **Kişiler Tablosu:**
   * "Proje Yönetimi" sütununda her kişinin durumu açık rozetlerle gösterilir:
     * **Yetkili (Yeşil):** Bağlı hesabı var ve `CanCreateProjects = true`.
     * **Yetkisiz (Gri):** Bağlı hesabı var ancak `CanCreateProjects = false`.
     * **Hesap Yok (Nötr):** Kişiye ait bağlı bir giriş hesabı bulunmuyor.
     * **Pasif Hesap (Turuncu):** Bağlı hesap pasif durumdadır.
2. **Kişi Düzenleme / Uygulama Erişimi:**
   * **Bağlı Hesabı Olan Kişi:** Admin *"Proje ekleyebilir ve kendi projelerini düzenleyebilir"* anahtarını açıp kapatabilir. Değişiklik doğrudan `ApplicationUser.CanCreateProjects` alanına yazılır.
   * **Hesabı Olmayan Kişi:** Admin *"Uygulama Hesabı Oluştur"* butonuna tıklayarak başlangıç şifresi belirleyip tek adımda Identity hesabı oluşturabilir ve kişiye bağlayabilir. Eğer aynı e-posta ile mevcut bağımsız bir hesap varsa sistem *"Mevcut Hesabı Bağla"* seçeneğini sunar.

---

## 4. Sahiplik ve Yetki Kuralları

1. **Proje Sahipliği (`Project.CreatedByUserId`):**
   * Yeni bir proje oluşturulduğunda backend sunucu tarafında `Project.CreatedByUserId = CurrentUserId` atamasını otomatik olarak yapar.
   * İstemci bu alanı manipüle edemez veya taklit edemez.
2. **Proje Düzenleme Kuralı:**
   * Kullanıcı **Admin** ise -> Tüm projeleri düzenleyebilir.
   * Kullanıcı **Admin değilse** -> `CanCreateProjects == true` VE `Project.CreatedByUserId == CurrentUserId` koşulu aranır. Başka birinin projesini düzenlemeye çalıştığında sunucu **403 Forbidden** döner.
3. **Yayınlama ve Arşivleme Kuralı:**
   * Projeleri yayına alma (`IsPublished = true`), yayından kaldırma, arşivleme ve geri yükleme yetkisi **yalnızca Admin** kullanıcılara aittir.
4. **Yetki Geri Alma (`Revoke`):**
   * Admin yetkiyi kaldırdığında (`CanCreateProjects = false`), kullanıcı yönetim paneli erişimini ve düzenleme hakkını anında kaybeder.
   * Geçmişte oluşturduğu projeler silinmez; `CreatedByUserId` geçmiş sahiplik kaydı bozulmadan kalır.
5. **Silme Güvenliği:**
   * Bağlı giriş hesabı olan veya mevcut projelerde görev alan `Member` kayıtları doğrudan silinemez (koruma altındadır).

---

## 5. Yetki Matrisi

| İşlem / Alan | Admin | Proje Girişi Yetkilisi | Normal Kullanıcı |
| :--- | :---: | :---: | :---: |
| **Kütüphaneyi ve Projeleri İnceleme** | Evet | Evet | Evet |
| **Yönetim Paneli / Projeler (`/admin/projects`)** | Evet | Evet (Yalnızca kendi projeleri) | Hayır |
| **Yeni Proje Oluşturma** | Evet | Evet | Hayır |
| **Kendi Oluşturduğu Projeyi Düzenleme** | Evet | Evet | Hayır |
| **Başkasına Ait Projeyi Düzenleme** | Evet | Hayır | Hayır |
| **Projeyi Yayınlama / Yayından Kaldırma** | Evet | Hayır | Hayır |
| **Projeyi Arşivleme / Geri Yükleme** | Evet | Hayır | Hayır |
| **Organizasyon Yönetimi (Ekip, Departman, Kişiler)** | Evet | Hayır | Hayır |
| **Kişiye Proje Giriş İzni Verme / Hesap Bağlama** | Evet | Hayır | Hayır |

---

## 6. Geliştirme Test Hesapları

| Kullanıcı | E-Posta | Bağlı Kişi (Member) | Yetki Durumu |
| :--- | :--- | :--- | :--- |
| **Yönetici** | `admin@demirexport.com` | Admin (Bağımsız Yönetici) | Admin (Tam yetkili) |
| **Proje Girişi Yetkilisi** | `creator@demirexport.com` | Ayşe Kaya | Proje Girişi Yetkili (`CanCreateProjects = true`) |
| **Normal Kullanıcı** | `user@demirexport.com` | Zeynep Şahin | Standart Kullanıcı (`CanCreateProjects = false`) |

> [!NOTE]
> Geliştirme ortamı hesap şifreleri dokümantasyon güvenliği gereği yerel ortam yapılandırmasında ve `DevelopmentDataSeeder` kaynak kodunda yönetilir. Dokümantasyonda düz metin şifre saklanmaz.

