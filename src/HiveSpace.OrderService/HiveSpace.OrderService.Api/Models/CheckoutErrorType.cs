namespace HiveSpace.OrderService.Api.Models;

public enum CheckoutErrorType
{
    ValidationFailed,
    InventoryUnavailable,
    CODLimitExceeded,
    Timeout,
    InternalError
}
