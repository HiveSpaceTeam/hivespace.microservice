using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.DataQueries;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Infrastructure;

public class ProductDataQueryTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetProductDetailAsync_ResolvesStoreByIdInsteadOfOwnerId(bool hasStore)
    {
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var storeId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var product = Product.CreateProduct("Book", "book", "Description", "Short",
            ProductStatus.Available, storeId, ProductCondition.New, false,
            [], [], [], [], [], now, "creator");
        db.Products.Add(product);
        // This other store must never be selected merely because its owner matches the product's store ID.
        db.StoreRef.Add(new StoreRef(Guid.NewGuid(), storeId, "Other store", null, null, "Address", now, now));
        if (hasStore)
            db.StoreRef.Add(new StoreRef(storeId, Guid.NewGuid(), "Bookshop", null, "https://example.com/logo.png", "Address", now, now));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var detail = await new ProductDataQuery(db).GetProductDetailAsync(product.Id);

        detail.Should().NotBeNull();
        if (hasStore)
        {
            detail!.CurrentSeller.Should().NotBeNull();
            detail.CurrentSeller!.Id.Should().Be(storeId);
            detail.CurrentSeller.StoreName.Should().Be("Bookshop");
            detail.CurrentSeller.LogoUrl.Should().Be("https://example.com/logo.png");
        }
        else
            detail!.CurrentSeller.Should().BeNull();
    }
}
