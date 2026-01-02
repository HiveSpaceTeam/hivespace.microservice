namespace HiveSpace.MediaService.Func.Core.Interfaces;

public interface IQueueService
{
    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}
