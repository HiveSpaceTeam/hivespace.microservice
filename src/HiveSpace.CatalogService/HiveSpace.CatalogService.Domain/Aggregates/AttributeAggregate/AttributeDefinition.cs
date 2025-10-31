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

        // Parameterless constructor for Entity Framework
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

    }
}
