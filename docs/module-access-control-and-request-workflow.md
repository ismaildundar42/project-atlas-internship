# Modül Düzeyinde Erişim Kontrolü ve Erişim Talepleri İş Akışı (Module Access Control & Request Workflow)

**Proje:** DeUygulamaVitrini / Demir Export Proje Kütüphanesi (Project Hub)  
**Tarih:** 2026-10-07  
**Sürüm:** 1.0.0 (Enterprise Authorization Extension — LDAP / AD / Entra-Ready Design)

---

## 1. İş Gereksinimi (Business Requirement)

Demir Export Proje Kütüphanesi platformunda standart kimlik doğrulama yapmış normal kullanıcılar ile yönetici kullanıcılar bulunmaktadır. Bazı kurumsal analitik ve organizasyonel modüller (Raporlar ve Ekipler), tüm kullanıcılara varsayılan olarak açık olmamalı; kontrollü, talep edilebilir ve denetlenebilir bir yetkilendirme modeliyle yönetilmelidir.

Kullanıcıların bu modüllerin varlığını bilmesi, ancak erişimi yoksa kilitli olarak görüp sistem üzerinden gerekçeli erişim talebi (access request) iletebilmesi ve yöneticilerin bu talepleri onaylayıp/reddedebilmesi kurumsal bir zorunluluktur.

---

## 2. Mevcut Yetkilendirme Mimarisi (Existing Authorization Architecture)

Proje Kütüphanesi'nde mevcut kaynak kodla doğrulanmış mimari katmanlar:

1. **Global Roller (ASP.NET Core Identity):**
   - `SuperAdmin`: Sistemdeki tüm yetkilere, rol atamalarına ve tam denetime sahiptir.
   - `Admin`: Proje yönetimi, kullanıcı yönetimi, organizasyon yönetimi ve onay süreçlerine sahiptir.
   - Rol atanmamış kullanıcılar: Standart oturum açmış kullanıcılardır (Normal Authenticated User).

2. **Kabiliyetler (Capabilities):**
   - `ApplicationUser.CanCreateProjects`: Proje oluşturabilme yeteneğidir. Global admin rolünden bağımsızdır.

3. **Proje Düzeyi Sahiplik:**
   - `Project.CreatedByUserId`: Proje oluşturan kullanıcı kendi taslak (Draft) projelerini görebilir ve düzenleyebilir.
   - Onaylanan ve yayınlanan projeler ise tüm kullanıcılara açıktır.

---

## 3. Yeni Modül Erişim Mimarisi (New Module Access Architecture)

Mevcut RBAC ve kabiliyet modelini bozmadan, üzerine modül düzeyinde bağımsız bir erişim katmanı inşa edilmiştir:

```
+-------------------------------------------------------------------------+
|                              KULLANICI                                   |
+-------------------------------------------------------------------------+
       |                                              |
       v                                              v
+-----------------------------+        +----------------------------------+
| Global Roller & Kabiliyetler |        |  Modül Erişim Yetkilendirmesi   |
| - SuperAdmin                |        |  (IModuleAccessService)          |
| - Admin                     |        +----------------------------------+
| - CanCreateProjects         |                       |
+-----------------------------+                       |
                                                      v
                                        +----------------------------------+
                                        | Varsayılan Modüller:             |
                                        | - Dashboard (Ana Sayfa)          |
                                        | - Projects (Proje Kütüphanesi)   |
                                        |                                  |
                                        | Kısıtlı / Talep Edilebilir:      |
                                        | - Reports (Raporlar)             |
                                        | - Teams (Ekipler)                |
                                        +----------------------------------+
```

---

## 4. Varlık Modeli (Entity Model)

Yeni eklenen domain varlıkları ve veri tabanı ilişkileri:

### 4.1. `UserModulePermission`
Kullanıcıya atanmış aktif modül izinlerini temsil eder.

- `Id` (int, PK)
- `UserId` (int, FK -> `AspNetUsers.Id`, DeleteBehavior.Restrict)
- `Module` (ApplicationModule enum: 1=Reports, 2=Teams)
- `GrantedByUserId` (int, FK -> `AspNetUsers.Id`, DeleteBehavior.Restrict)
- `GrantedAt` (DateTime, UTC)

**Kısıt:** `(UserId, Module)` bileşik anahtarı üzerinde **Unique Index** bulunmaktadır. Mükerrer izin satırı oluşturulamaz.

### 4.2. `ModuleAccessRequest`
Kullanıcıların yaptığı erişim taleplerinin yaşam döngüsünü ve geçmişini tutar.

