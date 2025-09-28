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

        private readonly List<SkuVariant> _skuVariants = [];
        public IReadOnlyCollection<SkuVariant> SkuVariants => _skuVariants.AsReadOnly();
        private readonly List<SkuImage> _images = [];
        public IReadOnlyCollection<SkuImage> Images => _images.AsReadOnly();


        public int Quantity { get; private set; }

        public bool IsActive { get; private set; }

        public Money Price { get; private set; }
        #endregion

        #region Constructors
        // Parameterless constructor for Entity Framework
        private Sku()
        {
            SkuNo = string.Empty;
            Price = new Money(0, Currency.USD);
        }

        public Sku(string skuNo, Guid productId, List<SkuVariant> skuVariants, List<SkuImage> images, int quantity, bool isActive, Money price)
        {
            SkuNo = skuNo;
            ProductId = productId;
            _skuVariants = skuVariants;
            _images = images;
            Quantity = quantity;
            IsActive = isActive;
            Price = price;
        }



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
