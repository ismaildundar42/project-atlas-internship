# Database

Bu klasör, veritabanı yönetimiyle ilgili kayıt ve notları barındırır.

## İçerik Planı

İlerleyen aşamalarda bu klasöre eklenecekler:

- **migration-notes.md** — EF Core migration geçmişi ve önemli notlar
- **seed/** — Seed data script'leri veya açıklamaları
- **diagrams/** — Entity ilişki diyagramları (ERD)

## Şu Anki Durum

Domain modeli henüz tasarlanmamıştır. Veritabanı şeması ve migration'lar bir sonraki geliştirme aşamasında oluşturulacaktır.

EF Core **Code First** yaklaşımı kullanılmaktadır:
- Entity'ler `DeUygulamaVitrini.Domain` projesinde C# sınıfları olarak tanımlanacaktır.
- Schema değişiklikleri `dotnet ef migrations add` komutuyla migration olarak kayıt altına alınacaktır.
- Migration geçmişi `backend/src/DeUygulamaVitrini.Infrastructure/Migrations/` altında tutulacaktır.
