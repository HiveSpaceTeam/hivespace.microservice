using Azure.Storage.Queues;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Core.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.MediaService.Func.Infrastructure.Storage;

public class AzureQueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    public AzureQueueService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new DomainException(500, MediaDomainErrorCode.StorageConfigurationMissing, "AzureQueueService");
            
        var queueName = configuration["AzureStorage:QueueName"]
            ?? throw new DomainException(500, MediaDomainErrorCode.StorageConfigurationMissing, "AzureQueueService");

        var options = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _queueClient = new QueueClient(connectionString, queueName, options);
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        if (await _queueClient.ExistsAsync(cancellationToken))
        {
             await _queueClient.SendMessageAsync(message, cancellationToken);
        }
    }
}
