using FluentAssertions;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class DimensionsTests
{
    [Fact]
    public void Create_WithNegativeValue_ThrowsDomainException()
    {
        var act = () => new Dimensions(-1, 10, 10, DimensionUnit.Centimeter);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoDimensionsWithSameValuesAndUnit_AreEqual()
    {
        var a = new Dimensions(10, 20, 30, DimensionUnit.Centimeter);
        var b = new Dimensions(10, 20, 30, DimensionUnit.Centimeter);
        a.Should().Be(b);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var d = new Dimensions(10, 20, 30, DimensionUnit.Centimeter);
        d.ToString().Should().Be("10x20x30 Centimeter");
    }
}
