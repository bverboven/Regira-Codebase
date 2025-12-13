using Microsoft.Extensions.DependencyInjection;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Regira.Web.Swagger.Security;

public static class JwtAuthenticationExtensions
{
    public static void AddJwtAuthentication(this SwaggerGenOptions o, string authenticationScheme = "Bearer")
    {
#if NET10_0_OR_GREATER
        // ToDo: verify this implementation
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Scheme = authenticationScheme,
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Description = "Put your JWT Bearer token on textbox below",
        };

        o.AddSecurityDefinition(authenticationScheme, jwtSecurityScheme);
        o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {new OpenApiSecuritySchemeReference(authenticationScheme, doc), []}
        });
#else
        // https://stackoverflow.com/questions/43447688/setting-up-swagger-asp-net-core-using-the-authorization-headers-bearer/#answer-64899768
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Scheme = authenticationScheme,
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Description = "Put your JWT Bearer token on textbox below",

            Reference = new OpenApiReference
            {
                Id = authenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        o.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {jwtSecurityScheme, Array.Empty<string>()}
        });
#endif
    }
}