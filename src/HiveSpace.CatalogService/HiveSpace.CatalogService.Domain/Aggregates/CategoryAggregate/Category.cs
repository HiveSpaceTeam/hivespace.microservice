using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate
{
    public class Category: AggregateRoot<Guid>
    {
        public string Name { get; private set; }
        public Guid? ParentId { get; private set; }
        private readonly List<Guid> _childrenIds;
    }
}
