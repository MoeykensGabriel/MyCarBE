namespace MyCarBE.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>Saves a file stream and returns the public URL.</summary>
    Task<string> SaveAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);

    /// <summary>Deletes the file at the given public URL. No-ops if not found.</summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}
