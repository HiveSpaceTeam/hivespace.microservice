using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.Core.Helpers;

public static class ExceptionHelper
{
    public static List<Error> CreateErrorList(IErrorCode errorCode, string? source = null)
    {
        return
        [
            new Error(errorCode, source)
        ];
    }

    public static BadRequestException BadRequestException(IErrorCode errorCode, string? source = null, Exception? innerException = null)
    {
        return innerException is null
            ? new BadRequestException(CreateErrorList(errorCode, source))
            : new BadRequestException(CreateErrorList(errorCode, source), innerException);
    }

    public static ForbiddenException ForbiddenException(IErrorCode errorCode, string? source = null, Exception? innerException = null)
    {
        return innerException is null
            ? new ForbiddenException(CreateErrorList(errorCode, source))
            : new ForbiddenException(CreateErrorList(errorCode, source), innerException);
    }

    public static Exceptions.ApplicationException BaseException(IErrorCode errorCode, string? source = null, Exception? innerException = null, int? httpCode = 500)
    {
        return innerException is null
            ? new Exceptions.ApplicationException(CreateErrorList(errorCode, source), httpCode, false)
            : new Exceptions.ApplicationException(CreateErrorList(errorCode, source), innerException, httpCode, false);
    }
}
