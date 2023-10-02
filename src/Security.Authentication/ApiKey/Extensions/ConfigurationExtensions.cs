using Microsoft.Extensions.Configuration;
using Regira.Security.Authentication.ApiKey.Models;

namespace Regira.Security.Authentication.ApiKey.Extensions;

public static class ConfigurationExtensions
{
    public static IList<ApiKeyOwner> ToApiKeyOwners(this IConfigurationSection apiKeysSection)
    {
        return apiKeysSection
            .GetChildren()
            .Select(ToApiKeyOwner)
            .ToList();
    }
    public static ApiKeyOwner ToApiKeyOwner(this IConfigurationSection apiKeySection)
    {
        return new ApiKeyOwner
        {
            Key = apiKeySection["Key"],
            OwnerId = apiKeySection["OwnerId"]
        };
    }
}