namespace HiveSpace.OrderService.Application.Orders;

public interface IOrderCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
