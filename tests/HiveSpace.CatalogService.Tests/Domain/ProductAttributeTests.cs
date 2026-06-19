using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class ProductAttributeTests
{
    [Fact]
    public void Create_WithSelectedValues_IsValid()
    {
        var attr = new ProductAttribute(1, [10, 20]);
        attr.AttributeId.Should().Be(1);
        attr.SelectedValueIds.Should().BeEquivalentTo([10, 20]);
        attr.FreeTextValue.Should().BeNull();
    }

    [Fact]
    public void Create_WithFreeTextValue_IsValid()
    {
        var attr = new ProductAttribute(1, freeTextValue: "  Brand A  ");
        attr.FreeTextValue.Should().Be("Brand A");
        attr.SelectedValueIds.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithAttributeIdZero_ThrowsInvalidAttributeException()
    {
        var act = () => new ProductAttribute(0, [10]);
        act.Should().Throw<InvalidAttributeException>();
    }

    [Fact]
    public void Create_WithNoValuesAndNoFreeText_ThrowsInvalidAttributeException()
    {
        var act = () => new ProductAttribute(1);
        act.Should().Throw<InvalidAttributeException>();
    }

    [Fact]
    public void Create_WithWhitespaceFreeTextAndNoSelectedValues_ThrowsInvalidAttributeException()
    {
        var act = () => new ProductAttribute(1, freeTextValue: "   ");
        act.Should().Throw<InvalidAttributeException>();
    }

    [Fact]
    public void SetSelectedValues_WithValues_ReplacesCollection()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.SetSelectedValues([30, 40]);
        attr.SelectedValueIds.Should().BeEquivalentTo([30, 40]);
    }

    [Fact]
    public void SetSelectedValues_WithNull_WhenNoFreeText_ThrowsInvalidAttributeException()
    {
        var attr = new ProductAttribute(1, [10]);
        var act = () => attr.SetSelectedValues(null!);
        act.Should().Throw<InvalidAttributeException>();
    }

    [Fact]
    public void SetSelectedValues_WithEmptyList_WhenNoFreeText_ThrowsInvalidAttributeException()
    {
        var attr = new ProductAttribute(1, [10]);
        var act = () => attr.SetSelectedValues([]);
        act.Should().Throw<InvalidAttributeException>();
    }

    [Fact]
    public void AddSelectedValue_WithZeroId_DoesNotAdd()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.AddSelectedValue(0);
        attr.SelectedValueIds.Should().HaveCount(1);
    }

    [Fact]
    public void AddSelectedValue_WithNewId_AddsToCollection()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.AddSelectedValue(20);
        attr.SelectedValueIds.Should().Contain(20);
    }

    [Fact]
    public void AddSelectedValue_WithDuplicateId_DoesNotAddDuplicate()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.AddSelectedValue(10);
        attr.SelectedValueIds.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSelectedValue_WithZeroId_DoesNothing()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.RemoveSelectedValue(0);
        attr.SelectedValueIds.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSelectedValue_WithExistingId_RemovesFromCollection()
    {
        var attr = new ProductAttribute(1, [10, 20]);
        attr.RemoveSelectedValue(10);
        attr.SelectedValueIds.Should().NotContain(10);
        attr.SelectedValueIds.Should().Contain(20);
    }

    [Fact]
    public void RemoveSelectedValue_WithLastId_ThrowsInvalidAttributeException()
    {
        var attr = new ProductAttribute(1, [10]);
        var act = () => attr.RemoveSelectedValue(10);
        act.Should().Throw<InvalidAttributeException>();
    }

    [Fact]
    public void SetFreeTextValue_WithValidValue_SetsValue()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.SetFreeTextValue("  Brand A  ");
        attr.FreeTextValue.Should().Be("Brand A");
    }

    [Fact]
    public void SetFreeTextValue_WithNull_WhenHasSelectedValues_SetsToNull()
    {
        var attr = new ProductAttribute(1, [10]);
        attr.SetFreeTextValue(null);
        attr.FreeTextValue.Should().BeNull();
    }

    [Fact]
    public void SetFreeTextValue_WithNull_WhenNoSelectedValues_ThrowsInvalidAttributeException()
    {
        var attr = new ProductAttribute(1, freeTextValue: "Brand");
        var act = () => attr.SetFreeTextValue(null);
        act.Should().Throw<InvalidAttributeException>();
    }
}
