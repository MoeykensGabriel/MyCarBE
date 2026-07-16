using Amazon.S3;
using Amazon.S3.Model;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.API.Services;

/// <summary>
/// Storage de archivos en Cloudflare R2 (API compatible con S3). Reemplaza a
/// LocalFileStorageService en producción — el filesystem de Railway es efímero
/// y las fotos se perderían en cada redeploy.
///
/// Config (Storage:*, por env vars Storage__* en Railway):
///   ServiceUrl    = https://{accountId}.r2.cloudflarestorage.com
///   Bucket        = nombre del bucket
///   AccessKey     = R2 API token access key
///   SecretKey     = R2 API token secret key
///   PublicBaseUrl = URL pública del bucket (r2.dev o dominio custom), SIN barra final
///
/// Devuelve URLs ABSOLUTAS ({PublicBaseUrl}/{key}) — el FE ya las soporta.
/// </summary>
public class R2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _client;
    private readonly string    _bucket;
    private readonly string    _publicBaseUrl;

    public R2FileStorageService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage");

        var serviceUrl = section["ServiceUrl"]
            ?? throw new InvalidOperationException("Storage:ServiceUrl no está configurado.");
        _bucket = section["Bucket"]
            ?? throw new InvalidOperationException("Storage:Bucket no está configurado.");
        _publicBaseUrl = (section["PublicBaseUrl"]
            ?? throw new InvalidOperationException("Storage:PublicBaseUrl no está configurado.")).TrimEnd('/');

        _client = new AmazonS3Client(
            section["AccessKey"] ?? throw new InvalidOperationException("Storage:AccessKey no está configurado."),
            section["SecretKey"] ?? throw new InvalidOperationException("Storage:SecretKey no está configurado."),
            new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                // R2 requiere path-style y no soporta los checksums nuevos del SDK de AWS.
                ForcePathStyle = true,
                RequestChecksumCalculation  = Amazon.Runtime.RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation  = Amazon.Runtime.ResponseChecksumValidation.WHEN_REQUIRED,
            });
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var ext    = Path.GetExtension(fileName).ToLowerInvariant();
        var unique = $"{Guid.NewGuid()}{ext}";
        var key    = $"uploads/{folder}/{unique}";

        var request = new PutObjectRequest
        {
            BucketName  = _bucket,
            Key         = key,
            InputStream = stream,
            ContentType = ContentTypeFor(ext),
            // R2 no soporta el header de payload firmado por chunks del SDK
            DisablePayloadSigning = true,
        };

        await _client.PutObjectAsync(request, cancellationToken);

        return $"{_publicBaseUrl}/{key}";
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        // URL pública → key del objeto. Si la URL no es de este bucket, no-op.
        if (!url.StartsWith(_publicBaseUrl, StringComparison.OrdinalIgnoreCase))
            return;

        var key = url[(_publicBaseUrl.Length + 1)..];

        try
        {
            await _client.DeleteObjectAsync(_bucket, key, cancellationToken);
        }
        catch (AmazonS3Exception)
        {
            // No-op si no existe — mismo contrato que LocalFileStorageService.
        }
    }

    private static string ContentTypeFor(string ext) => ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png"            => "image/png",
        ".webp"           => "image/webp",
        ".gif"            => "image/gif",
        ".heic"           => "image/heic",
        _                 => "application/octet-stream",
    };
}
