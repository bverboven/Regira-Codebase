using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Regira.Security.Authentication.Jwt.Models;

public class JwtTokenOptions
{
    public string Secret { get; set; } = null!;
    public string? Algorithm { get; set; }
    public string AuthenticationScheme { get; set; } = JwtBearerDefaults.AuthenticationScheme;

    public string? Authority { get; set; }
    public string? Audience { get; set; }
    public ICollection<string>? Audiences { get; set; }

    /// <summary>
    /// Token lifespan in seconds (default 2 hours)
    /// </summary>
    public int LifeSpan { get; set; } = 60 * 60 * 2;
    public bool IncludeIssuedDate { get; set; } = true;

    public string NameClaimType { get; set; } = JwtClaimTypes.Name;
    public string RoleClaimType { get; set; } = JwtClaimTypes.Role;

    public bool UseJwtClaimTypes { get; set; } = true;
}