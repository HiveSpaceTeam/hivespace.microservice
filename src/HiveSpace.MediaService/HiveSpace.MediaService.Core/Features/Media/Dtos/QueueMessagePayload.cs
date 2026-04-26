namespace HiveSpace.MediaService.Core.Features.Media.Dtos;

public class QueueMessagePayload
{
    public Guid MediaAssetId { get; set; }
    public string Action { get; set; } = string.Empty;
}
