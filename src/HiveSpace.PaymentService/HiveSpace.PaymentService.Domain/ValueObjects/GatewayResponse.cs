using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.PaymentService.Domain.ValueObjects;

public class GatewayResponse : ValueObject
{
    public string RawResponse { get; private set; } = null!;
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }

    private GatewayResponse() { }

    public GatewayResponse(string rawResponse, bool success, string? errorMessage = null)
    {
        RawResponse = rawResponse;
        Success = success;
        ErrorMessage = errorMessage;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RawResponse;
        yield return Success;
    }
}
