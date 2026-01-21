using Microsoft.Extensions.Configuration;

namespace HiveSpace.MediaService.Func.Core.Configuration;

/// <summary>
/// Centralized storage configuration to avoid duplicate config reads
/// </summary>
public class StorageConfiguration(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public string TempContainer => _configuration.GetValue<string>("AzureStorage:TempContainer", "temp-media-upload")!;
    public string PublicContainer => _configuration.GetValue<string>("AzureStorage:PublicContainer", "public-assets")!;
    public string QueueName => _configuration.GetValue<string>("AzureStorage:QueueName", "image-processing-queue")!;
    public string CdnHost => _configuration.GetValue<string>("AzureStorage:CdnHost", "")!;
    public int PresignUrlExpiryMinutes => _configuration.GetValue<int>("MediaService:PresignUrlExpiryMinutes", 10);
}
