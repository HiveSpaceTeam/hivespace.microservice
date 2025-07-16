using HiveSpace.Domain.Shared;

namespace HiveSpace.Core.Exceptions.Models;
public class Error(IErrorCode errorCode, string? source)
{
    public IErrorCode ErrorCode { get; set; } = errorCode;
    public string? Source { get; set; } = source;

}
