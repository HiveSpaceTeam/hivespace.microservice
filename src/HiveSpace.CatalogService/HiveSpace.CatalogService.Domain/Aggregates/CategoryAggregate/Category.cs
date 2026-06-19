using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate
{
    public class Category : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public int? ParentId { get; private set; }
        public int? ProductSetId { get; private set; }
        public bool? IsActive { get; private set; }
        public string? ImageFileId { get; private set; }
        public string? ImageUrl { get; private set; }

        private readonly List<CategoryAttribute> _categoryAttributes = [];
        public IReadOnlyCollection<CategoryAttribute> CategoryAttributes => _categoryAttributes.AsReadOnly();

        private Category()
        {
            Name = string.Empty;
        }

        public Category(string name, int? parentId = null, int? productSetId = null, bool? isActive = null, string? imageFileId = null)
        {
            Name = name;
            ParentId = parentId;
            ProductSetId = productSetId;
            IsActive = isActive;
            ImageFileId = imageFileId;
        }

        public Category(int id, string name, int? parentId = null, int? productSetId = null, bool? isActive = null, string? imageFileId = null)
            : this(name, parentId, productSetId, isActive, imageFileId)
        {
            Id = id;
        }

        public void SetImageUrl(string url)
        {
            ImageUrl = url;
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
