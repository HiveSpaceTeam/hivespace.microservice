using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ValidateCatalogImportBundle;

public class ValidateCatalogImportBundleCommandHandler(
    ICatalogImportBundleRepository repository,
    IUserContext userContext,
    ICatalogImportJobScheduler jobScheduler)
    : ICommandHandler<ValidateCatalogImportBundleCommand, CatalogImportJobSubmissionDto>
{
    public async Task<CatalogImportJobSubmissionDto> Handle(
        ValidateCatalogImportBundleCommand request,
        CancellationToken cancellationToken)
    {
        var bundle = await repository.GetByIdAsync(request.BundleId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));

        var existingJob = await repository.GetActiveJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType.ValidateBundle,
            bundle.SourceSystem,
            bundle.SourceFingerprint,
            cancellationToken);
        if (existingJob is not null)
            return CatalogImportJobMapper.ToSubmissionDto(existingJob);

        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ValidateBundle,
            bundle.SourceSystem,
            userContext.UserId,
            sourceFingerprint: bundle.SourceFingerprint,
            sourceFileName: bundle.SourceFileName,
            bundleId: bundle.Id);

        repository.AddJob(job);
        await jobScheduler.ScheduleAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return CatalogImportJobMapper.ToSubmissionDto(job);
    }
}
