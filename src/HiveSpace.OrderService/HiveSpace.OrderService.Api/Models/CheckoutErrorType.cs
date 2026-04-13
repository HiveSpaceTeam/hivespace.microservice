namespace HiveSpace.OrderService.Api.Models;

public enum CheckoutErrorType
{
    ValidationFailed,
    InventoryUnavailable,
    CODLimitExceeded,
    PaymentFailed,
    Timeout,
    InternalError
}
