using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate
{
    public class CategoryAttribute(int attributeId, int categoryId) : ValueObject
    {
        public int AttributeId { get; private set; } = attributeId;

        public int CategoryId { get; private set; } = categoryId;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AttributeId;
            yield return CategoryId;
        }
    }
}
