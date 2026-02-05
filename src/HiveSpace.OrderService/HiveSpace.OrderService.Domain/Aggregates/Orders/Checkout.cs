using HiveSpace.Domain.Shared.Entities;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class Checkout : Entity<Guid>
{
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Checkout() { }

    public static Checkout Create(PaymentMethod paymentMethod, Money amount)
    {
        return new Checkout
        {
            Id = Guid.NewGuid(),
            PaymentMethod = paymentMethod,
            Amount = amount,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
