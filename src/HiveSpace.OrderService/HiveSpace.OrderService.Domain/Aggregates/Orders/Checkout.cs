using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class Checkout : ValueObject
{
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Checkout() { }

    public static Checkout Create(PaymentMethod paymentMethod, Money amount)
    {
        return new Checkout
        {
            PaymentMethod = paymentMethod,
            Amount = amount,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PaymentMethod;
        yield return Amount;
        yield return CreatedAt;
    }
}
