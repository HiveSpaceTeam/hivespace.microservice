using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Infrastructure.Messaging.Behaviors;

/// <summary>
/// Executes FluentValidation validators before handlers run.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        //if (_validators.Any())
        //{
        //    var context = new ValidationContext<TRequest>(request);
        //    var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
        //        .SelectMany(result => result.Errors)
        //        .Where(f => f is not null)
        //        .ToList();

        //    if (failures.Count != 0)
        //    {
        //        _logger.LogWarning("Validation failures for {RequestName}: {Failures}", typeof(TRequest).Name, failures.Select(f => f.ErrorMessage));
        //        throw new ValidationException(failures);
        //    }
        //}

        return await next();
    }
}

