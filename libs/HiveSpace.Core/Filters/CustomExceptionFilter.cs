using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;

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

            if (context.Exception is Exceptions.ApplicationException exception) 
            {
                var errorList = new List<ErrorCodeDto>();
                foreach (Error error in exception.ErrorCodeList)
                {
                    var errorDto = new ErrorCodeDto(
                        error.ErrorCode.Code,
                        error.ErrorCode.Name,
                        error.Source
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
                    domainException.Source
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
    }
}
