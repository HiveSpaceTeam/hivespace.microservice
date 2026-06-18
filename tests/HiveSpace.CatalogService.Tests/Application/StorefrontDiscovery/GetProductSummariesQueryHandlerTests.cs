using FluentAssertions;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Domain.Shared.Enumerations;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.StorefrontDiscovery;

public class GetProductSummariesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetProductSummariesQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithActiveProducts_ReturnsSummaries()
    {
        _fixture.DbContext.Products.AddRange(
            NewProduct(ProductStatus.Available, "summary-d", 70001),
            NewProduct(ProductStatus.Available, "summary-e", 70002));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductSummariesQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var result = await handler.Handle(
            new GetProductSummariesQuery(new ProductSearchRequestDto(Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_WithUnpublishedProducts_ExcludesThemFromResults()
    {
        _fixture.DbContext.Products.Add(NewProduct(ProductStatus.Unpublish, "summary-unpub", 70003));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductSummariesQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var result = await handler.Handle(
            new GetProductSummariesQuery(new ProductSearchRequestDto(Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().NotContain(p => p.Name == "Summary Product" && p.Id == 70003);
    }

    [Fact]
    public async Task Handle_WithNullKeyword_FallsBackToEmptySearch()
    {
        _fixture.DbContext.Products.Add(NewProduct(ProductStatus.Available, "summary-null-kw", 70010));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetProductSummariesQueryHandler(new SqlProductRepository(_fixture.DbContext));

        var result = await handler.Handle(
            new GetProductSummariesQuery(new ProductSearchRequestDto(Keyword: null!, Page: 1, PageSize: 10)),
            CancellationToken.None);

        result.Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    private static Product NewProduct(ProductStatus status, string slug, int id) =>
        Product.CreateProduct("Summary Product", slug, "Description", "Short",
            status, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, Guid.NewGuid().ToString(), id);
}
