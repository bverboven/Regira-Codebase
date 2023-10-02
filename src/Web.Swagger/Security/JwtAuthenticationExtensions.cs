using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Regira.Web.Swagger.Security;

public static class JwtAuthenticationExtensions
{
    public static void AddJwtAuthentication(this SwaggerGenOptions o, string authenticationScheme = "Bearer")
    {
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
    }
}