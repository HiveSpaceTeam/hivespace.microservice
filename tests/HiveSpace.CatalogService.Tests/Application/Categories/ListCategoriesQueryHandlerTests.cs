using FluentAssertions;
using HiveSpace.CatalogService.Application.Categories.Dtos;
using HiveSpace.CatalogService.Application.Categories.Queries.GetCategories;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.Categories;

public class ListCategoriesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    public ListCategoriesQueryHandlerTests(CatalogServiceFixture fixture) { }

    [Fact]
    public async Task Handle_WithSeededCategories_ReturnsCategoryList()
    {
        var categories = new List<CategoryDto>
        {
            new(1, "Electronics", "Electronics", null, null),
            new(2, "Books", "Books", null, null),
            new(3, "Sports", "Sports", null, null),
        };
        var handler = new GetCategoriesQueryHandler(new FakeCategoryDataQuery(categories));

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Name == "Books");
    }

    [Fact]
    public async Task Handle_WithNoCategories_ReturnsEmptyList()
    {
        var handler = new GetCategoriesQueryHandler(new FakeCategoryDataQuery());

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
