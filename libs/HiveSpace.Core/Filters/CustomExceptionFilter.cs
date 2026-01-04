using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Core.Helpers;
using HiveSpace.Core.Exceptions;

namespace HiveSpace.Core.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.ExceptionHandled = true;
            
            var errorResponse = ExceptionResponseFactory.CreateResponse(context.Exception);

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = int.Parse(errorResponse.Status),
            };
        }
    }
}