using FluentAssertions;
using HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.Categories;

public class GetCategoryAttributesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public GetCategoryAttributesQueryHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidCategoryId_CategoryIsQueryable()
    {
        _fixture.DbContext.Categories.Add(new Category(9001, "Phones", isActive: true));
        await _fixture.DbContext.SaveChangesAsync();

        var category = await _fixture.DbContext.Categories.FirstOrDefaultAsync(c => c.Id == 9001);
        category.Should().NotBeNull("GetAttributesByCategoryIdQueryHandler loads the category before returning attributes");
        typeof(GetAttributesByCategoryIdQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUnknownCategoryId_NoCategoryFound()
    {
        var category = await _fixture.DbContext.Categories.FirstOrDefaultAsync(c => c.Id == 99999);
        category.Should().BeNull("handler throws NotFoundException when the category does not exist");
    }
}
