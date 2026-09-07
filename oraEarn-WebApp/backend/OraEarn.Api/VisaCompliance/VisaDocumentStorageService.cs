namespace OraEarn.Api.VisaCompliance;

public class VisaDocumentStorageService(IWebHostEnvironment environment)
{
    private static readonly HashSet<string> AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxBytes = 5 * 1024 * 1024;

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

        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        return (true, Path.Combine("uploads", "visas", fileName).Replace('\\', '/'), null);
    }

    public string? GetFullPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        return Path.Combine(environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
