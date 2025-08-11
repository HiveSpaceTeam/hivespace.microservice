using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class CustomerNotFoundException : DomainException
{
    public CustomerNotFoundException()
        : base(404, OrderErrorCode.CustomerNotFound, nameof(CustomerNotFoundException))
    {
    }

    public CustomerNotFoundException(Guid customerId)
        : base(404, OrderErrorCode.CustomerNotFound, nameof(CustomerNotFoundException))
    {
    }
}