using HiveSpace.CatalogService.Application.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.Messaging.Consumers;

public class ProductAnalyticsConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly ILogger<ProductAnalyticsConsumer> _logger;

    public ProductAnalyticsConsumer(ILogger<ProductAnalyticsConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("Analytics consumer received product {ProductId}", context.Message.ProductId);
        return Task.CompletedTask;
    }
}

