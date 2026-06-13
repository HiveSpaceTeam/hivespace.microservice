namespace HiveSpace.ApiGateway.Tests.Fixtures;

public sealed class GatewayFixture
{
    private static readonly IReadOnlyDictionary<string, string> RouteOwners = new Dictionary<string, string>
    {
        ["/api/v1/accounts"] = "IdentityService",
        ["/api/v1/admins"] = "IdentityService",
        ["/api/v1/users"] = "UserService",
        ["/api/v1/stores"] = "UserService",
        ["/api/v1/categories"] = "CatalogService",
        ["/api/v1/products"] = "CatalogService",
        ["/api/v1/carts"] = "OrderService",
        ["/api/v1/coupons"] = "OrderService",
        ["/api/v1/orders"] = "OrderService",
        ["/api/v1/payments"] = "PaymentService",
        ["/api/v1/wallets"] = "PaymentService",
        ["/api/v1/media"] = "MediaService",
        ["/api/v1/notifications"] = "NotificationService",
        ["/api/v1/notification-preferences"] = "NotificationService",
        ["/hubs/notifications"] = "NotificationService"
    };

    public string? ResolveOwner(string path)
    {
        return RouteOwners
            .Where(route => path.StartsWith(route.Key, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(route => route.Key.Length)
            .Select(route => route.Value)
            .FirstOrDefault();
    }
}
