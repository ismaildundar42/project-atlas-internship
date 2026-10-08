# Mimari Kararlar — Demir Export Proje Kütüphanesi

> **Belge Türü:** Architecture Decision Record (ADR) — Özet  
> **Versiyon:** 1.0  
> **Tarih:** Eylül 2026  
> **Hazırlayan:** Ar-Ge Yazılım Ekibi

---

## Genel Bakış

Bu belge, Demir Export Proje Kütüphanesi uygulamasının temel mimari kararlarını ve bu kararların arkasındaki gerekçeleri açıklamaktadır. Amaç; geliştirme ekibinin mimari prensipleri anlayarak doğru kararlar alabilmesini sağlamak, ileride katılacak ekip üyelerine bağlam sunmaktır.

---

## 1. Monorepo Yaklaşımı

**Karar:** Frontend ve backend aynı Git repository'sinde tutulmaktadır.

**Gerekçe:**
- Küçük-orta ölçekli kurumsal ekipler için yönetimi daha kolaydır.
- Frontend/backend değişiklikleri tek bir commit/PR üzerinden takip edilebilir.
- CI/CD pipeline kurulumu basitleşir.
- Gerçek anlamda ayrı takımlar ve deploy pipeline'ları ihtiyacı doğduğunda polyrepo'ya geçiş mümkündür.

---

## 2. Frontend: React + TypeScript + Vite

**Karar:** React 18, TypeScript ve Vite kullanılmaktadır.

**Gerekçe:**
- React, kurumsal ekosistemde geniş topluluk desteğine ve uzun vadeli istikrara sahiptir.
- TypeScript, özellikle kurumsal projelerde tip güvenliği, IDE desteği ve refactoring kolaylığı sağlar.
- Vite, Webpack'e kıyasla geliştirme deneyimini (HMR hızı, cold-start) önemli ölçüde iyileştirir.
- Next.js gibi SSR framework'leri bu projede gereksizdir; SPA mimarisi yeterlidir.

**Feature-Oriented Klasör Yapısı:**
- `features/` altında her feature kendi component, hook ve servislerine sahip olur.
- `components/` yalnızca global, yeniden kullanılabilir UI parçalarını barındırır.
- Bu yaklaşım, ekip büyüdükçe paralel geliştirmeyi ve kod sahipliğini kolaylaştırır.

---

## 3. State Management Stratejisi

**Karar:** Redux eklenmemiştir. İleride server state için TanStack Query, local state için React'in yerleşik hook'ları kullanılacaktır.

**Gerekçe:**
- Uygulamanın büyük bölümü server state'e (API'den gelen veriler) dayanmaktadır.
- TanStack Query, cache yönetimi, refetch stratejisi ve loading/error state'leri için Redux'a kıyasla çok daha az boilerplate ile çözüm sunar.
- Gerçek global client-side state ihtiyacı oluşursa (örn. authentication state), Zustand veya Context API değerlendirilebilir.
- "Redux kullanmak için Redux eklemek" anti-pattern'inden kaçınılmıştır.

---

## 4. Backend: Pragmatic Layered Architecture

**Karar:** ASP.NET Core Web API ile 4 katmanlı pragmatic mimari kullanılmaktadır:  
`API → Application → Domain → Infrastructure`

**Gerekçe:**
- Domain Driven Design (DDD) veya Clean Architecture'ın tüm soyutlama katmanları bu ölçekte gereksiz karmaşıklık yaratır.
- Seçilen yapı, SOLID prensiplerine uymaktadır ancak gereksiz abstraction içermez.
- Junior geliştiriciler katmanların sorumluluklarını kolayca anlayabilir.
- İleride gerçek karmaşıklık gerektiren alanlarda (örn. domain event'ler, saga pattern) mimari bu yapı üzerinde genişletilebilir.

**Katman Sorumlulukları:**

| Katman         | Sorumluluk                                                         |
|----------------|---------------------------------------------------------------------|
| Domain         | Entity'ler, value object'ler, domain interface'leri                |
| Application    | Use case'ler, DTO'lar, servis interface'leri, validasyon            |
| Infrastructure | EF Core DbContext, repository implementasyonları, dış servisler    |
| API            | Controller'lar, middleware, DI composition root, konfigürasyon     |

---

## 5. Repository Pattern Kararı

**Karar:** Generic Repository Pattern **eklenmemiştir**. EF Core'un `DbContext` ve `DbSet` yapısı doğrudan kullanılmaktadır.

**Gerekçe:**
- EF Core zaten Unit of Work ve Repository pattern'in iyi bir implementasyonudur.
- Üstüne generic `IRepository<T>` eklemek, testability'yi küçük ölçüde iyileştirirken önemli miktarda boilerplate ekler.
- Application katmanında `IApplicationDbContext` interface'i tanımlanarak EF Core'un doğrudan DbContext'e bağımlılığı gevşetilmiştir; bu yaklaşım birim testi için yeterlidir.

---

## 6. Code First Database Yaklaşımı

**Karar:** EF Core Code First; entity'ler C# sınıfları olarak tanımlanır, schema kod tarafından yönetilir.

**Gerekçe:**
- Geliştirici deneyimini iyileştirir; SQL'i ayrıca yönetmek yerine C# üzerinden schema değişiklikleri yapılır.
- Migration'lar kaynak koda dahil edildiğinden version control ile takip edilir.
- Koç grubu kurumsal standartlarında yaygın kullanılan yaklaşımla örtüşmektedir.

---

## 7. CORS Politikası

**Karar:** Development ortamında yalnızca `http://localhost:5173` adresine izin veren named CORS policy kullanılmaktadır.

**Gerekçe:**
- Wildcard (`*`) CORS politikası güvenlik açığı oluşturur.
- Production ortamında izin verilen origin'ler environment variable üzerinden yapılandırılacak ve kısıtlı tutulacaktır.

---

## 8. Authentication Ertelemesi

**Karar:** Authentication bu aşamada implement edilmemiştir; ancak mimari buna hazır bırakılmıştır.

**Gerekçe:**
- Erken authentication implementasyonu gereksiz complexity ekler ve geliştirme hızını düşürür.
- Katmanlı mimari ve dependency injection yapısı sayesinde ileride JWT-based authentication veya Active Directory entegrasyonu kolayca eklenebilir.
- Role-based authorization için gerekli middleware pipeline konumları korunmuştur.

---

## 9. Environment Configuration

**Karar:** Frontend için `VITE_` prefix'li environment variable'lar, backend için `appsettings.json` / `appsettings.{Environment}.json` kullanılmaktadır.

**Gerekçe:**
- Vite, build sırasında yalnızca `VITE_` prefix'li değişkenleri bundle'a dahil eder; hassas sunucu değişkenlerinin frontend'e sızması önlenir.
- ASP.NET Core'un yerleşik konfigürasyon sistemi, environment bazlı override ve secret yönetimini destekler.
- `.env` dosyaları `.gitignore` kapsamındadır; `.env.example` şablonlar kaynak koda dahildir.

---

## Sonraki Aşamalar İçin Notlar

- **Domain Modeli:** Project entity ve ilgili tablolar (Ekip, Lokasyon, Teknoloji vb.) bir sonraki phase'te ayrıca tasarlanacaktır.
- **Authentication:** JWT veya kurumsal SSO (LDAP/AD) entegrasyonu ileride değerlendirilecektir.
- **Testing:** Unit ve integration test altyapısı domain modeli oturduktan sonra eklenecektir.
- **Erişilebilirlik:** WCAG 2.2 AA hedefi, UI geliştirme aşamasında sistematik olarak uygulanacaktır.
