using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.ValueObjects
{
    using HiveSpace.CatalogService.Domain.Enums;

    public class Weight : ValueObject
    {
        public decimal Value { get; private set; }
        public WeightUnit Unit { get; private set; }

        private Weight() { }

        public Weight(decimal value, WeightUnit unit)
        {
            if (value < 0)
                throw new ArgumentException("Weight cannot be negative");

            Value = value;
            Unit = unit;
        }

        public decimal ToGrams()
        {
            return Unit switch
            {
                WeightUnit.Gram => Value,
                WeightUnit.Kilogram => Value * 1000,
                WeightUnit.Pound => Value * 453.592m,
                WeightUnit.Ounce => Value * 28.3495m,
                _ => Value
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Unit;
        }

        public override string ToString() => $"{Value} {Unit}";
    }

}
