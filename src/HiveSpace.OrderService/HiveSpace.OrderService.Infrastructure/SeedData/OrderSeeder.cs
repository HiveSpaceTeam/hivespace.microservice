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

    private static readonly Guid AliceId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid BobId   = new("22222222-2222-2222-2222-222222222222");

    private static readonly Guid TikiStoreId  = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid GiverStoreId = new("e5f6a7b8-c9d0-1234-ef01-234567890123");
    private static readonly Guid PDStoreId    = new("f6a7b8c9-d0e1-2345-f012-345678901234");

    private static readonly Guid TikiOwnerId  = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid GiverOwnerId = new("c3d4e5f6-a7b8-9012-cdef-012345678901");
    private static readonly Guid PDOwnerId    = new("d4e5f6a7-b8c9-0123-def0-123456789012");

    // ── Existing orders (ReadyToShip × 2, ReturnedOrCancelled × 2) ──────────────────
    private static readonly Guid AliceOrder1Id      = new("aa000001-1111-1111-1111-111111111111"); // Tiki,   Confirmed  (ReadyToShip)
    private static readonly Guid AliceOrder2Id      = new("aa000002-1111-1111-1111-111111111111"); // Giver,  Rejected   (ReturnedOrCancelled)
    private static readonly Guid BobOrder1Id        = new("bb000001-2222-2222-2222-222222222222"); // PD,     Confirmed  (ReadyToShip)
    private static readonly Guid BobOrder2Id        = new("bb000002-2222-2222-2222-222222222222"); // Tiki,   Rejected   (ReturnedOrCancelled)
    private static readonly Guid BobOrder1PaymentId = new("cc000001-3333-3333-3333-333333333333");

    // ── Stable orders (idempotent) ────────────────────────────────────────────
    private static readonly Guid AliceOrder4Id = new("aa000004-1111-1111-1111-111111111111"); // PD,     Shipped    (Shipping)
    private static readonly Guid AliceOrder5Id = new("aa000005-1111-1111-1111-111111111111"); // Giver,  Delivered  (Delivered)
    private static readonly Guid BobOrder4Id   = new("bb000004-2222-2222-2222-222222222222"); // Tiki,   Shipped    (Shipping)
    private static readonly Guid BobOrder5Id   = new("bb000005-2222-2222-2222-222222222222"); // PD,     Delivered  (Delivered)

    private static readonly Guid AliceOrder4ShippingId = new("dd000004-4444-4444-4444-444444444444");
    private static readonly Guid AliceOrder5ShippingId = new("dd000005-4444-4444-4444-444444444444");
    private static readonly Guid BobOrder4ShippingId   = new("dd000006-4444-4444-4444-444444444444");
    private static readonly Guid BobOrder5ShippingId   = new("dd000007-4444-4444-4444-444444444444");

    // PendingConfirmation orders are excluded — they are always reset on startup with Guid.NewGuid()
    private static readonly Guid[] StableOrderIds =
    [
        AliceOrder1Id, AliceOrder2Id, AliceOrder4Id, AliceOrder5Id,
        BobOrder1Id,   BobOrder2Id,   BobOrder4Id,   BobOrder5Id,
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var productRefs = await db.ProductRefs.ToDictionaryAsync(p => p.Id, ct);
            var skuRefs     = await db.SkuRefs.ToDictionaryAsync(s => s.Id, ct);

            if (productRefs.Count == 0 || skuRefs.Count == 0)
            {
                logger.LogWarning("No ProductRefs or SkuRefs found. Skipping order seeding.");
                return;
            }

            var currentIds = await db.Orders
                .Where(o => StableOrderIds.Contains(o.Id))
                .Select(o => o.Id)
                .ToListAsync(ct);
            var currentIdSet = currentIds.ToHashSet();

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            // ── ReadyToShip ×2 ────────────────────────────────────────────────
            if (!currentIdSet.Contains(AliceOrder1Id))
            {
                var o = Build(AliceOrder1Id, AliceId, "Alice", "0901234567", TikiStoreId,
                              1011L, 10011L, 1012L, 10012L, 15000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Confirm(TikiOwnerId);
                db.Orders.Add(o);
            }

            if (!currentIdSet.Contains(BobOrder1Id))
            {
                var o = Build(BobOrder1Id, BobId, "Bob", "0987654321", PDStoreId,
                              1003L, 10003L, 1004L, 10004L, 25000L, productRefs, skuRefs);
                o.AddCheckout(PaymentMethod.BankTransfer, Money.Create(o.TotalAmount.Amount, "VND"));
                o.MarkAsPaid(BobOrder1PaymentId);
                o.Confirm(PDOwnerId);
                db.Orders.Add(o);
            }

            // ── ReturnedOrCancelled ×2 ───────────────────────────────────────────────
            if (!currentIdSet.Contains(AliceOrder2Id))
            {
                var o = Build(AliceOrder2Id, AliceId, "Alice", "0901234567", GiverStoreId,
                              1001L, 10001L, 1002L, 10002L, 20000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Reject("Out of stock", GiverOwnerId);
                db.Orders.Add(o);
            }

            if (!currentIdSet.Contains(BobOrder2Id))
            {
                var o = Build(BobOrder2Id, BobId, "Bob", "0987654321", TikiStoreId,
                              1013L, 10013L, 1014L, 10014L, 15000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Reject("Buyer cancelled", TikiOwnerId);
                db.Orders.Add(o);
            }

            // ── PendingConfirmation ×2 (always reset — fresh GUIDs prevent notification dedup) ────
            var stalePending = await db.Orders
                .Where(o => (o.UserId == AliceId && o.StoreId == TikiStoreId)
                         || (o.UserId == BobId   && o.StoreId == GiverStoreId))
                .ToListAsync(ct);

            var stalePendingCod = stalePending.Where(o => o.Status == OrderStatus.COD).ToList();
            if (stalePendingCod.Count > 0)
            {
                var staleIds = stalePendingCod.Select(o => o.Id).ToList();
                await db.FulfillmentSagaStates
                    .Where(s => staleIds.Contains(s.CorrelationId))
                    .ExecuteDeleteAsync(ct);
                db.Orders.RemoveRange(stalePendingCod);
            }

            var aliceOrder3 = Build(Guid.NewGuid(), AliceId, "Alice", "0901234567", TikiStoreId,
                                    1016L, 10016L, 1017L, 10017L, 15000L, productRefs, skuRefs);
            aliceOrder3.MarkAsCOD();
            db.Orders.Add(aliceOrder3);

            var bobOrder3 = Build(Guid.NewGuid(), BobId, "Bob", "0987654321", GiverStoreId,
                                  1002L, 10002L, 1010L, 10010L, 20000L, productRefs, skuRefs);
            bobOrder3.MarkAsCOD();
            db.Orders.Add(bobOrder3);

            // ── Shipping ×2 ───────────────────────────────────────────────────
            if (!currentIdSet.Contains(AliceOrder4Id))
            {
                var o = Build(AliceOrder4Id, AliceId, "Alice", "0901234567", PDStoreId,
                              1007L, 10007L, 1008L, 10008L, 25000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Confirm(PDOwnerId);
                o.AssignShipping(AliceOrder4ShippingId);
                o.Ship();
                db.Orders.Add(o);
            }

            if (!currentIdSet.Contains(BobOrder4Id))
            {
                var o = Build(BobOrder4Id, BobId, "Bob", "0987654321", TikiStoreId,
                              1018L, 10018L, 1019L, 10019L, 15000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Confirm(TikiOwnerId);
                o.AssignShipping(BobOrder4ShippingId);
                o.Ship();
                db.Orders.Add(o);
            }

            // ── Delivered ×2 ─────────────────────────────────────────────────
            if (!currentIdSet.Contains(AliceOrder5Id))
            {
                var o = Build(AliceOrder5Id, AliceId, "Alice", "0901234567", GiverStoreId,
                              1005L, 10005L, 1006L, 10006L, 20000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Confirm(GiverOwnerId);
                o.AssignShipping(AliceOrder5ShippingId);
                o.Ship();
                o.MarkAsDelivered();
                db.Orders.Add(o);
            }

            if (!currentIdSet.Contains(BobOrder5Id))
            {
                var o = Build(BobOrder5Id, BobId, "Bob", "0987654321", PDStoreId,
                              1003L, 10003L, 1009L, 10009L, 25000L, productRefs, skuRefs);
                o.MarkAsCOD();
                o.Confirm(PDOwnerId);
                o.AssignShipping(BobOrder5ShippingId);
                o.Ship();
                o.MarkAsDelivered();
                db.Orders.Add(o);
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded Orders via OrderSeeder.");
    }

    private static OrderAggregate Build(
        Guid orderId, Guid userId, string recipientName, string phone,
        Guid storeId, long p1Id, long s1Id, long p2Id, long s2Id,
        long shippingFeeCents,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs)
    {
        var address = new DeliveryAddress(recipientName, new PhoneNumber(phone),
            userId == AliceId ? "123 Main St" : "456 Side St",
            userId == AliceId ? "Commune 1"  : "Commune 2",
            "HCM City");

        var order = OrderAggregate.Create(userId, address, storeId, orderId);
        AddItem(order, p1Id, s1Id, productRefs, skuRefs);
        AddItem(order, p2Id, s2Id, productRefs, skuRefs);
        order.SetShippingFee(Money.Create(shippingFeeCents, "VND"), false);
        return order;
    }

    private static void AddItem(OrderAggregate order, long pId, long sId,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs)
    {
        if (!productRefs.TryGetValue(pId, out var p) || !skuRefs.TryGetValue(sId, out var s))
            return;

        var unitPrice     = Money.Create(s.Price, s.Currency);
        var snapshotPrice = Money.Create(s.Price, s.Currency);
        var snapshot      = ProductSnapshot.Capture(
            pId, sId,
            p.Name,
            string.IsNullOrWhiteSpace(s.SkuName) ? p.Name : s.SkuName,
            snapshotPrice,
            s.ImageUrl ?? p.ThumbnailUrl ?? string.Empty);

        order.AddItem(pId, sId, 1, unitPrice, snapshot, isCOD: true);
    }
}
