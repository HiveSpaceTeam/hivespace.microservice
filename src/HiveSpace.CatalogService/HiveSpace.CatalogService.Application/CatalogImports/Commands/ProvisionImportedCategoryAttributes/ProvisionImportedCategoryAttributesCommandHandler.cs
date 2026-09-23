using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using System.Text.Json;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategoryAttributes;

public class ProvisionImportedCategoryAttributesCommandHandler(
    ICatalogImportBundleRepository repository,
    ICatalogImportJobScheduler jobScheduler)
    : ICommandHandler<ProvisionImportedCategoryAttributesCommand, CatalogImportJobSubmissionDto>
{
    public async Task<CatalogImportJobSubmissionDto> Handle(
        ProvisionImportedCategoryAttributesCommand request,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var existingJob = await repository.GetJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType.ProvisionCategoryAttributes,
            payload.Source.System,
            payload.Crawl.SourceFingerprint,
            cancellationToken);
        if (existingJob is not null)
            return CatalogImportJobMapper.ToSubmissionDto(existingJob);

        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ProvisionCategoryAttributes,
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
