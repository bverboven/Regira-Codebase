using Microsoft.AspNetCore.Authentication;

namespace Regira.Security.Authentication.ApiKey.Models;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string Scheme => ApiKeyConstants.AuthenticationScheme;
    public string AuthenticationType = ApiKeyConstants.AuthenticationScheme;
}