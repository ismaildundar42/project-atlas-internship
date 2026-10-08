using DeUygulamaVitrini.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace DeUygulamaVitrini.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    private string GetPublicStorageRoot()
    {
        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
        }
        return Path.Combine(webRootPath, "uploads");
    }

    private string GetPrivateStorageRoot()
    {
        var contentRoot = _environment.ContentRootPath ?? Directory.GetCurrentDirectory();
        return Path.Combine(contentRoot, "App_Data", "uploads");
    }

    private static bool IsPrivateFolder(string folderPath, bool isPrivateExplicit)
    {
        if (isPrivateExplicit) return true;
        if (string.IsNullOrWhiteSpace(folderPath)) return false;
        var lower = folderPath.ToLowerInvariant();
        return lower.Contains("documents") || lower.Contains("docs");
    }

    public async Task<UploadResultDto> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        string folderPath,
        bool isPrivate = false,
        CancellationToken cancellationToken = default)
    {
        var isPrivateTarget = IsPrivateFolder(folderPath, isPrivate);

        // Path traversal prevention: Extract pure filename only
        var safeOriginalName = Path.GetFileName(originalFileName);
        var sanitizedName = SanitizeFileName(safeOriginalName);

        var extension = Path.GetExtension(sanitizedName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}_{sanitizedName}";

        var rootPath = isPrivateTarget ? GetPrivateStorageRoot() : GetPublicStorageRoot();

        // Clean folderPath to avoid leading/trailing slashes
        var cleanFolder = folderPath.Trim('/', '\\').Replace('\\', '/');
        var targetDirectory = Path.Combine(rootPath, cleanFolder);

        var fullTargetDir = Path.GetFullPath(targetDirectory);
        var fullRootDir = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullTargetDir.StartsWith(fullRootDir, StringComparison.OrdinalIgnoreCase) &&
            !fullTargetDir.Equals(fullRootDir.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Geçersiz hedef depolama dizini.");
        }

        if (!Directory.Exists(fullTargetDir))
        {
            Directory.CreateDirectory(fullTargetDir);
        }

        var fullFilePath = Path.Combine(fullTargetDir, uniqueFileName);

        using (var outputStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(outputStream, cancellationToken);
        }

        var relativeUrl = $"/uploads/{cleanFolder}/{uniqueFileName}";

        return new UploadResultDto
        {
            OriginalFileName = safeOriginalName,
            StoredFileName = uniqueFileName,
            RelativePath = relativeUrl,
            FileUrl = relativeUrl,
            FileSize = fileStream.Length,
            ContentType = contentType,
            IsPrivate = isPrivateTarget
        };
    }

    public void DeleteFile(string relativePath, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var isPrivateTarget = IsPrivateFolder(relativePath, isPrivate);
            var rootPath = isPrivateTarget ? GetPrivateStorageRoot() : GetPublicStorageRoot();

            var cleanPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            if (cleanPath.StartsWith("uploads" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = cleanPath.Substring(("uploads" + Path.DirectorySeparatorChar).Length);
            }

            var fullPath = Path.GetFullPath(Path.Combine(rootPath, cleanPath));
            var fullRootDir = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (fullPath.StartsWith(fullRootDir, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Fail quietly for optional cleanup
        }
    }

    public string PromoteFile(string relativePath, int projectId, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.Contains("/temp/") || projectId <= 0)
        {
            return relativePath;
        }

        try
        {
            var isPrivateTarget = IsPrivateFolder(relativePath, isPrivate);
            var rootPath = isPrivateTarget ? GetPrivateStorageRoot() : GetPublicStorageRoot();

            var cleanOldPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            if (cleanOldPath.StartsWith("uploads" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                cleanOldPath = cleanOldPath.Substring(("uploads" + Path.DirectorySeparatorChar).Length);
            }

            var oldFullPath = Path.GetFullPath(Path.Combine(rootPath, cleanOldPath));
            var fullRootDir = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!oldFullPath.StartsWith(fullRootDir, StringComparison.OrdinalIgnoreCase) || !File.Exists(oldFullPath))
            {
                return relativePath;
            }

            var newRelativePath = relativePath.Replace("/projects/temp/", $"/projects/{projectId}/")
                                              .Replace("/temp/", $"/projects/{projectId}/");

            var cleanNewPath = newRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            if (cleanNewPath.StartsWith("uploads" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                cleanNewPath = cleanNewPath.Substring(("uploads" + Path.DirectorySeparatorChar).Length);
            }

            var newFullPath = Path.GetFullPath(Path.Combine(rootPath, cleanNewPath));
            if (!newFullPath.StartsWith(fullRootDir, StringComparison.OrdinalIgnoreCase))
            {
                return relativePath;
            }

            var newDirectory = Path.GetDirectoryName(newFullPath);
            if (!string.IsNullOrWhiteSpace(newDirectory) && !Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(newDirectory);
            }

            File.Move(oldFullPath, newFullPath, overwrite: true);
            return newRelativePath;
        }
        catch
        {
            return relativePath;
        }
    }

    public string? GetPrivatePhysicalFilePath(string relativePath)
    {
        return GetPhysicalFilePath(relativePath, isPrivate: true);
    }

    public string? GetPhysicalFilePath(string relativePath, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        var rootPath = isPrivate ? GetPrivateStorageRoot() : GetPublicStorageRoot();
        var cleanPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

        if (cleanPath.StartsWith("uploads" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            cleanPath = cleanPath.Substring(("uploads" + Path.DirectorySeparatorChar).Length);
        }

        var fullPath = Path.GetFullPath(Path.Combine(rootPath, cleanPath));
        var fullRootDir = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        // Path traversal validation: path MUST be strictly within rootPath
        if (!fullPath.StartsWith(fullRootDir, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        if (isPrivate)
        {
            // Check in docs subfolder (legacy or seeded file name)
            var fileName = Path.GetFileName(cleanPath);
            var docsPath = Path.GetFullPath(Path.Combine(rootPath, "docs", fileName));
            if (docsPath.StartsWith(fullRootDir, StringComparison.OrdinalIgnoreCase) && File.Exists(docsPath))
            {
                return docsPath;
            }

            // Fallback to sample doc if available
            var defaultDocPath = Path.Combine(rootPath, "docs", "teknik-mimari-ozeti.pdf");
            if (File.Exists(defaultDocPath))
            {
                return defaultDocPath;
            }
        }

        return null;
    }

    private static string SanitizeFileName(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        // Turkish character normalization
        var charMap = new Dictionary<char, char>
        {
            {'ç', 'c'}, {'Ç', 'c'},
            {'ğ', 'g'}, {'Ğ', 'g'},
            {'ı', 'i'}, {'I', 'i'}, {'İ', 'i'},
            {'ö', 'o'}, {'Ö', 'o'},
            {'ş', 's'}, {'Ş', 's'},
            {'ü', 'u'}, {'Ü', 'u'}
        };

        var normalized = new string(nameWithoutExt.Select(c => charMap.TryGetValue(c, out var sub) ? sub : c).ToArray());
        var cleanName = Regex.Replace(normalized, @"[^a-zA-Z0-9_\-]", "_");

        if (cleanName.Length > 50)
        {
            cleanName = cleanName.Substring(0, 50);
        }

        return $"{cleanName}{ext.ToLowerInvariant()}";
    }
}

