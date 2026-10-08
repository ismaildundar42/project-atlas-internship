namespace DeUygulamaVitrini.Application.Common.Models;

public enum ProjectImportSessionStatus
{
    Available = 0,
    Confirming = 1,
    Consumed = 2
}

public class ProjectImportSession
{
    public string FileToken { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public ProjectImportSessionStatus Status { get; set; } = ProjectImportSessionStatus.Available;
}
