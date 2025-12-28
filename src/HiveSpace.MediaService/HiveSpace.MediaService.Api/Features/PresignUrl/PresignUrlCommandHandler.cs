using HiveSpace.Application.Shared.Handlers;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Data;
using HiveSpace.MediaService.Core.Interfaces;

using Azure.Storage.Sas;
using HiveSpace.MediaService.Core.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.MediaService.Api.Features.PresignUrl;

public class PresignUrlCommandHandler(
    IStorageService storageService,
    MediaDbContext dbContext,
    IConfiguration configuration) : ICommandHandler<PresignUrlCommand, PresignUrlResponse>
{
    public async Task<PresignUrlResponse> Handle(PresignUrlCommand command, CancellationToken cancellationToken)
    {
        // 1. Generate Storage Path
        var fileExtension = Path.GetExtension(command.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var storagePath = $"{command.EntityType.ToString().ToLower()}/{uniqueFileName}";

        // 2. Generate SAS Token using StorageService


        var containerName = configuration["AzureStorage:TempContainer"] 
                            ?? throw new DomainException(500, MediaDomainErrorCode.StorageConfigurationMissing, nameof(PresignUrlCommandHandler));
        
        var expiryMinutes = 10; 
        
        // Use the new GenerateSasToken method
        var sasUri = storageService.GenerateSasToken(
            containerName, 
            storagePath, 
            BlobSasPermissions.Write | BlobSasPermissions.Create, 
            expiryMinutes
        ).ToString();
            
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // 3. Save Pending Metadata to DB
        var mediaAsset = new MediaAsset(
            fileName: uniqueFileName,
            originalFileName: command.FileName,
            storagePath: storagePath,
            mimeType: command.ContentType,
            fileSize: command.FileSize,
            entityType: command.EntityType,
            entityId: command.EntityId
        );

        dbContext.MediaAssets.Add(mediaAsset);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PresignUrlResponse(
            mediaAsset.Id,
            sasUri,
            storagePath,
            expiresAt
        );
    }
}
