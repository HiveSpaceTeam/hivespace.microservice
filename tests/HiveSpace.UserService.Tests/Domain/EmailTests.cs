using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class EmailTests
{
    [Fact]
    public void Create_WithValidFormat_Succeeds()
    {
        var email = Email.Create("user@example.com");
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_WithInvalidFormat_ThrowsInvalidEmailException()
    {
        var act = () => Email.Create("not-an-email");
        act.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void TwoEmailsWithSameAddress_AreEqual()
    {
        var a = Email.Create("User@Example.COM");
        var b = Email.Create("user@example.com");
        a.Should().Be(b);
    }

    [Fact]
    public void Create_WithNull_ThrowsInvalidEmailException()
    {
        var act = () => Email.Create(null!);
        act.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void CreateOrDefault_WithNull_ReturnsNull()
    {
        Email.CreateOrDefault(null).Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithWhitespace_ReturnsNull()
    {
        Email.CreateOrDefault("   ").Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithValidEmail_ReturnsInstance()
    {
        var email = Email.CreateOrDefault("user@example.com");
        email.Should().NotBeNull();
        email!.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void ImplicitOperator_ConvertsToString()
    {
        var email = Email.Create("user@example.com");
        string value = email;
        value.Should().Be("user@example.com");
    }

    [Fact]
    public void ExplicitOperator_ConvertsFromString()
    {
        var email = (Email)"user@example.com";
        email.Value.Should().Be("user@example.com");
    }
}
