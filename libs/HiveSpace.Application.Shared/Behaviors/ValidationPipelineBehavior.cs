using FluentValidation;
using HiveSpace.Core.Helpers;
using MediatR;

namespace HiveSpace.Application.Shared.Behaviors;

public class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var results = validators
            .Select(v => v.Validate(request))
            .ToList();

        ValidationHelper.ValidateResult(results);

        return await next();
    }
}
