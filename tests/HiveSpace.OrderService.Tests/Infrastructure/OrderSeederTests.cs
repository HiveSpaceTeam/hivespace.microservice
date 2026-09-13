using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Infrastructure;
using HiveSpace.OrderService.Infrastructure.Data;
using HiveSpace.OrderService.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HiveSpace.OrderService.Tests.Infrastructure;

public class OrderSeederTests
{
    [Theory]
    [InlineData("product", 11)]
    [InlineData("sku", 19)]
    [InlineData("mismatch", 3)]
    [InlineData("imported-only", 0)]
    public async Task SeedAsync_WithUnavailableProductSkuPairs_PreservesExistingOrders(string problem, int affectedId)
    {
        OrderIdGeneratorFixture.EnsureInitialized();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<OrderDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddOrderServiceRepositories();
        await using var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<OrderDbContext>();
        var existing = Order.Create(Guid.NewGuid(),
            new DeliveryAddress("Buyer", new PhoneNumber("0901234567"), "Street", "Ward", "City"),
            Guid.NewGuid(), Guid.Parse("aa000001-1111-1111-1111-111111111111"));
        db.Orders.Add(existing);
        if (problem == "imported-only")
        {
            db.ProductRefs.Add(new ProductRef(1, Guid.NewGuid(), "Imported", null, ProductStatus.Available));
            db.SkuRefs.Add(new SkuRef(1, 1, "imported", 10000, "VND", null, null));
        }
        else
        {
            foreach (var id in Enumerable.Range(1, 19).Where(id => id != 15))
            {
                if (problem != "product" || id != affectedId)
                    db.ProductRefs.Add(new ProductRef(1000 + id, Guid.NewGuid(), "Product", null, ProductStatus.Available));
                if (problem != "sku" || id != affectedId)
                    db.SkuRefs.Add(new SkuRef(10000 + id,
                        problem == "mismatch" && id == affectedId ? 999 : 1000 + id,
                        $"sku-{id}", 10000, "VND", null, null));
            }
        }
        await db.SaveChangesAsync();
        var seeder = provider.GetServices<ISeeder>().Single(s => s.Order == 5);

        await seeder.SeedAsync();

        var orders = await db.Orders.AsNoTracking().ToListAsync();
        orders.Should().ContainSingle().Which.Id.Should().Be(existing.Id);
    }
}
