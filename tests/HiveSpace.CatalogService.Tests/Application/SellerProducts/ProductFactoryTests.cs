using FluentAssertions;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Helpers;
using HiveSpace.Domain.Shared.ValueObjects;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.SellerProducts;

public class ProductFactoryTests
{
    [Fact]
    public void CreateProductCategories_WhenCategoryIdIsZero_ReturnsEmpty()
    {
        var result = ProductFactory.CreateProductCategories(0);

        result.Should().BeEmpty();
    }

    [Fact]
    public void CreateProductVariants_WhenOptionsIsNull_CreatesVariantWithNoOptions()
    {
        var result = ProductFactory.CreateProductVariants(
        [
            new ProductVariantRequestDto(0, "Size", Options: null)
        ]);

        result.Should().ContainSingle();
        result[0].Options.Should().BeEmpty();
    }

    [Fact]
    public void CreateProductSkus_WhenSkuVariantsIsNull_CreatesSkuWithNoVariants()
    {
        var result = ProductFactory.CreateProductSkus(
        [
            new ProductSkuRequestDto(0, null, Money.FromVND(100_000), 5, "SKU-001")
        ]);

        result.Should().ContainSingle();
        result[0].SkuVariants.Should().BeEmpty();
    }

    [Fact]
    public void CreateProductSkus_WhenSkuNoIsNull_UsesEmptyString()
    {
        var result = ProductFactory.CreateProductSkus(
        [
            new ProductSkuRequestDto(0, [], Money.FromVND(100_000), 3, null!)
        ]);

        result.Should().ContainSingle();
        result[0].SkuNo.Should().BeEmpty();
    }

    [Theory]
    [InlineData("USD", 1050L)]
    [InlineData("EUR", 2505L)]
    public void CreateProductSkus_WhenPriceUsesSmallestUnitForeignCurrency_PreservesExactAmount(string currencyCode, long smallestUnitAmount)
    {
        var result = ProductFactory.CreateProductSkus(
        [
            new ProductSkuRequestDto(0, [], Money.Create(smallestUnitAmount, currencyCode), 3, "SKU-FOREIGN")
        ]);

        result.Should().ContainSingle();
        result[0].Price.Amount.Should().Be(smallestUnitAmount);
        result[0].Price.Currency.ToString().Should().Be(currencyCode);
    }
}
