#if NET9_0_OR_GREATER
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Regira.Security.Authentication.ApiKey.Models;

namespace Regira.Security.Authentication.Web.OpenApi.Transformers;

public class ApiKeySecurityDocumentTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == ApiKeyDefaults.AuthenticationScheme))
        {
            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = ApiKeyDefaults.HeaderName,
                Description = $"API key required in the '{ApiKeyDefaults.HeaderName}' header"
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes.TryAdd(ApiKeyDefaults.AuthenticationScheme, scheme);
        }
    }
}
#endif