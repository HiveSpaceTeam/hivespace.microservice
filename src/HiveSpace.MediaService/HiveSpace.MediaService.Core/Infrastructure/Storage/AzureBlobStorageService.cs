using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Enums;
using HiveSpace.MediaService.Core.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HiveSpace.MediaService.Core.Infrastructure.Storage;

public class AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger) : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient = CreateBlobServiceClient(configuration, logger);

    private static BlobServiceClient CreateBlobServiceClient(IConfiguration configuration, ILogger logger)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            logger.LogWarning("AzureStorage:ConnectionString not found, trying fallback configuration");
            connectionString = configuration["Values:AzureStorage:ConnectionString"];
        }

        if (string.IsNullOrEmpty(connectionString))
            throw new DomainException(500, MediaDomainErrorCode.StorageConfigurationMissing, "Storage connection string is not configured");

        logger.LogInformation("AzureBlobStorageService initialized successfully");
        return new BlobServiceClient(connectionString);
    }

    public Uri GeneratePresignedUrl(string containerName, string blobName, StoragePermissions permissions, int expiryMinutes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
            throw new DomainException(500, MediaDomainErrorCode.StorageConfigurationMissing, "Cannot generate SAS URI - storage account key required");

        BlobSasPermissions sasPermissions = 0;
        if (permissions.HasFlag(StoragePermissions.Read)) sasPermissions |= BlobSasPermissions.Read;
        if (permissions.HasFlag(StoragePermissions.Write)) sasPermissions |= BlobSasPermissions.Write;
        if (permissions.HasFlag(StoragePermissions.Delete)) sasPermissions |= BlobSasPermissions.Delete;
        if (permissions.HasFlag(StoragePermissions.Create)) sasPermissions |= BlobSasPermissions.Create;
        if (permissions.HasFlag(StoragePermissions.List)) sasPermissions |= BlobSasPermissions.List;

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        };
        sasBuilder.SetPermissions(sasPermissions);

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

        try
        {
            return await blobClient.OpenReadAsync();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            throw new DomainException(404, MediaDomainErrorCode.BlobNotFound, $"Blob '{blobName}' not found in container '{containerName}'");
        }
    }

    public async Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHeaders = new BlobHttpHeaders
        {
            ContentType = contentType,
            CacheControl = "public, max-age=31536000"
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHeaders });
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        try
        {
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            // Ignore if blob not found
        }
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

    public async Task EnsureContainerExistsAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
    }

    public string GetPublicUrl(string containerName, string blobName, string? cdnHost = null)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobUrl = blobClient.Uri.ToString();

        if (string.IsNullOrEmpty(cdnHost))
            return blobUrl;

        var builder = new UriBuilder(blobClient.Uri)
        {
            Host = cdnHost,
            Scheme = "https",
            Port = -1
        };
        return builder.ToString();
    }
}
