using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Jwt.Extensions;
using Regira.Security.Authentication.Web.Constants;
using Regira.Security.Authentication.Web.Models;
using Regira.Web.Utilities;
using System.Security.Claims;

namespace Regira.Security.Authentication.Web.Controllers;

[ApiController]
[Route("auth")]
public abstract class AccountControllerBase<TUser>(ITokenHelper tokenHelper, UserManager<TUser> userManager, IUserClaimsPrincipalFactory<TUser> claimsFactory, ILogger? logger = null) : ControllerBase
    where TUser : IdentityUser<string>
{
    [AllowAnonymous]
    [HttpPost]
    [Route("", Name = RouteNames.Authenticate)]
    public virtual async Task<IActionResult> Authenticate([FromBody] AuthenticateInput model, [FromQuery] string clientApp)
    {
        bool? isLockedOut = null;
        DateTimeOffset? lockedOutEnd = null;

        var user = await userManager.FindByNameAsync(model.Username!);
        if (user != null)
        {
            isLockedOut = await userManager.IsLockedOutAsync(user);
            if (isLockedOut == false)
            {
                bool isAuthenticated = await userManager.CheckPasswordAsync(user, model.Password ?? string.Empty);
                if (isAuthenticated)
                {
                    var principal = await claimsFactory.CreateAsync(user);
                    return Ok(CreateSuccessResponse(principal.Claims, clientApp));
                }
                // authentication failed
                await userManager.AccessFailedAsync(user);
            }
            else
            {
                lockedOutEnd = await userManager.GetLockoutEndDateAsync(user);
                logger?.LogWarning($"User {user.Id} {Request.GetIPAddress()} locked out until {lockedOutEnd:HH:mm:ss}");
            }
        }

        return StatusCode(StatusCodes.Status401Unauthorized, CreateFailedResponse(isLockedOut, lockedOutEnd));
    }

    [HttpPost("validate")]
    public virtual async Task<IActionResult> Validate()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            // check if user is valid
            var exists = await userManager.FindByIdAsync(User.FindUserId()!) != null;
            return exists ? NoContent() : Forbid();
        }

        return Unauthorized();
    }
    [HttpPost("refresh")]
    public virtual async Task<IActionResult> Refresh()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                isAuthenticated = false
            });
        }
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(new
            {
                isAuthenticated = false
            });
        }
        var principal = await claimsFactory.CreateAsync(user);
        return Ok(CreateSuccessResponse(principal.Claims, User.FindFirstValue("aud")!));
    }


    [HttpGet("personal-data")]
    public virtual async Task<IActionResult> GetPersonalData()
    {
        var user = await userManager.FindByIdAsync(User.FindUserId()!);
        if (user == null)
        {
            return Unauthorized();
        }
        var principal = await claimsFactory.CreateAsync(user);
        var personalDataClaimTypes = new[]
        {
            JwtClaimTypes.GivenName, JwtClaimTypes.FamilyName
        };
        var personalData = principal.Claims.Where(c => personalDataClaimTypes.Contains(c.Type))
            .ToDictionary(x => x.Type, x => x.Value);
        return Ok(personalData);
    }


    protected AuthenticateResponseDto CreateFailedResponse(bool? isLockedOut = null, DateTimeOffset? lockedOutEnd = null)
    {
        return new AuthenticateResponseDto
        {
            IsLockedOut = isLockedOut,
            // datetime without timezone
            LockedOutEnd = lockedOutEnd.HasValue ? new DateTime(lockedOutEnd.Value.Ticks) : null
        };
    }
    protected AuthenticateResponseDto CreateSuccessResponse(IEnumerable<Claim> claims, string? audience = null)
    {
        var token = tokenHelper.Create(claims, audience);
        return new AuthenticateResponseDto
        {
            IsAuthenticated = true,
            Token = token
        };
    }
}
