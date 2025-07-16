using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;
public class ApplicationException : Exception
{
    private readonly int _httpCode = 500;

    private readonly bool _enableData;

    private readonly IEnumerable<Error> _errorList = [];

    public int HttpCode => _httpCode;

    public IEnumerable<Error> ErrorCodeList => _errorList;

    public bool EnableData => _enableData;

    public ApplicationException()
    {
    }

    public ApplicationException(Exception inner) : base(inner.Message, inner)
    {
    }

    public ApplicationException(IEnumerable<Error> errorCodeList, int? httpCode, bool? enableData) 
    {
        _errorList = errorCodeList;
        _httpCode = httpCode ?? _httpCode;
        _enableData = enableData ?? _enableData;
    }

    public ApplicationException(IEnumerable<Error> errorCodeList, Exception inner, int? httpCode, bool? enableData)
        : base(inner.Message, inner)
    {
        _errorList = errorCodeList;
        _httpCode = httpCode ?? _httpCode;
        _enableData = enableData ?? _enableData;
    }
}