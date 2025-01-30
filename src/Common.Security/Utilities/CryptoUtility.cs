using System.Security.Cryptography;

namespace Regira.Security.Utilities;

public static class CryptoUtility
{
    static readonly byte[] DefaultSalt = [82, 101, 103, 105, 114, 97, 39, 115, 32, 115, 101, 99, 114, 101, 116, 32, 83, 65, 76, 84
    ];
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
    public static Rfc2898DeriveBytes GetRfc2898DeriveBytes(byte[] key, int iterations = 1000)
    {
#if NETSTANDARD2_0
            return new Rfc2898DeriveBytes(key, DefaultSalt, iterations);
#else
        return new Rfc2898DeriveBytes(key, DefaultSalt, iterations, HashAlgorithmName.SHA512);
#endif
    }
}