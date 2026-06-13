namespace HiveSpace.Testing.Shared.Fakes;

public sealed record VNPayResult(
    bool Success,
    string TransactionRef,
    string ResponseCode = "00",
    string TransactionNo = "TEST-TXN");

public sealed class PaymentProviderFake
{
    private readonly Dictionary<string, VNPayResult> _results = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _lookups = [];

    public IReadOnlyList<string> Lookups => _lookups;

    public void SetupReturn(string transactionRef, VNPayResult result)
    {
        _results[transactionRef] = result;
    }

    public VNPayResult GetStub(string transactionRef)
    {
        _lookups.Add(transactionRef);

        if (_results.TryGetValue(transactionRef, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"No VNPay stub configured for transaction reference '{transactionRef}'.");
    }
}
