using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.Core.Contexts;
using System.Text.Json;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;

public class SubmitCatalogImportBundleCommandHandler(
    ICatalogImportBundleRepository repository,
    IUserContext userContext,
    ICatalogImportJobLifecyclePublisher lifecyclePublisher)
    : ICommandHandler<SubmitCatalogImportBundleCommand, CatalogImportJobSubmissionDto>
{
    public async Task<CatalogImportJobSubmissionDto> Handle(
        SubmitCatalogImportBundleCommand request,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var existingJob = await repository.GetJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType.SubmitBundle,
            payload.Source.System,
            payload.Crawl.SourceFingerprint,
            cancellationToken);
        if (existingJob is not null)
            return CatalogImportJobMapper.ToSubmissionDto(existingJob);

        var existingBundle = await repository.GetBySourceFingerprintAsync(payload.Crawl.SourceFingerprint, cancellationToken);
        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.SubmitBundle,
            payload.Source.System,
            userContext.UserId,
            sourceFingerprint: payload.Crawl.SourceFingerprint,
            sourceFileName: request.SourceFileName,
            bundleId: existingBundle?.Id,
            requestPayloadJson: JsonSerializer.Serialize(payload));

        repository.AddJob(job);
        await lifecyclePublisher.PublishQueuedAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return CatalogImportJobMapper.ToSubmissionDto(job);
    }
}
