using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Media;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Api.Consumers.Sync;

public class MediaAssetProcessedConsumer(
    CatalogDbContext dbContext,
    IProductEventPublisher productEventPublisher,
    ILogger<MediaAssetProcessedConsumer> logger)
    : IConsumer<MediaAssetProcessedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<MediaAssetProcessedIntegrationEvent> context)
    {
        var msg = context.Message;
        var fileId = msg.FileId.ToString();
        var ct = context.CancellationToken;

        switch (msg.EntityType)
        {
            case "product_thumbnail":
                await HandleProductThumbnailAsync(fileId, msg.PublicUrl, ct);
                break;

            case "product_image":
                await HandleProductImageAsync(fileId, msg.PublicUrl, ct);
                break;

            case "sku_image":
                await HandleSkuImageAsync(fileId, msg.PublicUrl, ct);
                break;

            case "category":
                await HandleCategoryAsync(fileId, msg.PublicUrl, ct);
                break;

            default:
                logger.LogDebug("EntityType {EntityType} not handled by CatalogService consumer.", msg.EntityType);
                break;
        }
    }

    private async Task HandleProductThumbnailAsync(string fileId, string publicUrl, CancellationToken ct)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.ThumbnailFileId == fileId, ct);

        if (product is null) return;

        product.SetThumbnailUrl(publicUrl);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Updated ThumbnailUrl for product {ProductId}.", product.Id);
    }

    private async Task HandleProductImageAsync(string fileId, string publicUrl, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Images.Any(i => i.FileId == fileId), ct);

        if (product is null) return;

        product.UpdateProductImageUrl(fileId, publicUrl);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Updated ImageUrl for product image. FileId={FileId}.", fileId);
    }

    private async Task HandleSkuImageAsync(string fileId, string publicUrl, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Skus).ThenInclude(s => s.Images)
            .Include(p => p.Skus).ThenInclude(s => s.SkuVariants)
            .FirstOrDefaultAsync(p => p.Skus.Any(s => s.Images.Any(i => i.FileId == fileId)), ct);

        if (product is null) return;

        var sku = product.Skus.First(s => s.Images.Any(i => i.FileId == fileId));
        sku.UpdateSkuImageUrl(fileId, publicUrl);
        await dbContext.SaveChangesAsync(ct);

        await productEventPublisher.PublishSkuUpdatedAsync(product, ct);
        logger.LogInformation("Updated ImageUrl for sku image and re-published SKU event. FileId={FileId}.", fileId);
    }

    private async Task HandleCategoryAsync(string fileId, string publicUrl, CancellationToken ct)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.ImageFileId == fileId, ct);

        if (category is null) return;

        category.SetImageUrl(publicUrl);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Updated ImageUrl for category {CategoryId}.", category.Id);
    }
}
