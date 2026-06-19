using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class SimpleValueObjectTests
{
    [Fact]
    public void ProductCategory_SameId_AreEqual()
    {
        var a = new ProductCategory(10);
        var b = new ProductCategory(10);
        a.Should().Be(b);
    }

    [Fact]
    public void ProductCategory_DifferentId_AreNotEqual()
    {
        var a = new ProductCategory(10);
        var b = new ProductCategory(20);
        a.Should().NotBe(b);
    }

    [Fact]
    public void ProductVariantOption_SameValue_AreEqual()
    {
        var a = new ProductVariantOption("Red");
        var b = new ProductVariantOption("Red");
        a.Should().Be(b);
    }

    [Fact]
    public void ProductVariantOption_DifferentValue_AreNotEqual()
    {
        var a = new ProductVariantOption("Red");
        var b = new ProductVariantOption("Blue");
        a.Should().NotBe(b);
    }

    [Fact]
    public void SkuVariant_SameValue_AreEqual()
    {
        var a = new SkuVariant("Color", "Red");
        var b = new SkuVariant("Color", "Red");
        a.Should().Be(b);
    }

    [Fact]
    public void CategoryAttribute_SameIds_AreEqual()
    {
        var a = new CategoryAttribute(100, 5);
        var b = new CategoryAttribute(100, 5);
        a.Should().Be(b);
    }

    [Fact]
    public void CategoryAttribute_DifferentAttributeId_AreNotEqual()
    {
        var a = new CategoryAttribute(100, 5);
        var b = new CategoryAttribute(200, 5);
        a.Should().NotBe(b);
    }
}
