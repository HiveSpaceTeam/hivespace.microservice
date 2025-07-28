namespace HiveSpace.Core.Exceptions.Models;
public class ExceptionModel
{
    public List<ErrorCodeDto> Errors { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

