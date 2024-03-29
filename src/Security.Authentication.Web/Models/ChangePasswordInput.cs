using System.ComponentModel.DataAnnotations;

namespace Regira.Security.Authentication.Web.Models;

public class ChangePasswordInput
{
    [Required]
    public string NewPassword { get; set; } = null!;
    [Required]
    public string CurrentPassword { get; set; } = null!;
}