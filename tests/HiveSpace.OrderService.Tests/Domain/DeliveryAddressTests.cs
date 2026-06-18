using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class DeliveryAddressTests
{
    private static PhoneNumber ValidPhone() => new("0901234567");

    [Fact]
    public void Constructor_WithValidFields_StoresAllProperties()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi", "Vietnam", "Leave at door");

        address.RecipientName.Should().Be("Alice");
        address.StreetAddress.Should().Be("123 Main St");
        address.Commune.Should().Be("Ward 1");
        address.Province.Should().Be("Hanoi");
        address.Country.Should().Be("Vietnam");
        address.Notes.Should().Be("Leave at door");
    }

    [Fact]
    public void Constructor_WithBlankCountry_DefaultsToVietnam()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi", "");

        address.Country.Should().Be("Vietnam");
    }

    [Fact]
    public void Constructor_WithEmptyRecipientName_ThrowsDomainException()
    {
        var act = () => new DeliveryAddress("", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithNullPhone_ThrowsDomainException()
    {
        var act = () => new DeliveryAddress("Alice", null!, "123 Main St", "Ward 1", "Hanoi");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithEmptyStreetAddress_ThrowsDomainException()
    {
        var act = () => new DeliveryAddress("Alice", ValidPhone(), "", "Ward 1", "Hanoi");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithEmptyCommune_ThrowsDomainException()
    {
        var act = () => new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "", "Hanoi");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithEmptyProvince_ThrowsDomainException()
    {
        var act = () => new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GetFullAddress_ReturnsCommaJoinedParts()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");

        var full = address.GetFullAddress();

        full.Should().Be("123 Main St, Ward 1, Hanoi, Vietnam");
    }

    [Fact]
    public void GetShippingLabel_WithoutNotes_ExcludesNotesLine()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");

        var label = address.GetShippingLabel();

        label.Should().Contain("Alice");
        label.Should().NotContain("Notes:");
    }

    [Fact]
    public void GetShippingLabel_WithNotes_IncludesNotesLine()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi", "Vietnam", "Ring bell");

        var label = address.GetShippingLabel();

        label.Should().Contain("Notes: Ring bell");
    }

    [Fact]
    public void WithNotes_ReturnsNewInstanceWithUpdatedNotes()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");

        var updated = address.WithNotes("New note");

        updated.Notes.Should().Be("New note");
        address.Notes.Should().BeEmpty();
    }

    [Fact]
    public void Equality_SameStreetCommuneProvinceCountry_AreEqual()
    {
        var a1 = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");
        var a2 = new DeliveryAddress("Bob", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");

        a1.Should().Be(a2);
    }

    [Fact]
    public void Equality_DifferentStreet_AreNotEqual()
    {
        var a1 = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");
        var a2 = new DeliveryAddress("Alice", ValidPhone(), "456 Other St", "Ward 1", "Hanoi");

        a1.Should().NotBe(a2);
    }

    [Fact]
    public void ToString_ReturnsFullAddress()
    {
        var address = new DeliveryAddress("Alice", ValidPhone(), "123 Main St", "Ward 1", "Hanoi");

        address.ToString().Should().Be("123 Main St, Ward 1, Hanoi, Vietnam");
    }
}
