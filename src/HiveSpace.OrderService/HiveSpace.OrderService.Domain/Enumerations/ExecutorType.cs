using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.Enumerations;

public class ExecutorType(int id, string name) : Enumeration(id, name)
{
    public static readonly ExecutorType User = new(1, "USER");
    public static readonly ExecutorType System = new(2, "SYSTEM");
    public static readonly ExecutorType Webhook = new(3, "WEBHOOK");
}
