using FluentAssertions;
using HiveSpace.CatalogService.Infrastructure;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Infrastructure;

public class DataSeederTests
{
    [Fact]
    public async Task InitializeAsync_WhenSampleDataDisabled_RunsReferenceAndBootstrapOnly()
    {
        var calls = await ExecuteAsync(seedSampleData: false);

        calls.Should().Equal("reference", "bootstrap");
    }

    [Fact]
    public async Task InitializeAsync_WhenSampleDataEnabled_RunsAllSeedKindsInOrder()
    {
        var calls = await ExecuteAsync(seedSampleData: true);

        calls.Should().Equal("reference", "bootstrap", "sample");
    }

    private static async Task<IReadOnlyList<string>> ExecuteAsync(bool seedSampleData)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));

        var calls = new List<string>();
        builder.Services.AddSingleton<ISeeder>(new RecordingSeeder("reference", 1, SeedKind.ReferenceData, calls));
        builder.Services.AddSingleton<ISeeder>(new RecordingSeeder("sample", 3, SeedKind.SampleData, calls));
        builder.Services.AddSingleton<ISeeder>(new RecordingSeeder("bootstrap", 2, SeedKind.BootstrapData, calls));

        var app = builder.Build();
        await DataSeeder.InitializeAsync(app, autoMigrate: false, seedSampleData: seedSampleData);
        await app.DisposeAsync();

        return calls;
    }

    private sealed class RecordingSeeder(
        string name,
        int order,
        SeedKind kind,
        ICollection<string> calls) : ISeeder
    {
        public int Order => order;
        public SeedKind Kind => kind;

        public Task SeedAsync(CancellationToken ct = default)
        {
            calls.Add(name);
            return Task.CompletedTask;
        }
    }
}
