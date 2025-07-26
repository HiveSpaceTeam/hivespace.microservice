using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.CatalogService.Domain.Common.Enums;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.Aggregates.SkuAggregate
{
    public class Sku : AggregateRoot<int>
    {
        #region Properties
        public string SkuNo { get; private set; }

        public int ProductId { get; private set; }

        private readonly List<SkuVariant> _skuVariants = [];

        public IReadOnlyCollection<SkuVariant> SkuVariants => _skuVariants.AsReadOnly();

        public int Quantity { get; private set; }

        public bool InActive { get; private set; }

        public Money Price { get; private set; }
        #endregion

        #region Constructors
        private Sku()
        {
        }

        public Sku(string skuNo, int productId, List<SkuVariant> skuVariants, int quantity, bool inActive, double amount, Currency currency)
        {
            SkuNo = skuNo;
            ProductId = productId;
            _skuVariants = skuVariants;
            Quantity = quantity;
            InActive = inActive;
            Price = new Money(amount, currency);

            if (IsInvalid())
            {
                throw new DomainException
                {
                    Errors =
                    [
                       new() {
                        Field="Quantity",
                        MessageCode= "i18nSku.InvalidSku",
                        ErrorCode=ErrorCode.InvalidSku
                    }
                    ]
                };
            }
        }

        #endregion

        #region Methods
        public void UpdateQuantity(int quantity)
        {
            this.Quantity = quantity;
        }
        private bool IsInvalid()
        {
            return Quantity < 0;
        }
        #endregion
    }
}
