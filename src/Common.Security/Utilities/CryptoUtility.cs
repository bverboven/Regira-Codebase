using System.Security.Cryptography;

namespace Regira.Security.Utilities;

public static class CryptoUtility
{
    public const int DefaultSaltSize = 16; // 128-bit salt
    public const int DefaultIterations = 10000;

    public static HashAlgorithm CreateHasher(string? algorithm = null)
    {
        switch (algorithm?.ToUpper())
        {
            case "SHA384":
                return SHA384.Create();
            case "MD5":
                return MD5.Create();
            default:
                //case "SHA512":
                return SHA512.Create();
        }
    }

    public static byte[] GenerateSalt(int size = DefaultSaltSize)
    {
        var salt = new byte[size];
#if NETSTANDARD2_0
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
#else
        RandomNumberGenerator.Fill(salt);
#endif
        return salt;
    }

    public static Rfc2898DeriveBytes GetRfc2898DeriveBytes(byte[] key, byte[]? salt = null, int iterations = DefaultIterations)
    {
        salt ??= GenerateSalt(DefaultSaltSize);
#if NETSTANDARD2_0
            return new Rfc2898DeriveBytes(key, salt, iterations);
#else
#pragma warning disable SYSLIB0060
        return new Rfc2898DeriveBytes(key, salt, iterations, HashAlgorithmName.SHA512);
#pragma warning restore SYSLIB0060
#endif
    }

    // convenience: derive bytes directly
    public static byte[] DeriveBytes(byte[] key, int count, byte[]? salt = null, int iterations = DefaultIterations)
    {
#if NETSTANDARD2_0
        using var pbkdf2 = GetRfc2898DeriveBytes(key, salt, iterations);
        return pbkdf2.GetBytes(count);
#else
        salt ??= GenerateSalt(DefaultSaltSize);
        return Rfc2898DeriveBytes.Pbkdf2(key, salt, iterations, HashAlgorithmName.SHA512, count);
#endif
    }

    public static bool FixedTimeEquals(byte[] a, byte[] b)
    {
#if NETSTANDARD2_0
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
#else
        return CryptographicOperations.FixedTimeEquals(a, b);
#endif
    }
}