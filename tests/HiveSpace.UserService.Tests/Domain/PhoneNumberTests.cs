using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class PhoneNumberTests
{
    [Fact]
    public void Create_WithTenDigitNumber_PrependsUSCountryCode()
    {
        var phone = PhoneNumber.Create("5551234567");
        phone.Value.Should().Be("15551234567");
    }

    [Fact]
    public void Create_WithInternationalFormat_NormalizesToDigitsOnly()
    {
        var phone = PhoneNumber.Create("+1 555 123 4567");
        phone.Value.Should().Be("15551234567");
    }

    [Fact]
    public void Create_WithElevenPlusDigits_AcceptsAsIs()
    {
        var phone = PhoneNumber.Create("15551234567");
        phone.Value.Should().Be("15551234567");
    }

    [Fact]
    public void Create_WithEmptyString_ThrowsInvalidPhoneNumberException()
    {
        var act = () => PhoneNumber.Create("");
        act.Should().Throw<InvalidPhoneNumberException>();
    }

    [Fact]
    public void Create_WithTooFewDigits_ThrowsInvalidPhoneNumberException()
    {
        // 5 digits → normalizes to "" (too short) → fails IsValidPhoneNumberFormat
        var act = () => PhoneNumber.Create("12345");
        act.Should().Throw<InvalidPhoneNumberException>();
    }

    [Fact]
    public void CreateOrDefault_WithNull_ReturnsNull()
    {
        PhoneNumber.CreateOrDefault(null).Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithWhitespace_ReturnsNull()
    {
        PhoneNumber.CreateOrDefault("   ").Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithValidNumber_ReturnsInstance()
    {
        var phone = PhoneNumber.CreateOrDefault("5551234567");
        phone.Should().NotBeNull();
        phone!.Value.Should().Be("15551234567");
    }

    [Fact]
    public void FormattedValue_ForUSNumber_ReturnsUSFormat()
    {
        var phone = PhoneNumber.Create("5551234567");
        phone.FormattedValue.Should().Be("+1 (555) 123-4567");
    }

    [Fact]
    public void FormattedValue_ForUKNumber_StartsWithPlusFortyFour()
    {
        var phone = PhoneNumber.Create("442079460958");
        phone.FormattedValue.Should().StartWith("+44");
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        var phone = PhoneNumber.Create("5551234567");
        string value = phone;
        value.Should().Be("15551234567");
    }

    [Fact]
    public void ExplicitOperator_ConvertsFromString()
    {
        var phone = (PhoneNumber)"5551234567";
        phone.Value.Should().Be("15551234567");
    }

    [Fact]
    public void FormattedValue_ForOtherCountryCode_ReturnsGenericFormat()
    {
        // Vietnamese +84 number — triggers the "other country codes" for-loop branch
        var phone = PhoneNumber.Create("84912345678");
        phone.FormattedValue.Should().StartWith("+8");
    }

    [Fact]
    public void Create_WithNonDigitString_ThrowsInvalidPhoneNumberException()
    {
        // No digits → NormalizePhoneNumber returns empty → IsValidPhoneNumberFormat fails
        var act = () => PhoneNumber.Create("abc!!!");
        act.Should().Throw<InvalidPhoneNumberException>();
    }

    [Fact]
    public void TwoPhoneNumbers_WithSameValue_AreEqual()
    {
        var a = PhoneNumber.Create("5551234567");
        var b = PhoneNumber.Create("5551234567");
        a.Should().Be(b);
    }
}
