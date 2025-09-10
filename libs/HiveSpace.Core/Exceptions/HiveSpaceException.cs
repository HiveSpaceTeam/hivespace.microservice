using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.Core.Exceptions;

/// <summary>
/// Base exception class for all HiveSpace application exceptions.
/// Provides common functionality including error collections, HTTP status codes, and data enablement.
/// Designed for dual-language systems - no hardcoded messages, only error codes.
/// </summary>
public abstract class HiveSpaceException : Exception
{
    private readonly int _httpCode;
    private readonly bool _enableData;
    private readonly IEnumerable<Error> _errorList;

    public int HttpCode => _httpCode;
    public IEnumerable<Error> ErrorCodeList => _errorList;
    public bool EnableData => _enableData;

    protected HiveSpaceException(IEnumerable<Error> errorList, int httpCode = 500, bool? enableData = false)
    {
        _errorList = errorList ?? [];
        _httpCode = httpCode;
        _enableData = enableData ?? false;
    }

    protected HiveSpaceException(IEnumerable<Error> errorList, Exception innerException, int httpCode = 500, bool? enableData = false)
        : base(innerException?.Message, innerException)
    {
        _errorList = errorList ?? [];
        _httpCode = httpCode;
        _enableData = enableData ?? false;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        if (_errorList.Any())
        {
            var errors = string.Join(", ", _errorList.Select(e => $"{e.ErrorCode.Code}"));
            return $"{baseString}\nError Codes: [{errors}]";
        }
        return baseString;
    }
}
