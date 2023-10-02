using System.Security.Claims;

namespace Regira.Security.Authentication.Jwt.Extensions;

public static class PrincipalExtensions
{
    public static string? FindUserId(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
    public static string? FindUserName(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    }
    public static string? FindEmail(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    }
}