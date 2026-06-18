using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class PhoneNumberTests
{
    [Fact]
    public void Constructor_WithLocalFormat_StoresCleanedValue()
    {
        var phone = new PhoneNumber("0901234567");

        phone.Value.Should().Be("0901234567");
    }

    [Fact]
    public void Constructor_WithPlusPrefixFormat_StoresValue()
    {
        var phone = new PhoneNumber("+84901234567");

        phone.Value.Should().Be("+84901234567");
    }

    [Fact]
    public void Constructor_With84PrefixFormat_StoresValue()
    {
        var phone = new PhoneNumber("84901234567");

        phone.Value.Should().Be("84901234567");
    }

    [Fact]
    public void Constructor_WithSpacesAndDashes_StripsAndValidates()
    {
        var phone = new PhoneNumber("090-123 4567");

        phone.Value.Should().Be("0901234567");
    }

    [Fact]
    public void Constructor_WithEmptyValue_ThrowsDomainException()
    {
        var act = () => new PhoneNumber("");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithWhitespaceValue_ThrowsDomainException()
    {
        var act = () => new PhoneNumber("   ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithInvalidFormat_ThrowsDomainException()
    {
        var act = () => new PhoneNumber("12345");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithLandline_ThrowsDomainException()
    {
        // Vietnam landlines start with 02 — not supported by this VO
        var act = () => new PhoneNumber("0211234567");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GetDisplayFormat_ForLocalNumber_ReturnsSame()
    {
        var phone = new PhoneNumber("0901234567");

        phone.GetDisplayFormat().Should().Be("0901234567");
    }

    [Fact]
    public void GetDisplayFormat_ForPlusPrefixNumber_ConvertsToLocal()
    {
        var phone = new PhoneNumber("+84901234567");

        phone.GetDisplayFormat().Should().Be("0901234567");
    }

    [Fact]
    public void GetDisplayFormat_For84PrefixNumber_ConvertsToLocal()
    {
        var phone = new PhoneNumber("84901234567");

        phone.GetDisplayFormat().Should().Be("0901234567");
    }

    [Fact]
    public void GetInternationalFormat_ForLocalNumber_ReturnsPlusPrefix()
    {
        var phone = new PhoneNumber("0901234567");

        phone.GetInternationalFormat().Should().Be("+84901234567");
    }

    [Fact]
    public void GetInternationalFormat_ForPlusPrefixNumber_ReturnsSame()
    {
        var phone = new PhoneNumber("+84901234567");

        phone.GetInternationalFormat().Should().Be("+84901234567");
    }

    [Fact]
    public void GetInternationalFormat_For84PrefixNumber_AddsPlus()
    {
        var phone = new PhoneNumber("84901234567");

        phone.GetInternationalFormat().Should().Be("+84901234567");
    }

    [Fact]
    public void ImplicitToString_ReturnsValue()
    {
        var phone = new PhoneNumber("0901234567");

        string str = phone;

        str.Should().Be("0901234567");
    }

    [Fact]
    public void ToString_ReturnsDisplayFormat()
    {
        var phone = new PhoneNumber("+84901234567");

        phone.ToString().Should().Be("0901234567");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var p1 = new PhoneNumber("0901234567");
        var p2 = new PhoneNumber("0901234567");

        p1.Should().Be(p2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var p1 = new PhoneNumber("0901234567");
        var p2 = new PhoneNumber("0912345678");

        p1.Should().NotBe(p2);
    }
}
