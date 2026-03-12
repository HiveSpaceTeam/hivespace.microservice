namespace HiveSpace.Domain.Shared.IdGeneration;

/// <summary>
/// Generates unique identifiers of type <typeparamref name="T"/>.
/// </summary>
public interface IIdGenerator<T> where T : notnull
{
    T NewId();
}
