using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Sync;

public class ProductRefSyncConsumer(OrderDbContext db, ILogger<ProductRefSyncConsumer> logger)
    : IConsumer<ProductCreatedEvent>,
      IConsumer<ProductUpdatedEvent>,
      IConsumer<ProductDeletedEvent>,
      IConsumer<ProductSkuUpdatedEvent>
{
    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var msg = context.Message;
        return UpsertProductRefAsync(msg.Id, msg.StoreId, msg.Name, msg.ThumbnailUrl, msg.Status, context.CancellationToken);
    }

    public Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var msg = context.Message;
        return UpsertProductRefAsync(msg.Id, msg.StoreId, msg.Name, msg.ThumbnailUrl, msg.Status, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var existing = await db.ProductRefs.FindAsync([context.Message.Id], context.CancellationToken);
        if (existing is not null)
        {
            db.ProductRefs.Remove(existing);
            await db.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("ProductRef removed. ProductId={ProductId}", context.Message.Id);
        }
    }

    public async Task Consume(ConsumeContext<ProductSkuUpdatedEvent> context)
    {
        var msg = context.Message;
        var existing = await db.SkuRefs.FindAsync([msg.SkuId], context.CancellationToken);
        if (existing is null)
        {
            db.SkuRefs.Add(new SkuRef(msg.SkuId, msg.ProductId, msg.SkuNo, msg.Price, msg.Currency,
                imageUrl: null, attributes: null, skuName: msg.SkuName));
            logger.LogInformation("SkuRef created. SkuId={SkuId}", msg.SkuId);
        }
        else
        {
            existing.Update(msg.SkuNo, msg.Price, msg.Currency, imageUrl: null, attributes: null, skuName: msg.SkuName);
            logger.LogInformation("SkuRef updated. SkuId={SkuId}", msg.SkuId);
        }
        await db.SaveChangesAsync(context.CancellationToken);
    }

    private async Task UpsertProductRefAsync(long id, Guid storeId, string name,
        string? thumbnailUrl, ProductStatus status, CancellationToken ct)
    {
        var existing = await db.ProductRefs.FindAsync([id], ct);
        if (existing is null)
        {
            db.ProductRefs.Add(new ProductRef(id, storeId, name, thumbnailUrl, status));
            logger.LogInformation("ProductRef created. ProductId={ProductId}", id);
        }
        else
        {
            existing.Update(storeId, name, thumbnailUrl, status);
            logger.LogInformation("ProductRef updated. ProductId={ProductId}", id);
        }
        await db.SaveChangesAsync(ct);
    }
}
