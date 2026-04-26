using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Security.Testing.Infrastructure;

[ApiController]
[Route("")]
public class TestController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("welcome");
    }

    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok("You're safe");
    }

    [Authorize(Roles = "admin")]
    [HttpGet("protected/admin")]
    public IActionResult AdminOnly()
    {
        return Ok("admin area");
    }

    [HttpGet("protected/me")]
    public IActionResult Me()
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(ownerId);
    }
}