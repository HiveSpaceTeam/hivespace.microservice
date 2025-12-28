using System.Text.Json;
using HiveSpace.MediaService.Core.Data;
using HiveSpace.MediaService.Core.DomainModels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using HiveSpace.MediaService.Core.Exceptions;
using HiveSpace.MediaService.Core.Contracts;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.MediaService.Func.Functions;

public class ImageProcessingFunction(
    ILogger<ImageProcessingFunction> logger,
    IConfiguration configuration,
    IStorageService storageService,
    MediaDbContext dbContext)
{
    private const string QueueName = "image-processing-queue";
    private string PublicContainerName => configuration["AzureStorage:PublicContainer"] ?? "public-assets";
    private string TempContainerName => configuration["AzureStorage:TempContainer"] ?? "temp-media-upload";

    [Function(nameof(ImageProcessingFunction))]
    public async Task Run([QueueTrigger(QueueName, Connection = "AzureWebJobsStorage")] string message)
    {
        logger.LogInformation("Processing queue message: {Message}", message);

        // We do *not* catch generic exceptions here to allow the runtime to handle retries 
        // and move to poison queue if necessary.
        
        var queueMessage = JsonSerializer.Deserialize<QueueMessagePayload>(message);
        if (queueMessage == null || queueMessage.MediaAssetId == Guid.Empty)
        {
            logger.LogError("Invalid message format or missing MediaAssetId.");
            return; 
        }

        var mediaAsset = await dbContext.MediaAssets.FindAsync(queueMessage.MediaAssetId);
        if (mediaAsset is null || mediaAsset.EntityId is null)
        {
            logger.LogWarning("MediaAsset {MediaAssetId} not found.", queueMessage.MediaAssetId);
            // We return here because retrying won't find the record unless it's a replication lag issue,
            // but assuming strong consistency or eventual consistency within reasonable time.
            return;
        }

        await HandleMediaAssetAsync(mediaAsset);
    }

    private async Task HandleMediaAssetAsync(MediaAsset mediaAsset)
    {
        // 1. Download original image
        
        try
        {
            using var originalStream = await storageService.DownloadBlobAsync(TempContainerName, mediaAsset.StoragePath);
            
            if (originalStream.CanSeek && originalStream.Length == 0)
            {
                logger.LogError("Stream is empty! Blob {StoragePath} has 0 bytes.", mediaAsset.StoragePath);
                return;
            }

            // 2. Process and Upload
            await ProcessAndUploadAsync(originalStream, mediaAsset);

            // 3. Update DB with URLs
            await UpdateMediaAssetUrlsAsync(mediaAsset);

            logger.LogInformation("Successfully processed image {MediaAssetId}", mediaAsset.Id);
        }
        catch (DomainException ex) when (ex.ErrorCode == MediaDomainErrorCode.BlobNotFound)
        {
            logger.LogError("Blob {StoragePath} not found in {ContainerName}.", mediaAsset.StoragePath, TempContainerName);
            mediaAsset.MarkAsFailed();
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task ProcessAndUploadAsync(Stream originalStream, MediaAsset mediaAsset)
    {
        if (IsImage(mediaAsset))
        {
            // Setup buffering to MemoryStream to avoid ImageSharp issues with Network/Blob streams
            using var memoryStream = new MemoryStream();
            if (originalStream.CanSeek && originalStream.Position != 0) originalStream.Position = 0;
            await originalStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (memoryStream.Length == 0)
            {
                logger.LogError("MemoryStream is empty after copy! Original Stream Length: {Length}", 
                    originalStream.CanSeek ? originalStream.Length : -1);
                return;
            }


            using var image = await Image.LoadAsync(memoryStream);
            
            // Standardize to WebP for everything (Outcome of user request)
            
            // Upload Main Image (Resized if needed, converted to WebP)
            await UploadMainImageAsync(image, mediaAsset);

            // Create and Upload Thumbnail (WebP)
            await UploadThumbnailAsync(image, mediaAsset);
        }
        else
        {
            logger.LogInformation("Asset {MediaAssetId} is not an image ({MimeType}). Uploading original.", mediaAsset.Id, mediaAsset.MimeType);
            
            if (originalStream.CanSeek) originalStream.Position = 0;
            
            await storageService.UploadBlobAsync(PublicContainerName, mediaAsset.StoragePath, originalStream, mediaAsset.MimeType ?? "application/octet-stream");
        }
    }

    private async Task UploadMainImageAsync(Image image, MediaAsset mediaAsset)
    {
        if (image.Width > 1024)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1024, 0),
                Mode = ResizeMode.Max
            }));
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream); 
        outputStream.Position = 0;

        // Update Path definition to WebP
        var newPath = Path.ChangeExtension(mediaAsset.StoragePath, ".webp");
        mediaAsset.UpdateStorageDetails(newPath, "image/webp");
        
        mediaAsset.UpdateFileSize(outputStream.Length);

        await storageService.UploadBlobAsync(PublicContainerName, mediaAsset.StoragePath, outputStream, "image/webp");
    }

    private async Task UploadThumbnailAsync(Image image, MediaAsset mediaAsset)
    {
        // Clone or mutate? Since we are done with the main image, we can mutate the current instance for thumbnail
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(150, 0),
            Mode = ResizeMode.Max
        }));

        using var thumbStream = new MemoryStream();
        await image.SaveAsWebpAsync(thumbStream);
        thumbStream.Position = 0;

        var thumbPath = GetThumbnailPath(mediaAsset.StoragePath);
        await storageService.UploadBlobAsync(PublicContainerName, thumbPath, thumbStream, "image/webp");
    }

    private async Task UpdateMediaAssetUrlsAsync(MediaAsset mediaAsset)
    {
        var publicUrl = GetPublicUrl(mediaAsset.StoragePath);
        string? thumbnailUrl = null;

        if (IsImage(mediaAsset))
        {
            var thumbPath = GetThumbnailPath(mediaAsset.StoragePath);
            thumbnailUrl = GetPublicUrl(thumbPath);
        }

        mediaAsset.MarkAsProcessed(publicUrl, thumbnailUrl);
        await dbContext.SaveChangesAsync();
    }

    private string GetPublicUrl(string path)
    {
        var url = $"{storageService.GetContainerUrl(PublicContainerName)}/{path}";
        var cdnHost = configuration["AzureStorage:CdnHost"];
        
        if (!string.IsNullOrEmpty(cdnHost))
        {
            return GetCdnUrl(url, cdnHost);
        }

        return url;
    }

    private static string GetThumbnailPath(string originalPath)
    {
        if (string.IsNullOrWhiteSpace(originalPath))
        {
            return string.Empty;
        }

        var directory = Path.GetDirectoryName(originalPath);
        var fileNameNoExt = Path.GetFileNameWithoutExtension(originalPath);
        var thumbFileName = $"{fileNameNoExt}_thumb.webp";

        var result = string.IsNullOrEmpty(directory)
            ? thumbFileName
            : Path.Combine(directory, thumbFileName);

        // Preserve usage of forward slashes if detected in original path
        return originalPath.Contains('/') 
            ? result.Replace('\\', '/') 
            : result;
    }

    private static bool IsImage(MediaAsset asset)
    {
        return asset.MimeType != null && asset.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCdnUrl(string url, string cdnHost)
    {
        var builder = new UriBuilder(new Uri(url))
        {
            Host = cdnHost,
            Scheme = "https",
            Port = -1
        };
        return builder.ToString();
    }
}
