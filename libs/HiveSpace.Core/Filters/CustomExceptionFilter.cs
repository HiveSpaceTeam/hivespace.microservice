using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared;

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
                Timestamp = DateTime.UtcNow,
                TraceId = Guid.NewGuid().ToString(),
                Version = "1.0"
            };

            if (context.Exception is Exceptions.ApplicationException exception) 
            {
                var errorList = new List<ErrorCodeDto>();
                foreach (Error error in exception.ErrorCodeList)
                {
                    var errorDto = new ErrorCodeDto
                    {
                        Code = error.ErrorCode.Code,
                        MessageCode = error.ErrorCode.Name,
                        Source = error.Source
                    };
                    errorList.Add(errorDto);
                }
                errorResponse.Errors = [.. errorList];
                errorResponse.Status = exception.HttpCode.ToString();
            }
            else if (context.Exception is DomainException domainException)
            {
                var errorCode = domainException.ErrorCode;
                var errorDto = new ErrorCodeDto
                {
                    Code = domainException.ErrorCode.Code,
                    MessageCode = domainException.ErrorCode.Name,
                    Source = domainException.Source,
                };
                errorResponse.Errors = [errorDto];
                errorResponse.Status = domainException.HttpCode.ToString();
            }
            else
            {
                var error = new ErrorCodeDto
                {
                    Code = "000000",
                    MessageCode = "ServerError",
                    Source = null,
                };
#if DEBUG
                error.MessageCode = context.Exception.Message;
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
