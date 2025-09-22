using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate
{
    public class Attribute : AggregateRoot<Guid>
    {
        public string Name { get; private set; }
        public int ValueId { get; private set; }
        public string DisplayName { get; private set; }
        public int? ParentId { get; private set; }
        public Attribute? Parent { get; private set; }

        private readonly List<Attribute> _children = [];
        public IReadOnlyCollection<Attribute> Children => _children.AsReadOnly();

        public bool Mandatory { get; private set; }
        public Attribute(string name, string displayName, int? parentId, bool mandatory, int valueId)
        {
            Name = name;
            ValueId = valueId;
            DisplayName = displayName;
            ParentId = parentId;
            Mandatory = mandatory;
        }



    }
}
