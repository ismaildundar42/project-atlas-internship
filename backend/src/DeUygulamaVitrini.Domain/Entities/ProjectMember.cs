namespace DeUygulamaVitrini.Domain.Entities;

/// <summary>
/// Project ↔ Member many-to-many ilişkisi için explicit join entity.
///
/// Role alanı buradaki "proje rolü"dür; authentication/authorization rolü DEĞİLDİR.
/// Örnekler: "Proje Yöneticisi", "Yazılım Geliştirici", "Veri Mühendisi"
/// Bu ayrım bilinçli olarak korunmuştur.
/// </summary>
public class ProjectMember
{
    public int ProjectId { get; set; }
    public int MemberId { get; set; }

    /// <summary>
    /// Kişinin bu projedeki rolü.
    /// Serbest metin olarak tutulmuştur çünkü proje rolleri authentication rollerinden bağımsızdır.
    /// </summary>
    public string? ProjectRole { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
