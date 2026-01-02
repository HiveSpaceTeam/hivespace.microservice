using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Func.Core.Interfaces;
using Microsoft.Extensions.Configuration;
// Exceptions? Need to migrate them too or use Shared exceptions.
// existing: HiveSpace.MediaService.Core.Exceptions. using HiveSpace.MediaService.Func.Core.Exceptions?
// I'll stick to generic exceptions for now if custom ones are not crucial or migrate them later.
// Existing code uses MediaDomainErrorCode.
// I need `MediaDomainErrorCode` as well.

namespace HiveSpace.MediaService.Func.Infrastructure.Storage;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        var connectionString = _configuration["AzureStorage:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
             Console.WriteLine("!!! CRITICAL: AzureBlobStorageService - connectionString is NULL !!!");
             connectionString = _configuration["Values:AzureStorage:ConnectionString"];
             Console.WriteLine($"Trying Values:AzureStorage:ConnectionString -> {(connectionString == null ? "NULL" : "FOUND")}");
        }

        if (connectionString == null)
            throw new Exception("StorageConfigurationMissing"); // Simplified for now
        
        
        _blobServiceClient = new BlobServiceClient(connectionString);
        Console.WriteLine($"SUCCESS: AzureBlobStorageService initialized. Connection string len: {connectionString.Length}");
    }

    public Uri GenerateSasToken(string containerName, string blobName, BlobSasPermissions permissions, int expiryMinutes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        containerClient.CreateIfNotExists(PublicAccessType.Blob);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new Exception("StorageConfigurationMissing");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        };
        sasBuilder.SetPermissions(permissions);

        return blobClient.GenerateSasUri(sasBuilder);
    }

    public string GetContainerUrl(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        return containerClient.Uri.ToString();
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            throw new Exception("BlobNotFound");
        }

        return await blobClient.OpenReadAsync();
    }

    public async Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHeaders = new BlobHttpHeaders 
        { 
            ContentType = contentType,
            CacheControl = "public, max-age=31536000" 
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions 
        { 
            HttpHeaders = blobHeaders 
        });
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
    }
    public async Task ConfigureCorsAsync(string[] allowedOrigins, CancellationToken cancellationToken)
    {
        var properties = await _blobServiceClient.GetPropertiesAsync(cancellationToken);
        
        var corsRules = properties.Value.Cors;
        corsRules.Clear();

        var originString = string.Join(",", allowedOrigins);
        if (string.IsNullOrEmpty(originString)) originString = "*";

        corsRules.Add(new BlobCorsRule
        {
            AllowedHeaders = "*",
            AllowedMethods = "GET,PUT,POST,OPTIONS,HEAD,DELETE",
            AllowedOrigins = originString, 
            ExposedHeaders = "*",
            MaxAgeInSeconds = 3600
        });

        await _blobServiceClient.SetPropertiesAsync(
            new BlobServiceProperties 
            { 
               Cors = corsRules,
               DefaultServiceVersion = properties.Value.DefaultServiceVersion,
               DeleteRetentionPolicy = properties.Value.DeleteRetentionPolicy,
               HourMetrics = properties.Value.HourMetrics,
               Logging = properties.Value.Logging,
               MinuteMetrics = properties.Value.MinuteMetrics,
               StaticWebsite = properties.Value.StaticWebsite
            }, 
            cancellationToken);
    }
}
