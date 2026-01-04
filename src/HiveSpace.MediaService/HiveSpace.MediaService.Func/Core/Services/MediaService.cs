using HiveSpace.MediaService.Func.Core.Enums;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Func.Core.Contracts;
using HiveSpace.MediaService.Func.Core.DomainModels;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using HiveSpace.MediaService.Func.Core.Exceptions;

namespace HiveSpace.MediaService.Func.Core.Services;

public class MediaService : IMediaService
{
    private readonly IStorageService _storageService;
    private readonly MediaDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IQueueService _queueService;

    public MediaService(
        IStorageService storageService, 
        MediaDbContext dbContext, 
        IConfiguration configuration,
        IQueueService queueService)
    {
        _storageService = storageService;
        _dbContext = dbContext;
        _configuration = configuration;
        _queueService = queueService;
    }

    public async Task<PresignUrlResponse> GeneratePresignedUrlAsync(PresignUrlRequest request)
    {
        // 1. Generate Storage Path
        var fileExtension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var storagePath = $"{request.EntityType.ToLower()}/{uniqueFileName}";

        var containerName = _configuration["AzureStorage:TempContainer"] 
                            ?? throw new DomainException(500, MediaDomainErrorCode.StorageConfigurationMissing, nameof(MediaService));
        
        var expiryMinutes = 10; 
        
        // 2. Generate SAS Token
        var sasUri = _storageService.GeneratePresignedUrl(
            containerName, 
            storagePath, 
            StoragePermissions.Write | StoragePermissions.Create, 
            expiryMinutes
        ).ToString();
            
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // 3. Save Pending Metadata to DB
        var mediaAsset = new MediaAsset(
            fileName: uniqueFileName,
            originalFileName: request.FileName,
            storagePath: storagePath,
            mimeType: request.ContentType,
            fileSize: request.FileSize,
            entityType: request.EntityType,
            entityId: request.EntityId
        );

        _dbContext.MediaAssets.Add(mediaAsset);
        await _dbContext.SaveChangesAsync();

        return new PresignUrlResponse(
            mediaAsset.Id,
            sasUri,
            storagePath,
            expiresAt
        );
    }

    public async Task ConfirmUploadAsync(ConfirmUploadRequest request)
    {
        var mediaAsset = await _dbContext.MediaAssets.FindAsync(request.Id) 
            ?? throw new NotFoundException(MediaDomainErrorCode.MediaNotFound, nameof(MediaService));

        mediaAsset.SetEntityId(request.EntityId);
        mediaAsset.MarkAsUploaded();
        
        await _dbContext.SaveChangesAsync();

        // Publish to queue
        var message = new QueueMessagePayload
        {
            MediaAssetId = mediaAsset.Id,
            Action = "Uploaded"
        };
        
        var jsonMessage = JsonSerializer.Serialize(message);
        await _queueService.SendMessageAsync(jsonMessage);
    }
}
