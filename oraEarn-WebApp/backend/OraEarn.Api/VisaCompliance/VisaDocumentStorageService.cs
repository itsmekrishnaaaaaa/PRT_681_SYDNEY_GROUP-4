namespace OraEarn.Api.VisaCompliance;

// Stores identity documents on local disk under the content root (requirement NFR-2.6). Files are
// served only through the admin API, never as static files, because they contain personal data.
public class VisaDocumentStorageService(IWebHostEnvironment environment)
{
    // Extension allow-list rather than content sniffing: simple, and enough to stop executables
    // being stored. It does not prove the file really is a PDF or image.
    private static readonly HashSet<string> AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Validates and saves an uploaded identity document, returning its path relative to the content root.
    /// Validation problems are returned as an error message instead of thrown.
    /// </summary>
    public async Task<(bool Success, string? RelativePath, string? Error)> SaveAsync(
        IFormFile file,
        string userId)
    {
        if (file.Length <= 0)
        {
            return (false, null, "Visa document is required.");
        }

        if (file.Length > MaxBytes)
        {
            return (false, null, "Visa document must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return (false, null, "Visa document must be a PDF or image (JPG, PNG, WEBP).");
        }

        var uploadsRoot = Path.Combine(environment.ContentRootPath, "uploads", "visas");
        Directory.CreateDirectory(uploadsRoot);

        // The client's file name is never used on disk, which rules out path traversal, and prefixing
        // the user id keeps different users' files apart. The original name is kept on the user row.
        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        return (true, Path.Combine("uploads", "visas", fileName).Replace('\\', '/'), null);
    }

    /// <summary>Resolves a stored relative document path to an absolute path, or null if none is stored.</summary>
    public string? GetFullPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return Path.Combine(environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
