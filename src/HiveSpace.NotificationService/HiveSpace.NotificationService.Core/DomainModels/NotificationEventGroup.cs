using HiveSpace.Core.Contexts;

namespace HiveSpace.NotificationService.Core.DomainModels;

public static class NotificationEventGroup
{
    public const string OrderUpdates = "order_updates";
    public const string Payment      = "payment";
    public const string Promotions   = "promotions";
    public const string Surveys      = "surveys";
    public const string SellerOrders = "seller_orders";
    public const string Inventory    = "inventory";

    public static readonly IReadOnlyList<string> All =
        [OrderUpdates, Payment, Promotions, Surveys, SellerOrders, Inventory];

    public static readonly IReadOnlyList<string> SellerGroups =
        [SellerOrders, Inventory, OrderUpdates, Payment];

    public static readonly IReadOnlyList<string> BuyerGroups =
        [OrderUpdates, Payment, Promotions, Surveys];

    public static readonly IReadOnlyList<string> AdminGroups = [];

    public static IReadOnlyList<string> ForRole(string? role) => role switch
    {
        "Seller"                      => SellerGroups,
        "Buyer"                       => BuyerGroups,
        "Admin" or "SystemAdmin"      => AdminGroups,
        _                             => [],
    };

    public static IReadOnlyList<string> ForRole(IUserContext ctx)
    {
        if (ctx.IsSeller)                     return SellerGroups;
        if (ctx.IsBuyer)                      return BuyerGroups;
        if (ctx.IsAdmin || ctx.IsSystemAdmin) return AdminGroups;
        return [];
    }

    public static string FromEventType(string eventType) => eventType switch
    {
        NotificationEventType.OrderConfirmed
        or NotificationEventType.OrderCancelled
        or NotificationEventType.OrderDelivered
        or NotificationEventType.OrderShipped       => OrderUpdates,

        NotificationEventType.PaymentSucceeded
        or NotificationEventType.PaymentFailed
        or NotificationEventType.RefundProcessed    => Payment,

        NotificationEventType.NewOrderReceived      => SellerOrders,
        NotificationEventType.LowStockAlert         => Inventory,
        _                                           => Promotions,
    };
}
