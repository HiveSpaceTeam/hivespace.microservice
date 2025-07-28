using MediatR;

namespace HiveSpace.Application.Shared.Commands;

/// <summary>
/// Marker interface for commands that don't return a value
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Interface for commands that return a value
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
