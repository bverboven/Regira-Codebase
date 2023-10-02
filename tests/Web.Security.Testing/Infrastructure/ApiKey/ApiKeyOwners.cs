using Regira.Security.Authentication.ApiKey.Models;

namespace Web.Security.Testing.Infrastructure.ApiKey;

public static class ApiKeyOwners
{
    public static IList<ApiKeyOwner> Value => new[]
    {
        new ApiKeyOwner { Key = "TEST", OwnerId = "TestOwnerId" }
    };
}