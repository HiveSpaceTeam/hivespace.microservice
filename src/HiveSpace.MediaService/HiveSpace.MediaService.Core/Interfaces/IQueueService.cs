namespace HiveSpace.MediaService.Core.Interfaces;

public interface IQueueService
{
    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}
