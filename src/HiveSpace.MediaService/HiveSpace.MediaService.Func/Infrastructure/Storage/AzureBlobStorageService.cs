using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Core.Enums;
using Microsoft.Extensions.Configuration;

using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Func.Core.Exceptions;

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
            throw new Exception("StorageConfigurationMissing");
        
        _blobServiceClient = new BlobServiceClient(connectionString);
        Console.WriteLine($"SUCCESS: AzureBlobStorageService initialized. Connection string len: {connectionString.Length}");
    }

    public Uri GeneratePresignedUrl(string containerName, string blobName, StoragePermissions permissions, int expiryMinutes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new Exception("StorageConfigurationMissing");
        }

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
            throw new NotFoundException(MediaDomainErrorCode.MediaNotFound, nameof(blobName));
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

        await blobClient.UploadAsync(content, new BlobUploadOptions 
        { 
            HttpHeaders = blobHeaders 
        });
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
}
