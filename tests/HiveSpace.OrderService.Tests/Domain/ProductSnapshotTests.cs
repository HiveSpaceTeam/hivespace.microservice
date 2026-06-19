using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class ProductSnapshotTests
{
    [Fact]
    public void Capture_WithValidFields_StoresAllProperties()
    {
        var price = Money.FromVND(100_000);

        var snapshot = ProductSnapshot.Capture(1L, 2L, "Widget", "Widget Red", price, "img.jpg");

        snapshot.ProductId.Should().Be(1L);
        snapshot.SkuId.Should().Be(2L);
        snapshot.ProductName.Should().Be("Widget");
        snapshot.SkuName.Should().Be("Widget Red");
        snapshot.Price.Amount.Should().Be(100_000);
        snapshot.ImageUrl.Should().Be("img.jpg");
        snapshot.CapturedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Capture_WithNullSkuName_FallsBackToProductName()
    {
        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", null!, Money.FromVND(100_000), "img.jpg");

        snapshot.SkuName.Should().Be("Widget");
    }

    [Fact]
    public void Capture_WithAttributes_StoresAttributes()
    {
        var attrs = new Dictionary<string, string> { { "Color", "Red" }, { "Size", "M" } };

        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg", attrs);

        snapshot.HasAttribute("Color").Should().BeTrue();
        snapshot.GetAttributeValue("Color").Should().Be("Red");
    }

    [Fact]
    public void Capture_WithZeroProductId_ThrowsDomainException()
    {
        var act = () => ProductSnapshot.Capture(0L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Capture_WithNegativeProductId_ThrowsDomainException()
    {
        var act = () => ProductSnapshot.Capture(-1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Capture_WithZeroSkuId_ThrowsDomainException()
    {
        var act = () => ProductSnapshot.Capture(1L, 0L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Capture_WithEmptyProductName_ThrowsDomainException()
    {
        var act = () => ProductSnapshot.Capture(1L, 1L, "", "Widget", Money.FromVND(100_000), "img.jpg");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Capture_WithNullPrice_ThrowsDomainException()
    {
        var act = () => ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", null!, "img.jpg");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GetAttributeValue_WithMissingKey_ReturnsNull()
    {
        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        snapshot.GetAttributeValue("NonExistent").Should().BeNull();
    }

    [Fact]
    public void HasAttribute_WithMissingKey_ReturnsFalse()
    {
        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        snapshot.HasAttribute("Color").Should().BeFalse();
    }

    [Fact]
    public void GetDisplayName_WithNoAttributes_ReturnsProductName()
    {
        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        snapshot.GetDisplayName().Should().Be("Widget");
    }

    [Fact]
    public void GetDisplayName_WithAttributes_ReturnsFormattedName()
    {
        var attrs = new Dictionary<string, string> { { "Color", "Red" } };
        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg", attrs);

        snapshot.GetDisplayName().Should().Be("Widget (Color: Red)");
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        var snapshot = ProductSnapshot.Capture(1L, 1L, "Widget", "Widget", Money.FromVND(100_000), "img.jpg");

        snapshot.ToString().Should().Be("Widget");
    }
}
