using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.ValueObjects
{
    /// <summary>
    /// Represents a phone number with validation for Vietnam format.
    /// Immutable value object.
    /// </summary>
    public class PhoneNumber : ValueObject
    {
        public string Value { get; private set; }

        private PhoneNumber() 
        {
            Value = null!;
        }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidFieldException(OrderDomainErrorCode.PhoneRequired, nameof(value));

            var cleaned = CleanPhoneNumber(value);

            if (!IsValidVietnamesePhoneNumber(cleaned))
                throw new InvalidFieldException(OrderDomainErrorCode.PhoneInvalidFormat, nameof(value));

            Value = cleaned;
        }

        private static string CleanPhoneNumber(string phoneNumber)
        {
            // Remove spaces, dashes, and parentheses
            return phoneNumber.Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");
        }

        private static bool IsValidVietnamesePhoneNumber(string phoneNumber)
        {
            // Vietnam phone number patterns:
            // - Mobile: starts with 03, 05, 07, 08, 09 followed by 8 digits
            // - Can start with +84 or 84 or 0
            var pattern = @"^(\+84|84|0)[3|5|7|8|9][0-9]{8}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        /// <summary>
        /// Formats the phone number for display (e.g., 0901234567).
        /// </summary>
        public string GetDisplayFormat()
        {
            if (Value.StartsWith("+84"))
                return string.Concat("0", Value.AsSpan(3));
            if (Value.StartsWith("84"))
                return string.Concat("0", Value.AsSpan(2));
            return Value;
        }

        /// <summary>
        /// Gets the international format (e.g., +84901234567).
        /// </summary>
        public string GetInternationalFormat()
        {
            if (Value.StartsWith("+84"))
                return Value;
            if (Value.StartsWith("84"))
                return "+" + Value;
            if (Value.StartsWith('0'))
                return string.Concat("+84", Value.AsSpan(1));
            return "+84" + Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => GetDisplayFormat();

        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber ?? "";
    }
}
