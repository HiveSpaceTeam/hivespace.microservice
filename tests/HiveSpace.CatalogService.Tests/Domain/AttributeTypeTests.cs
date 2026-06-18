using FluentAssertions;
using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain;

public class AttributeTypeTests
{
    [Fact]
    public void IsMultiValue_WhenMaxValueCountGreaterThanOne_ReturnsTrue()
    {
        var type = new AttributeType(AttributeValueType.String, InputType.Textbox, maxValueCount: 3);
        type.IsMultiValue.Should().BeTrue();
    }

    [Fact]
    public void IsMultiValue_WhenMaxValueCountIsOne_ReturnsFalse()
    {
        var type = new AttributeType(AttributeValueType.String, InputType.Textbox, maxValueCount: 1);
        type.IsMultiValue.Should().BeFalse();
    }

    [Fact]
    public void IsSingleSelect_WhenValueTypeIsSingleSelect_ReturnsTrue()
    {
        var type = new AttributeType(AttributeValueType.SingleSelect, InputType.Dropdown);
        type.IsSingleSelect.Should().BeTrue();
    }

    [Fact]
    public void IsSingleSelect_WhenValueTypeIsNotSingleSelect_ReturnsFalse()
    {
        var type = new AttributeType(AttributeValueType.String, InputType.Textbox);
        type.IsSingleSelect.Should().BeFalse();
    }

    [Fact]
    public void IsMultiSelect_WhenValueTypeIsMultiSelect_ReturnsTrue()
    {
        var type = new AttributeType(AttributeValueType.MultiSelect, InputType.Checkbox);
        type.IsMultiSelect.Should().BeTrue();
    }

    [Fact]
    public void IsMultiSelect_WhenValueTypeIsNotMultiSelect_ReturnsFalse()
    {
        var type = new AttributeType(AttributeValueType.String, InputType.Textbox);
        type.IsMultiSelect.Should().BeFalse();
    }

    [Fact]
    public void TwoAttributeTypesWithSameFields_AreEqual()
    {
        var a = new AttributeType(AttributeValueType.SingleSelect, InputType.Dropdown, true, 1);
        var b = new AttributeType(AttributeValueType.SingleSelect, InputType.Dropdown, true, 1);
        a.Should().Be(b);
    }
}
