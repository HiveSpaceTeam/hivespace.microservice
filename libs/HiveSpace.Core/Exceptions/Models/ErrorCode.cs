using HiveSpace.Domain.Shared;

namespace HiveSpace.Core.Exceptions.Models;
public class ErrorCode(string code, string messageCode, string? source)
{
    public string Code { get; set; } = code;
    public string MessageCode { get; set; } = messageCode;
    public string? Source { get; set; } = source;

}
