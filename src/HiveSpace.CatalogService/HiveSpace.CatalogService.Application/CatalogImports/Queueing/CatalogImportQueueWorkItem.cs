using HiveSpace.CatalogService.Domain.CatalogImports.Enums;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queueing;

public sealed record CatalogImportQueueWorkItem(
    Guid JobId,
    CatalogImportJobOperationType OperationType,
    int Attempt,
    string CorrelationId,
    DateTimeOffset QueuedAt,
    Guid? RequestedByUserId = null,
    Guid? SourceBundleId = null);
