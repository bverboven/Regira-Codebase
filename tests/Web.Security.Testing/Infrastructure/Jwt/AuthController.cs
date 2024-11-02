using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Regira.Security.Authentication.Jwt.Abstraction;
using System.Security.Claims;

namespace Web.Security.Testing.Infrastructure.Jwt;

[ApiController]
[Route("auth")]
public class AuthController(ITokenHelper tokenHelper) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(AuthInput input)
    {
        var user = JwtUsers.Value.FirstOrDefault(x => x.Name == input.Username);
        // for testing purposes, accept any password except when empty
        if (user == null || string.IsNullOrWhiteSpace(input.Password))
        {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
        var token = tokenHelper.Create(
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserId!),
            new Claim(ClaimTypes.Name, user.Name!)
        ]);

        return Ok(new TokenResult
        {
            IsAuthenticated = true,
            Token = token
        });
    }
}