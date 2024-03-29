using System.ComponentModel.DataAnnotations;

namespace Regira.Security.Authentication.Web.Models;

public class ResetPasswordInput
{
    [Required]
    public string Token { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
