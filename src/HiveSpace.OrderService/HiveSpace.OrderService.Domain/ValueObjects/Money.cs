using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Enums;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.ValueObjects;

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
            throw new DomainException(400, OrderErrorCode.InvalidMoney, nameof(Money));
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