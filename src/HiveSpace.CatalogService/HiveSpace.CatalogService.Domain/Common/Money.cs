using HiveSpace.CatalogService.Domain.Common.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json.Serialization;

namespace HiveSpace.CatalogService.Domain.Common
{
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public Currency Currency { get; private set; }

        [JsonConstructor]
        public Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
            if (IsInvalid())
            {
                throw new InvalidMoneyException();
            }
        }

        private Money() { }

        private bool IsInvalid()
        {
            return Amount < 0m;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
