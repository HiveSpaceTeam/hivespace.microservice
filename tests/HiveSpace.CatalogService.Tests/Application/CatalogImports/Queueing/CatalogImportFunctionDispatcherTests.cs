using System.Text.Json;
using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports.Queueing;

public class CatalogImportFunctionDispatcherTests
{
    [Fact]
    public async Task HandleAsync_WithMatchingAttempt_ProcessesJob()
    {
        var executionGate = Substitute.For<ICatalogImportJobExecutionGate>();
        executionGate.TryClaimAsync(Arg.Any<CatalogImportQueueWorkItem>(), Arg.Any<CancellationToken>())
            .Returns(CatalogImportJobClaimResult.Success());
        var processor = Substitute.For<ICatalogImportJobProcessor>();
        var workItem = CreateWorkItem();
        var dispatcher = CreateDispatcher(executionGate, processor);

        await dispatcher.HandleAsync(Serialize(workItem), "RabbitMQ", CancellationToken.None);

        await processor.Received(1).ProcessClaimedQueuedJobAsync(
            Arg.Is<CatalogImportQueueWorkItem>(x =>
                x.JobId == workItem.JobId
                && x.Attempt == workItem.Attempt
                && x.CorrelationId == workItem.CorrelationId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithWorkItem_ProcessesJob()
    {
        var executionGate = Substitute.For<ICatalogImportJobExecutionGate>();
        executionGate.TryClaimAsync(Arg.Any<CatalogImportQueueWorkItem>(), Arg.Any<CancellationToken>())
            .Returns(CatalogImportJobClaimResult.Success());
        var processor = Substitute.For<ICatalogImportJobProcessor>();
        var workItem = CreateWorkItem();
        var dispatcher = CreateDispatcher(executionGate, processor);

        await dispatcher.HandleAsync(workItem, "RabbitMQ", CancellationToken.None);

        await processor.Received(1).ProcessClaimedQueuedJobAsync(workItem, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithSkippedClaim_DoesNotProcessJob()
    {
        var executionGate = Substitute.For<ICatalogImportJobExecutionGate>();
        executionGate.TryClaimAsync(Arg.Any<CatalogImportQueueWorkItem>(), Arg.Any<CancellationToken>())
            .Returns(CatalogImportJobClaimResult.Skipped("StaleAttempt"));
        var processor = Substitute.For<ICatalogImportJobProcessor>();
        var workItem = CreateWorkItem();
        var dispatcher = CreateDispatcher(executionGate, processor);

        await dispatcher.HandleAsync(workItem, "RabbitMQ", CancellationToken.None);

        await processor.DidNotReceiveWithAnyArgs()
            .ProcessClaimedQueuedJobAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_WithUnsupportedOperation_FailsVisibly()
    {
        var executionGate = Substitute.For<ICatalogImportJobExecutionGate>();
        var processor = Substitute.For<ICatalogImportJobProcessor>();
        var workItem = CreateWorkItem() with { OperationType = (CatalogImportJobOperationType)999 };
        var dispatcher = CreateDispatcher(executionGate, processor);

        var act = () => dispatcher.HandleAsync(Serialize(workItem), "RabbitMQ", CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        await processor.DidNotReceiveWithAnyArgs()
            .ProcessClaimedQueuedJobAsync(default!, default);
    }

    private static CatalogImportFunctionDispatcher CreateDispatcher(
        ICatalogImportJobExecutionGate executionGate,
        ICatalogImportJobProcessor processor)
        => new(executionGate, processor, NullLogger<CatalogImportFunctionDispatcher>.Instance);

    private static CatalogImportQueueWorkItem CreateWorkItem()
        => new(
            Guid.NewGuid(),
            CatalogImportJobOperationType.SubmitBundle,
            1,
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid());

    private static string Serialize(CatalogImportQueueWorkItem workItem)
        => JsonSerializer.Serialize(workItem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}