- `Id` (int, PK)
- `RequestedByUserId` (int, FK -> `AspNetUsers.Id`, DeleteBehavior.Restrict)
- `Module` (ApplicationModule enum: 1=Reports, 2=Teams)
- `Reason` (nvarchar(1000), opsiyonel talep gerekçesi)
- `Status` (AccessRequestStatus enum: 1=Pending, 2=Approved, 3=Rejected)
- `RequestedAt` (DateTime, UTC)
- `ReviewedByUserId` (int?, Nullable FK -> `AspNetUsers.Id`, DeleteBehavior.Restrict)
- `ReviewedAt` (DateTime?, Nullable UTC)
- `ReviewNote` (nvarchar(1000), opsiyonel yönetici inceleme notu)

**İndeksler:**
- `(RequestedByUserId, Module, Status)`
- `Status`

---

## 5. Erişim Talebi Yaşam Döngüsü (Access Request Lifecycle)

```
[Normal Kullanıcı]                                   [Yönetici]
        |                                                 |
        |--- 1. POST /api/module-access/requests -------->|
        |    (Status: Pending)                            |
        |    - AuditLog: ModuleAccessRequested            |
        |    - Bildirim: Yöneticilere iletildi            |
        |                                                 |
        |                   [İnceleme]                    |
        |                        |                        |
        |         +--------------+--------------+         |
        |         |                             |         |
        |      [ONAY]                        [RET]        |
        |         v                             v         |
        |-- 2a. Status: Approved         2b. Status: Rejected
        |   - UserModulePermission           - İzin verilmez
        |     kaydı oluşturulur              - AuditLog: Rejected
        |   - AuditLog: Approved             - Bildirim: Reddedildi
        |   - Bildirim: Onaylandı               |
        |   - Modül kullanıma açılır            |
        |                                       v
        |<------------------------- [Yeniden Talep Yapılabilir]
```

---

## 6. Efektif Yetkilendirme Kuralları (Effective Authorization Rules)

`IModuleAccessService.CanAccessModuleAsync(userId, isAdmin, module)` merkezi yetkilendirme motoru şu kuralları işletir:

1. **SuperAdmin & Admin:** Her zaman `TRUE` (Örtük/implicit erişim, izin tablosunda satıra gerek yoktur).
2. **Aktiflik Kontrolü:** Kullanıcının `IsActive == false` olması durumunda izin satırı olsa dahi her zaman `FALSE`.
3. **Varsayılan Modüller:** `Dashboard` ve `ProjectLibrary` her zaman `TRUE`.
4. **Normal Kullanıcı:** `UserModulePermissions` tablosunda `(UserId, Module)` eşleşmesi varsa `TRUE`, aksi takdirde `FALSE`.

---

## 7. Admin / SuperAdmin Davranışı

- Admin ve SuperAdmin kullanıcılar için veritabanında gereksiz `UserModulePermission` satırları oluşturulmaz.
- Rol yetkisi sayesinde otomatik olarak tüm modüllere erişirler.
- Adminler `AdminModuleAccessController` ve frontend'deki "Erişim Talepleri" yönetim panelinden bekleyen tüm talepleri listeleyebilir, onaylayabilir veya reddedebilir.

---

## 8. `CanCreateProjects` Bağımsızlığı

- `CanCreateProjects = true` olan bir kullanıcı, `Reports` veya `Teams` iznine otomatik olarak sahip **olmaz**.
- `Reports` iznine sahip olan bir kullanıcı, `CanCreateProjects` veya proje yönetimi haklarına sahip **olmaz**.
- İki yetki boyutu tamamen ortogonal ve bağımsızdır.

---

## 9. Backend Güvenlik Sınırı (Backend Enforcement)

Frontend'deki kilitli görünüm yalnızca bir UX unsurudur; yetkilendirmenin nihai güvenlik sınırı backend API katmanıdır:

- `GET /api/reports/overview`: `CanAccessModuleAsync(userId, isAdmin, Reports)` kontrolü başarısız ise **HTTP 403 Forbidden** döner.
- `GET /api/teams/summary`: `CanAccessModuleAsync(userId, isAdmin, Teams)` kontrolü başarısız ise **HTTP 403 Forbidden** döner.
- `GET /api/teams`: `CanAccessModuleAsync(userId, isAdmin, Teams)` kontrolü başarısız ise **HTTP 403 Forbidden** döner.
- `GET /api/teams/{id}`: `CanAccessModuleAsync(userId, isAdmin, Teams)` kontrolü başarısız ise **HTTP 403 Forbidden** döner.

---

## 10. Frontend Kullanıcı Deneyimi (Frontend UX)

