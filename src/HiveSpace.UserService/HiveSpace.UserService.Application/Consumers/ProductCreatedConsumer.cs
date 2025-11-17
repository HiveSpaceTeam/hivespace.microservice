using HiveSpace.CatalogService.Application.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Application.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("User service received product {ProductId}", context.Message.ProductId);
        return Task.CompletedTask;
    }
}

