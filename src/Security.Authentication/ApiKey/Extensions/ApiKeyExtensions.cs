using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Regira.Security.Authentication.ApiKey.Abstraction;
using Regira.Security.Authentication.ApiKey.Models;
using Regira.Security.Authentication.ApiKey.Services;

namespace Regira.Security.Authentication.ApiKey.Extensions;

public static class ApiKeyExtensions
{
    public static AuthenticationBuilder AddApiKeyAuthentication(this IServiceCollection services, Action<ApiKeyAuthenticationOptions>? configure = null)
    {
        return services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyConstants.AuthenticationScheme;
                options.DefaultChallengeScheme = ApiKeyConstants.AuthenticationScheme;
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyConstants.AuthenticationScheme, configure ?? (c => { }));
    }

    public static AuthenticationBuilder AddInMemoryApiKeyAuthentication(this AuthenticationBuilder builder, IEnumerable<ApiKeyOwner> apiKeyOwners)
    {
        builder.Services
            .AddSingleton<IApiKeyOwnerService>(_ => new InMemoryApiKeyOwnerService(apiKeyOwners));

        return builder;
    }
}