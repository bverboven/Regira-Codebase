using System.Text;
using Regira.Security.Abstractions;
using Regira.Security.Core;
using Regira.Security.Utilities;

namespace Regira.Security.Hashing;

public class Hasher(CryptoOptions? options = null) : IHasher
{
    private readonly string _salt = options?.Secret ?? DefaultSecuritySettings.SaltKey;
    private readonly Encoding _encoding = options?.Encoding ?? DefaultSecuritySettings.Encoding;
    private readonly string _algorithm = options?.AlgorithmType ?? DefaultSecuritySettings.HashAlgorithm;


    public string Hash(string? plainText)
    {
        plainText ??= string.Empty;
        var plainTextBytes = _encoding.GetBytes(plainText);
        var saltBytes = CryptoUtility.GetRfc2898DeriveBytes(_encoding.GetBytes(_salt)).Salt;
        var plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];
        for (var i = 0; i < plainTextBytes.Length; i++)
        {
            plainTextWithSaltBytes[i] = plainTextBytes[i];
        }
        for (var i = 0; i < saltBytes.Length; i++)
        {
            plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
        }

        var hash = CryptoUtility.CreateHasher(_algorithm);
        var hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
        var hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];
        for (var i = 0; i < hashBytes.Length; i++)
        {
            hashWithSaltBytes[i] = hashBytes[i];
        }
        for (var i = 0; i < saltBytes.Length; i++)
        {
            hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
        }

        return Convert.ToBase64String(hashWithSaltBytes);
    }
    public bool Verify(string? plainText, string hashedValue)
    {
        plainText ??= string.Empty;
        var expectedHashString = Hash(plainText);
        return hashedValue == expectedHashString;
    }
}