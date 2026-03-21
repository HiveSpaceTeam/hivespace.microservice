using System.Security.Cryptography;
using HiveSpace.Domain.Shared.IdGeneration;

namespace HiveSpace.Core.IdGeneration;

/// <summary>
/// Generates UUID version 7 (time-ordered) identifiers.
/// Compatible with .NET 8 (backport of <c>Guid.CreateVersion7()</c> from .NET 9).
///
/// RFC 9562 layout (128 bits):
///  Bits  0–47  : Unix timestamp in milliseconds (big-endian)
///  Bits 48–51  : Version = 0111 (7)
///  Bits 52–63  : rand_a  (12 random bits)
///  Bits 64–65  : Variant = 10
///  Bits 66–127 : rand_b  (62 random bits)
/// </summary>
public sealed class GuidV7Generator : IIdGenerator<Guid>
{
    public Guid NewId()
    {
        // 16 random bytes as the base
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        // Embed 48-bit Unix timestamp (ms) in the first 6 bytes (big-endian)
        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        bytes[0] = (byte)(ms >> 40);
        bytes[1] = (byte)(ms >> 32);
        bytes[2] = (byte)(ms >> 24);
        bytes[3] = (byte)(ms >> 16);
        bytes[4] = (byte)(ms >> 8);
        bytes[5] = (byte) ms;

        // Set version nibble (bits 48–51) = 0111
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

        // Set variant bits (bits 64–65) = 10xx xxxx
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        // Guid(byte[]) constructor on .NET interprets bytes in a mixed-endian
        // order for the first three components. We bypass that by using the
        // explicit int/short/short overload to preserve the big-endian layout.
        int   a = (bytes[0]  << 24) | (bytes[1]  << 16) | (bytes[2]  << 8) | bytes[3];
        short b = (short)((bytes[4] << 8) | bytes[5]);
        short c = (short)((bytes[6] << 8) | bytes[7]);

        return new Guid(a, b, c,
            bytes[8],  bytes[9],  bytes[10], bytes[11],
            bytes[12], bytes[13], bytes[14], bytes[15]);
    }
}
