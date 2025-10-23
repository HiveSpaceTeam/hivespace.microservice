using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.Domain.Shared.Entities;
using System;

namespace HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate
{
    public class AttributeDefinition : AggregateRoot<Guid>
    {

        public string Name { get; private set; }
        public AttributeType Type { get; private set; }
        public Guid? ParentId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<AttributeValue> _values = new();
        public IReadOnlyList<AttributeValue> Values => _values.AsReadOnly();

    }
}
