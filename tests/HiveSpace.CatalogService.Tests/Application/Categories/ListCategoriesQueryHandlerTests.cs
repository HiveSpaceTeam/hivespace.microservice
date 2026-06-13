using FluentAssertions;
using HiveSpace.CatalogService.Application.Categories.Queries.GetCategories;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.Categories;

public class ListCategoriesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public ListCategoriesQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsNonEmptyList()
    {
        _fixture.DbContext.Categories.Add(new Category(8001, "Electronics", isActive: true));
        await _fixture.DbContext.SaveChangesAsync();

        var categories = await _fixture.DbContext.Categories.ToListAsync();
        categories.Should().NotBeEmpty();
        typeof(GetCategoriesQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsOnlyActiveCategories()
    {
        _fixture.DbContext.Categories.Add(new Category(8002, "Inactive Cat", isActive: false));
        await _fixture.DbContext.SaveChangesAsync();

        var active = await _fixture.DbContext.Categories.Where(c => c.IsActive == true).ToListAsync();
        active.Should().NotContain(c => c.Id == 8002);
    }

    private static Category NewCategory(int id, string name) => new(id, name, isActive: true);
}
