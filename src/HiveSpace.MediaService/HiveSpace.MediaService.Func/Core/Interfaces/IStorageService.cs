using HiveSpace.MediaService.Func.Core.Enums;

namespace HiveSpace.MediaService.Func.Core.Interfaces;

public interface IStorageService
{
    Uri GeneratePresignedUrl(string containerName, string blobName, StoragePermissions permissions, int expiryMinutes);
    string GetContainerUrl(string containerName);
    Task<Stream> DownloadBlobAsync(string containerName, string blobName);
    Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType);
    Task DeleteBlobAsync(string containerName, string blobName);
    Task ConfigureCorsAsync(string[] allowedOrigins, CancellationToken cancellationToken);
    Task EnsureContainerExistsAsync(string containerName);
    string GetPublicUrl(string containerName, string blobName, string? cdnHost = null);
}
