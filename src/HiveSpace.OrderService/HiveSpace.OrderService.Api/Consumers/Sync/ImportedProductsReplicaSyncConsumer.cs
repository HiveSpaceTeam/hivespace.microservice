using HiveSpace.Infrastructure.Messaging.Shared.Events.Products;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HiveSpace.OrderService.Api.Consumers.Sync;

public class ImportedProductsReplicaSyncConsumer(OrderDbContext db, ILogger<ImportedProductsReplicaSyncConsumer> logger)
    : IConsumer<ImportedProductsReplicaSyncIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ImportedProductsReplicaSyncIntegrationEvent> context)
    {
        foreach (var product in context.Message.Products)
        {
            var existingProduct = await db.ProductRefs.FindAsync([product.ProductId], context.CancellationToken);
            if (existingProduct is null)
            {
                db.ProductRefs.Add(new ProductRef(
                    product.ProductId,
                    product.StoreId,
                    product.Name,
                    product.ThumbnailUrl,
                    product.Status));
            }
            else
            {
                existingProduct.Update(
                    product.StoreId,
                    product.Name,
                    product.ThumbnailUrl,
                    product.Status);
            }

            foreach (var sku in product.Skus)
            {
                var existingSku = await db.SkuRefs.FindAsync([sku.SkuId], context.CancellationToken);
                if (existingSku is null)
                {
                    db.SkuRefs.Add(new SkuRef(
                        sku.SkuId,
                        sku.ProductId,
                        sku.SkuNo,
                        sku.Price,
                        sku.Currency,
                        sku.ImageUrl,
                        attributes: null,
                        skuName: sku.SkuName));
                }
                else
                {
                    existingSku.Update(
                        sku.SkuNo,
                        sku.Price,
                        sku.Currency,
                        sku.ImageUrl,
                        attributes: null,
                        skuName: sku.SkuName);
                }
            }
        }

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation(
            "Imported product replica sync applied. BundleId={BundleId}, ProductCount={ProductCount}",
            context.Message.BundleId,
            context.Message.Products.Count);
    }
}
