using MediatR;

namespace HiveSpace.Application.Shared.Queries;

/// <summary>
/// Interface for queries that return a value
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
{
}