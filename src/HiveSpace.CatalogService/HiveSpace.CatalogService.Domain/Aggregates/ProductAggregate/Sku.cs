using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;

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
            Price = new Money(0m, Currency.USD);
        }

        public Sku(string skuNo, Guid productId, List<SkuVariant> skuVariants, List<SkuImage> images, int quantity, bool isActive, Money price)
        {
            SkuNo = skuNo;
            ProductId = productId;
            _skuVariants = new List<SkuVariant>(skuVariants);
            _images = new List<SkuImage>(images);
            Quantity = quantity;
            IsActive = isActive;
            Price = price;
        }

        public Sku(Guid id, string skuNo, Guid productId, List<SkuVariant> skuVariants, List<SkuImage> images, int quantity, bool isActive, Money price)
        {
            Id = id;
            SkuNo = skuNo;
            ProductId = productId;
            _skuVariants = new List<SkuVariant>(skuVariants);
            _images = new List<SkuImage>(images);
            Quantity = quantity;
            IsActive = isActive;
            Price = price;
        }



        #endregion

        #region Methods
        public void UpdateQuantity(int quantity)
        {
            if (quantity < 0) { 
                throw new InvalidQuantityException();
            }
            Quantity = quantity;
        }
        #endregion
    }
}
