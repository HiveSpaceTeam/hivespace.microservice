namespace HiveSpace.Domain.Shared.Interfaces;

/// <summary>
/// Marker interface for domain services.
/// Domain services contain business logic that doesn't naturally fit within a single aggregate.
/// They are stateless and coordinate operations across multiple aggregates or handle complex business rules.
/// </summary>
public interface IDomainService
{
}
