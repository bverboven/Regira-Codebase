using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Jwt.Models;
using Regira.Security.Authentication.Jwt.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Regira.Security.Authentication.Jwt.Extensions;

public static class JwtAuthenticationServiceCollectionExtensions
{
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, Action<JwtTokenOptions> configureOptions)
    {
        var options = new JwtTokenOptions();
        configureOptions(options);

        if (string.IsNullOrEmpty(options.Secret))
        {
            throw new NullReferenceException($"Missing {nameof(options.Secret)} in {typeof(JwtTokenOptions).FullName}");
        }

        if (options.UseJwtClaimTypes)
        {
            UseJwtClaimTypes();
        }

        return services
            .AddTransient<ITokenHelper>(_ => new JwtTokenHelper(options))
            .AddAuthentication(options.AuthenticationScheme)
            .AddJwtBearer(options.AuthenticationScheme, x =>
            {
                var secretKeyBytes = Encoding.ASCII.GetBytes(options.Secret);
                var symmetricKey = new SymmetricSecurityKey(secretKeyBytes);

                var audiences = options.Audiences ??
                                (!string.IsNullOrWhiteSpace(options.Audience) ? new[] { options.Audience } : null);


                x.RequireHttpsMetadata = true;
                x.SaveToken = false;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = options.NameClaimType,
                    RoleClaimType = options.RoleClaimType,
                    ValidIssuer = options.Authority,
                    ValidAudiences = audiences,
                    IssuerSigningKey = symmetricKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = options.Authority != null,
                    ValidateAudience = audiences?.Any() ?? false,
                    ValidateLifetime = options.LifeSpan > 0,
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    public static void UseJwtClaimTypes()
    {
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Add(ClaimTypes.NameIdentifier, JwtClaimTypes.Subject);
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Add(ClaimTypes.Name, JwtClaimTypes.Name);
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Add(ClaimTypes.Email, JwtClaimTypes.Email);

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add(JwtClaimTypes.Subject, ClaimTypes.NameIdentifier);
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add(JwtClaimTypes.Name, ClaimTypes.Name);
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add(JwtClaimTypes.Email, ClaimTypes.Email);
    }
}