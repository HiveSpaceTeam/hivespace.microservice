using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;
public class ApplicationException : Exception
{
    private readonly int _httpCode = 500;

    private readonly bool _enableData;

    private readonly IEnumerable<ErrorCode> _errorCodeList = [];

    public int HttpCode => _httpCode;

    public IEnumerable<ErrorCode> ErrorCodeList => _errorCodeList;

    public bool EnableData => _enableData;

    public ApplicationException()
    {
    }

    public ApplicationException(Exception inner) : base(inner.Message, inner)
    {
    }

    public ApplicationException(IEnumerable<ErrorCode> errorCodeList, int? httpCode, bool? enableData) 
    {
        _errorCodeList = errorCodeList;
        _httpCode = httpCode ?? _httpCode;
        _enableData = enableData ?? _enableData;
    }

    public ApplicationException(IEnumerable<ErrorCode> errorCodeList, Exception inner, int? httpCode, bool? enableData)
        : base(inner.Message, inner)
    {
        _errorCodeList = errorCodeList;
        _httpCode = httpCode ?? _httpCode;
        _enableData = enableData ?? _enableData;
    }
}