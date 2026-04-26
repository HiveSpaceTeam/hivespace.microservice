using HiveSpace.Application.Shared.Commands;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Exceptions;
using HiveSpace.MediaService.Core.Interfaces;
using System.Text.Json;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.ConfirmUpload;

public class ConfirmUploadCommandHandler(
    IMediaAssetRepository repository,
    IQueueService queueService) : ICommandHandler<ConfirmUploadCommand>
{
    public async Task Handle(ConfirmUploadCommand command, CancellationToken cancellationToken)
    {
        var mediaAsset = await repository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new NotFoundException(MediaDomainErrorCode.MediaNotFound, nameof(MediaAsset));

        mediaAsset.SetEntityId(command.EntityId);
        mediaAsset.MarkAsUploaded();

        await repository.SaveChangesAsync(cancellationToken);

        var message = new { MediaAssetId = mediaAsset.Id, Action = "Uploaded" };
        await queueService.SendMessageAsync(JsonSerializer.Serialize(message));
    }
}
