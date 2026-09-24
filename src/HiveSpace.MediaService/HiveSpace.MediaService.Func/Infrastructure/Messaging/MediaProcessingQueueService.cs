using System.Text.Json;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Core.Exceptions;
using HiveSpace.MediaService.Core.Features.Media.Dtos;
using HiveSpace.MediaService.Core.Interfaces;
using MassTransit;

namespace HiveSpace.MediaService.Func.Infrastructure.Messaging;

public sealed class MediaProcessingQueueService(IPublishEndpoint publishEndpoint) : IQueueService
{
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<QueueMessagePayload>(
            message,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        if (payload is null || payload.MediaAssetId == Guid.Empty)
            throw new DomainException(400, MediaDomainErrorCode.MediaProcessingFailed, nameof(QueueMessagePayload));

        await publishEndpoint.Publish(payload, cancellationToken);
    }
}
