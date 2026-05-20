using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.API.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var ext       = Path.GetExtension(fileName).ToLowerInvariant();
        var unique    = $"{Guid.NewGuid()}{ext}";
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", folder);

        Directory.CreateDirectory(uploadDir);

        var fullPath = Path.Combine(uploadDir, unique);
        await using var fs = File.Create(fullPath);
        await stream.CopyToAsync(fs, cancellationToken);

        // Return public URL (served by UseStaticFiles)
        return $"/uploads/{folder}/{unique}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        // Convert public URL → physical path
        var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, relative);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
