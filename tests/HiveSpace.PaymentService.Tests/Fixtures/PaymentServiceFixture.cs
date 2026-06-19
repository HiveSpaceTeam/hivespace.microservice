using HiveSpace.PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Fixtures;

public sealed class PaymentServiceFixture : IAsyncLifetime
{
    public PaymentDbContext DbContext { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase($"payment-tests-{Guid.NewGuid()}")
            .Options;

        DbContext = new PaymentDbContext(options);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
