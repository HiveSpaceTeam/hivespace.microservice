using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports.Queueing;

public sealed class CatalogImportQueueOutboxDispatcher(
    IServiceProvider serviceProvider,
    ILogger<CatalogImportQueueOutboxDispatcher> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Catalog import queue outbox dispatch failed.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<ICatalogImportQueuePublisher>();

        var messages = await context.CatalogImportQueueOutboxMessages
            .Where(x => x.DispatchedAt == null)
            .OrderBy(x => x.QueuedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await DispatchAsync(context, publisher, message, cancellationToken);
        }
    }

    private static async Task DispatchAsync(
        CatalogDbContext context,
        ICatalogImportQueuePublisher publisher,
        CatalogImportQueueOutboxMessage message,
        CancellationToken cancellationToken)
    {
        var attemptedAt = DateTimeOffset.UtcNow;
        var workItem = new CatalogImportQueueWorkItem(
            message.JobId,
            message.OperationType,
            message.Attempt,
            message.CorrelationId,
            message.QueuedAt,
            message.RequestedByUserId,
            message.SourceBundleId);

        try
        {
            await publisher.PublishAsync(workItem, message.PayloadJson, cancellationToken);
            message.MarkDispatched(attemptedAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            message.MarkDispatchFailed(ex.Message, attemptedAt);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
