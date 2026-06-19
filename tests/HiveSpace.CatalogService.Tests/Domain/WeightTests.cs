using FluentAssertions;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class WeightTests
{
    [Fact]
    public void Create_WithNegativeValue_ThrowsDomainException()
    {
        var act = () => new Weight(-1, WeightUnit.Gram);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoWeightsWithSameValueAndUnit_AreEqual()
    {
        var a = new Weight(500, WeightUnit.Gram);
        var b = new Weight(500, WeightUnit.Gram);
        a.Should().Be(b);
    }

    [Fact]
    public void ToGrams_WithGram_ReturnsSameValue()
    {
        var w = new Weight(500, WeightUnit.Gram);
        w.ToGrams().Should().Be(500m);
    }

    [Fact]
    public void ToGrams_WithKilogram_ReturnsValueMultipliedBy1000()
    {
        var w = new Weight(1, WeightUnit.Kilogram);
        w.ToGrams().Should().Be(1000m);
    }

    [Fact]
    public void ToGrams_WithPound_ReturnsCorrectConversion()
    {
        var w = new Weight(1, WeightUnit.Pound);
        w.ToGrams().Should().Be(453.592m);
    }

    [Fact]
    public void ToGrams_WithOunce_ReturnsCorrectConversion()
    {
        var w = new Weight(1, WeightUnit.Ounce);
        w.ToGrams().Should().Be(28.3495m);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var w = new Weight(500, WeightUnit.Gram);
        w.ToString().Should().Be("500 Gram");
    }

    [Fact]
    public void ToGrams_WithUnknownUnit_ReturnsSameValue()
    {
        var w = new Weight(100, (WeightUnit)99);
        w.ToGrams().Should().Be(100m);
    }
}
