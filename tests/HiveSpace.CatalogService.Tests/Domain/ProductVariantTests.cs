using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class ProductVariantTests
{
    [Fact]
    public void Create_WithName_SetsName()
    {
        var variant = new ProductVariant("Color");
        variant.Name.Should().Be("Color");
        variant.Options.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithIdAndOptions_SetsAllProperties()
    {
        var options = new List<ProductVariantOption> { new("Red"), new("Blue") };
        var variant = new ProductVariant(1, "Color", options);
        variant.Id.Should().Be(1);
        variant.Name.Should().Be("Color");
        variant.Options.Should().HaveCount(2);
    }

    [Fact]
    public void AddOption_WithLabel_AppearsInOptions()
    {
        var variant = new ProductVariant("Color");
        variant.AddOption("Red");
        variant.Options.Should().ContainSingle(o => o.Value == "Red");
    }

    [Fact]
    public void AddOptions_WithMultipleOptions_AllAppearInCollection()
    {
        var variant = new ProductVariant("Size");
        variant.AddOptions([new ProductVariantOption("S"), new ProductVariantOption("M"), new ProductVariantOption("L")]);
        variant.Options.Should().HaveCount(3);
    }
}
