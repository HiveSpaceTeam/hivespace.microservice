using HiveSpace.MediaService.Core.Features.Media.Dtos;
using HiveSpace.MediaService.Func.Functions.Queue;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.MediaService.Func.Consumers;

public sealed class MediaProcessingQueueConsumer(
    ImageProcessingFunction imageProcessingFunction,
    ILogger<MediaProcessingQueueConsumer> logger)
    : IConsumer<QueueMessagePayload>
{
    public async Task Consume(ConsumeContext<QueueMessagePayload> context)
    {
        logger.LogInformation(
            "Received media processing queue item {MediaAssetId}",
            context.Message.MediaAssetId);

        await imageProcessingFunction.ProcessAsync(context.Message);
    }
}
