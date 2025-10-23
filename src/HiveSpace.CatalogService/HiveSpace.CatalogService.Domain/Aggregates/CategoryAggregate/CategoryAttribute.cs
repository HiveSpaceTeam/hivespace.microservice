using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate
{
    public class CategoryAttribute(Guid attributeId, Guid categoryId) : ValueObject
    {
        public Guid AttributeId { get; private set; } = attributeId;

        public Guid CategoryId { get; private set; } = categoryId;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AttributeId;
            yield return CategoryId;
        }
    }
}
