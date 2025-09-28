using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductAttribute : Entity<Guid>
    {
        #region Properties
        public Guid AttributeId { get; set; }
        public string Value { get; set; }
        public Guid ProductId { get; set; }
        #endregion

        #region Constructors
        // Parameterless constructor for Entity Framework
        private ProductAttribute()
        {
            Value = string.Empty;
        }

        public ProductAttribute(Guid attributeId, string value, Guid productId)
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

        #endregion
    }
}
