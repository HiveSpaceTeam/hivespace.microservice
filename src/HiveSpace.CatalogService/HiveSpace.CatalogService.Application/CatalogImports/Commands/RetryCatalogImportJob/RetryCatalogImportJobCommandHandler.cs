using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.RetryCatalogImportJob;

public class RetryCatalogImportJobCommandHandler(
    ICatalogImportBundleRepository repository,
    ICatalogImportJobScheduler jobScheduler)
    : ICommandHandler<RetryCatalogImportJobCommand, CatalogImportJobSubmissionDto>
{
    public async Task<CatalogImportJobSubmissionDto> Handle(
        RetryCatalogImportJobCommand request,
        CancellationToken cancellationToken)
    {
        var job = await repository.GetJobByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportJobNotFound, nameof(CatalogImportJob));

        if (!string.IsNullOrWhiteSpace(job.SourceFingerprint))
        {
            var activeJob = await repository.GetActiveJobByOperationAndSourceFingerprintAsync(
                job.OperationType,
                job.SourceSystem,
                job.SourceFingerprint,
                cancellationToken);

            if (activeJob is not null && activeJob.Id != job.Id)
                return CatalogImportJobMapper.ToSubmissionDto(activeJob);
        }

        job.Requeue();
        await jobScheduler.ScheduleAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return CatalogImportJobMapper.ToSubmissionDto(job);
    }
}
