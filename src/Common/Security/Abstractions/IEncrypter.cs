namespace Regira.Security.Abstractions;

public interface IEncrypter
{
    string Encrypt(string plainText, string? key = null);
    string Decrypt(string encryptedText, string? key = null);
}