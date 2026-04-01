namespace HiveSpace.OrderService.Domain.Enumerations;

public static class OrderTrackingType
{
    public const string Created = "CREATED";
    public const string Paid = "PAID";
    public const string COD = "COD";
    public const string Confirmed = "CONFIRMED";
    public const string OrderConfirmed  = "ORDER_CONFIRMED";
    public const string OrderRejected   = "ORDER_REJECTED";
    public const string Shipped = "SHIPPED";
    public const string Delivered = "DELIVERED";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";
    public const string Expired = "EXPIRED";
}
