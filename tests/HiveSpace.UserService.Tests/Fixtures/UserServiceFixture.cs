using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Fixtures;

public sealed class UserServiceFixture : IAsyncLifetime
{
    public UserDbContext DbContext { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase($"user-tests-{Guid.NewGuid()}")
            .Options;

        DbContext = new UserDbContext(options);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
