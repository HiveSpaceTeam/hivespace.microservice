using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.OrderService.Domain.ValueObjects
{
    /// <summary>
    /// Money value object - UPDATED to handle multiple currencies
    /// Stores amount as smallest unit (long):
    /// - VND: 1 unit = 1 VND (50000 = 50,000 VND)
    /// - USD: 1 unit = 1 cent (1050 = $10.50)
    /// - EUR: 1 unit = 1 cent (1050 = €10.50)
    /// </summary>
    public class Money : ValueObject
    {
        public long Amount { get; private set; }
        public Currency Currency { get; private set; }

        private Money() { }

        public Money(long amount, Currency currency = Currency.VND)
        {
            if (amount < 0)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyNegativeAmount, nameof(amount));

            Amount = amount;
            Currency = currency;
        }

        #region Factory Methods

        /// <summary>
        /// Create money in VND (no decimal places)
        /// </summary>
        public static Money FromVND(long vnd) => new(vnd, Currency.VND);

        /// <summary>
        /// Create money in USD from dollar amount (has 2 decimal places)
        /// </summary>
        public static Money FromUSD(decimal dollars)
        {
            var cents = (long)Math.Round(dollars * 100, MidpointRounding.AwayFromZero);
            return new Money(cents, Currency.USD);
        }

        /// <summary>
        /// Create money in USD from cents
        /// </summary>
        public static Money FromUSDCents(long cents) => new(cents, Currency.USD);

        /// <summary>
        /// Create money in EUR from euro amount (has 2 decimal places)
        /// </summary>
        public static Money FromEUR(decimal euros)
        {
            var cents = (long)Math.Round(euros * 100, MidpointRounding.AwayFromZero);
            return new Money(cents, Currency.EUR);
        }

        /// <summary>
        /// Create money in EUR from cents
        /// </summary>
        public static Money FromEURCents(long cents) => new(cents, Currency.EUR);

        /// <summary>
        /// Create zero money
        /// </summary>
        public static Money Zero(Currency currency = Currency.VND) => new(0, currency);

        #endregion

        #region Conversion Methods

        /// <summary>
        /// Get decimal representation for display/calculation
        /// VND: 50000 → 50000.00
        /// USD: 1050 → 10.50
        /// EUR: 1050 → 10.50
        /// </summary>
        public decimal ToDecimal()
        {
            var decimalPlaces = GetDecimalPlaces(Currency);
            var divisor = (decimal)Math.Pow(10, decimalPlaces);
            return Amount / divisor;
        }

        private static int GetDecimalPlaces(Currency currency)
        {
            return currency switch
            {
                Currency.VND => 0,
                Currency.USD => 2,
                Currency.EUR => 2,
                _ => 2
            };
        }

        private static string GetCurrencySymbol(Currency currency)
        {
            return currency switch
            {
                Currency.VND => "₫",
                Currency.USD => "$",
                Currency.EUR => "€",
                _ => currency.ToString() // Fallback to enum name
            };
        }

        #endregion

        #region Business Methods

        /// <summary>
        /// Check if amount exceeds COD limit (2,000,000 VND)
        /// Only applicable for VND
        /// </summary>
        public bool ExceedsCODLimit()
        {
            if (Currency != Currency.VND)
                return false;

            return Amount > 2_000_000;
        }

        /// <summary>
        /// Apply service fee and return the amount after deduction
        /// </summary>
        public Money ApplyServiceFee(decimal feeRate = 0.099m)
        {
            if (feeRate < 0 || feeRate > 1)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyInvalidFeeRate, nameof(feeRate));

            var feeAmount = (long)(Amount * feeRate);
            return new Money(Amount - feeAmount, Currency);
        }

        /// <summary>
        /// Calculate service fee amount
        /// </summary>
        public Money CalculateServiceFee(decimal feeRate = 0.099m)
        {
            if (feeRate < 0 || feeRate > 1)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyInvalidFeeRate, nameof(feeRate));

            var feeAmount = (long)(Amount * feeRate);
            return new Money(feeAmount, Currency);
        }

        /// <summary>
        /// Applies a percentage discount.
        /// </summary>
        public Money ApplyPercentageDiscount(decimal percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyInvalidPercentage, nameof(percentage));

            var discountAmount = (long)(Amount * (percentage / 100m));
            return new Money(Amount - discountAmount, Currency);
        }

        /// <summary>
        /// Applies a fixed amount discount.
        /// </summary>
        public Money ApplyDiscount(Money discount)
        {
            if (discount.Currency != Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(discount));

            var newAmount = Amount - discount.Amount;
            return new Money(newAmount < 0 ? 0 : newAmount, Currency);
        }

        public bool IsZero() => Amount == 0;
        public bool IsPositive() => Amount > 0;

        public bool IsNegative() => Amount < 0;

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            var newAmount = (long)Math.Round(money.Amount * multiplier, MidpointRounding.AwayFromZero);
            return new Money(newAmount, money.Currency);
        }

        public static Money operator *(Money money, int multiplier)
        {
            return new Money(money.Amount * multiplier, money.Currency);
        }

        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor == 0)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyDivideByZero, nameof(divisor));

            var newAmount = (long)Math.Round(money.Amount / divisor, MidpointRounding.AwayFromZero);
            return new Money(newAmount, money.Currency);
        }

        public static bool operator >(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return a.Amount > b.Amount;
        }

        public static bool operator <(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return a.Amount < b.Amount;
        }

        public static bool operator >=(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return a.Amount >= b.Amount;
        }

        public static bool operator <=(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return a.Amount <= b.Amount;
        }

        #endregion

        #region Aggregate Functions

        public static Money Sum(IEnumerable<Money> monies)
        {
            var moneyList = monies.ToList();
            
            if (moneyList.Count == 0)
                return Zero(Currency.VND);

            var currency = moneyList.First().Currency;
            
            if (moneyList.Any(m => m.Currency != currency))
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            var totalAmount = moneyList.Sum(m => m.Amount);
            return new Money(totalAmount, currency);
        }

        public static Money Min(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return a.Amount <= b.Amount ? a : b;
        }

        public static Money Max(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidFieldException(OrderDomainErrorCode.MoneyCurrencyMismatch, nameof(Currency));

            return a.Amount >= b.Amount ? a : b;
        }

        #endregion

        #region Display Methods

        public override string ToString()
        {
            var decimalPlaces = GetDecimalPlaces(Currency);
            var decimalValue = ToDecimal();
            var symbol = GetCurrencySymbol(Currency);
            var code = Currency.ToString(); // Helper to prevent repeated ToString calls

            return Currency switch
            {
                Currency.VND => $"{decimalValue:N0} {symbol}",           // "50,000 ₫"
                Currency.USD => $"{symbol}{decimalValue:N2}",            // "$10.50"
                Currency.EUR => $"{symbol}{decimalValue:N2}",            // "€10.50"
                _ => $"{decimalValue.ToString($"N{decimalPlaces}")} {code}"
            };
        }

        public string ToStringWithCurrencyCode()
        {
            var decimalPlaces = GetDecimalPlaces(Currency);
            var decimalValue = ToDecimal();
            var code = Currency.ToString();

            return Currency switch
            {
                Currency.VND => $"{decimalValue:N0} {code}",
                _ => $"{decimalValue.ToString($"N{decimalPlaces}")} {code}"
            };
        }

        #endregion

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
