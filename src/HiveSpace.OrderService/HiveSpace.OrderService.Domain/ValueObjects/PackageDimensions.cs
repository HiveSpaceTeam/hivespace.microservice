using System;
using System.Collections.Generic;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.ValueObjects
{
    /// <summary>
    /// Represents package dimensions and weight for shipping calculations.
    /// Immutable value object.
    /// </summary>
    public class PackageDimensions : ValueObject
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Length { get; private set; }
        public int Weight { get; private set; }

        // Extra dimensions from actual measurements by shipping provider
        public int ExtraWidth { get; private set; }
        public int ExtraHeight { get; private set; }
        public int ExtraLength { get; private set; }
        public int ExtraWeight { get; private set; }

        // Full dimensions including extras
        public int FullWidth => Width + ExtraWidth;
        public int FullHeight => Height + ExtraHeight;
        public int FullLength => Length + ExtraLength;
        public int FullWeight => Weight + ExtraWeight;

        private PackageDimensions() { }

        public PackageDimensions(int width, int height, int length, int weight)
        {
            if (width <= 0)
                throw new InvalidFieldException(OrderDomainErrorCode.DimensionsInvalidWidth, nameof(width));

            if (height <= 0)
                throw new InvalidFieldException(OrderDomainErrorCode.DimensionsInvalidHeight, nameof(height));

            if (length <= 0)
                throw new InvalidFieldException(OrderDomainErrorCode.DimensionsInvalidLength, nameof(length));

            if (weight <= 0)
                throw new InvalidFieldException(OrderDomainErrorCode.DimensionsInvalidWeight, nameof(weight));

            Width = width;
            Height = height;
            Length = length;
            Weight = weight;
            ExtraWidth = 0;
            ExtraHeight = 0;
            ExtraLength = 0;
            ExtraWeight = 0;
        }

        /// <summary>
        /// Creates a new instance with actual measurements from shipping provider.
        /// </summary>
        public PackageDimensions WithActualMeasurements(int extraWidth, int extraHeight, int extraLength, int extraWeight)
        {
            if (extraWidth < 0 || extraHeight < 0 || extraLength < 0 || extraWeight < 0)
                throw new InvalidFieldException(OrderDomainErrorCode.DimensionsInvalidExtras, nameof(extraWidth));

            return new PackageDimensions(Width, Height, Length, Weight)
            {
                ExtraWidth = extraWidth,
                ExtraHeight = extraHeight,
                ExtraLength = extraLength,
                ExtraWeight = extraWeight
            };
        }

        /// <summary>
        /// Calculates the volumetric weight (in grams).
        /// Formula: (Length × Width × Height) / 6000
        /// </summary>
        public int CalculateVolumetricWeight()
        {
            return FullLength * FullWidth * FullHeight / 6000;
        }

        /// <summary>
        /// Gets the chargeable weight (higher of actual weight or volumetric weight).
        /// </summary>
        public int GetChargeableWeight()
        {
            return Math.Max(FullWeight, CalculateVolumetricWeight());
        }

        /// <summary>
        /// Checks if actual measurements differ from declared measurements.
        /// </summary>
        public bool HasDiscrepancy()
        {
            return ExtraWidth > 0 || ExtraHeight > 0 || ExtraLength > 0 || ExtraWeight > 0;
        }

        /// <summary>
        /// Calculates the percentage increase in chargeable weight.
        /// </summary>
        public decimal GetWeightIncreasePercentage()
        {
            if (Weight == 0) return 0;

            var declaredChargeableWeight = Math.Max(Weight, CalculateDeclaredVolumetricWeight());
            var actualChargeableWeight = GetChargeableWeight();

            if (declaredChargeableWeight == 0) return 0;

            return (decimal)actualChargeableWeight - declaredChargeableWeight / declaredChargeableWeight * 100;
        }

        private int CalculateDeclaredVolumetricWeight()
        {
            return Length * Width * Height / 6000;
        }

        /// <summary>
        /// Creates standard package dimensions.
        /// </summary>
        public static PackageDimensions CreateStandard()
        {
            return new PackageDimensions(30, 20, 10, 500); // 30cm x 20cm x 10cm, 500g
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FullWidth;
            yield return FullHeight;
            yield return FullLength;
            yield return FullWeight;
        }

        public override string ToString() => 
            $"{FullLength}cm × {FullWidth}cm × {FullHeight}cm, {FullWeight}g";
    }
}
