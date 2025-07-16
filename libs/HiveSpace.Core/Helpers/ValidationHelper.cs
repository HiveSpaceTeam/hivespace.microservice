using FluentValidation.Results;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Helpers;

public static class ValidationHelper
{
    public static void ValidateResult(IEnumerable<ValidationResult> validationResults)
    {
        var errors = validationResults.SelectMany(x => ValidateResultWithState(x));
        if (errors.Any())
        {
            throw new BadRequestException([.. errors]);
        }
    }

    public static void ValidateResult(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new BadRequestException([.. ValidateResultWithState(validationResult)]);
        }
    }

    public static IEnumerable<Error> ValidateResultWithState(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            return [.. validationResult.Errors.Select(x => x.CustomState as Error ?? 
                new Error(ApplicationErrorCode.FluentValidationError, null))];
        }
        return [];
    }
}
