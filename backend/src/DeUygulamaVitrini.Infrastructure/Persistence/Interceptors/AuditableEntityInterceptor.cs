using DeUygulamaVitrini.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DeUygulamaVitrini.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor'ı.
///
/// Amaç: Her kayıt/güncelleme işleminde BaseEntity'nin audit alanlarını
/// (CreatedAt, UpdatedAt) otomatik olarak doldurmak.
///
/// Bu yaklaşım sayesinde uygulama kodu içinde manuel olarak
///   entity.UpdatedAt = DateTime.UtcNow;
/// gibi satırlar yazılmaz; merkezi ve tutarlı audit sağlanır.
///
/// Tüm tarihler UTC olarak saklanır — time zone bağımsızlığı için.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.CreatedAt = now;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // CreatedAt değiştirilmemeli — sadece ilk kayıtta set edilir.
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}
