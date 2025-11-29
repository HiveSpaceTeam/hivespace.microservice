using HiveSpace.Application.Shared.Events.Products;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.Messaging.Consumers;

public class ProductAuditConsumer : IConsumer<ProductUpdatedIntegrationEvent>
{
    private readonly ILogger<ProductAuditConsumer> _logger;

    public ProductAuditConsumer(ILogger<ProductAuditConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
    {
        _logger.LogInformation("Audit consumer captured update for product {ProductId}", context.Message.ProductId);
        return Task.CompletedTask;
    }
}

