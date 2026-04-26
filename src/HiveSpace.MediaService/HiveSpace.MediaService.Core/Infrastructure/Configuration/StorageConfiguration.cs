using Microsoft.Extensions.Configuration;

namespace HiveSpace.MediaService.Core.Infrastructure.Configuration;

public class StorageConfiguration(IConfiguration configuration)
{
    public string TempContainer => configuration.GetValue<string>("AzureStorage:TempContainer", "temp-media-upload")!;
    public string PublicContainer => configuration.GetValue<string>("AzureStorage:PublicContainer", "public-assets")!;
    public string QueueName => configuration.GetValue<string>("AzureStorage:QueueName", "image-processing-queue")!;
    public string CdnHost => configuration.GetValue<string>("AzureStorage:CdnHost", "")!;
    public int PresignUrlExpiryMinutes => configuration.GetValue<int>("MediaService:PresignUrlExpiryMinutes", 10);
}
