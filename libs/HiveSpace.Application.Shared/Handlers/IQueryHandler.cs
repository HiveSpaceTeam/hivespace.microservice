using HiveSpace.Application.Shared.Queries;
using MediatR;

namespace HiveSpace.Application.Shared.Handlers;

/// <summary>
/// Interface for query handlers
/// </summary>
/// <typeparam name="TQuery">The type of query</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}