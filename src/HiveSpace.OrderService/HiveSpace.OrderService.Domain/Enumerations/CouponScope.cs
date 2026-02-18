using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.Enumerations;

public class CouponScope(int id, string name) : Enumeration(id, name)
{
    public static readonly CouponScope ShippingPrice = new(1, "shipping_price");
    public static readonly CouponScope TotalPrice = new(2, "total_price");
    public static readonly CouponScope ItemPrice = new(3, "item_price");
}
