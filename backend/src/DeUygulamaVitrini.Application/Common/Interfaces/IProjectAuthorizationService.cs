namespace DeUygulamaVitrini.Application.Common.Interfaces;

/// <summary>
/// Proje ve Yönetim Paneli Yetkilendirme Servisi Arayüzü.
/// Kurallar:
/// 1. Admin her şeyi yönetebilir.
/// 2. CanCreateProjects = true olan kullanıcılar yönetim paneline erişebilir, yeni proje oluşturabilir ve sadece kendi oluşturdukları projeleri düzenleyebilir.
/// 3. Normal kullanıcıların yönetim paneline erişimi yoktur.
/// </summary>
public interface IProjectAuthorizationService
{
    Task<bool> CanAccessManagementAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> CanCreateProjectAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> CanEditProjectAsync(int userId, bool isAdmin, int projectId, CancellationToken cancellationToken = default);
    Task<bool> CanPublishProjectAsync(int userId, bool isAdmin, int projectId, CancellationToken cancellationToken = default);
    Task<bool> CanArchiveProjectAsync(int userId, bool isAdmin, int projectId, CancellationToken cancellationToken = default);
}
