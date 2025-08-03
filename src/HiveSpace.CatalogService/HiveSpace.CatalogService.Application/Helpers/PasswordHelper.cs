using System.Security.Cryptography;

namespace HiveSpace.CatalogService.Application.Helpers;

public class PasswordHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = GenerateHashPassword(password, salt);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public static bool Verify(string password, string passwordHashed)
    {
        string[] parts = passwordHashed.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);
        byte[] inputHash = GenerateHashPassword(password, salt);

        return hash.SequenceEqual(inputHash);
    }

    private static byte[] GenerateHashPassword(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
    }
}
