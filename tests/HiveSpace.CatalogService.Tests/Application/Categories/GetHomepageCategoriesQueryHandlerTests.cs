using FluentAssertions;
using HiveSpace.CatalogService.Application.Categories.Dtos;
using HiveSpace.CatalogService.Application.Categories.Queries.GetHomepageCategories;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.Categories;

public class GetHomepageCategoriesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    public GetHomepageCategoriesQueryHandlerTests(CatalogServiceFixture fixture) { }

    [Fact]
    public async Task Handle_WithSeededCategories_ReturnsCategoryList()
    {
        var categories = new List<CategoryDto>
        {
            new(1, "Electronics", "Electronics", null, null),
            new(2, "Fashion", "Fashion", null, null),
        };
        var handler = new GetHomepageCategoriesQueryHandler(new FakeCategoryDataQuery(categories));

        var result = await handler.Handle(new GetHomepageCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Electronics");
    }

    [Fact]
    public async Task Handle_WithNoCategories_ReturnsEmptyList()
    {
        var handler = new GetHomepageCategoriesQueryHandler(new FakeCategoryDataQuery());

        var result = await handler.Handle(new GetHomepageCategoriesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
