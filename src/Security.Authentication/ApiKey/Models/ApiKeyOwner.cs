namespace Regira.Security.Authentication.ApiKey.Models;

public class ApiKeyOwner
{
    public string OwnerId { get; set; } = null!;
    public string Key { get; set; } = null!;

    public ICollection<string> Roles { get; set; } = new List<string>();
}