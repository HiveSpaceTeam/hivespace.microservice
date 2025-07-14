using HiveSpace.Domain.Shared;

namespace HiveSpace.Core.Exceptions.Models;
public class ErrorCode(ApplicationErrorCode code, string? source)
{
    public ApplicationErrorCode Code { get; set; } = code;
    public string? Source { get; set; } = source;

}
