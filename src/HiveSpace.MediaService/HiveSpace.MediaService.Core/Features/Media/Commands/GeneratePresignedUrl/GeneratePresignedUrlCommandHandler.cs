using HiveSpace.Application.Shared.Commands;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Infrastructure.Configuration;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Features.Media.Dtos;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;

public class GeneratePresignedUrlCommandHandler(
    IMediaAssetRepository repository,
    IStorageService storageService,
    StorageConfiguration storageConfig) : ICommandHandler<GeneratePresignedUrlCommand, PresignUrlResponse>
{
    public async Task<PresignUrlResponse> Handle(GeneratePresignedUrlCommand command, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(command.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var storagePath = $"{command.EntityType.ToLower()}/{uniqueFileName}";

        var containerName = storageConfig.TempContainer;
        var expiryMinutes = storageConfig.PresignUrlExpiryMinutes;

        await storageService.EnsureContainerExistsAsync(containerName);

        var sasUri = storageService.GeneratePresignedUrl(
            containerName,
            storagePath,
            StoragePermissions.Create,
            expiryMinutes
        ).ToString();

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

        var mediaAsset = new MediaAsset(
            fileName: uniqueFileName,
            originalFileName: command.FileName,
            storagePath: storagePath,
            mimeType: command.ContentType,
            fileSize: command.FileSize,
            entityType: command.EntityType,
            entityId: command.EntityId
        );

        repository.Add(mediaAsset);
        await repository.SaveChangesAsync(cancellationToken);

        return new PresignUrlResponse(mediaAsset.Id, sasUri, storagePath, expiresAt);
    }
}
