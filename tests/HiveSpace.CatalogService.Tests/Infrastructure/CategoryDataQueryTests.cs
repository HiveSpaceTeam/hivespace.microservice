using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.CatalogService.Infrastructure.DataQueries;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Infrastructure;

public class CategoryDataQueryTests
{
    [Fact]
    public async Task GetHomepageCategoriesAsync_ReturnsOnlyRootCategoriesWithImages()
    {
        var db = CreateDb();
        var rootWithUrl = new Category(2, "Root With Url", null);
        rootWithUrl.SetImageUrl("https://cdn.example.com/root.png");

        db.Categories.AddRange(
            new Category(1, "Root With File", null, imageFileId: "root-file"),
            rootWithUrl,
            new Category(3, "Child With File", 1, imageFileId: "child-file"),
            new Category(4, "Root Without Image", null));
        await db.SaveChangesAsync();

        var query = new CategoryDataQuery(db);

        var result = await query.GetHomepageCategoriesAsync();

        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().BeEquivalentTo([1, 2]);
    }

    private static CatalogDbContext CreateDb()
        => new(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"category-query-tests-{Guid.NewGuid():N}")
            .Options);
}
