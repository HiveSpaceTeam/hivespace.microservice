using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportJob;

public class GetCatalogImportJobQueryHandler(ICatalogImportBundleRepository repository)
    : IQueryHandler<GetCatalogImportJobQuery, CatalogImportJobDto>
{
    public async Task<CatalogImportJobDto> Handle(GetCatalogImportJobQuery request, CancellationToken cancellationToken)
    {
        var job = await repository.GetJobByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportJobNotFound, nameof(CatalogImportJob));

        return CatalogImportJobMapper.ToDto(job);
    }
}
