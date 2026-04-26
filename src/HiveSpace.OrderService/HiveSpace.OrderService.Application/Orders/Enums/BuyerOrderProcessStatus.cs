namespace HiveSpace.OrderService.Application.Orders.Enums;

public enum BuyerOrderProcessStatus
{
    All = 0,
    WaitingPayment = 1,
    Processing = 2,
    Shipping = 3,
    Delivered = 4,
    Cancelled = 5,
    ReturnRefund = 6
}
