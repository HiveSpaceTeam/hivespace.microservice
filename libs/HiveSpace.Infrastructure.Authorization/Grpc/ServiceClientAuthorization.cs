using Microsoft.AspNetCore.Authorization;

namespace HiveSpace.Infrastructure.Authorization.Grpc;

public static class ServiceClientAuthorization
{
    public const string SellerProvisioningPolicy = "RequireSellerProvisioningServiceClient";
    public const string UserServiceSellerProvisioningPolicy = "RequireUserServiceSellerProvisioningClient";
    public const string CatalogServiceSellerProvisioningPolicy = "RequireCatalogServiceSellerProvisioningClient";

    public const string UserServiceClientId = "user-service";
    public const string CatalogServiceClientId = "catalog-service";

    public static bool IsAuthorizedSellerProvisioningClient(AuthorizationHandlerContext context)
    {
        var clientId = context.User.FindFirst("client_id")?.Value;
        return string.Equals(clientId, UserServiceClientId, StringComparison.Ordinal)
               || string.Equals(clientId, CatalogServiceClientId, StringComparison.Ordinal);
    }

    public static bool IsAuthorizedUserServiceSellerProvisioningClient(AuthorizationHandlerContext context)
    {
        var clientId = context.User.FindFirst("client_id")?.Value;
        return string.Equals(clientId, UserServiceClientId, StringComparison.Ordinal);
    }

    public static bool IsAuthorizedCatalogServiceSellerProvisioningClient(AuthorizationHandlerContext context)
    {
        var clientId = context.User.FindFirst("client_id")?.Value;
        return string.Equals(clientId, CatalogServiceClientId, StringComparison.Ordinal);
    }
}
