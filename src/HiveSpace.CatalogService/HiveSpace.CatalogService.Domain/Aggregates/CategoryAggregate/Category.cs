using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate
{
    public class Category: AggregateRoot<int>
    {
        public string Name { get; private set; }
        public int? ParentId { get; private set; }

        private readonly List<CategoryAttribute> _categoryAttributes = [];
        public IReadOnlyCollection<CategoryAttribute> CategoryAttributes => _categoryAttributes.AsReadOnly();

        // Parameterless constructor for Entity Framework
        private Category()
        {
            Name = string.Empty;
        }

        public Category(string name, int? parentId = null)
        {
            Name = name;
            ParentId = parentId;
        }

        public void AddAttribute(int attributeId)
        {
            var categoryAttribute = new CategoryAttribute(attributeId, Id);
            _categoryAttributes.Add(categoryAttribute);
        }

        public void RemoveAttribute(int attributeId)
        {
            var categoryAttribute = _categoryAttributes.FirstOrDefault(ca => ca.AttributeId == attributeId);
            if (categoryAttribute != null)
            {
                _categoryAttributes.Remove(categoryAttribute);
            }
        }
    }
}