1. **Sidebar / Navigasyon:**
   - Kullanıcının erişimi olmayan `Raporlar` ve `Ekipler` menü öğeleri gizlenmez.
   - Yanında kilit simgesi (`Lock`) ve hafif mutelenmiş ancak WCAG standartlarına uygun kontrastta gösterilir.
2. **Talep Modalı (AccessRequestModal):**
   - Kilitli modüle tıklandığında korumalı sayfaya yönlenmez; erişim talep modalı açılır.
   - Kullanıcı gerekçe yazarak "Erişim Talebi Gönder" butonuna basar.
3. **Bekleme Durumu (Pending):**
   - Talep gönderildikten sonra sidebar menüsünde "Talep Bekliyor" rozeti görüntülenir. Mükerrer talep gönderimi engellenir.
4. **Route Guard (ProtectedRoute):**
   - URL üzerinden doğrudan `/reports` veya `/teams` adresine girmeye çalışan yetkisiz kullanıcıya kilitli erişim ekranı gösterilir.

---

## 11. Bildirimler (Notifications)

Mevcut `INotificationService` altyapısı kullanılmıştır:
- Yeni talep oluşturulduğunda: Tüm Admin ve SuperAdmin kullanıcılarına bildirim gider.
- Talep onaylandığında: Talep sahibine "Erişim Talebiniz Onaylandı" bildirimi ve modül linki gider.
- Talep reddedildiğinde: Talep sahibine "Erişim Talebiniz Reddedildi" bildirimi ve yönetici notu gider.
- Yetki iptal edildiğinde: Kullanıcıya "Modül Erişim Yetkisi Kaldırıldı" bildirimi gider.

---

## 12. Denetim İzi (Audit Logging)

Mevcut `IAuditLogService` altyapısı kullanılarak şu olaylar denetim günlüğüne kaydedilir:
- `ModuleAccessRequested`
- `ModuleAccessApproved`
- `ModuleAccessRejected`
- `ModuleAccessRevoked`
- `ModuleAccessGranted`

---

## 13. Yetki İptali (Revocation) ve Yeniden Talep

- Yöneticiler bir kullanıcının mevcut modül iznini diledikleri zaman iptal edebilir (`DELETE /api/admin/module-access/users/{userId}/revoke/{module}`).
- Yetki iptal edildiğinde geçmiş `ModuleAccessRequest` kayıtları **silinmez**, denetim geçmişi korunur.
- Yetkisi alınan kullanıcı, modüle tekrar erişim ihtiyacı duyduğunda **yeni bir talep** (Status: Pending) oluşturabilir.

---

## 14. Eşzamanlılık ve Mükerrer Kayıt Koruması (Concurrency Protection)

- Çift tıklama veya paralel isteklerle mükerrer `Pending` talep açılması uygulama katmanında ve veritabanı indekslerinde engellenir.
- Bir talep onaylandığında `UserModulePermissions` tablosundaki `(UserId, Module)` unique constraint sayesinde mükerrer izin satırı oluşamaz.
- Daha önce onaylanmış veya reddedilmiş bir talep ikinci kez onaylanamaz/reddedilemez (`InvalidOperationException: Bu talep zaten sonuçlandırılmıştır`).

---

## 15. Veritabanı Geçişi (Database Migration)

- Migration Adı: `20261007144500_AddModuleAccessControlAndRequests`
- Etkilenen Tablolar: `UserModulePermissions`, `ModuleAccessRequests`.
- İlişki Silme Davranışları: `DeleteBehavior.Restrict` (Kullanıcı hesapları üzerinde işlem yapıldığında geçmiş talepler ve izinlerin kaskad silinmesi engellenmiştir).

---

## 16. Test Kapsamı (Test Coverage)

`DeUygulamaVitrini.AiTests` altında Phase 23 güvenlik testleri (15 senaryo) uygulanmış ve doğrulanmıştır:
- TEST 1: İzni olmayan normal kullanıcı Reports API -> 403 Forbidden
- TEST 2: İzni olan normal kullanıcı Reports API -> 200 OK
- TEST 3: Admin kullanıcısı Reports API -> 200 OK (Örtük yetki)
- TEST 4: SuperAdmin kullanıcısı Reports API -> 200 OK (Örtük yetki)
- TEST 5: `CanCreateProjects=true` ancak Reports izni yok -> 403 Forbidden
- TEST 6: Normal kullanıcı Reports talep oluşturma -> 201 Created (Pending + Audit + Notif)
- TEST 7: Mükerrer bekleyen talep engelleme -> 400 BadRequest
- TEST 8: Admin talep onaylama -> 200 OK (Approved + İzin Kaydı + Audit + Notif)
- TEST 9: Admin talep reddetme -> 200 OK (Rejected + İzin Yok + Audit + Notif)
- TEST 10: Normal kullanıcı onaylama denemesi -> Yetkisiz / Engellendi
- TEST 11: Yetki iptali -> Reports API tekrar 403 Forbidden
- TEST 12: Yetkisi alınan kullanıcının yeniden talep açabilmesi -> 201 Created (Pending)
- TEST 13: Yetkisiz Teams API erişimi -> 403 Forbidden
- TEST 14: Teams izni tanımlanması -> 200 OK
- TEST 15: Pasif kullanıcı (`IsActive = false`) -> İzin satırı olsa dahi 403 Forbidden

