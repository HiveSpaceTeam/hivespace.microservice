using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal static class ProductSeedReplayPublisher
{
    public static async Task ReplayAsync(
        CatalogDbContext db,
        IProductEventPublisher productEventPublisher,
        IEnumerable<long> productIds,
        CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToArray();
        if (ids.Length == 0)
            return;

        var products = await db.Products
            .Where(product => ids.Contains(product.Id))
            .Include(product => product.Skus)
                .ThenInclude(sku => sku.Images)
            .OrderBy(product => product.Id)
            .ToListAsync(ct);

        foreach (var product in products)
        {
            await productEventPublisher.PublishProductCreatedAsync(product, ct);
            await productEventPublisher.PublishSkuUpdatedAsync(product, ct);
        }
    }
}
