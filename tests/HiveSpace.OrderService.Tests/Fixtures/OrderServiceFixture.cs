using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Fixtures;

public sealed class OrderServiceFixture : IAsyncLifetime
{
    public OrderDbContext DbContext { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase($"order-tests-{Guid.NewGuid()}")
            .Options;

        DbContext = new OrderDbContext(options);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
