using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.ValueObjects
{
    /// <summary>
    /// Represents a delivery address for orders.
    /// Immutable value object.
    /// </summary>
    public class DeliveryAddress : ValueObject
    {
        public string RecipientName { get; private set; }
        public PhoneNumber Phone { get; private set; }
        public string StreetAddress { get; private set; }
        public string Ward { get; private set; }
        public string Province { get; private set; }
        public string Country { get; private set; }
        public string Notes { get; private set; }

        private DeliveryAddress() 
        { 
            RecipientName = null!;
            Phone = null!;
            StreetAddress = null!;
            Ward = null!;
            Province = null!;
            Country = null!;
            Notes = null!;
        }

        public DeliveryAddress(
            string recipientName,
            PhoneNumber phone,
            string streetAddress,
            string ward,
            string province,
            string country = "Vietnam",
            string notes = "")
        {
            if (string.IsNullOrWhiteSpace(recipientName))
                throw new InvalidFieldException(OrderDomainErrorCode.AddressRecipientRequired, nameof(recipientName));

            if (phone is null)
                throw new InvalidFieldException(OrderDomainErrorCode.AddressPhoneRequired, nameof(phone));

            if (string.IsNullOrWhiteSpace(streetAddress))
                throw new InvalidFieldException(OrderDomainErrorCode.AddressStreetRequired, nameof(streetAddress));

            if (string.IsNullOrWhiteSpace(ward))
                throw new InvalidFieldException(OrderDomainErrorCode.AddressWardRequired, nameof(ward));

            if (string.IsNullOrWhiteSpace(province))
                throw new InvalidFieldException(OrderDomainErrorCode.AddressProvinceRequired, nameof(province));

            RecipientName = recipientName.Trim();
            Phone = phone;
            StreetAddress = streetAddress.Trim();
            Ward = ward.Trim();
            Province = province.Trim();
            Country = string.IsNullOrWhiteSpace(country) ? "Vietnam" : country.Trim();
            Notes = notes;
        }

        /// <summary>
        /// Gets the full formatted address.
        /// </summary>
        public string GetFullAddress()
        {
            var parts = new List<string>
            {
                StreetAddress,
                Ward,
                Province,
                Country
            };

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Gets the address formatted for shipping labels.
        /// </summary>
        public string GetShippingLabel()
        {
            var label = $"{RecipientName}\n{Phone.GetDisplayFormat()}\n{GetFullAddress()}";
            
            if (!string.IsNullOrWhiteSpace(Notes))
                label += $"\nNotes: {Notes}";

            return label;
        }

        /// <summary>
        /// Creates a copy with updated notes.
        /// </summary>
        public DeliveryAddress WithNotes(string notes)
        {
            return new DeliveryAddress(
                RecipientName,
                Phone,
                StreetAddress,
                Ward,
                Province,
                Country,
                notes
            );
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StreetAddress.ToLowerInvariant();
            yield return Ward.ToLowerInvariant();
            yield return Province.ToLowerInvariant();
            yield return Country.ToLowerInvariant();
        }

        public override string ToString() => GetFullAddress();
    }
}
