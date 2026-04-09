namespace HiveSpace.OrderService.Application.Orders.Enums;

public enum SellerOrderProcessStatus
{
    All = 0,
    PendingConfirmation = 1,
    ReadyToShip = 2,
    Shipping = 3,
    Delivered = 4,
    ReturnCancel = 5
}
