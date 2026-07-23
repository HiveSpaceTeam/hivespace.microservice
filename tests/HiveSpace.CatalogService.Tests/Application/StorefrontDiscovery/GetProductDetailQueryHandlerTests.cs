using FluentAssertions;
using HiveSpace.Application.Shared.Dtos;
using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductDetail;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Exceptions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.StorefrontDiscovery;

public class GetProductDetailQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    public GetProductDetailQueryHandlerTests(CatalogServiceFixture fixture) { }

    [Fact]
    public async Task Handle_WithExistingProduct_ReturnsProductDetailDto()
    {
        var dto = new ProductDetailDto
        {
            Id = 1,
            Name = "Blue Widget",
            Description = "A great widget",
            SellerId = Guid.NewGuid(),
            Skus =
            [
                new ProductSkuDto(
                    10,
                    "SKU-USD",
                    "USD SKU",
                    MoneyResponseDto.Valid(2_505, "USD"),
                    3,
                    true,
                    [new ProductImageDto("file-1", "https://cdn.example.com/sku-1.png")],
                    null)
            ]
        };
        var handler = new GetProductDetailQueryHandler(new FakeProductDataQuery(dto));

        var result = await handler.Handle(new GetProductDetailQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Blue Widget");
        result.Skus.Should().ContainSingle();
        result.Skus[0].Price.CurrencyCode.Should().Be("USD");
        result.Skus[0].Price.IsValid.Should().BeTrue();
        result.Skus[0].Images.Should().ContainSingle();
        result.Skus[0].Images[0].ImageUrl.Should().Be("https://cdn.example.com/sku-1.png");
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ThrowsNotFoundException()
    {
        var handler = new GetProductDetailQueryHandler(new FakeProductDataQuery(null));

        var act = () => handler.Handle(new GetProductDetailQuery(99999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
