using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class AttributeDefinitionTests
{
    [Fact]
    public void AttributeDefinition_Create_SetsNameAndType()
    {
        var type = new AttributeType(AttributeValueType.String, InputType.Textbox);
        var attr = new AttributeDefinition("Brand", type);
        attr.Name.Should().Be("Brand");
        attr.Type.Should().Be(type);
        attr.IsActive.Should().BeTrue();
        attr.ParentId.Should().BeNull();
        attr.Values.Should().BeEmpty();
    }

    [Fact]
    public void AttributeDefinition_Create_WithParentAndInactive_SetsProperties()
    {
        var type = new AttributeType(AttributeValueType.SingleSelect, InputType.Dropdown);
        var attr = new AttributeDefinition("Color", type, parentId: 5, isActive: false);
        attr.ParentId.Should().Be(5);
        attr.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AttributeValue_Create_SetsAllProperties()
    {
        var value = new AttributeValue(1, "red", "Red", null, true, 1);
        value.AttributeId.Should().Be(1);
        value.Name.Should().Be("red");
        value.DisplayName.Should().Be("Red");
        value.ParentValueId.Should().BeNull();
        value.IsActive.Should().BeTrue();
        value.SortOrder.Should().Be(1);
    }

    [Fact]
    public void AttributeValue_Create_WithParentValueId_SetsParent()
    {
        var value = new AttributeValue(1, "blue", "Blue", 10, false, 2);
        value.ParentValueId.Should().Be(10);
        value.IsActive.Should().BeFalse();
        value.SortOrder.Should().Be(2);
    }
}
