using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Domain;

public class OtpChallengeTests
{
    [Fact]
    public void Create_SetsInitialState()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
        var canResendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        var challenge = OtpChallenge.Create(
            "BUYER@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            "token-123",
            "654321",
            expiresAt,
            canResendAt);

        challenge.Id.Value.Should().NotBeEmpty();
        challenge.EmailNormalized.Should().Be("BUYER@HIVESPACE.LOCAL");
        challenge.Purpose.Should().Be(OtpChallengePurpose.SignIn);
        challenge.ChallengeToken.Should().Be("token-123");
        challenge.Code.Should().Be("654321");
        challenge.ExpiresAt.Should().Be(expiresAt);
        challenge.CanResendAt.Should().Be(canResendAt);
        challenge.AttemptCount.Should().Be(0);
        challenge.IsUsed.Should().BeFalse();
        challenge.IsInvalidated.Should().BeFalse();
        challenge.IsActiveAt(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void LifecycleMethods_UpdateChallengeState()
    {
        var challenge = OtpChallenge.Create(
            "BUYER@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            "token-456",
            "123456",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddMinutes(1));

        challenge.IncrementAttempt();
        challenge.IncrementAttempt();
        challenge.AttemptCount.Should().Be(2);

        challenge.MarkUsed();
        challenge.IsUsed.Should().BeTrue();
        challenge.IsActiveAt(DateTimeOffset.UtcNow).Should().BeFalse();

        var invalidated = OtpChallenge.Create(
            "SELLER@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            "token-789",
            "999999",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddMinutes(1));

        invalidated.Invalidate();
        invalidated.IsInvalidated.Should().BeTrue();
        invalidated.IsActiveAt(DateTimeOffset.UtcNow).Should().BeFalse();

        var expired = OtpChallenge.Create(
            "EXPIRED@HIVESPACE.LOCAL",
            OtpChallengePurpose.SignIn,
            "token-expired",
            "111111",
            DateTimeOffset.UtcNow.AddSeconds(-1),
            DateTimeOffset.UtcNow.AddMinutes(-1));

        expired.IsActiveAt(DateTimeOffset.UtcNow).Should().BeFalse();
    }
}
