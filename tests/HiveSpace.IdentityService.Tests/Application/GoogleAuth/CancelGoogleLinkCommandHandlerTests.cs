using FluentAssertions;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CancelGoogleLink;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.GoogleAuth;

public class CancelGoogleLinkCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidToken_ClearsStore()
    {
        var linkToken = "link-token-abc";
        var state = new PendingGoogleLinkState(
            "Google", "gid-1", "Google", "user@gmail.com",
            Guid.NewGuid(), "storefront", null, null,
            DateTimeOffset.UtcNow.AddMinutes(5), linkToken);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync(linkToken, Arg.Any<CancellationToken>()).Returns(state);

        var handler = new CancelGoogleLinkCommandHandler(store);
        await handler.Handle(new CancelGoogleLinkCommand(linkToken), CancellationToken.None);

        await store.Received(1).ClearAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStoreFails_PropagatesException()
    {
        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PendingGoogleLinkState>(new InvalidOperationException("not found")));

        var handler = new CancelGoogleLinkCommandHandler(store);
        var act = () => handler.Handle(new CancelGoogleLinkCommand("bad-token"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
