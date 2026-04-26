namespace HiveSpace.NotificationService.Core.Dispatch.Models;

public class DeliveryResult
{
    public bool    Success          { get; private init; }
    public string? ErrorMessage     { get; private init; }
    public string? ProviderResponse { get; private init; }

    public static DeliveryResult Ok(string? providerResponse = null)
        => new() { Success = true, ProviderResponse = providerResponse };

    public static DeliveryResult Fail(string errorMessage, string? providerResponse = null)
        => new() { Success = false, ErrorMessage = errorMessage, ProviderResponse = providerResponse };
}
