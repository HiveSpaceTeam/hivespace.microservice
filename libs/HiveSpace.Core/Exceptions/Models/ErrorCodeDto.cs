namespace HiveSpace.Core.Exceptions.Models;
public class ErrorCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string MessageCode { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; } = [];
    public string? Source { get; set; }
}
