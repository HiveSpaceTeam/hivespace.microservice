using System.Text.Json;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.ValueObjects;
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

    private static readonly Guid AliceOrder1Id      = new("aa000001-1111-1111-1111-111111111111");
    private static readonly Guid AliceOrder2Id      = new("aa000002-1111-1111-1111-111111111111");
    private static readonly Guid BobOrder1Id        = new("bb000001-2222-2222-2222-222222222222");
    private static readonly Guid BobOrder2Id        = new("bb000002-2222-2222-2222-222222222222");
    private static readonly Guid BobOrder1PaymentId = new("cc000001-3333-3333-3333-333333333333");

    private static readonly Guid AliceOrder4Id = new("aa000004-1111-1111-1111-111111111111");
    private static readonly Guid AliceOrder5Id = new("aa000005-1111-1111-1111-111111111111");
    private static readonly Guid AliceOrder6Id = new("aa000006-1111-1111-1111-111111111111");
    private static readonly Guid BobOrder4Id   = new("bb000004-2222-2222-2222-222222222222");
    private static readonly Guid BobOrder5Id   = new("bb000005-2222-2222-2222-222222222222");
    private static readonly Guid BobOrder6Id   = new("bb000006-2222-2222-2222-222222222222");

    private static readonly Guid AliceOrder4ShippingId = new("dd000004-4444-4444-4444-444444444444");
    private static readonly Guid AliceOrder5ShippingId = new("dd000005-4444-4444-4444-444444444444");
    private static readonly Guid BobOrder4ShippingId   = new("dd000006-4444-4444-4444-444444444444");
    private static readonly Guid BobOrder5ShippingId   = new("dd000007-4444-4444-4444-444444444444");

    private static readonly IReadOnlyDictionary<Guid, string> OngoingCouponCodesByStore =
        new Dictionary<Guid, string>
        {
            [TikiStoreId] = "TKESONG01",
            [GiverStoreId] = "GVESONG01",
            [PDStoreId] = "PDESONG01",
        };

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            var productRefs = await db.ProductRefs.ToDictionaryAsync(p => p.Id, ct);
            var skuRefs = await db.SkuRefs.ToDictionaryAsync(s => s.Id, ct);
            var couponsByCode = await db.Coupons
                .Where(c => OngoingCouponCodesByStore.Values.Contains(c.Code))
                .ToDictionaryAsync(c => c.Code, ct);

            if (productRefs.Count == 0 || skuRefs.Count == 0)
            {
                logger.LogWarning("No ProductRefs or SkuRefs found. Skipping order seeding.");
                return;
            }

            var missingCouponCodes = OngoingCouponCodesByStore.Values
                .Where(code => !couponsByCode.ContainsKey(code))
                .Distinct()
                .ToList();

            if (missingCouponCodes.Count > 0)
                logger.LogWarning("Missing seeded coupon(s) for order discounts: {CouponCodes}", string.Join(", ", missingCouponCodes));

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            Guid[] currentSeedIds =
            [
                AliceOrder1Id, AliceOrder2Id, AliceOrder4Id, AliceOrder5Id, AliceOrder6Id,
                BobOrder1Id,   BobOrder2Id,   BobOrder4Id,   BobOrder5Id,   BobOrder6Id,
            ];

            await db.FulfillmentSagaStates
                .Where(s => currentSeedIds.Contains(s.CorrelationId))
                .ExecuteDeleteAsync(ct);

            await db.Orders
                .Where(o => currentSeedIds.Contains(o.Id))
                .ExecuteDeleteAsync(ct);
            // cascade delete removes order_items automatically (FK_order_items_orders_OrderId ON DELETE CASCADE)

            db.Orders.Add(BuildReadyToShipOrder(AliceOrder1Id, AliceId, "Alice", "0901234567", TikiStoreId,
                1011L, 10011L, 1012L, 10012L, 15000L, TikiOwnerId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildPaidReadyToShipOrder(BobOrder1Id, BobId, "Bob", "0987654321", PDStoreId,
                1003L, 10003L, 1004L, 10004L, 25000L, PDOwnerId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildRejectedOrder(AliceOrder2Id, AliceId, "Alice", "0901234567", GiverStoreId,
                1001L, 10001L, 1002L, 10002L, 20000L, "Out of stock", GiverOwnerId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildRejectedOrder(BobOrder2Id, BobId, "Bob", "0987654321", TikiStoreId,
                1013L, 10013L, 1014L, 10014L, 15000L, "Buyer cancelled", TikiOwnerId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildPendingOrder(AliceOrder6Id, AliceId, "Alice", "0901234567", TikiStoreId,
                1016L, 10016L, 1017L, 10017L, 15000L, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildPendingOrder(BobOrder6Id, BobId, "Bob", "0987654321", GiverStoreId,
                1002L, 10002L, 1010L, 10010L, 20000L, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildShippedOrder(AliceOrder4Id, AliceId, "Alice", "0901234567", PDStoreId,
                1007L, 10007L, 1008L, 10008L, 25000L, PDOwnerId, AliceOrder4ShippingId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildShippedOrder(BobOrder4Id, BobId, "Bob", "0987654321", TikiStoreId,
                1018L, 10018L, 1019L, 10019L, 15000L, TikiOwnerId, BobOrder4ShippingId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildDeliveredOrder(AliceOrder5Id, AliceId, "Alice", "0901234567", GiverStoreId,
                1005L, 10005L, 1006L, 10006L, 20000L, GiverOwnerId, AliceOrder5ShippingId, productRefs, skuRefs, couponsByCode));

            db.Orders.Add(BuildDeliveredOrder(BobOrder5Id, BobId, "Bob", "0987654321", PDStoreId,
                1003L, 10003L, 1009L, 10009L, 25000L, PDOwnerId, BobOrder5ShippingId, productRefs, skuRefs, couponsByCode));

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded Orders via OrderSeeder.");
    }

    private static OrderAggregate BuildReadyToShipOrder(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        Guid confirmedBy,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs,
        IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        var order = Build(orderId, userId, recipientName, phone, storeId, p1Id, s1Id, p2Id, s2Id, shippingFeeCents, productRefs, skuRefs);
        ApplySeedDiscount(order, couponsByCode);
        AddCheckoutAndMarkAsCod(order);
        order.Confirm(confirmedBy);
        return order;
    }

    private static OrderAggregate BuildPaidReadyToShipOrder(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        Guid confirmedBy,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs,
        IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        var order = Build(orderId, userId, recipientName, phone, storeId, p1Id, s1Id, p2Id, s2Id, shippingFeeCents, productRefs, skuRefs);
        ApplySeedDiscount(order, couponsByCode);
        order.AddCheckout(PaymentMethod.BankTransfer, Money.Create(order.TotalAmount.Amount, "VND"));
        order.MarkAsPaid(BobOrder1PaymentId);
        order.Confirm(confirmedBy);
        return order;
    }

    private static OrderAggregate BuildRejectedOrder(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        string reason,
        Guid rejectedBy,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs,
        IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        var order = Build(orderId, userId, recipientName, phone, storeId, p1Id, s1Id, p2Id, s2Id, shippingFeeCents, productRefs, skuRefs);
        ApplySeedDiscount(order, couponsByCode);
        AddCheckoutAndMarkAsCod(order);
        order.Reject(reason, rejectedBy);
        return order;
    }

    private static OrderAggregate BuildPendingOrder(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs,
        IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        var order = Build(orderId, userId, recipientName, phone, storeId, p1Id, s1Id, p2Id, s2Id, shippingFeeCents, productRefs, skuRefs);
        ApplySeedDiscount(order, couponsByCode);
        AddCheckoutAndMarkAsCod(order);
        return order;
    }

    private static OrderAggregate BuildShippedOrder(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        Guid confirmedBy,
        Guid shippingId,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs,
        IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        var order = Build(orderId, userId, recipientName, phone, storeId, p1Id, s1Id, p2Id, s2Id, shippingFeeCents, productRefs, skuRefs);
        ApplySeedDiscount(order, couponsByCode);
        AddCheckoutAndMarkAsCod(order);
        order.Confirm(confirmedBy);
        order.AssignShipping(shippingId);
        order.Ship();
        return order;
    }

    private static OrderAggregate BuildDeliveredOrder(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        Guid confirmedBy,
        Guid shippingId,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs,
        IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        var order = BuildShippedOrder(orderId, userId, recipientName, phone, storeId, p1Id, s1Id, p2Id, s2Id, shippingFeeCents, confirmedBy, shippingId, productRefs, skuRefs, couponsByCode);
        order.MarkAsDelivered();
        return order;
    }

    private static OrderAggregate Build(
        Guid orderId,
        Guid userId,
        string recipientName,
        string phone,
        Guid storeId,
        long p1Id,
        long s1Id,
        long p2Id,
        long s2Id,
        long shippingFeeCents,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs)
    {
        var address = new DeliveryAddress(
            recipientName,
            new PhoneNumber(phone),
            userId == AliceId ? "123 Main St" : "456 Side St",
            userId == AliceId ? "Commune 1" : "Commune 2",
            "HCM City");

        var order = OrderAggregate.Create(userId, address, storeId, orderId);
        AddItem(order, p1Id, s1Id, productRefs, skuRefs);
        AddItem(order, p2Id, s2Id, productRefs, skuRefs);
        order.SetShippingFee(Money.Create(shippingFeeCents, "VND"), false);
        return order;
    }

    private static void AddItem(
        OrderAggregate order,
        long pId,
        long sId,
        Dictionary<long, ProductRef> productRefs,
        Dictionary<long, SkuRef> skuRefs)
    {
        if (!productRefs.TryGetValue(pId, out var productRef) || !skuRefs.TryGetValue(sId, out var skuRef))
            return;

        var unitPrice = Money.Create(skuRef.Price, skuRef.Currency);
        var snapshotPrice = Money.Create(skuRef.Price, skuRef.Currency);
        var snapshot = ProductSnapshot.Capture(
            pId,
            sId,
            productRef.Name,
            string.IsNullOrWhiteSpace(skuRef.SkuName) ? productRef.Name : skuRef.SkuName,
            snapshotPrice,
            skuRef.ImageUrl ?? productRef.ThumbnailUrl ?? string.Empty,
            ParseSnapshotAttributes(skuRef.Attributes));

        order.AddItem(pId, sId, 1, unitPrice, snapshot, isCOD: true);
    }

    private static void AddCheckoutAndMarkAsCod(OrderAggregate order)
    {
        order.AddCheckout(PaymentMethod.COD, Money.Create(order.TotalAmount.Amount, "VND"));
        order.MarkAsCOD();
    }

    private static void ApplySeedDiscount(OrderAggregate order, IReadOnlyDictionary<string, Coupon> couponsByCode)
    {
        if (!OngoingCouponCodesByStore.TryGetValue(order.StoreId, out var couponCode))
            return;

        if (!couponsByCode.TryGetValue(couponCode, out var coupon))
            return;

        order.ApplyDiscount(coupon);
    }

    private static Dictionary<string, string> ParseSnapshotAttributes(string? rawAttributes)
    {
        if (string.IsNullOrWhiteSpace(rawAttributes))
            return [];

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(rawAttributes) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
