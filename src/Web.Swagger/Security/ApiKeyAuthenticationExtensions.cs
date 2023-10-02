﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Regira.Web.Swagger.Security;

public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Adds the input form element for the ApiKey 
    /// </summary>
    public static void AddApiKeyAuthentication(this SwaggerGenOptions o, string authenticationScheme = "ApiKey", string parameterName = "X-Api-Key")
    {
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
    }
}