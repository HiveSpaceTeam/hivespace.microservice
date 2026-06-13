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
}
