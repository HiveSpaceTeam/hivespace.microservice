using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class OrderErrorCode(int id, string name, string code) : DomainErrorCode(id, name, code)
{
    public static readonly OrderErrorCode OrderNotFound = new(1, "OrderNotFound", "ORD0001");
    public static readonly OrderErrorCode OrderItemNotFound = new(2, "OrderItemNotFound", "ORD0002");
    public static readonly OrderErrorCode InvalidOrder = new(3, "InvalidOrder", "ORD0003");
    public static readonly OrderErrorCode OutOfStock = new(4, "OutOfStock", "ORD0004");
    public static readonly OrderErrorCode InvalidOrderStatus = new(5, "InvalidOrderStatus", "ORD0005");
    public static readonly OrderErrorCode OrderCannotBeCanceled = new(6, "OrderCannotBeCanceled", "ORD0006");
    public static readonly OrderErrorCode InvalidCustomer = new(7, "InvalidCustomer", "ORD0007");
    public static readonly OrderErrorCode InvalidShippingAddress = new(8, "InvalidShippingAddress", "ORD0008");
    public static readonly OrderErrorCode InvalidOrderItem = new(9, "InvalidOrderItem", "ORD0009");
    public static readonly OrderErrorCode InvalidPaymentMethod = new(10, "InvalidPaymentMethod", "ORD0010");
    public static readonly OrderErrorCode InsufficientInventory = new(11, "InsufficientInventory", "ORD0011");
    public static readonly OrderErrorCode PaymentFailed = new(12, "PaymentFailed", "ORD0012");
    public static readonly OrderErrorCode CustomerNotFound = new(13, "CustomerNotFound", "ORD0013");
    public static readonly OrderErrorCode ProductNotFound = new(14, "ProductNotFound", "ORD0014");
    public static readonly OrderErrorCode InvalidMoney = new(15, "InvalidMoney", "ORD0015");
    public static readonly OrderErrorCode InvalidPhoneNumber = new(16, "InvalidPhoneNumber", "ORD0016");
}