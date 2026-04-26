using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.Domain.Shared.Entities;

public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    where TKey : struct, IEquatable<TKey>
{
}
