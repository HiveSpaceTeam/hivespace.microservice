using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate
{
    /// <summary>
    /// Attribute type enumeration
    /// </summary>
    public enum AttributeValueType
    {
        String = 1,
        SingleSelect = 2,
        Number = 3,
        Date = 4,
        MultiSelect = 5
    }

    /// <summary>
    /// Input type enumeration for attribute validation
    /// </summary>
    public enum InputType
    {
        Textbox = 1,
        Dropdown = 2,
        Radio = 3,
        Checkbox = 4,
        ComboBox = 5
    }

    /// <summary>
    /// Attribute type value object
    /// </summary>
    public sealed class AttributeType : ValueObject
    {
        public AttributeValueType ValueType { get; private set; }
        public InputType InputType { get; private set; }
        public bool IsMandatory { get; private set; }
        public int MaxValueCount { get; private set; }

        public AttributeType(
            AttributeValueType valueType,
            InputType inputType,
            bool isMandatory = false,
            int maxValueCount = 1)
        {
            ValueType = valueType;
            InputType = inputType;
            IsMandatory = isMandatory;
            MaxValueCount = maxValueCount;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ValueType;
            yield return InputType;
            yield return IsMandatory;
            yield return MaxValueCount;
        }

        public bool IsMultiValue => MaxValueCount > 1;
        public bool IsSingleSelect => ValueType == AttributeValueType.SingleSelect;
        public bool IsMultiSelect => ValueType == AttributeValueType.MultiSelect;
    }
}
