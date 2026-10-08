namespace DeUygulamaVitrini.Application.Common.Interfaces;

public class UploadResultDto
{
    public required string OriginalFileName { get; set; }
    public required string StoredFileName { get; set; }
    public required string RelativePath { get; set; }
    public required string FileUrl { get; set; }
    public long FileSize { get; set; }
    public required string ContentType { get; set; }
    public bool IsPrivate { get; set; }
}

public interface IFileStorageService
{
    Task<UploadResultDto> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        string folderPath,
        bool isPrivate = false,
        CancellationToken cancellationToken = default);

    void DeleteFile(string relativePath, bool isPrivate = false);

    string PromoteFile(string relativePath, int projectId, bool isPrivate = false);

    string? GetPrivatePhysicalFilePath(string relativePath);

    string? GetPhysicalFilePath(string relativePath, bool isPrivate = false);
}

