using System;
using System.Collections.Generic;
using System.Linq;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.ValueObjects
{
    /// <summary>
    /// Immutable snapshot of product details at the time of order creation.
    /// Preserves product information even if the product is later modified or deleted.
    /// </summary>
    public class ProductSnapshot : ValueObject
    {
        public long ProductId { get; private set; }
        public long SkuId { get; private set; }
        public string ProductName { get; private set; }
        public string SkuName { get; private set; }
        public Money Price { get; private set; }
        public string ImageUrl { get; private set; }
        public IReadOnlyDictionary<string, string> Attributes { get; private set; }
        public DateTimeOffset CapturedAt { get; private set; }

        private ProductSnapshot()
        {
            ProductName = null!;
            SkuName = null!;
            Price = null!;
            ImageUrl = null!;
            Attributes = null!;
        }

        private ProductSnapshot(
            long productId,
            long skuId,
            string productName,
            string skuName,
            Money price,
            string imageUrl,
            Dictionary<string, string> attributes,
            DateTimeOffset capturedAt)
        {
            ProductId = productId;
            SkuId = skuId;
            ProductName = productName;
            SkuName = skuName;
            Price = price;
            ImageUrl = imageUrl;
            Attributes = attributes?.AsReadOnly() ?? new Dictionary<string, string>().AsReadOnly();
            CapturedAt = capturedAt;
        }

        /// <summary>
        /// Captures a snapshot of the product at the current moment.
        /// </summary>
        public static ProductSnapshot Capture(
            long productId,
            long skuId,
            string productName,
            string skuName,
            Money price,
            string imageUrl,
            Dictionary<string, string>? attributes = null)
        {
            if (productId <= 0)
                throw new InvalidFieldException(OrderDomainErrorCode.SnapshotInvalidProductId, nameof(productId));

            if (skuId <= 0)
                throw new InvalidFieldException(OrderDomainErrorCode.SnapshotInvalidSkuId, nameof(skuId));

            if (string.IsNullOrWhiteSpace(productName))
                throw new InvalidFieldException(OrderDomainErrorCode.SnapshotProductNameRequired, nameof(productName));

            if (price == null)
                throw new InvalidFieldException(OrderDomainErrorCode.SnapshotPriceRequired, nameof(price));

            return new ProductSnapshot(
                productId,
                skuId,
                productName,
                skuName ?? productName,
                price,
                imageUrl,
                attributes != null ? new Dictionary<string, string>(attributes) : [],
                DateTimeOffset.UtcNow
            );
        }

        /// <summary>
        /// Gets a specific attribute value by key.
        /// </summary>
        public string? GetAttributeValue(string key)
        {
            return Attributes.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Checks if a specific attribute exists.
        /// </summary>
        public bool HasAttribute(string key)
        {
            return Attributes.ContainsKey(key);
        }

        /// <summary>
        /// Gets a formatted display name including SKU attributes.
        /// </summary>
        public string GetDisplayName()
        {
            if (!Attributes.Any())
                return ProductName;

            var attributeString = string.Join(", ", Attributes.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            return $"{ProductName} ({attributeString})";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ProductId;
            yield return SkuId;
            yield return CapturedAt;
        }

        public override string ToString() => GetDisplayName();
    }
}
