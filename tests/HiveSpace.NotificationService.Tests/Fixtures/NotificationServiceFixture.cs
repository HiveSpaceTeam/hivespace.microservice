using HiveSpace.NotificationService.Core.Persistence;
using HiveSpace.Testing.Shared.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Fixtures;

public sealed class NotificationServiceFixture : IAsyncLifetime
{
    public NotificationDbContext DbContext { get; private set; } = null!;
    public EmailDeliveryFake EmailDelivery { get; } = new();
    public SignalRHubFake SignalRHub { get; } = new();

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase($"notification-tests-{Guid.NewGuid()}")
            .Options;

        DbContext = new NotificationDbContext(options);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
