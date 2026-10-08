namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// İş ve güvenlik kritik işlemlerin değişmez (append-only) kayıt günlüğü.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    /// <summary>İşlemin gerçekleştiği UTC zamanı.</summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>İşlemi gerçekleştiren kullanıcı ID (varsa).</summary>
    public int? ActorUserId { get; set; }

    /// <summary>İşlem anındaki kullanıcı ad soyad snapshot'ı.</summary>
    public string? ActorDisplayNameSnapshot { get; set; }

    /// <summary>İşlem anındaki kullanıcı e-posta snapshot'ı.</summary>
    public string? ActorEmailSnapshot { get; set; }

    /// <summary>İşlem türü (ör: ProjectCreated, ProjectApproved vb.).</summary>
    public required string Action { get; set; }

    /// <summary>İlişkili varlık türü (ör: Project, Member, UserAccess).</summary>
    public required string EntityType { get; set; }

    /// <summary>İlişkili varlık ID (string/int mantıksal ID, cascade delete yapılmaz).</summary>
    public string? EntityId { get; set; }

    /// <summary>İşlem anındaki varlık görüntü adı snapshot'ı (ör: Proje Adı).</summary>
    public string? EntityDisplayNameSnapshot { get; set; }

    /// <summary>İnsan tarafından okunabilir açıklama.</summary>
    public required string Description { get; set; }

    /// <summary>Opsiyonel yapılandırılmış ek veri JSON'ı.</summary>
    public string? MetadataJson { get; set; }
}
