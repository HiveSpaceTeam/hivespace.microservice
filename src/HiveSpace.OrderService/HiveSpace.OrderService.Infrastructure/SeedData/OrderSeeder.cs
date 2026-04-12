using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderAggregate = HiveSpace.OrderService.Domain.Aggregates.Orders.Order;

namespace HiveSpace.OrderService.Infrastructure.SeedData;

internal sealed class OrderSeeder(OrderDbContext db, ILogger<OrderSeeder> logger) : ISeeder
{
    public int Order => 5; // After CartSeeder

    private static readonly Guid AliceId = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BobId   = new Guid("22222222-2222-2222-2222-222222222222");

    private static readonly Guid TikiStoreId  = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid GiverStoreId = new Guid("e5f6a7b8-c9d0-1234-ef01-234567890123");
    private static readonly Guid PDStoreId    = new Guid("f6a7b8c9-d0e1-2345-f012-345678901234");

    private static readonly Guid StoreAdminId = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    // Fixed order IDs for deterministic seeding
    private static readonly Guid AliceOrder1Id = new Guid("aa000001-1111-1111-1111-111111111111");
    private static readonly Guid AliceOrder2Id = new Guid("aa000002-1111-1111-1111-111111111111");
    private static readonly Guid BobOrder1Id   = new Guid("bb000001-2222-2222-2222-222222222222");
    private static readonly Guid BobOrder2Id   = new Guid("bb000002-2222-2222-2222-222222222222");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existingOrders = await db.Orders.AnyAsync(ct);
        if (existingOrders)
        {
            logger.LogDebug("Orders already seeded. Skipping.");
            return;
        }

        var productRefs = await db.ProductRefs.ToDictionaryAsync(p => p.Id, ct);
        var skuRefs = await db.SkuRefs.ToDictionaryAsync(s => s.Id, ct);

        if (productRefs.Count == 0 || skuRefs.Count == 0)
        {
             logger.LogWarning("No ProductRefs or SkuRefs found to seed Orders.");
             return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // Alice Order 1: Tiki, Confirmed
            var aliceOrder1 = CreateOrder(AliceOrder1Id, AliceId,
                new DeliveryAddress("Alice", new PhoneNumber("0901234567"), "123 Main St", "District 1", "HCM City"),
                TikiStoreId, 1011L, 10011L, 1012L, 10012L, productRefs, skuRefs);
            aliceOrder1.SetShippingFee(Money.Create(15000, "VND"), false);
            aliceOrder1.MarkAsCOD();
            aliceOrder1.Confirm(StoreAdminId);

            // Alice Order 2: Giver, Rejected
            var aliceOrder2 = CreateOrder(AliceOrder2Id, AliceId,
                new DeliveryAddress("Alice", new PhoneNumber("0901234567"), "123 Main St", "District 1", "HCM City"),
                GiverStoreId, 1001L, 10001L, 1002L, 10002L, productRefs, skuRefs);
            aliceOrder2.SetShippingFee(Money.Create(20000, "VND"), false);
            aliceOrder2.MarkAsCOD();
            aliceOrder2.Reject("Out of stock", StoreAdminId);

            // Bob Order 1: PhuongDong, Confirmed
            var bobOrder1 = CreateOrder(BobOrder1Id, BobId,
                new DeliveryAddress("Bob", new PhoneNumber("0987654321"), "456 Side St", "District 2", "HCM City"),
                PDStoreId, 1003L, 10003L, 1004L, 10004L, productRefs, skuRefs);
            bobOrder1.SetShippingFee(Money.Create(25000, "VND"), false);
            bobOrder1.AddCheckout(PaymentMethod.BankTransfer, new Money(bobOrder1.TotalAmount.Amount, bobOrder1.TotalAmount.Currency));
            bobOrder1.MarkAsPaid(Guid.NewGuid());
            bobOrder1.Confirm(StoreAdminId);

            // Bob Order 2: Tiki, Rejected
            var bobOrder2 = CreateOrder(BobOrder2Id, BobId,
                new DeliveryAddress("Bob", new PhoneNumber("0987654321"), "456 Side St", "District 2", "HCM City"),
                TikiStoreId, 1013L, 10013L, 1014L, 10014L, productRefs, skuRefs);
            bobOrder2.SetShippingFee(Money.Create(15000, "VND"), false);
            bobOrder2.MarkAsCOD();
            bobOrder2.Reject("Customer cancelled", StoreAdminId);

            db.Orders.AddRange(aliceOrder1, aliceOrder2, bobOrder1, bobOrder2);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded 4 Orders via OrderSeeder.");
    }

    private OrderAggregate CreateOrder(
        Guid orderId, Guid userId, DeliveryAddress address, Guid storeId,
        long p1Id, long s1Id, long p2Id, long s2Id,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs)
    {
        var order = OrderAggregate.Create(userId, address, storeId, orderId);
        AddItemInternal(order, p1Id, s1Id, productRefs, skuRefs);
        AddItemInternal(order, p2Id, s2Id, productRefs, skuRefs);
        return order;
    }

    private void AddItemInternal(OrderAggregate order, long pId, long sId,
         Dictionary<long, ProductRef> productRefs,
         Dictionary<long, SkuRef> skuRefs)
    {
        if (productRefs.TryGetValue(pId, out var p) && skuRefs.TryGetValue(sId, out var s))
        {
            var unitPrice = Money.Create(s.Price, s.Currency);
            var snapshotPrice = Money.Create(s.Price, s.Currency); // separate instance for EF tracking
            var snapshot = ProductSnapshot.Capture(pId, sId, p.Name, p.Name, snapshotPrice, p.ThumbnailUrl ?? string.Empty);
            order.AddItem(pId, sId, 1, unitPrice, snapshot, isCOD: true);
        }
    }
}
