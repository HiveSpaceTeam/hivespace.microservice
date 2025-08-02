using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductAttribute : Entity<int>
    {
        #region Properties
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public int ProductId { get; set; }
        #endregion

        #region Constructors
        public ProductAttribute()
        {
            
        }

        public ProductAttribute(int attributeId, string value, int productId)
        {
            AttributeId = attributeId;
            Value = value;
            ProductId = productId;
            if (IsInvalid())
            {
                throw new InvalidAttributeException();
            }
        }


        #endregion

        #region Methods
        private bool IsInvalid()
        {
            return string.IsNullOrEmpty(Value);
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