**Toplam Test Sonucu:** 225 Başarılı, 0 Başarısız.

---

## 17. Kurumsal Kimlik (LDAP / AD / Entra ID) Gelecek Uyumluluğu

Yetkilendirme katmanı kimlik doğrulama mekanizmasından tamamen ayrıştırılmıştır:
1. İzinler ve talepler e-posta veya kullanıcı adı stringlerine değil, `ApplicationUser.Id` (int) birincil anahtarına bağlanmıştır.
2. Parola veya çerez implementasyon detaylarına bağımlılık yoktur.
3. İleride Active Directory, LDAP veya Microsoft Entra ID entegrasyonu yapıldığında `ApplicationUser` eşleştirmesi üzerinden mevcut yetkilendirme ve talep mekanizması **hiçbir kod değişikliği gerektirmeden** aynen çalışacaktır.

---

## 18. Bilinen Sınırlar (Known Limitations)

- Bu sürümde talep edilebilir modüller `Reports` ve `Teams` olarak belirlenmiştir. Yeni bir modül eklenmek istendiğinde `ApplicationModule` enumuna eklenmesi yeterlidir.
- SignalR yerine mevcut bildirim ve sorgu yenileme mimarisi kullanılmıştır.

---

## 19. Değiştirilen / Eklenen Dosyalar (Files Changed)

### Backend
- `backend/src/DeUygulamaVitrini.Domain/Enums/ApplicationModule.cs`
- `backend/src/DeUygulamaVitrini.Domain/Enums/AccessRequestStatus.cs`
- `backend/src/DeUygulamaVitrini.Domain/Entities/UserModulePermission.cs`
- `backend/src/DeUygulamaVitrini.Domain/Entities/ModuleAccessRequest.cs`
- `backend/src/DeUygulamaVitrini.Application/DTOs/ModuleAccess/ModuleAccessDtos.cs`
- `backend/src/DeUygulamaVitrini.Application/Common/Interfaces/IModuleAccessService.cs`
- `backend/src/DeUygulamaVitrini.Application/Common/Interfaces/IApplicationDbContext.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/Persistence/Configurations/UserModulePermissionConfiguration.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/Persistence/Configurations/ModuleAccessRequestConfiguration.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/Persistence/ApplicationDbContext.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/Persistence/Migrations/20261007144500_AddModuleAccessControlAndRequests.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/Services/ModuleAccessService.cs`
- `backend/src/DeUygulamaVitrini.Infrastructure/DependencyInjection.cs`
- `backend/src/DeUygulamaVitrini.API/Controllers/ModuleAccessController.cs`
- `backend/src/DeUygulamaVitrini.API/Controllers/AdminModuleAccessController.cs`
- `backend/src/DeUygulamaVitrini.API/Controllers/ReportsController.cs`
- `backend/src/DeUygulamaVitrini.API/Controllers/TeamsController.cs`
- `backend/tests/DeUygulamaVitrini.AiTests/Program.cs`
- `backend/tests/DeUygulamaVitrini.AiTests/DeUygulamaVitrini.AiTests.csproj`

### Frontend
- `frontend/src/types/moduleAccess.ts`
- `frontend/src/services/moduleAccessService.ts`
- `frontend/src/hooks/useModuleAccess.ts`
- `frontend/src/components/access/AccessRequestModal.tsx`
- `frontend/src/pages/AdminModuleAccessRequestsPage.tsx`
- `frontend/src/components/auth/ProtectedRoute.tsx`
- `frontend/src/routes/router.tsx`
- `frontend/src/components/navigation/Sidebar.tsx`
- `frontend/src/components/navigation/MobileDrawer.tsx`
- `frontend/src/i18n/resources/tr/access.ts`
- `frontend/src/i18n/resources/en/access.ts`
- `frontend/src/i18n/resources/tr/navigation.ts`
- `frontend/src/i18n/resources/en/navigation.ts`
- `frontend/src/i18n/resources/tr/index.ts`
- `frontend/src/i18n/resources/en/index.ts`

### Dokümantasyon
- `docs/module-access-control-and-request-workflow.md`
