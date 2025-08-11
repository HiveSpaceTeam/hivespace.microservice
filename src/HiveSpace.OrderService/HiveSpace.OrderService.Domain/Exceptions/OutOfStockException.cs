using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class OutOfStockException : DomainException
{
    public OutOfStockException()
        : base(400, OrderErrorCode.OutOfStock, nameof(OutOfStockException))
    {
    }

    public OutOfStockException(int skuId, int requestedQuantity, int availableQuantity)
        : base(400, OrderErrorCode.OutOfStock, nameof(OutOfStockException))
    {
    }
}