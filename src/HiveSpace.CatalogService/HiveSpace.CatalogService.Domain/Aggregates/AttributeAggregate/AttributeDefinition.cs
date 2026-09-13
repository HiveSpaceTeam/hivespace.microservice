using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.Domain.Shared.Entities;
using System;

namespace HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate
{
    public class AttributeDefinition : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public AttributeType Type { get; private set; }
        public int? ParentId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<AttributeValue> _values = new();
        public IReadOnlyList<AttributeValue> Values => _values.AsReadOnly();

        private AttributeDefinition()
        {
            Name = string.Empty;
            Type = default!;
        }

        public AttributeDefinition(string name, AttributeType type, int? parentId = null, bool isActive = true)
        {
            Name = name;
            Type = type;
            ParentId = parentId;
            IsActive = isActive;
            CreatedAt = DateTime.UtcNow;
        }

        public AttributeDefinition(int id, string name, AttributeType type, int? parentId = null, bool isActive = true)
            : this(name, type, parentId, isActive)
        {
            Id = id;
        }

        public void UpdateDefinition(string name, AttributeType type, bool isActive)
        {
            Name = name;
            Type = type;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public AttributeValue AddValue(string name, string displayName, int? parentValueId = null, bool isActive = true, int sortOrder = 0)
        {
            var value = new AttributeValue(Id, name, displayName, parentValueId, isActive, sortOrder);
            _values.Add(value);
            UpdatedAt = DateTime.UtcNow;
            return value;
        }

        public AttributeValue AddValue(int id, string name, string displayName, int? parentValueId = null, bool isActive = true, int sortOrder = 0)
        {
            var value = new AttributeValue(id, Id, name, displayName, parentValueId, isActive, sortOrder);
            _values.Add(value);
            UpdatedAt = DateTime.UtcNow;
            return value;
        }

        public AttributeValue? FindValue(string name, string displayName)
            => _values.FirstOrDefault(v =>
                string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(v.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
    }
}
