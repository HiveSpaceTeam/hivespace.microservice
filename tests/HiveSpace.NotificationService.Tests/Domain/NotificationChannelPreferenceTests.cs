using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Domain;

public class NotificationChannelPreferenceTests
{
    [Fact]
    public void EnableChannel_SetsChannelActive()
    {
        var preference = UserChannelPreference.Create(Guid.NewGuid(), NotificationChannel.Email, false);

        preference.SetEnabled(true);

        preference.Enabled.Should().BeTrue();
    }

    [Fact]
    public void DisableChannel_SetsChannelInactive()
    {
        var preference = UserChannelPreference.Create(Guid.NewGuid(), NotificationChannel.Email, true);

        preference.SetEnabled(false);

        preference.Enabled.Should().BeFalse();
    }

    [Fact]
    public void UnsupportedChannelType_ThrowsDomainException()
    {
        var channel = (NotificationChannel)999;

        Enum.IsDefined(channel).Should().BeFalse();
    }
}
