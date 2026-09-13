using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;
using HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundles;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ListCatalogImportBundlesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public ListCatalogImportBundlesQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithRecentBundles_ReturnsPagedSummaryCounts()
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
        var request = SubmitCatalogImportBundleCommandHandlerTestsHelper.CreateRequest("sha256:list");
        var job = await submit.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, new SqlCategoryRepository(_fixture.DbContext), new NullCatalogImportJobLifecyclePublisher())
            .ProcessJobAsync(job.JobId, CancellationToken.None);

        var handler = new ListCatalogImportBundlesQueryHandler(repository);
        var result = await handler.Handle(new ListCatalogImportBundlesQuery(1, 10), CancellationToken.None);

        result.Data.Should().ContainSingle(x => x.Crawl.SourceFingerprint == "sha256:list" && x.Summary.TotalProducts == 1);
        result.Pagination.CurrentPage.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalItems.Should().BeGreaterThanOrEqualTo(1);
    }
}
