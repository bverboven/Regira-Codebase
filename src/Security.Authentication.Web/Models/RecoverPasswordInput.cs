using System.ComponentModel.DataAnnotations;

namespace Regira.Security.Authentication.Web.Models;

public class RecoverPasswordInput
{
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string SiteUrl { get; set; } = null!;
    [Required]
    public string? SiteName { get; set; }
}