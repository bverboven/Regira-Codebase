namespace Regira.Security.Abstractions;

public interface IHasher
{
    string Hash(string plainText);
    bool Verify(string plainText, string hashedValue);
}