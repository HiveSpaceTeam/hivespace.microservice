using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.Enumerations;

public class OrderStatus(int id, string name) : Enumeration(id, name)
{
    public static readonly OrderStatus Created = new(1, nameof(Created));
    public static readonly OrderStatus Paid = new(2, nameof(Paid));
    public static readonly OrderStatus COD = new(3, nameof(COD));
    public static readonly OrderStatus Confirmed = new(4, nameof(Confirmed));
    public static readonly OrderStatus Rejected = new(5, nameof(Rejected));
    public static readonly OrderStatus Shipped = new(6, nameof(Shipped));
    public static readonly OrderStatus Delivered = new(7, nameof(Delivered));
    public static readonly OrderStatus Completed = new(8, nameof(Completed));
    public static readonly OrderStatus Cancelled = new(9, nameof(Cancelled));
    public static readonly OrderStatus Claimed = new(10, nameof(Claimed));
    public static readonly OrderStatus Refunding = new(11, nameof(Refunding));
    public static readonly OrderStatus Refunded = new(12, nameof(Refunded));
    public static readonly OrderStatus Solved = new(13, nameof(Solved));
    public static readonly OrderStatus Expired = new(14, nameof(Expired));

    public bool IsFinal() => this == Completed || this == Refunded || this == Solved || this == Expired;
    public bool CanBeCancelled() => this == Created || this == Paid || this == COD || this == Confirmed;
    public bool CanBeShipped() => this == Confirmed;
    public bool IsInProgress() => !IsFinal() && this != Cancelled && this != Rejected;
}
