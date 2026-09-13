using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportBundleDetail;

public class GetCatalogImportBundleDetailQueryHandler(ICatalogImportBundleRepository repository)
    : IQueryHandler<GetCatalogImportBundleDetailQuery, CatalogImportBundleSummaryDto>
{
    public async Task<CatalogImportBundleSummaryDto> Handle(
        GetCatalogImportBundleDetailQuery request,
        CancellationToken cancellationToken)
    {
        var bundle = await repository.GetByIdAsync(request.BundleId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));

        return CatalogImportMapper.ToSummaryDto(bundle);
    }
}
