using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportJobs;

public class ListCatalogImportJobsQueryHandler(ICatalogImportBundleRepository repository)
    : IQueryHandler<ListCatalogImportJobsQuery, CatalogImportPagedResponseDto<CatalogImportJobDto>>
{
    public async Task<CatalogImportPagedResponseDto<CatalogImportJobDto>> Handle(
        ListCatalogImportJobsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (jobs, total) = await repository.ListJobsAsync(
            pageNumber,
            pageSize,
            Parse<CatalogImportJobStatus>(request.Status),
            Parse<CatalogImportJobOperationType>(request.OperationType),
            Normalize(request.SourceSystem),
            request.BundleId,
            request.RequestedFrom,
            request.RequestedTo,
            cancellationToken);

        return new CatalogImportPagedResponseDto<CatalogImportJobDto>(
            jobs.Select(CatalogImportJobMapper.ToDto).ToList(),
            CatalogImportPaginationDto.Create(pageNumber, pageSize, total));
    }

    private static TEnum? Parse<TEnum>(string? value)
        where TEnum : struct
        => Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : null;

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
