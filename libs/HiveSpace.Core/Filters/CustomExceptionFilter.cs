using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json;
using HiveSpace.Core.Exceptions;

namespace HiveSpace.Core.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.ExceptionHandled = true;
            var errorResponse = new ExceptionModel
            {
                Errors = [],
                Status = "500",
                Timestamp = DateTimeOffset.Now,
                TraceId = Guid.NewGuid().ToString(),
                Version = "1.0"
            };

            if (context.Exception is HiveSpaceException exception) 
            {
                var errorList = new List<ErrorCodeDto>();
                foreach (Error error in exception.ErrorCodeList)
                {
                    var errorDto = new ErrorCodeDto(
                        error.ErrorCode.Code,
                        error.ErrorCode.Name,
                        ToCamelCase(error.Source)
                    );
                    errorList.Add(errorDto);
                }
                errorResponse.Errors = [.. errorList];
                errorResponse.Status = exception.HttpCode.ToString();
            }
            else if (context.Exception is DomainException domainException)
            {
                var errorCode = domainException.ErrorCode;
                var errorDto = new ErrorCodeDto(
                    domainException.ErrorCode.Code,
                    domainException.ErrorCode.Name,
                    ToCamelCase(domainException.Source)
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
                error = error with { MessageCode = context.Exception.Message };
#endif
                errorResponse.Errors = [error];
            }

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = int.Parse(errorResponse.Status),
            };
        }

        private static string? ToCamelCase(string? source)
        {
            if (string.IsNullOrEmpty(source))
                return source;

            // Use JsonNamingPolicy.CamelCase for consistent camelCase conversion
            return JsonNamingPolicy.CamelCase.ConvertName(source);
        }
    }
}