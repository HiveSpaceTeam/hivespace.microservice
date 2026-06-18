using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.UserService.Tests.Domain;

public class DateOfBirthTests
{
    [Fact]
    public void Create_WithValidDate_Succeeds()
    {
        var date = DateTimeOffset.UtcNow.AddYears(-25);
        var dob = DateOfBirth.Create(date);
        dob.Value.Should().Be(date);
    }

    [Fact]
    public void Create_WithFutureDate_ThrowsInvalidDateOfBirthException()
    {
        var act = () => DateOfBirth.Create(DateTimeOffset.UtcNow.AddDays(1));
        act.Should().Throw<InvalidDateOfBirthException>();
    }

    [Fact]
    public void Create_WithDateOlderThan120Years_ThrowsInvalidDateOfBirthException()
    {
        var act = () => DateOfBirth.Create(DateTimeOffset.UtcNow.AddYears(-121));
        act.Should().Throw<InvalidDateOfBirthException>();
    }

    [Fact]
    public void CreateOrDefault_WithNull_ReturnsNull()
    {
        DateOfBirth.CreateOrDefault(null).Should().BeNull();
    }

    [Fact]
    public void CreateOrDefault_WithValidDate_ReturnsInstance()
    {
        var date = DateTimeOffset.UtcNow.AddYears(-30);
        var dob = DateOfBirth.CreateOrDefault(date);
        dob.Should().NotBeNull();
        dob!.Value.Should().Be(date);
    }

    [Fact]
    public void Age_CalculatesCorrectly()
    {
        var dob = DateOfBirth.Create(DateTimeOffset.UtcNow.AddYears(-30).AddDays(-1));
        dob.Age.Should().Be(30);
    }

    [Fact]
    public void ImplicitOperator_ConvertsToDateTimeOffset()
    {
        var date = DateTimeOffset.UtcNow.AddYears(-25);
        var dob = DateOfBirth.Create(date);
        DateTimeOffset result = dob;
        result.Should().Be(date);
    }

    [Fact]
    public void ExplicitOperator_ConvertsFromDateTimeOffset()
    {
        var date = DateTimeOffset.UtcNow.AddYears(-25);
        var dob = (DateOfBirth)date;
        dob.Value.Should().Be(date);
    }

    [Fact]
    public void Age_WhenBirthdayNotYetOccurredThisYear_IsOneLessThanYearDifference()
    {
        // Born exactly 30 years ago + 1 day → birthday is tomorrow → age is still 29
        var dob = DateOfBirth.Create(DateTimeOffset.UtcNow.AddYears(-30).AddDays(1));
        dob.Age.Should().Be(29);
    }

    [Fact]
    public void TwoDateOfBirthsWithSameDate_AreEqual()
    {
        var date = DateTimeOffset.UtcNow.AddYears(-25);
        var a = DateOfBirth.Create(date);
        var b = DateOfBirth.Create(date);
        a.Should().Be(b);
    }
}
