using FluentAssertions;
using HiveSpace.CatalogService.Application.Categories.Dtos;
using HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Tests.Fakes;
using HiveSpace.CatalogService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.Categories;

public class GetCategoryAttributesQueryHandlerTests : IClassFixture<CatalogServiceFixture>
{
    public GetCategoryAttributesQueryHandlerTests(CatalogServiceFixture fixture) { }

    [Fact]
    public async Task Handle_WithAttributesForCategory_ReturnsAttributeList()
    {
        var attributes = new List<AttributeDto>
        {
            new(1, "Color", AttributeValueType.String, InputType.Dropdown, false, 5, true, DateTime.UtcNow, null, []),
        };
        var handler = new GetAttributesByCategoryIdQueryHandler(new FakeCategoryDataQuery(attributes: attributes));

        var result = await handler.Handle(new GetAttributesByCategoryIdQuery(CategoryId: 10), CancellationToken.None);

        result.Should().ContainSingle(a => a.Name == "Color");
    }

    [Fact]
    public async Task Handle_WithNoAttributesForCategory_ReturnsEmptyList()
    {
        var handler = new GetAttributesByCategoryIdQueryHandler(new FakeCategoryDataQuery());

        var result = await handler.Handle(new GetAttributesByCategoryIdQuery(CategoryId: 99), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
