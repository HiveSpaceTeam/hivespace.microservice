using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate
{
    public  class AttributeValue : Entity<int>
    {
        public int AttributeId { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public int? ParentValueId { get; private set; }
        public bool IsActive { get; private set; }
        public int SortOrder { get; private set; }

        public AttributeValue(int attributeId, string name, string displayName, int? parentValueId, bool isActive, int sortOrder)
        {
            AttributeId = attributeId;
            Name = name;
            DisplayName = displayName;
            ParentValueId = parentValueId;
            IsActive = isActive;
            SortOrder = sortOrder;
        }

        public AttributeValue(int id, int attributeId, string name, string displayName, int? parentValueId, bool isActive, int sortOrder)
            : this(attributeId, name, displayName, parentValueId, isActive, sortOrder)
        {
            Id = id;
        }

        public void Update(string name, string displayName, bool isActive, int sortOrder)
        {
            Name = name;
            DisplayName = displayName;
            IsActive = isActive;
            SortOrder = sortOrder;
        }
    }
}
