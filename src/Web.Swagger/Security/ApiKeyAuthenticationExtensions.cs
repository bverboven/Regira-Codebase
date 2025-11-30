using Microsoft.Extensions.DependencyInjection;
#if NET10_0
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Regira.Web.Swagger.Security;

public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Adds the input form element for the ApiKey 
    /// </summary>
    public static void AddApiKeyAuthentication(this SwaggerGenOptions o, string authenticationScheme = "ApiKey", string parameterName = "X-Api-Key")
    {
#if NET10_0
        // ToDo: verify this implementation
        var apiKeySecurityScheme = new OpenApiSecurityScheme
        {
            Scheme = authenticationScheme,
            Name = parameterName,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Description = "ApiKey"
        };

        o.AddSecurityDefinition(authenticationScheme, apiKeySecurityScheme);
        o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference(authenticationScheme), [] }
        });
#else
        var apiKeySecurityScheme = new OpenApiSecurityScheme
        {
            Scheme = authenticationScheme,
            Name = parameterName,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Description = "ApiKey",

            Reference = new OpenApiReference
            {
                Id = authenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        o.AddSecurityDefinition(apiKeySecurityScheme.Reference.Id, apiKeySecurityScheme);
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {apiKeySecurityScheme, Array.Empty<string>()}
        });
#endif
    }
}