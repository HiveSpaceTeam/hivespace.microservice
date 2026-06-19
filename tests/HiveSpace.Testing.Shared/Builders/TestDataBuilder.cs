using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.Testing.Shared.Builders;

public static class TestDataBuilder
{
    private static long _sequence;

    public static Guid NewUlid()
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(Interlocked.Increment(ref _sequence)).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    public static Money MoneyStub(decimal amount, string currency = "VND")
    {
        return currency.ToUpperInvariant() switch
        {
            "USD" => Money.FromUSD(amount),
            "EUR" => Money.FromEUR(amount),
            _ => Money.FromVND((long)amount)
        };
    }

    public static TestAddress AddressStub(string city = "Hanoi")
    {
        return new TestAddress("HiveSpace Tester", "0900000000", "1 Test Street", city);
    }
}
