using HiveSpace.CatalogService.Domain.Common.Enums;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.Common
{
    public class Money : ValueObject
    {
        public double Amount { get; private set; }

        public Currency Currency { get; private set; }

        public Money(double amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
            if (IsInvalid())
            {
                throw new DomainException
                {
                    Errors =
                   [
                       new() {
                        Field="Amount",
                        MessageCode="i18nMoney.InvalidMoney",
                        ErrorCode=ErrorCode.InvalidMoney
                    }
                   ]
                };
            }
        }

        private bool IsInvalid()
        {
            return Amount <= 0;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
