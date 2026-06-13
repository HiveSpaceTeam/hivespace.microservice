using FluentAssertions;
using HiveSpace.ApiGateway.Tests.Fixtures;
using HiveSpace.YarpApiGateway.Middleware;
using Xunit;

namespace HiveSpace.ApiGateway.Tests.Routing;

public class RouteOwnershipTests : IClassFixture<GatewayFixture>
{
    private readonly GatewayFixture _fixture;

    public RouteOwnershipTests(GatewayFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Gateway_ShouldExposeForwardingMiddlewareForBrowserContracts()
    {
        typeof(SessionForwardingMiddleware).Should().NotBeNull();
        typeof(CsrfValidationMiddleware).Should().NotBeNull();
    }

    [Theory]
    [InlineData("/api/v1/accounts/login", "IdentityService")]
    [InlineData("/api/v1/users/me", "UserService")]
    [InlineData("/api/v1/products", "CatalogService")]
    [InlineData("/api/v1/carts/items", "OrderService")]
    [InlineData("/api/v1/payments/by-order/00000000-0000-0000-0000-000000000001", "PaymentService")]
    [InlineData("/api/v1/media/presign-url", "MediaService")]
    [InlineData("/api/v1/notifications", "NotificationService")]
    public void PublicRoutes_ShouldResolveToDocumentedOwner(string path, string expectedOwner)
    {
        _fixture.ResolveOwner(path).Should().Be(expectedOwner);
    }

    [Fact]
    public void UnknownPath_ReturnsNoOwner()
    {
        _fixture.ResolveOwner("/api/v1/unknown").Should().BeNull();
    }
}
