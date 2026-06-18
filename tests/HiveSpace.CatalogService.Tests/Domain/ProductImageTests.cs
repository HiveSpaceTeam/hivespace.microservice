using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Domain.Shared.Exceptions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class ProductImageTests
{
    [Fact]
    public void ProductImage_Create_WithImageUrl_SetsUrl()
    {
        var image = new ProductImage(1, "file-001", "http://initial.url");
        image.ProductId.Should().Be(1);
        image.FileId.Should().Be("file-001");
        image.ImageUrl.Should().Be("http://initial.url");
    }

    [Fact]
    public void ProductImage_WithImageUrl_CreatesNewInstanceWithUrl()
    {
        var image = new ProductImage(1, "file-001");
        var updated = image.WithImageUrl("http://new.url");
        updated.ImageUrl.Should().Be("http://new.url");
        updated.FileId.Should().Be("file-001");
        updated.ProductId.Should().Be(1);
    }

    [Fact]
    public void ProductImage_WithEmptyUrl_ThrowsInvalidFieldException()
    {
        var image = new ProductImage(1, "file-001");
        var act = () => image.WithImageUrl("");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void ProductImage_WithWhitespaceUrl_ThrowsInvalidFieldException()
    {
        var image = new ProductImage(1, "file-001");
        var act = () => image.WithImageUrl("  ");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void ProductImage_SameProductIdAndFileId_AreEqual()
    {
        var a = new ProductImage(1, "file-001");
        var b = new ProductImage(1, "file-001");
        a.Should().Be(b);
    }

    [Fact]
    public void ProductImage_DifferentFileId_AreNotEqual()
    {
        var a = new ProductImage(1, "file-001");
        var b = new ProductImage(1, "file-002");
        a.Should().NotBe(b);
    }

    [Fact]
    public void SkuImage_Create_WithImageUrl_SetsUrl()
    {
        var image = new SkuImage("file-001", "http://initial.url");
        image.FileId.Should().Be("file-001");
        image.ImageUrl.Should().Be("http://initial.url");
    }

    [Fact]
    public void SkuImage_WithImageUrl_CreatesNewInstanceWithUrl()
    {
        var image = new SkuImage("file-001");
        var updated = image.WithImageUrl("http://sku.url");
        updated.ImageUrl.Should().Be("http://sku.url");
        updated.FileId.Should().Be("file-001");
    }

    [Fact]
    public void SkuImage_WithEmptyUrl_ThrowsInvalidFieldException()
    {
        var image = new SkuImage("file-001");
        var act = () => image.WithImageUrl("");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void SkuImage_WithWhitespaceUrl_ThrowsInvalidFieldException()
    {
        var image = new SkuImage("file-001");
        var act = () => image.WithImageUrl(" ");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void SkuImage_SameFileId_AreEqual()
    {
        var a = new SkuImage("file-001");
        var b = new SkuImage("file-001");
        a.Should().Be(b);
    }
}
