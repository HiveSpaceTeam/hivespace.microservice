using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Core.Helpers;

public static class ExceptionResponseFactory
{
    public static ExceptionModel CreateResponse(Exception exception)
    {
        var errorResponse = new ExceptionModel
        {
            Errors = [],
            Status = "500",
            Timestamp = DateTimeOffset.Now,
            TraceId = Guid.NewGuid().ToString(),
            Version = "1.0"
        };

        if (exception is HiveSpaceException hiveException) 
        {
            var errorList = new List<ErrorCodeDto>();
            foreach (Error error in hiveException.ErrorCodeList)
            {
                var errorDto = new ErrorCodeDto(
                    error.ErrorCode.Code,
                    error.ErrorCode.Name,
                    StringHelper.ToCamelCase(error.Source)
                );
                errorList.Add(errorDto);
            }
            errorResponse.Errors = [.. errorList];
            errorResponse.Status = hiveException.HttpCode.ToString();
        }
        else if (exception is DomainException domainException)
        {
            var errorCode = domainException.ErrorCode;
            var errorDto = new ErrorCodeDto(
                domainException.ErrorCode.Code,
                domainException.ErrorCode.Name,
                StringHelper.ToCamelCase(domainException.Source)
            );
            errorResponse.Errors = [errorDto];
            errorResponse.Status = domainException.HttpCode.ToString();
        }
        else
        {
            var error = new ErrorCodeDto(
                "000000",
                "ServerError",
                null
            );
#if DEBUG
            error = error with { MessageCode = exception.Message };
#endif
            errorResponse.Errors = [error];
        }

        return errorResponse;
    }
}
