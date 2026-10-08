namespace DeUygulamaVitrini.Application.DTOs.Admin;

public class RejectProjectRequestDto
{
    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }

    public string GetEffectiveReason()
    {
        if (!string.IsNullOrWhiteSpace(RejectionReason)) return RejectionReason.Trim();
        if (!string.IsNullOrWhiteSpace(Reason)) return Reason.Trim();
        return string.Empty;
    }
}
