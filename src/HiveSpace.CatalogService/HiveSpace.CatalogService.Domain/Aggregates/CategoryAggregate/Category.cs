using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate
{
    public class Category: AggregateRoot<int>
    {
        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public string? FileImageId { get; private set; }

        private readonly List<Category> _children = [];

        public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

        public bool HasChildren => _children.Count > 0;

        public int? ParentId { get; private set; }
        public Category? Parent { get; private set; }

        public List<int> AttributeIds { get; set; }

        public Category(int id, string name, string displayName, int? parentId, string? fileImageId = null)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            ParentId = parentId;
            FileImageId = fileImageId;
        }
    }
}
