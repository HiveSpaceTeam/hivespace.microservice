//using HiveSpace.Core.Exceptions;
//using HiveSpace.Core.Exceptions.Models;

//namespace HiveSpace.Core.Helpers;

//public static class ExceptionHelper
//{
//    public static List<ErrorCode> CreateErrorCodeList(Enum errorCode, IEnumerable<ErrorData>? errorData = null, string? source = null)
//    {
//        return
//        [
//            new()
//            {
//                Code = errorCode,
//                Data = errorData is null ? [] : errorData.ToList(),
//                Source = source
//            }
//        ];
//    }

//    public static ErrorCode CreateErrorCode(Enum? errorCode, string? source)
//    {
//        return new ErrorCode
//        {
//            Code = errorCode,
//            Source = source
//        };
//    }

//    public static BadRequestException BadRequestException(Enum errorCode, IEnumerable<ErrorData>? errorData = null, Exception? innerException = null)
//    {
//        return innerException is null
//            ? new BadRequestException(CreateErrorCodeList(errorCode, errorData), errorData is not null)
//            : new BadRequestException(CreateErrorCodeList(errorCode, errorData), innerException, errorData is not null);
//    }

//    /// <summary>
//    /// Create a HTTP 400 Bad Request exception.
//    /// </summary>
//    public static BadRequestException BadRequestException(Enum errorCode, string errorKey, object errorValue, Exception? innerException = null)
//    {
//        return BadRequestException(errorCode, [new ErrorData(errorKey, StringHelper.ToStringOrEmpty(errorValue))], innerException);
//    }

//    /// <summary>
//    /// Create a HTTP 403 Forbidden exception.
//    /// </summary>
//    public static ForbiddenException ForbiddenException(Enum errorCode, IEnumerable<ErrorData>? errorData = null, Exception? innerException = null)
//    {
//        return innerException is null
//            ? new ForbiddenException(CreateErrorCodeList(errorCode, errorData), errorData is not null)
//            : new ForbiddenException(CreateErrorCodeList(errorCode, errorData), innerException, errorData is not null);
//    }

//    /// <summary>
//    /// Create a HTTP 403 Forbidden exception.
//    /// </summary>
//    public static ForbiddenException ForbiddenException(Enum errorCode, string errorKey, object errorValue, Exception? innerException = null)
//    {
//        return ForbiddenException(errorCode, [new ErrorData(errorKey, StringHelper.ToStringOrEmpty(errorValue))], innerException);
//    }

//    /// <summary>
//    /// Create a HTTP 404 Not Found exception.
//    /// </summary>
//    public static NotFoundException NotFoundException(Enum errorCode, IEnumerable<ErrorData>? errorData = null, Exception? innerException = null)
//    {
//        return innerException is null
//            ? new NotFoundException(CreateErrorCodeList(errorCode, errorData), errorData is not null)
//            : new NotFoundException(CreateErrorCodeList(errorCode, errorData), innerException, errorData is not null);
//    }

//    /// <summary>
//    /// Create a HTTP 404 Not Found exception.
//    /// </summary>
//    public static NotFoundException NotFoundException(Enum errorCode, string errorKey, object errorValue, Exception? innerException = null)
//    {
//        return NotFoundException(errorCode, [new ErrorData(errorKey, StringHelper.ToStringOrEmpty(errorValue))], innerException);
//    }

//    /// <summary>
//    /// Create a Base exception.
//    /// </summary>
//    public static Exceptions.ApplicationException BaseException(Enum errorCode, IEnumerable<ErrorData>? errorData = null, Exception? innerException = null, int? httpCode = 500)
//    {
//        return innerException is null
//            ? new Exceptions.ApplicationException(CreateErrorCodeList(errorCode, errorData), httpCode, errorData is not null)
//            : new Exceptions.ApplicationException(CreateErrorCodeList(errorCode, errorData), innerException, httpCode, errorData is not null);
//    }
//}
