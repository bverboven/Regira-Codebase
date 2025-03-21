using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Regira.Security.Authentication.Jwt.Samples;

public class AccessTokenHelper(IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
{
    /// <summary>
    /// Gets and returns an access token. Refreshes the token when needed.
    /// </summary>
    /// <param name="refreshOptions">Needed to refresh the token if expired</param>
    /// <returns>Access token</returns>
    public async Task<string?> GetToken(RefreshOptions? refreshOptions = null)
    {
        if (!await IsExpired())
        {
            var context = httpContextAccessor.HttpContext ?? throw new NotSupportedException("HttpContext does not exist");
            // no need to refresh
            return await context.GetTokenAsync("access_token");
        }

        if (refreshOptions == null)
        {
            throw new ArgumentNullException($"{nameof(refreshOptions)} cannot be null when token is expired");
        }

        return await RefreshToken(refreshOptions);
    }
    /// <summary>
    /// Checks if token is expired
    /// </summary>
    /// <returns>true when token is expired</returns>
    public async Task<bool> IsExpired()
    {
        var context = httpContextAccessor.HttpContext ?? throw new NotSupportedException("HttpContext does not exist");
        var expiresAt = await context.GetTokenAsync("expires_at");
        if (expiresAt == null)
        {
            throw new Exception("Token (expires_at) not found");
        }
        var expiresAtAsDateTimeOffset = DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture);
        return expiresAtAsDateTimeOffset.AddSeconds(-60).ToUniversalTime() <= DateTime.UtcNow;
    }
    /// <summary>
    /// Refreshes access token and stores the new IdToken, AccessToken and RefreshToken in the HttpContext.
    /// </summary>
    /// <param name="refreshOptions">Parameters to authenticate the Client at the Authority</param>
    /// <returns>Valid access token</returns>
    public async Task<string> RefreshToken(RefreshOptions refreshOptions)
    {
        var context = httpContextAccessor.HttpContext ?? throw new NotSupportedException("HttpContext does not exist");

        // discovery document
        var discoveryResponse = await httpClient.GetDiscoveryDocumentAsync();

        // refresh the tokens
        var refreshToken = await context.GetTokenAsync("refresh_token");
        var refreshTokenRequest = new RefreshTokenRequest
        {
            Address = discoveryResponse.TokenEndpoint,
            ClientId = refreshOptions.ClientId,
            ClientSecret = refreshOptions.ClientSecret,
            RefreshToken = refreshToken
        };
        var refreshResponse = await httpClient.RequestRefreshTokenAsync(refreshTokenRequest);

        // store the tokens
        var updatedTokens = new[] {
            new AuthenticationToken {
                Name = "id_token",
                Value = refreshResponse.IdentityToken
            },
            new AuthenticationToken {
                Name = "access_token",
                Value = refreshResponse.AccessToken
            },
            new AuthenticationToken {
                Name = "refresh_token",
                Value = refreshResponse.RefreshToken
            },
            new AuthenticationToken {
                Name = "expires_at",
                Value = (DateTime.UtcNow + TimeSpan.FromSeconds(refreshResponse.ExpiresIn))
                    .ToString("o", CultureInfo.InvariantCulture)
            }
        };

        // get authenticate result, containing the current principal & properties
        var currentAuthenticateResult = await context.AuthenticateAsync(refreshOptions.AuthenticationScheme);

        // store the updated tokens
        currentAuthenticateResult.Properties?.StoreTokens(updatedTokens);

        // sign in
        await context.SignInAsync(refreshOptions.AuthenticationScheme, currentAuthenticateResult.Principal!, currentAuthenticateResult.Properties);

        return refreshResponse.RefreshToken;
    }
}