namespace HiveSpace.NotificationService.Core.DomainModels;

public static class NotificationEventType
{
    public const string OrderConfirmed    = "order.confirmed";
    public const string OrderCancelled    = "order.cancelled";
    public const string OrderDelivered    = "order.delivered";
    public const string OrderShipped      = "order.shipped";
    public const string NewOrderReceived  = "seller.new_order";
    public const string PaymentSucceeded  = "payment.succeeded";
    public const string PaymentFailed     = "payment.failed";
    public const string RefundProcessed   = "payment.refund_processed";
    public const string LowStockAlert     = "inventory.low_stock";
}
