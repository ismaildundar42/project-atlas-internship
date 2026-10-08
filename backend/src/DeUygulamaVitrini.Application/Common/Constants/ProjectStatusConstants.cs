namespace DeUygulamaVitrini.Application.Common.Constants;

/// <summary>
/// Dashboard ve Raporlama servislerinde ortak kullanılan proje durum sabitleri.
/// </summary>
public static class ProjectStatusConstants
{
    public const string Active = "ACTIVE";
    public const string Completed = "COMPLETED";

    /// <summary>
    /// Devam eden (In-Progress) olarak kabul edilen durum kodları.
    /// </summary>
    public static readonly string[] InProgressStatusCodes =
    [
        "PLANNING",
        "PROOF_OF_CONCEPT",
        "PILOT",
        "ACTIVE_DEVELOPMENT"
    ];
}
