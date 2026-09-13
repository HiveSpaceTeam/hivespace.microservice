using Microsoft.Extensions.Configuration;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;

namespace HiveSpace.CatalogService.Api.CatalogImports;

public class CatalogImportJobHostedService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<CatalogImportJobHostedService> logger) : BackgroundService
{
    private readonly TimeSpan _runningJobRecoveryThreshold =
        TimeSpan.FromMinutes(Math.Max(1, configuration.GetValue<int?>("CatalogImports:RunningJobRecoveryThresholdMinutes") ?? 30));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingJobsAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ProcessPendingJobsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<ICatalogImportJobProcessor>();
            await processor.RecoverStaleRunningJobsAsync(_runningJobRecoveryThreshold, limit: 5, stoppingToken);
            await processor.ProcessPendingJobsAsync(limit: 5, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Catalog import job worker failed while polling pending jobs");
        }
    }
}
