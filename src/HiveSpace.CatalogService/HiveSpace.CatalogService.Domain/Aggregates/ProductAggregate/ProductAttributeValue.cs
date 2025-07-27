using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductAttributeValue : Entity<int>
    {
        #region Properties
        public int AttributeId { get; private set; }
        public int? ValueId { get; private set; }
        public string? RawValue { get; private set; }
        #endregion

        #region Constructors
        public ProductAttributeValue(int attributeId, int? valueId, string? rawValue)
        {

            AttributeId = attributeId;
            ValueId = valueId;
            RawValue = rawValue;
            if (IsInvalid())
            {
                throw new Exception("Invalid attribute value");
            }
        }
        #endregion

        #region Methods
        private bool IsInvalid()
        {
            return (string.IsNullOrEmpty(RawValue) && (ValueId == 0 || ValueId is null)) || (!string.IsNullOrEmpty(RawValue) && ValueId is not null && ValueId != 0)
                || AttributeId == 0;
        }

        public void UpdateRawValue(string rawValue)
        {
            RawValue = rawValue;
            ValueId = null;
        }

        public void UpdateValueId(int valueId)
        {
            ValueId = valueId;
            RawValue = null;
        } 
        #endregion
    }
}
