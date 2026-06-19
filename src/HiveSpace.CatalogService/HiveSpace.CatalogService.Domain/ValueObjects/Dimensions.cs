using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.ValueObjects
{
    using HiveSpace.CatalogService.Domain.Enums;

    public class Dimensions : ValueObject
    {
        public decimal Length { get; private set; }
        public decimal Width { get; private set; }
        public decimal Height { get; private set; }
        public DimensionUnit Unit { get; private set; }

        private Dimensions() { }

        public Dimensions(decimal length, decimal width, decimal height, DimensionUnit unit)
        {
            if (length < 0 || width < 0 || height < 0)
                throw new ArgumentException("Dimensions cannot be negative");

            Length = length;
            Width = width;
            Height = height;
            Unit = unit;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Length;
            yield return Width;
            yield return Height;
            yield return Unit;
        }

        public override string ToString() => $"{Length}x{Width}x{Height} {Unit}";
    }

}
