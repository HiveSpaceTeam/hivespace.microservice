using System.Security.Cryptography;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Orders;

public class OrderCodeGenerator(IOrderRepository orderRepository) : IOrderCodeGenerator
{
    private const string Prefix = "ORD-";
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var orderCode = Prefix + NewUlidLikeToken();
            if (!await orderRepository.OrderCodeExistsAsync(orderCode, cancellationToken))
                return orderCode;
        }

        return Prefix + NewUlidLikeToken();
    }

    private static string NewUlidLikeToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        for (var i = 5; i >= 0; i--)
        {
            bytes[i] = (byte)(timestamp & 0xFF);
            timestamp >>= 8;
        }

        Span<char> chars = stackalloc char[26];
        var bitBuffer = 0;
        var bitCount = 0;
        var index = 0;

        foreach (var b in bytes)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitCount += 8;
            while (bitCount >= 5 && index < chars.Length)
            {
                bitCount -= 5;
                chars[index++] = Alphabet[(bitBuffer >> bitCount) & 31];
            }
        }

        if (index < chars.Length)
            chars[index] = Alphabet[(bitBuffer << (5 - bitCount)) & 31];

        return new string(chars);
    }
}
