using System.Security.Claims;

namespace Regira.Security.Authentication.Jwt.Abstraction;

public interface ITokenHelper
{
    string Create(IEnumerable<Claim> claims, string? audience = null, int? lifeSpan = null);
    bool Validate(string token);
}