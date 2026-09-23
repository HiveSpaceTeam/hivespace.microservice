using System.Text.Json;
using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Infrastructure.CatalogImports.Queueing;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Infrastructure.CatalogImports;

public class CatalogImportJobSchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_AddsOutboxMessageToCurrentUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-import-scheduler-{Guid.NewGuid()}")
            .Options;
        await using var context = new CatalogDbContext(options);
        var repository = new SqlCatalogImportBundleRepository(context);
        var scheduler = new CatalogImportJobScheduler(repository, NullLogger<CatalogImportJobScheduler>.Instance);
        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ValidateBundle,
            "tiki",
            Guid.NewGuid(),
            sourceFingerprint: "sha256:test",
            sourceFileName: "catalog.json",
            bundleId: Guid.NewGuid());
        repository.AddJob(job);

        await scheduler.ScheduleAsync(job, CancellationToken.None);

        context.CatalogImportQueueOutboxMessages.Local.Should().ContainSingle();

        await repository.SaveChangesAsync(CancellationToken.None);
        var message = await context.CatalogImportQueueOutboxMessages.SingleAsync();
        var workItem = JsonSerializer.Deserialize<CatalogImportQueueWorkItem>(
            message.PayloadJson,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        message.JobId.Should().Be(job.Id);
        message.OperationType.Should().Be(CatalogImportJobOperationType.ValidateBundle);
        message.Attempt.Should().Be(1);
        message.DispatchedAt.Should().BeNull();
        workItem!.JobId.Should().Be(job.Id);
        workItem.Attempt.Should().Be(1);
        job.Status.Should().Be(CatalogImportJobStatus.Pending);
    }
}
