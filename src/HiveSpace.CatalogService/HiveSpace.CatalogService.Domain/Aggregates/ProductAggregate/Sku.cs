using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.CatalogService.Domain.Common.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class Sku : Entity<Guid>
    {
        #region Properties
        public string SkuNo { get; private set; }

        public Guid ProductId { get; private set; }

        public IReadOnlyCollection<SkuVariant> SkuVariants { get; private set; }

        //private readonly List<SkuImage> _images = [];
        //public IReadOnlyCollection<SkuImage> Images { get; private set; }

        public int Quantity { get; private set; }

        public bool IsActive { get; private set; }

        public Money Price { get; private set; }
        #endregion

        #region Constructors
        [JsonConstructor]
        public Sku(string skuNo, Guid productId, IReadOnlyCollection<SkuVariant> skuVariants,  int quantity, bool isActive, Money price)
        {
            SkuNo = skuNo;
            ProductId = productId;
            SkuVariants = skuVariants;
            //Images = images;
            Quantity = quantity;
            IsActive = isActive;
            Price = price;
        }

        private Sku() { }
        #endregion

        #region Methods
        public void UpdateQuantity(int quantity)
        {
            Quantity = quantity;
        }
        private bool IsInvalid()
        {
            return Quantity < 0;
        }
        #endregion
    }
}
