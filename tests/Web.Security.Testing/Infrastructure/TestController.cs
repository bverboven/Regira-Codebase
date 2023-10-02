using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}