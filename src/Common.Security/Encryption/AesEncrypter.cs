using Regira.Security.Abstractions;
using Regira.Security.Core;
using Regira.Security.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace Regira.Security.Encryption;

public class AesEncrypter : IEncrypter
{
    private readonly string _defaultKey;
    private readonly Encoding _encoding;
    public AesEncrypter(CryptoOptions? options = null)
    {
        _defaultKey = options?.Secret ?? DefaultSecuritySettings.SaltKey;
        _encoding = options?.Encoding ?? DefaultSecuritySettings.Encoding;
    }


    public string Encrypt(string plainText, string? key = null)
    {
        key ??= _defaultKey;

        var plainTextBytes = _encoding.GetBytes(plainText);
        using var aes = Aes.Create();
        var rfc2898DeriveBytes = CryptoUtility.GetRfc2898DeriveBytes(_encoding.GetBytes(key));
        aes.Key = rfc2898DeriveBytes.GetBytes(32);
        aes.IV = rfc2898DeriveBytes.GetBytes(16);
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.Close();
        }
        var bytes = memoryStream.ToArray();
        return Convert.ToBase64String(bytes);
    }
    public string Decrypt(string encryptedText, string? key = null)
    {
        key ??= _defaultKey;

        var encryptedBytes = Convert.FromBase64String(encryptedText);
        using var aes = Aes.Create();
        var rfc2898DeriveBytes = CryptoUtility.GetRfc2898DeriveBytes(_encoding.GetBytes(key));
        aes.Key = rfc2898DeriveBytes.GetBytes(32);
        aes.IV = rfc2898DeriveBytes.GetBytes(16);
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            cryptoStream.Close();
        }
        var bytes = memoryStream.ToArray();
        return _encoding.GetString(bytes);
    }
}