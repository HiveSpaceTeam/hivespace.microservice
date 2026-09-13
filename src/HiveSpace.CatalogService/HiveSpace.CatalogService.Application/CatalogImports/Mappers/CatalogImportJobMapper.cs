using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.CatalogImports;
using System.Text.Json.Nodes;

namespace HiveSpace.CatalogService.Application.CatalogImports.Mappers;

public static class CatalogImportJobMapper
{
    public static CatalogImportJobSubmissionDto ToSubmissionDto(CatalogImportJob job)
        => new(
            job.Id,
            job.Status.ToString(),
            job.OperationType.ToString(),
            job.SourceFingerprint,
            job.SourceFileName,
            job.BundleId);

    public static CatalogImportJobDto ToDto(CatalogImportJob job)
        => new(
            job.Id,
            job.OperationType.ToString(),
            job.Status.ToString(),
            job.SourceSystem,
            job.SourceFingerprint,
            job.SourceFileName,
            job.BundleId,
            job.RequestedByUserId,
            job.RequestedAt,
            job.StartedAt,
            job.CompletedAt,
            new CatalogImportJobProgressDto(
                job.TotalCount,
                job.ProcessedCount,
                job.CreatedCount,
                job.MatchedCount,
                job.SkippedCount,
                job.BlockedCount,
                job.WarningCount,
                job.DuplicateCount,
                job.FailedCount,
                job.ConflictCount),
            ParseResultSummary(job.ResultSummaryJson),
            job.ErrorSummary);

    private static JsonNode? ParseResultSummary(string? resultSummaryJson)
    {
        if (string.IsNullOrWhiteSpace(resultSummaryJson))
            return null;

        return JsonNode.Parse(resultSummaryJson);
    }
}
