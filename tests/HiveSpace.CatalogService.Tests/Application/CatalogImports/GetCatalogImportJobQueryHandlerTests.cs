using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportJob;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Exceptions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class GetCatalogImportJobQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetCatalogImportJobQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithJobId_ReturnsJobStatusAndProgress()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ProvisionCategories,
            "tiki",
            Guid.NewGuid(),
            "sha256:job-status");
        job.Start();
        job.UpdateProgress(total: 3, processed: 2, created: 1, matched: 1);
        repository.AddJob(job);
        await repository.SaveChangesAsync(CancellationToken.None);

        var result = await new GetCatalogImportJobQueryHandler(repository)
            .Handle(new GetCatalogImportJobQuery(job.Id), CancellationToken.None);

        result.JobId.Should().Be(job.Id);
        result.Status.Should().Be("Running");
        result.OperationType.Should().Be("ProvisionCategories");
        result.Progress.Total.Should().Be(3);
        result.Progress.Processed.Should().Be(2);
        result.Progress.Created.Should().Be(1);
        result.Progress.Matched.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithMissingJob_ThrowsNotFoundException()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);

        var act = () => new GetCatalogImportJobQueryHandler(repository)
            .Handle(new GetCatalogImportJobQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
