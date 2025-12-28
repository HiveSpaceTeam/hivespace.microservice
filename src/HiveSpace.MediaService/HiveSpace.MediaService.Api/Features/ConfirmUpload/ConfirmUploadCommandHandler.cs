using HiveSpace.Application.Shared.Handlers;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Data;
using HiveSpace.MediaService.Core.Interfaces;
using System.Text.Json;
using HiveSpace.MediaService.Core.Contracts;
using HiveSpace.MediaService.Core.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.MediaService.Api.Features.ConfirmUpload;

public class ConfirmUploadCommandHandler(
    MediaDbContext dbContext,
    IQueueService queueService) : ICommandHandler<ConfirmUploadCommand>
{
    public async Task Handle(ConfirmUploadCommand request, CancellationToken cancellationToken)
    {
        var mediaAsset = await dbContext.MediaAssets.FindAsync([request.Id], cancellationToken) 
            ?? throw new NotFoundException(MediaDomainErrorCode.MediaNotFound, nameof(ConfirmUploadCommandHandler));

        mediaAsset.SetEntityId(request.EntityId);
        mediaAsset.MarkAsUploaded();
        
        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish to queue
        var message = new QueueMessagePayload
        {
            MediaAssetId = mediaAsset.Id,
            Action = "Uploaded"
        };
        
        var jsonMessage = JsonSerializer.Serialize(message);
        await queueService.SendMessageAsync(jsonMessage, cancellationToken);
    }
}
