using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class PackageDimensionsTests
{
    [Fact]
    public void Constructor_WithValidFields_StoresAllProperties()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        dims.Width.Should().Be(30);
        dims.Height.Should().Be(20);
        dims.Length.Should().Be(10);
        dims.Weight.Should().Be(500);
        dims.ExtraWidth.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithZeroWidth_ThrowsDomainException()
    {
        var act = () => new PackageDimensions(0, 20, 10, 500);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithZeroHeight_ThrowsDomainException()
    {
        var act = () => new PackageDimensions(30, 0, 10, 500);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithZeroLength_ThrowsDomainException()
    {
        var act = () => new PackageDimensions(30, 20, 0, 500);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithZeroWeight_ThrowsDomainException()
    {
        var act = () => new PackageDimensions(30, 20, 10, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void WithActualMeasurements_UpdatesExtraDimensions()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        var updated = dims.WithActualMeasurements(5, 3, 2, 100);

        updated.ExtraWidth.Should().Be(5);
        updated.ExtraHeight.Should().Be(3);
        updated.ExtraLength.Should().Be(2);
        updated.ExtraWeight.Should().Be(100);
        updated.FullWidth.Should().Be(35);
        updated.FullHeight.Should().Be(23);
        updated.FullLength.Should().Be(12);
        updated.FullWeight.Should().Be(600);
    }

    [Fact]
    public void WithActualMeasurements_WithNegativeExtraWidth_ThrowsDomainException()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        var act = () => dims.WithActualMeasurements(-1, 0, 0, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void WithActualMeasurements_WithNegativeExtraHeight_ThrowsDomainException()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        var act = () => dims.WithActualMeasurements(0, -1, 0, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void WithActualMeasurements_WithNegativeExtraLength_ThrowsDomainException()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        var act = () => dims.WithActualMeasurements(0, 0, -1, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void WithActualMeasurements_WithNegativeExtraWeight_ThrowsDomainException()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        var act = () => dims.WithActualMeasurements(0, 0, 0, -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CalculateVolumetricWeight_ReturnsFormulaResult()
    {
        // 60cm × 40cm × 30cm / 6000 = 12
        var dims = new PackageDimensions(40, 30, 60, 500);

        dims.CalculateVolumetricWeight().Should().Be(12);
    }

    [Fact]
    public void GetChargeableWeight_ReturnsHigherOfActualAndVolumetric()
    {
        // Very dense: actual weight 10000g > volumetric
        var dims = new PackageDimensions(10, 10, 10, 10_000);

        dims.GetChargeableWeight().Should().Be(10_000);
    }

    [Fact]
    public void GetChargeableWeight_WhenVolumetricHigher_ReturnsVolumetric()
    {
        // Very light but large: 100cm × 100cm × 100cm = 1,000,000 / 6000 ≈ 166g volumetric > 1g actual
        var dims = new PackageDimensions(100, 100, 100, 1);

        dims.GetChargeableWeight().Should().BeGreaterThan(1);
    }

    [Fact]
    public void HasDiscrepancy_WithExtras_ReturnsTrue()
    {
        var dims = new PackageDimensions(30, 20, 10, 500).WithActualMeasurements(1, 0, 0, 0);

        dims.HasDiscrepancy().Should().BeTrue();
    }

    [Fact]
    public void HasDiscrepancy_WithNoExtras_ReturnsFalse()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        dims.HasDiscrepancy().Should().BeFalse();
    }

    [Fact]
    public void GetWeightIncreasePercentage_WithExtras_ReturnsPositive()
    {
        var dims = new PackageDimensions(10, 10, 10, 100).WithActualMeasurements(0, 0, 0, 100);

        dims.GetWeightIncreasePercentage().Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetWeightIncreasePercentage_WithNoExtras_ReturnsZero()
    {
        var dims = new PackageDimensions(10, 10, 10, 1000);

        dims.GetWeightIncreasePercentage().Should().Be(0);
    }

    [Fact]
    public void CreateStandard_ReturnsDefaultDimensions()
    {
        var dims = PackageDimensions.CreateStandard();

        dims.Width.Should().Be(30);
        dims.Height.Should().Be(20);
        dims.Length.Should().Be(10);
        dims.Weight.Should().Be(500);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var dims = new PackageDimensions(30, 20, 10, 500);

        dims.ToString().Should().Be("10cm × 30cm × 20cm, 500g");
    }

    [Fact]
    public void Equality_SameFullDimensions_AreEqual()
    {
        var d1 = new PackageDimensions(30, 20, 10, 500);
        var d2 = new PackageDimensions(30, 20, 10, 500);

        d1.Should().Be(d2);
    }
}
