using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.GetCatalogImportBundleDetail;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class GetCatalogImportBundleDetailQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetCatalogImportBundleDetailQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithBundleId_ReturnsBundleSummaryOnly()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        repository.AddExternalCategoryLink(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            "[\"Nha sach Tiki\"]",
            1,
            "sha256:categories",
            Guid.NewGuid()));
        await repository.SaveChangesAsync(CancellationToken.None);
        var submit = new SubmitCatalogImportBundleCommandHandler(repository, new FakeUserContext { UserId = Guid.NewGuid() }, new NullCatalogImportJobLifecyclePublisher());
        var submitted = await submit.Handle(
            new SubmitCatalogImportBundleCommand(SubmitCatalogImportBundleCommandHandlerTestsHelper.CreateRequest("sha256:detail")),
            CancellationToken.None);
        await new CatalogImportJobProcessor(repository, new SqlCategoryRepository(_fixture.DbContext), new NullCatalogImportJobLifecyclePublisher())
            .ProcessJobAsync(submitted.JobId, CancellationToken.None);
        var processedJob = await repository.GetJobByIdAsync(submitted.JobId, CancellationToken.None);

        var handler = new GetCatalogImportBundleDetailQueryHandler(repository);
        var result = await handler.Handle(new GetCatalogImportBundleDetailQuery(processedJob!.BundleId!.Value), CancellationToken.None);

        result.BundleId.Should().Be(processedJob.BundleId.Value);
        result.Source.System.Should().Be("tiki");
        result.Crawl.SourceFingerprint.Should().Be("sha256:detail");
        result.Summary.TotalProducts.Should().Be(1);
        result.Summary.ReadyProducts.Should().Be(1);
        result.Summary.BlockedProducts.Should().Be(0);
        result.Summary.WarningCount.Should().Be(1);
        result.Summary.DuplicateCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMissingBundle_ThrowsNotFoundException()
    {
        var handler = new GetCatalogImportBundleDetailQueryHandler(new SqlCatalogImportBundleRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetCatalogImportBundleDetailQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
