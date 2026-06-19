using HiveSpace.MediaService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.MediaService.Tests.Fixtures;

public sealed class MediaServiceFixture : IAsyncLifetime
{
    public MediaDbContext DbContext { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase($"media-tests-{Guid.NewGuid()}")
            .Options;

        DbContext = new MediaDbContext(options);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
