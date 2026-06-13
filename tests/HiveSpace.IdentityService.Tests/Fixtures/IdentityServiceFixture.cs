using HiveSpace.IdentityService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Fixtures;

public sealed class IdentityServiceFixture : IAsyncLifetime
{
    public IdentityDbContext DbContext { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-tests-{Guid.NewGuid()}")
            .Options;

        DbContext = new IdentityDbContext(options);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
