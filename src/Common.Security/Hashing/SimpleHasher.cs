using System.Text;
using Regira.Security.Abstractions;
using Regira.Security.Core;
using Regira.Security.Utilities;

namespace Regira.Security.Hashing;

public class SimpleHasher(CryptoOptions? options = null) : IHasher
{
    private readonly string _salt = options?.Secret ?? DefaultSecuritySettings.SaltKey;
    private readonly Encoding _encoding = options?.Encoding ?? DefaultSecuritySettings.Encoding;
    private readonly string _algorithm = options?.AlgorithmType ?? DefaultSecuritySettings.HashAlgorithm;


    public string Hash(string? plainText)
    {
        plainText ??= string.Empty;
        var bytes = _encoding.GetBytes(plainText);
        using var algorithm = CryptoUtility.CreateHasher(_algorithm);
        var hashBytes = algorithm.ComputeHash(bytes);
        var saltedHashBytes = hashBytes.Concat(_encoding.GetBytes(_salt)).ToArray();
        var doubleHashedBytes = algorithm.ComputeHash(saltedHashBytes);
        return Convert.ToBase64String(doubleHashedBytes);
    }
    public bool Verify(string? plainValue, string hashedValue)
    {
        plainValue ??= string.Empty;
        return Hash(plainValue).Equals(hashedValue);
    }
}