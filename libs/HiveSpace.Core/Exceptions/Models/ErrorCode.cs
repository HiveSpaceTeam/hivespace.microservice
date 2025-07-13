using HiveSpace.Domain.Shared;

namespace HiveSpace.Core.Exceptions.Models;
public class ErrorCode
{
    public Enumeration? Code { get; set; }
    public List<ErrorData>? Data { get; set; }
    public string? Source { get; set; }
}
