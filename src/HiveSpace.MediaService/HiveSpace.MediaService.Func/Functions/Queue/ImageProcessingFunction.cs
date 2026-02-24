using System.Text.Json;
using HiveSpace.MediaService.Core.Infrastructure.Data;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Configuration;
using HiveSpace.MediaService.Core.Contracts;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HiveSpace.MediaService.Func.Functions.Queue;

public class ImageProcessingFunction(
    ILogger<ImageProcessingFunction> logger,
    IConfiguration configuration,
    IStorageService storageService,
    StorageConfiguration storageConfig,
    MediaDbContext dbContext
    )
{
    private const string QueueName = "image-processing-queue";

    [Function(nameof(ImageProcessingFunction))]
    public async Task Run([QueueTrigger(QueueName, Connection = "AzureStorage:ConnectionString")] string message)
    {
        logger.LogInformation("Processing queue message: {Message}", message);

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
            return;
        }

        await HandleMediaAssetAsync(mediaAsset);
    }

    private async Task HandleMediaAssetAsync(MediaAsset mediaAsset)
    {
        try
        {
            using var originalStream = await storageService.DownloadBlobAsync(storageConfig.TempContainer, mediaAsset.StoragePath);

            if (originalStream.CanSeek && originalStream.Length == 0)
            {
                logger.LogError("Stream is empty! Blob {StoragePath} has 0 bytes.", mediaAsset.StoragePath);
                return;
            }

            var success = await ProcessAndUploadAsync(originalStream, mediaAsset);
            if (success)
            {
                await UpdateMediaAssetUrlsAsync(mediaAsset);
            }

            logger.LogInformation("Successfully processed image {MediaAssetId}", mediaAsset.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process media asset {MediaAssetId}", mediaAsset.Id);
            mediaAsset.MarkAsFailed();
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task<bool> ProcessAndUploadAsync(Stream originalStream, MediaAsset mediaAsset)
    {
        var actualLength = originalStream.CanSeek ? originalStream.Length : -1;

        // 1. Prevent SAS bypass attacks: Reject files larger than what was originally declared
        if (actualLength > 0 && actualLength > mediaAsset.FileSize)
        {
            logger.LogError("Validation failed: Uploaded file size ({ActualLength} bytes) exceeds the originally declared size ({DeclaredSize} bytes) for Asset {MediaAssetId}.", actualLength, mediaAsset.FileSize, mediaAsset.Id);
            return false;
        }

        // 2. Prevent OOM attacks: Hard absolute system maximum of 25MB
        const long maxSystemLimit = 25 * 1024 * 1024; // 25 MB
        if (actualLength > maxSystemLimit)
        {
            logger.LogError("Validation failed: File size ({ActualLength} bytes) exceeds system maximum limit of 25MB for Asset {MediaAssetId}.", actualLength, mediaAsset.Id);
            return false;
        }

        if (IsImage(mediaAsset))
        {
            using var memoryStream = new MemoryStream();
            if (originalStream.CanSeek && originalStream.Position != 0) originalStream.Position = 0;
            await originalStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (memoryStream.Length == 0)
            {
                logger.LogError("MemoryStream is empty after copy! Original Stream Length: {Length}",
                    originalStream.CanSeek ? originalStream.Length : -1);
                return false;
            }

            using var image = await Image.LoadAsync(memoryStream);

            await UploadMainImageAsync(image, mediaAsset);
            await UploadThumbnailAsync(image, mediaAsset);
        }
        else
        {
            logger.LogInformation("Asset {MediaAssetId} is not an image ({MimeType}). Uploading original.", mediaAsset.Id, mediaAsset.MimeType);

            if (originalStream.CanSeek) originalStream.Position = 0;

            await storageService.UploadBlobAsync(storageConfig.PublicContainer, mediaAsset.StoragePath, originalStream, mediaAsset.MimeType ?? "application/octet-stream");
        }

        return true;
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

        var newPath = Path.ChangeExtension(mediaAsset.StoragePath, ".webp");
        mediaAsset.UpdateStorageDetails(newPath, "image/webp");
        mediaAsset.UpdateFileSize(outputStream.Length);

        await storageService.UploadBlobAsync(storageConfig.PublicContainer, mediaAsset.StoragePath, outputStream, "image/webp");
    }

    private async Task UploadThumbnailAsync(Image image, MediaAsset mediaAsset)
    {
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(150, 0),
            Mode = ResizeMode.Max
        }));

        using var thumbStream = new MemoryStream();
        await image.SaveAsWebpAsync(thumbStream);
        thumbStream.Position = 0;

        var thumbPath = GetThumbnailPath(mediaAsset.StoragePath);
        await storageService.UploadBlobAsync(storageConfig.PublicContainer, thumbPath, thumbStream, "image/webp");
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
        var url = $"{storageService.GetContainerUrl(storageConfig.PublicContainer)}/{path}";
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
            return string.Empty;

        var directory = Path.GetDirectoryName(originalPath);
        var fileNameNoExt = Path.GetFileNameWithoutExtension(originalPath);
        var thumbFileName = $"{fileNameNoExt}_thumb.webp";

        var result = string.IsNullOrEmpty(directory)
            ? thumbFileName
            : Path.Combine(directory, thumbFileName);

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
