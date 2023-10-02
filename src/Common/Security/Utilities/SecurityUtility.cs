using System.Security.Cryptography;

namespace Regira.Security.Utilities;

public static class SecurityUtility
{
    public static string GenerateRandomString(int length = 32)
    {
        var randomNumber = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}