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
}
