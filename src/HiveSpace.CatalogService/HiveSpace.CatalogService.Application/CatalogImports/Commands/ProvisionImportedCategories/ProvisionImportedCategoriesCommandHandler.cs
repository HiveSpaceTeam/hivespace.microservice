using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using System.Text.Json;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategories;

public class ProvisionImportedCategoriesCommandHandler(
    ICatalogImportBundleRepository repository,
    ICatalogImportJobScheduler jobScheduler)
    : ICommandHandler<ProvisionImportedCategoriesCommand, CatalogImportJobSubmissionDto>
{
    public async Task<CatalogImportJobSubmissionDto> Handle(
        ProvisionImportedCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var existingJob = await repository.GetJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType.ProvisionCategories,
            payload.Source.System,
            payload.Crawl.SourceFingerprint,
            cancellationToken);
        if (existingJob is not null)
            return CatalogImportJobMapper.ToSubmissionDto(existingJob);

        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ProvisionCategories,
            payload.Source.System,
            request.ProvisionedByUserId,
            sourceFingerprint: payload.Crawl.SourceFingerprint,
            sourceFileName: request.SourceFileName,
            requestPayloadJson: JsonSerializer.Serialize(payload));

        repository.AddJob(job);
        await jobScheduler.ScheduleAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return CatalogImportJobMapper.ToSubmissionDto(job);
    }
}
