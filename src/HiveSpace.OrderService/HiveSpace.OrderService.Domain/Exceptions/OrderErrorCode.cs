using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class OrderErrorCode(int id, string name, string code) : DomainErrorCode(id, name, code)
{
    public static readonly OrderErrorCode InvalidOrder = new(1, "InvalidOrder", "ORD0001");
    public static readonly OrderErrorCode OrderItemNotFound = new(2, "OrderItemNotFound", "ORD0002");
    public static readonly OrderErrorCode InvalidMoney = new(3, "InvalidMoney", "ORD0003");
    public static readonly OrderErrorCode InvalidPhoneNumber = new(4, "InvalidPhoneNumber", "ORD0004");
}
