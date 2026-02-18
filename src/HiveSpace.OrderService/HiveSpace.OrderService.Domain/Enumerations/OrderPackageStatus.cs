using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.Enumerations;

public class OrderPackageStatus(int id, string name) : Enumeration(id, name)
{
    public static readonly OrderPackageStatus Pending = new(1, nameof(Pending));
    public static readonly OrderPackageStatus Confirmed = new(2, nameof(Confirmed));
    public static readonly OrderPackageStatus Rejected = new(3, nameof(Rejected));
    public static readonly OrderPackageStatus ReadyToShip = new(4, nameof(ReadyToShip));
    public static readonly OrderPackageStatus Shipped = new(5, nameof(Shipped));
    public static readonly OrderPackageStatus Delivered = new(6, nameof(Delivered));
    public static readonly OrderPackageStatus Completed = new(7, nameof(Completed));
    public static readonly OrderPackageStatus Cancelled = new(8, nameof(Cancelled));
    public static readonly OrderPackageStatus Returned = new(9, nameof(Returned));

    public bool CanConfirm() => this == Pending;
    public bool CanReject() => this == Pending;
    public bool CanShip() => this == Confirmed || this == ReadyToShip;
    public bool CanCancel() => this == Pending || this == Confirmed;
}
