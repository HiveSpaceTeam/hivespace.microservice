namespace HiveSpace.MediaService.Func.Core.Contracts;

public class QueueMessagePayload
{
    public Guid MediaAssetId { get; set; }
    public string Action { get; set; } = string.Empty;
}
