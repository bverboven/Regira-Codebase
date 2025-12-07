using System.Security.Claims;

namespace Regira.Security.Authentication.Jwt.Extensions;

public static class PrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public string? FindUserId()
        {
            return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public string? FindUserName()
        {
            return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }

        public string? FindEmail()
        {
            return principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }
    }
}