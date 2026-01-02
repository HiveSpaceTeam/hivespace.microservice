using Azure.Storage.Sas;
using Azure.Storage.Blobs.Models;

namespace HiveSpace.MediaService.Func.Core.Interfaces;

public interface IStorageService
{
    Uri GenerateSasToken(string containerName, string blobName, BlobSasPermissions permissions, int expiryMinutes);
    string GetContainerUrl(string containerName);
    Task<Stream> DownloadBlobAsync(string containerName, string blobName);
    Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType);
    Task DeleteBlobAsync(string containerName, string blobName);
    Task ConfigureCorsAsync(string[] allowedOrigins, CancellationToken cancellationToken);
}
