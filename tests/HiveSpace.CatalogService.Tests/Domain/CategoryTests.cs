using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithNameOnly_SetsName()
    {
        var category = new Category("Electronics");
        category.Name.Should().Be("Electronics");
        category.ParentId.Should().BeNull();
        category.ProductSetId.Should().BeNull();
        category.IsActive.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllFields_SetsAllProperties()
    {
        var category = new Category(1, "Mobile", parentId: 10, productSetId: 5, isActive: true, imageFileId: "img-001");
        category.Id.Should().Be(1);
        category.Name.Should().Be("Mobile");
        category.ParentId.Should().Be(10);
        category.ProductSetId.Should().Be(5);
        category.IsActive.Should().BeTrue();
        category.ImageFileId.Should().Be("img-001");
    }

    [Fact]
    public void SetImageUrl_SetsUrl()
    {
        var category = new Category("Electronics");
        category.SetImageUrl("http://img.url");
        category.ImageUrl.Should().Be("http://img.url");
    }

    [Fact]
    public void AddAttribute_AppearsInCollection()
    {
        var category = new Category(1, "Electronics");
        category.AddAttribute(100);
        category.CategoryAttributes.Should().ContainSingle(a => a.AttributeId == 100);
    }

    [Fact]
    public void RemoveAttribute_WithExistingAttribute_RemovesFromCollection()
    {
        var category = new Category(1, "Electronics");
        category.AddAttribute(100);
        category.RemoveAttribute(100);
        category.CategoryAttributes.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAttribute_WithNonExistingAttribute_DoesNothing()
    {
        var category = new Category(1, "Electronics");
        var act = () => category.RemoveAttribute(999);
        act.Should().NotThrow();
        category.CategoryAttributes.Should().BeEmpty();
    }
}
