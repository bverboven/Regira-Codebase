# Regira Security — Examples

## Example 1: Hash and verify a password

```csharp
// BCrypt is the recommended hasher for passwords
IHasher hasher = new Regira.Security.Hashing.BCryptNet.Hasher();

// On registration
string stored = hasher.Hash(userInput.Password);
// Persist 'stored' to the database

// On login
bool valid = hasher.Verify(loginInput.Password, stored);
if (!valid)
    return Unauthorized();
```

---

## Example 2: Encrypt and decrypt a stored value

Use `AesEncrypter` for values that must be recoverable (API keys, tokens, PII).

```csharp
var enc = new AesEncrypter(new CryptoOptions
{
    Secret = configuration["Crypto:Secret"]
});

// Encrypt before storing
string cipher = enc.Encrypt(creditCardNumber);
await db.SaveAsync(new StoredSecret { Value = cipher });

// Decrypt on retrieval
var record    = await db.LoadAsync(id);
string plain  = enc.Decrypt(record.Value);
```

---

## Example 3: JWT authentication setup

```csharp
// Program.cs
services.AddJwtAuthentication(options =>
{
    options.Secret    = configuration["Jwt:Secret"]!;
    options.Authority = configuration["Jwt:Issuer"];
    options.Audience  = configuration["Jwt:Audience"];
    options.LifeSpan  = 3600;   // 1 hour
});

app.UseAuthentication();
app.UseAuthorization();
```

Issue a token after verifying credentials:

```csharp
[HttpPost("auth")]
public IActionResult Authenticate(
    [FromBody] AuthenticateInput input,
    ITokenHelper tokens,
    UserManager<AppUser> users)
{
    var user = await users.FindByNameAsync(input.Username);
    if (user == null || !await users.CheckPasswordAsync(user, input.Password))
        return Unauthorized();

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName!),
        new Claim(ClaimTypes.Email, user.Email!)
    };

    return Ok(new { Token = tokens.Create(claims) });
}
```

---

## Example 4: API Key authentication

```csharp
// Program.cs — in-memory keys
services.AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(new[]
        {
            new ApiKeyOwner { OwnerId = "partner-a", Key = "sk-abc123", Roles = ["read"] },
            new ApiKeyOwner { OwnerId = "admin",     Key = "sk-xyz789", Roles = ["read", "write"] }
        });

app.UseAuthentication();
app.UseAuthorization();
```

Protect an endpoint:

```csharp
[Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
[HttpGet("data")]
public IActionResult GetData() => Ok(data);
```

---

## Example 5: Pre-built AccountController

Inherit the base controller to get `/auth`, `/auth/validate`, and `/auth/refresh` for free:

```csharp
[ApiController]
[Route("[controller]")]
public class AuthController(
    ITokenHelper tokens,
    UserManager<AppUser> users,
    IUserClaimsPrincipalFactory<AppUser> factory)
    : AccountControllerBase<AppUser>(tokens, users, factory);
```

---

## Example 6: Pre-built UserController with email confirmation

```csharp
[ApiController]
[Route("[controller]")]
public class UsersController(
    UserManager<AppUser> users,
    ISerializer serializer)
    : UserControllerBase<AppUser>(users, serializer);

// POST /users — creates user, sends confirmation email
// POST /users/confirm-email — confirms with token
```

Register the mailer so Identity confirmation emails are sent:

```csharp
services.AddSingleton<IEmailSender>(provider =>
    new IdentityMailer(
        provider.GetRequiredService<IMailer>(),
        new IdentityMailerOptions { Sender = "no-reply@example.com" }
    ));
```

---

## Overview

1. [Index](../README.md) — Overview, encryption, hashing, JWT, API key, and controllers
1. **[Examples](examples.md)** — Hash passwords, JWT setup, API key auth, Identity controllers
